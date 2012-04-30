using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Chorus.Utilities;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers.Mercurial
{
	class HgResumeException : Exception
	{
		public HgResumeException(string message) : base(message) {}
	}
	class HgResumeOperationFailed : HgResumeException
	{
		public HgResumeOperationFailed(string message) : base(message) {}
	}

	public class HgResumeTransport : IHgTransport
	{
		private IProgress _progress;
		private HgRepository _repo;
		private string _targetLabel;
		private IApiServer _apiServer;

		private const int initialChunkSize = 5000;
		private const int maximumChunkSize = 250000;
		private const int timeoutInSeconds = 15;
		private const int targetTimeInSeconds = timeoutInSeconds/3;


		///<summary>
		///</summary>
		public HgResumeTransport(HgRepository repo, string targetLabel, IApiServer apiServer, IProgress progress)
		{
			_repo = repo;
			_targetLabel = targetLabel;
			_apiServer = apiServer;
			_progress = progress;
		}

		private string RepoIdentifier
		{
			get
			{
				if (_repo.Identifier != null)
				{
					return _repo.Identifier;
				}
				return _apiServer.Host.Replace('.', '_') + '-' + _apiServer.ProjectId;
			}
		}

		///<summary>
		/// Implements a simple file-based key:value db.  Should be replaced by something better in the future.  Stores the last known common base for a given api server
		/// File DB is line separated list of "remoteId|hash" pairs
		///
		/// TODO: implement this using an object serialization class like system.xml.serialization
		///</summary>
		private string LastKnownCommonBase
		{
			get
			{
				string storagePath = PathToLocalStorage;
				if (!Directory.Exists(storagePath))
				{
					Directory.CreateDirectory(storagePath);
				}
				string filePath = Path.Combine(storagePath, "remoteRepo.db");
				if (File.Exists(filePath))
				{
					string[] dbFileContents = File.ReadAllLines(filePath);
					string remoteId = _apiServer.Host;
					var db = dbFileContents.Select(i => i.Split('|'));
					var result = db.Where(x => x[0] == remoteId);
					if (result.Count() > 0)
					{
						return result.First()[1];
					}
				}
				return "";
			}
			set
			{
				string storagePath = PathToLocalStorage;
				if (!Directory.Exists(storagePath))
				{
					Directory.CreateDirectory(storagePath);
				}
				string filePath = Path.Combine(storagePath, "remoteRepo.db");

				string remoteId = _apiServer.Host;
				if (!File.Exists(filePath))
				{
					// this is the first time "set" has been called.  Write value and exit.
					File.WriteAllText(filePath, remoteId + "|" + value);
					return;
				}

				var dbFileContents = File.ReadAllLines(filePath);
				var db = dbFileContents.Select(i => i.Split('|'));
				var result = db.Where(x => x[0] == remoteId);
				if (result.Count() > 0)
				{
					string oldRev = result.Single()[1];
					if (oldRev == value)
					{
						return;
					}
					int indexToChange = Array.IndexOf(dbFileContents, remoteId + "|" + oldRev);
					dbFileContents[indexToChange] = remoteId + "|" + value;
				} else
				{
					var dbFileContentsAsList = dbFileContents.ToList();
					dbFileContentsAsList.Add(remoteId + "|" + value);
					dbFileContents = dbFileContentsAsList.ToArray();
				}
				File.WriteAllLines(filePath, dbFileContents);
			}
		}

		private string GetCommonBaseHashWithRemoteRepo()
		{
			return GetCommonBaseHashWithRemoteRepo(true);
		}

		private string GetCommonBaseHashWithRemoteRepo(bool useCache)
		{
			if (useCache && !string.IsNullOrEmpty(LastKnownCommonBase))
			{
				return LastKnownCommonBase;
			}
			int offset = 0;
			const int quantity = 200;
			IEnumerable<string> localRevisions = _repo.GetAllRevisions().Select(rev => rev.Number.Hash);
			string commonBase = "";
			while (string.IsNullOrEmpty(commonBase))
			{
				var remoteRevisions = GetRemoteRevisions(offset, quantity);
				if (remoteRevisions.Count() == 1 && remoteRevisions.First() == "0")
				{
					// special case when remote repo is empty (initialized with no changesets)
					commonBase = "0";
					break;
				}
				foreach (var rev in remoteRevisions)
				{
					if (localRevisions.Contains(rev))
					{
						commonBase = rev;
						break;
					}
				}
				if (string.IsNullOrEmpty(commonBase) && remoteRevisions.Count() < quantity)
				{
					commonBase = localRevisions.Last();
				}
				offset += quantity;
			}
			LastKnownCommonBase = commonBase;
			return commonBase;
		}


		private IEnumerable<string> GetRemoteRevisions(int offset, int quantity)
		{
			const int totalNumOfAttempts = 2;
			for (int attempt = 1; attempt <= totalNumOfAttempts; attempt++)
			{
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				try
				{
					var response = _apiServer.Execute("getRevisions", new HgResumeApiParameters
																		  {
																			  RepoId = _apiServer.ProjectId,
																			  StartOfWindow = offset,
																			  Quantity = quantity
																		  },
																		  timeoutInSeconds);
					_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
					// API returns either 200 OK or 400 Bad Request
					// HgR status can be: SUCCESS (200), FAIL (400) or UNKNOWNID (400)

					if (response != null) // null means server timed out
					{
						if (response.StatusCode == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
						{
							var msg = String.Format("Server temporarily unavailable: {0}",
							Encoding.UTF8.GetString(response.Content));
							_progress.WriteError(msg);
							return new List<string>();
						}
						if (response.StatusCode == HttpStatusCode.OK && response.Content.Length > 0)
						{
							string revString = Encoding.UTF8.GetString(response.Content);
							return revString.Split('|').ToList();
						}
						if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers.Status == "UNKNOWNID")
						{
							_progress.WriteWarning("The remote server {0} does not have repoId '{1}'", _targetLabel, _apiServer.ProjectId);
						}
						throw new HgResumeOperationFailed(String.Format("Failed to get remote revisions for {0}", _apiServer.ProjectId));
					}
					if (attempt < totalNumOfAttempts)
					{
						_progress.WriteWarning("Unable to contact server.  Retrying... ({0} of {1} attempts).", attempt + 1, totalNumOfAttempts);
					}
					else
					{
						_progress.WriteWarning("Failed to contact server.");
						throw new HgResumeOperationFailed(String.Format("Failed to get remote revisions for {0}", _apiServer.ProjectId));
					}
				}
				catch (WebException e)
				{
					_progress.WriteError(e.Message);
				}
			}
			throw new HgResumeOperationFailed(String.Format("Failed to get remote revisions for {0}", _apiServer.ProjectId));
		}

		public void Push()
		{
			string baseRevision = GetCommonBaseHashWithRemoteRepo();
			if (String.IsNullOrEmpty(baseRevision))
			{
				var errorMessage = "Push operation failed";
				_progress.WriteError(errorMessage);
				throw new HgResumeOperationFailed(errorMessage);
			}

			// create a bundle to push
			string tip = _repo.GetTip().Number.Hash;
			var bundleId = String.Format("{0}-{1}", baseRevision, tip);
			var bundleHelper = new PushStorageManager(PathToLocalStorage, bundleId);
			var bundleFileInfo = new FileInfo(bundleHelper.BundlePath);
			if (bundleFileInfo.Length == 0)
			{
				bool bundleCreatedSuccessfully = _repo.MakeBundle(baseRevision, bundleHelper.BundlePath);
				if (!bundleCreatedSuccessfully)
				{
					// try again after clearing revision cache
					LastKnownCommonBase = "";
					baseRevision = GetCommonBaseHashWithRemoteRepo();
					bundleCreatedSuccessfully = _repo.MakeBundle(baseRevision, bundleHelper.BundlePath);
					if (!bundleCreatedSuccessfully)
					{
						_progress.WriteError("Unable to create bundle for Push");
						const string errorMessage = "Push operation failed";
						_progress.WriteError(errorMessage);
						throw new HgResumeOperationFailed(errorMessage);
					}
				}
				bundleFileInfo.Refresh();
				if (bundleFileInfo.Length == 0)
				{
					bundleHelper.Cleanup();
					_progress.WriteMessage("No changes to send.  Push operation completed");
					return;
				}
			}

			var req = new HgResumeApiParameters();
			req.RepoId = _apiServer.ProjectId;
			req.TransId = bundleHelper.TransactionId;
			req.StartOfWindow = 0;
			req.BundleSize = (int) bundleFileInfo.Length;
			req.ChunkSize = (req.BundleSize < initialChunkSize) ? req.BundleSize : initialChunkSize;
			req.BaseHash = baseRevision;
			_progress.ProgressIndicator.Initialize();

			int loopCtr = 0;
			do // loop until finished... or until the user cancels
			{
				loopCtr++;
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}

				int dataRemaining = req.BundleSize - req.StartOfWindow;
				if (dataRemaining < req.ChunkSize)
				{
					req.ChunkSize = dataRemaining;
				}
				byte[] bundleChunk = bundleHelper.GetChunk(req.StartOfWindow, req.ChunkSize);

				/* API parameters
				 * $repoId, $baseHash, $bundleSize, $offset, $data, $transId
				 * */
				var response = PushOneChunk(req, bundleChunk);
				if (response.Status == PushStatus.NotAvailable)
				{
					_progress.ProgressIndicator.Initialize();
					_progress.ProgressIndicator.Finish();
					return;
				}
				if (response.Status == PushStatus.Timeout)
				{
					_progress.WriteWarning("Push operation timed out.  Retrying...");
					continue;
				}
				if (response.Status == PushStatus.InvalidHash)
				{
					// this should not happen...but sometimes it gets into a state where it remembers the wrong basehash of the server (CJH Feb-12)
					_progress.WriteVerbose("Invalid basehash response received from server... clearing cache and retrying");
					req.BaseHash = GetCommonBaseHashWithRemoteRepo(false);
					continue;
				}
				if (response.Status == PushStatus.Fail)
				{
					var errorMessage = "Push operation failed";
					_progress.WriteError(errorMessage);
					throw new HgResumeOperationFailed(errorMessage);
				}
				if (response.Status == PushStatus.Reset)
				{
					FinishPush(req.TransId);
					bundleHelper.Cleanup();
					var errorMessage = "Push operation failed";
					_progress.WriteError(errorMessage);
					throw new HgResumeOperationFailed(errorMessage);
				}
				if (response.Status == PushStatus.Complete)
				{
					_progress.WriteMessage("Finished Sending");
					_progress.ProgressIndicator.Finish();
					_progress.WriteVerbose("Push operation completed successfully");

					// update our local knowledge of what the server has
					LastKnownCommonBase = _repo.GetTip().Number.Hash;
					FinishPush(req.TransId);
					bundleHelper.Cleanup();
					return;
				}


				req.ChunkSize = response.ChunkSize;
				req.StartOfWindow = response.StartOfWindow;
				if (loopCtr == 1 && req.StartOfWindow > req.ChunkSize)
				{
					string message = String.Format("Resuming push operation at {0} sent", GetHumanReadableByteSize(req.StartOfWindow));
					_progress.WriteVerbose(message);
				}
				string eta = CalculateEstimatedTimeRemaining(req.BundleSize, req.ChunkSize, req.StartOfWindow);
				_progress.WriteStatus(string.Format("Sending {0} {1}", GetHumanReadableByteSize(req.BundleSize), eta));
				_progress.ProgressIndicator.PercentCompleted = (int)((long)req.StartOfWindow * 100 / req.BundleSize);
			} while (req.StartOfWindow < req.BundleSize);
		}

		private static string CalculateEstimatedTimeRemaining(int bundleSize, int chunkSize, int startOfWindow)
		{
			if (startOfWindow < 80000)
			{
				return ""; // wait until we've transferred 80K before calculating an ETA
			}
			int secondsRemaining = targetTimeInSeconds*(bundleSize - startOfWindow)/chunkSize;
			if (secondsRemaining < 60)
			{
				//secondsRemaining = (secondsRemaining/5+1)*5;
				return "(less than 1 minute)";
			}
			int minutesRemaining = secondsRemaining/60;
			string minutesString = (minutesRemaining > 1) ? "minutes" : "minute";
			return String.Format("(about {0} {1})", minutesRemaining, minutesString);
		}

		private static string GetHumanReadableByteSize(int length)
		{
			// lifted from http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-using-net
			try
			{
				string[] sizes = { "B", "KB", "MB", "GB" };
				int order = 0;
				while (length >= 1024 && order + 1 < sizes.Length)
				{
					order++;
					length = length / 1024;
				}
				return String.Format("{0:0.#}{1}", length, sizes[order]);
			}
			catch(Exception) // I'm not sure why I would get an overflow exception, but I did once and so I'm trying to catch it here
			{
				return "...";
			}
		}

		private PushStatus FinishPush(string transactionId)
		{
			var apiResponse = _apiServer.Execute("finishPushBundle", new HgResumeApiParameters { TransId = transactionId, RepoId = _apiServer.ProjectId }, 20);
			_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
			switch (apiResponse.StatusCode)
			{
				case HttpStatusCode.OK:
					return PushStatus.Complete;
				case HttpStatusCode.BadRequest:
					return PushStatus.Fail;
				case HttpStatusCode.ServiceUnavailable:
					return PushStatus.NotAvailable;
			}
			return PushStatus.Fail;
		}

		private PushResponse PushOneChunk(HgResumeApiParameters request, byte[] dataToPush)
		{
			var pushResponse = new PushResponse(PushStatus.Fail);
			try
			{
				var response = _apiServer.Execute("pushBundleChunk", request, dataToPush, timeoutInSeconds);
				_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
				/* API returns the following HTTP codes:
					* 200 OK (SUCCESS)
					* 202 Accepted (RECEIVED)
					* 412 Precondition Failed (RESEND)
					* 400 Bad Request (FAIL, UNKNOWNID, and RESET)
					*/
				if (response != null) // null means server timed out
				{
					if (response.Headers.HasNote)
					{
						_progress.WriteMessage(String.Format("Server replied: {0}", response.Headers.Note));
					}

					if (response.StatusCode == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
					{
						var msg = String.Format("Server temporarily unavailable: {0}",
												Encoding.UTF8.GetString(response.Content));
						_progress.WriteError(msg);
						pushResponse.Status = PushStatus.NotAvailable;
						return pushResponse;
					}
					// the chunk was received successfully
					if (response.StatusCode == HttpStatusCode.Accepted)
					{
						pushResponse.StartOfWindow = response.Headers.StartOfWindow;
						pushResponse.Status = PushStatus.Received;
						pushResponse.ChunkSize = CalculateChunkSize(request.ChunkSize, response.ResponseTimeInMilliseconds);
						return pushResponse;
					}

					// the final chunk was received successfully
					if (response.StatusCode == HttpStatusCode.OK)
					{
						pushResponse.Status = PushStatus.Complete;
						return pushResponse;
					}

					if (response.StatusCode == HttpStatusCode.BadRequest)
					{
						if (response.Headers.Status == "UNKNOWNID")
						{
							_progress.WriteError("The server {0} does not have the project '{1}'", _targetLabel, _apiServer.ProjectId);
							return pushResponse;
						}
						if (response.Headers.Status == "RESET")
						{
							_progress.WriteError("All chunks were pushed to the server, but the unbundle operation failed.  Try again later.");
							pushResponse.Status = PushStatus.Reset;
							return pushResponse;
						}
						if (response.Headers.HasError)
						{
							if (response.Headers.Error == "invalid baseHash")
							{
								pushResponse.Status = PushStatus.InvalidHash;
							}
							else
							{
								_progress.WriteWarning("Server Error: {0}", response.Headers.Error);
							}
							return pushResponse;
						}

					}
					_progress.WriteWarning("Invalid Server Response '{0}'", response.StatusCode);
					return pushResponse;
				}
				pushResponse.Status = PushStatus.Timeout;
				return pushResponse;
			}
			catch (WebException e)
			{
				_progress.WriteError(e.Message);
				_progress.WriteWarning("The push operation failed on the server");
				return pushResponse;
			}
		}

		private static int CalculateChunkSize(int chunkSize, long responseTimeInMilliseconds)
		{
			// just in case the response time is 0 milliseconds
			if (responseTimeInMilliseconds == 0)
			{
				responseTimeInMilliseconds = 1;
			}

			long newChunkSize = targetTimeInSeconds*1000*chunkSize/responseTimeInMilliseconds;

			if (newChunkSize > maximumChunkSize)
			{
				newChunkSize = maximumChunkSize;
			}

			// if the difference between the new chunksize value is less than 10K, don't suggest a new chunkSize, to avoid fluxuations in chunksizes
			if (Math.Abs(chunkSize - newChunkSize) < 10000)
			{
				return chunkSize;
			}
			return (int) newChunkSize;
		}

		public bool Pull()
		{
			var baseHash = GetCommonBaseHashWithRemoteRepo();
			if (baseHash == "0")
			{
				// a baseHash of 0 indicates that the server has an empty repo
				// in this case there is no reason to Pull
				return false;
			}
			return Pull(baseHash);
		}

		public bool Pull(string baseRevision)
		{
			var tipRevision = _repo.GetTip();
			string localTip = "0";
			string errorMessage;
			if (tipRevision != null)
			{
				localTip = tipRevision.Number.Hash;
			}

			if (String.IsNullOrEmpty(baseRevision))
			{
				errorMessage = "Pull operation failed";
				_progress.WriteError(errorMessage);
				throw new HgResumeOperationFailed(errorMessage);
			}

			var bundleHelper = new PullStorageManager(PathToLocalStorage, baseRevision + "_" + localTip);
			var req = new HgResumeApiParameters();
			req.RepoId = _apiServer.ProjectId;
			req.BaseHash = baseRevision;
			req.TransId = bundleHelper.TransactionId;
			req.StartOfWindow = bundleHelper.StartOfWindow;
			req.ChunkSize = initialChunkSize; // size in bytes
			int bundleSize = 0;

			int loopCtr = 1;
			bool retryLoop;

			do
			{
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				retryLoop = false;
				var response = PullOneChunk(req);
				if (response.Status == PullStatus.NotAvailable)
				{
					_progress.ProgressIndicator.Initialize();
					_progress.ProgressIndicator.Finish();
					return false;
				}
				if (response.Status == PullStatus.Timeout)
				{
					_progress.WriteWarning("Pull operation timed out.  Retrying...");
					retryLoop = true;
					continue;
				}
				if (response.Status == PullStatus.NoChange)
				{
					_progress.WriteMessage("No changes");
					bundleHelper.Cleanup();
					_progress.ProgressIndicator.Initialize();
					_progress.ProgressIndicator.Finish();
					return false;
				}
				if (response.Status == PullStatus.InvalidHash)
				{
					// this should not happen...but sometimes it gets into a state where it remembers the wrong basehash of the server (CJH Feb-12)
					retryLoop = true;
					_progress.WriteVerbose("Invalid basehash response received from server... clearing cache and retrying");
					req.BaseHash = GetCommonBaseHashWithRemoteRepo(false);
					continue;
				}
				if (response.Status == PullStatus.Fail)
				{
					errorMessage = "Pull operation failed";
					_progress.WriteError(errorMessage);
					_progress.ProgressIndicator.Initialize();
					_progress.ProgressIndicator.Finish();
					throw new HgResumeOperationFailed(errorMessage);
				}
				if (response.Status == PullStatus.Reset)
				{
					retryLoop = true;
					bundleHelper.Reset();
					_progress.WriteVerbose("Server's bundle cache has expired.  Restarting pull...");
					req.StartOfWindow = bundleHelper.StartOfWindow;
					continue;
				}

				//bundleSizeFromResponse = response.BundleSize;
				if (loopCtr == 1)
				{
					_progress.ProgressIndicator.Initialize();
					if (req.StartOfWindow != 0)
					{
						string message = String.Format("Resuming pull operation at {0} received", GetHumanReadableByteSize(req.StartOfWindow));
						_progress.WriteVerbose(message);
					}
				}

				bundleHelper.WriteChunk(req.StartOfWindow, response.Chunk);
				req.StartOfWindow = req.StartOfWindow + response.Chunk.Length;
				req.ChunkSize = response.ChunkSize;
				if (bundleSize == response.BundleSize && bundleSize != 0)
				{
					_progress.ProgressIndicator.PercentCompleted = (int)((long)req.StartOfWindow * 100 / bundleSize);
					string eta = CalculateEstimatedTimeRemaining(bundleSize, req.ChunkSize, req.StartOfWindow);
					_progress.WriteStatus(string.Format("Receiving {0} {1}", GetHumanReadableByteSize(bundleSize), eta));
				}
				else
				{
					// this is only useful when the bundle size is significantly large (like with a clone operation) such that
					// the server takes a long time to create the bundle, and the bundleSize continues to rise as the chunks are received
					bundleSize = response.BundleSize;
					_progress.WriteStatus(string.Format("Calculating data to receive (>{0})", GetHumanReadableByteSize(bundleSize)));
				}
				loopCtr++;

			} while (req.StartOfWindow < bundleSize || retryLoop);

			if (_repo.Unbundle(bundleHelper.BundlePath))
			{
				_progress.WriteMessage("Pull operation completed successfully");
				_progress.ProgressIndicator.Finish();
				_progress.WriteMessage("Finished Receiving");
				bundleHelper.Cleanup();
				var response = FinishPull(req.TransId);
				if (response == PullStatus.Reset)
				{
					/* Calling Pull recursively to finish up another pull will mess up the ProgressIndicator.  This case is
					// rare enough that it's not worth trying to get the progress indicator working for a recursive Pull()
					*/
					_progress.WriteMessage("Remote repo has changed.  Initiating additional pull operation");
					return Pull();
				}

				// TODO: we could avoid another network operation if we sent along the bundle's "tip" as chunk metadata
				LastKnownCommonBase = GetRemoteTip();
				return true;
			}
			_progress.WriteError("Received all data but local unbundle operation failed or resulted in multiple heads!");
			_progress.ProgressIndicator.Finish();
			bundleHelper.Cleanup();
			errorMessage = "Pull operation failed";
			_progress.WriteError(errorMessage);
			throw new HgResumeOperationFailed(errorMessage);
		}

		private string GetRemoteTip()
		{
			return GetRemoteRevisions(0, 1).FirstOrDefault();
		}

		private PullStatus FinishPull(string transactionId)
		{
			var apiResponse = _apiServer.Execute("finishPullBundle", new HgResumeApiParameters {TransId  = transactionId, RepoId = _apiServer.ProjectId }, 20);
			switch (apiResponse.StatusCode)
			{
				case HttpStatusCode.OK:
					return PullStatus.OK;
				case HttpStatusCode.BadRequest:
					if (apiResponse.Headers.Status == "RESET")
					{
						return PullStatus.Reset;
					}
					return PullStatus.Fail;
				case HttpStatusCode.ServiceUnavailable:
					return PullStatus.NotAvailable;
			}
			return PullStatus.Fail;
		}

		private PullResponse PullOneChunk(HgResumeApiParameters request)
		{
			var pullResponse = new PullResponse(PullStatus.Fail);
			try
			{
				var response = _apiServer.Execute("pullBundleChunk", request, timeoutInSeconds);
				_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
				/* API returns the following HTTP codes:
					* 200 OK (SUCCESS)
					* 304 Not Modified (NOCHANGE)
					* 400 Bad Request (FAIL, UNKNOWNID)
					*/
				if (response != null) // null means server timed out
				{
					if (response.Headers.HasNote)
					{
						_progress.WriteMessage(String.Format("Server replied: {0}", response.Headers.Note));
					}

					if (response.StatusCode == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
					{
						var msg = String.Format("Server temporarily unavailable: {0}",
						Encoding.UTF8.GetString(response.Content));
						_progress.WriteError(msg);
						pullResponse.Status = PullStatus.NotAvailable;
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.NotModified)
					{
						pullResponse.Status = PullStatus.NoChange;
						return pullResponse;
					}

					// chunk pulled OK
					if (response.StatusCode == HttpStatusCode.OK)
					{
						pullResponse.BundleSize = response.Headers.BundleSize;
						pullResponse.Status = PullStatus.OK;
						pullResponse.ChunkSize = CalculateChunkSize(request.ChunkSize, response.ResponseTimeInMilliseconds);

						pullResponse.Chunk = response.Content;
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers.Status == "UNKNOWNID")
					{
						// this is not implemented currently (feb 2012 cjh)
						_progress.WriteError("The server {0} does not have the project '{1}'", _targetLabel, request.RepoId);
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers.Status == "RESET")
					{
						pullResponse.Status = PullStatus.Reset;
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest)
					{
						if (response.Headers.HasError)
						{
							if (response.Headers.Error == "invalid baseHash")
							{
								pullResponse.Status = PullStatus.InvalidHash;
							} else
							{
								_progress.WriteWarning("Server Error: {0}", response.Headers.Error);
							}
						}
						return pullResponse;
					}
					_progress.WriteWarning("Invalid Server Response '{0}'", response.StatusCode);
					return pullResponse;
				}
				pullResponse.Status = PullStatus.Timeout;
				return pullResponse;
			}
			catch (WebException e)
			{
				_progress.WriteError(e.Message);
				_progress.WriteWarning("The pull operation failed on the server");
				return pullResponse;
			}
		}

		///<summary>
		/// returns something like %AppData%\Chorus\ChorusStorage\uniqueRepoId
		///</summary>
		public string PathToLocalStorage
		{
			get
			{
				string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chorus");
				return Path.Combine(appDataPath,
									Path.Combine("ChorusStorage",
												 RepoIdentifier)
					);
			}
		}

		public void Clone()
		{
			if (!_repo.IsInitialized)
			{
				_repo.Init();
			}
			try
			{
				Pull("0");
			}
			catch(HgResumeOperationFailed)
			{
				throw new HgResumeOperationFailed("Clone operation failed");
			}
		}

		public void RemoveCache()
		{
			var localStoragePath = PathToLocalStorage;
			if (Directory.Exists(localStoragePath))
			{
				Directory.Delete(localStoragePath, true);
			}
		}
	}
}