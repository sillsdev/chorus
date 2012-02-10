using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using Chorus.Utilities;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

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
				string storagePath = PathToLocalStorage(_repo.Identifier);
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
				string storagePath = PathToLocalStorage(_repo.Identifier);
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
			if (!string.IsNullOrEmpty(LastKnownCommonBase))
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
					var response = _apiServer.Execute("getRevisions",
													  new Dictionary<string, string>
														  {
															  { "repoId", _apiServer.ProjectId},
															  {"offset", offset.ToString()},
															  {"quantity", quantity.ToString()}
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
							_progress.WriteWarning(msg);
							return new List<string>();
						}
						if (response.StatusCode == HttpStatusCode.OK && response.Content.Length > 0)
						{
							string revString = Encoding.UTF8.GetString(response.Content);
							return revString.Split('|').ToList();
						}
						if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers["X-HgR-Status"] == "UNKNOWNID")
						{
							_progress.WriteWarning("The remote server {0} does not have repoId '{1}'", _targetLabel, _apiServer.ProjectId);
						}
						_progress.WriteWarning("Failed to get remote revisions for {0}", _apiServer.ProjectId);
						return new List<string>();
					}
					if (attempt < totalNumOfAttempts)
					{
						_progress.WriteWarning("Unable to contact server.  Retrying... ({0} of {1} attempts).", attempt + 1, totalNumOfAttempts);
					}
					else
					{
						_progress.WriteWarning("Failed to contact server.");
					}
				}
				catch (WebException e)
				{
					_progress.WriteError(e.Message);
				}
			}
			return new List<string>();
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
			var bundleHelper = new PushStorageManager(PathToLocalStorage(_repo.Identifier), bundleId);
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
					_progress.WriteStatus("No changes to send.  Push operation completed");
					return;
				}
			}

			string transactionId = bundleHelper.TransactionId;

			int startOfWindow = 0;
			int chunkSize = initialChunkSize;

			var bundleSize = (int) bundleFileInfo.Length;
			_progress.ProgressIndicator.Initialize();

			int loopCtr = 0;
			do // loop until finished... or until the user cancels
			{
				loopCtr++;
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				byte[] bundleChunk = bundleHelper.GetChunk(startOfWindow, chunkSize);
				/* API parameters
				 * $repoId, $baseHash, $bundleSize, $offset, $data, $transId
				 * */

				var requestParameters = new Dictionary<string, string>
											{
												{"repoId", _apiServer.ProjectId},
												{"baseHash", baseRevision},
												{"bundleSize", bundleSize.ToString()},
												{"offset", startOfWindow.ToString()},
												{"transId", transactionId}
											};
				var response = PushOneChunk(requestParameters, bundleChunk);
				if (response.Status == PushStatus.NotAvailable)
				{
					_progress.ProgressIndicator.Initialize();
					_progress.ProgressIndicator.Finish();
					return;
				}
				if (response.Status == PushStatus.Timeout)
				{
					_progress.WriteVerbose("Push operation timed out.  Retrying...");
					continue;
				}
				if (response.Status == PushStatus.InvalidHash)
				{
					// this should not happen...but sometimes it gets into a state where it remembers the wrong basehash of the server (CJH Feb-12)
					LastKnownCommonBase = "";
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
					FinishPush(transactionId);
					bundleHelper.Cleanup();
					var errorMessage = "Push operation failed";
					_progress.WriteError(errorMessage);
					throw new HgResumeOperationFailed(errorMessage);
				}
				if (response.Status == PushStatus.Complete)
				{
					_progress.WriteStatus("Finished Sending");
					_progress.ProgressIndicator.Finish();
					_progress.WriteVerbose("Push operation completed successfully");

					// update our local knowledge of what the server has
					LastKnownCommonBase = _repo.GetTip().Number.Hash;
					FinishPush(transactionId);
					bundleHelper.Cleanup();
					return;
				}


				chunkSize = response.ChunkSize;
				startOfWindow = response.StartOfWindow;
				if (loopCtr == 1 && startOfWindow > chunkSize)
				{
					string message = String.Format("Resuming push operation at {0} bytes", startOfWindow);
					_progress.WriteVerbose(message);
				}
				string eta = GetEstimatedTimeRemaining(bundleSize, chunkSize, startOfWindow);
				_progress.WriteStatus(string.Format("Sent {0} of {1} bytes ({2} remaining)", startOfWindow, bundleSize, eta));
				_progress.ProgressIndicator.PercentCompleted = startOfWindow * 100 / bundleSize;
			} while (startOfWindow < bundleSize);
		}

		private static string GetEstimatedTimeRemaining(int bundleSize, int chunkSize, int startOfWindow)
		{
			int secondsRemaining = targetTimeInSeconds*(bundleSize - startOfWindow)/chunkSize;
			if (secondsRemaining < 60)
			{
				return String.Format("{0} seconds", secondsRemaining);
			}
			return String.Format("{0} minutes {1} seconds", secondsRemaining/60, secondsRemaining%60);
		}

		private PushStatus FinishPush(string transactionId)
		{
			var apiResponse = _apiServer.Execute("finishPushBundle", new Dictionary<string, string> { { "transId", transactionId }, { "repoId", _apiServer.ProjectId } }, 20);
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

		private PushResponse PushOneChunk(Dictionary<string, string> parameters, byte[] dataToPush)
		{
			int chunkSize = dataToPush.Length;
			var pushResponse = new PushResponse(PushStatus.Fail);
			try
			{
				var response = _apiServer.Execute("pushBundleChunk", parameters, dataToPush, timeoutInSeconds);
				_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
				/* API returns the following HTTP codes:
					* 200 OK (SUCCESS)
					* 202 Accepted (RECEIVED)
					* 412 Precondition Failed (RESEND)
					* 400 Bad Request (FAIL, UNKNOWNID, and RESET)
					*/
				if (response != null) // null means server timed out
				{
					if (response.StatusCode == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
					{
						var msg = String.Format("Server temporarily unavailable: {0}",
												Encoding.UTF8.GetString(response.Content));
						_progress.WriteWarning(msg);
						pushResponse.Status = PushStatus.NotAvailable;
						return pushResponse;
					}
					// the chunk was received successfully
					if (response.StatusCode == HttpStatusCode.Accepted)
					{
						pushResponse.StartOfWindow = Convert.ToInt32(response.Headers["X-HgR-Sow"]);
						pushResponse.Status = PushStatus.Received;
						pushResponse.ChunkSize = CalculateChunkSize(chunkSize, response.ResponseTimeInMilliseconds);
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
						if (response.Headers["X-HgR-Status"] == "UNKNOWNID")
						{
							_progress.WriteWarning("The server {0} does not have repoId '{1}'", _targetLabel, _repo.Identifier);
							return pushResponse;
						}
						if (response.Headers["X-HgR-Status"] == "RESET")
						{
							_progress.WriteWarning("All chunks were pushed to the server, but the unbundle operation failed.  Try again later.");
							pushResponse.Status = PushStatus.Reset;
							return pushResponse;
						}
						if (response.Headers.ContainsKey("X-HgR-Error"))
						{
							if (response.Headers["X-HgR-Error"] == "invalid baseHash")
							{
								pushResponse.Status = PushStatus.InvalidHash;
							}
							else
							{
								_progress.WriteWarning("Server Error: {0}", response.Headers["X-HgR-Error"]);
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
			return Pull(GetCommonBaseHashWithRemoteRepo());
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

			var bundleHelper = new PullStorageManager(PathToLocalStorage(_repo.Identifier), baseRevision + "_" + localTip);
			string transactionId = bundleHelper.TransactionId;
			int startOfWindow = bundleHelper.StartOfWindow;
			int chunkSize = initialChunkSize; // size in bytes
			int bundleSize = 0;

			/* API parameters
			 * $repoId, $baseHash, $offset, $chunkSize, $transId
			 * */
			var requestParameters = new Dictionary<string, string>
											{
												{"repoId", _apiServer.ProjectId},
												{"baseHash", baseRevision},
												{"offset", startOfWindow.ToString()},
												{"chunkSize", chunkSize.ToString()},
												{"transId", transactionId}
											};
			int loopCtr = 1;
			bool retryLoop;

			do
			{
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				retryLoop = false;
				var response = PullOneChunk(requestParameters);
				if (response.Status == PullStatus.NotAvailable)
				{
					_progress.ProgressIndicator.Initialize();
					_progress.ProgressIndicator.Finish();
					return false;
				}
				if (response.Status == PullStatus.Timeout)
				{
					_progress.WriteVerbose("Pull operation timed out.  Retrying...");
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
					LastKnownCommonBase = "";
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

				bundleSize = response.BundleSize;
				if (loopCtr == 1)
				{
					_progress.ProgressIndicator.Initialize();
					if (startOfWindow != 0)
					{
						string message = String.Format("Resuming pull operation at {0} bytes", startOfWindow);
						_progress.WriteVerbose(message);
					}
				}

				bundleHelper.WriteChunk(startOfWindow, response.Chunk);
				startOfWindow = startOfWindow + response.Chunk.Length;
				requestParameters["offset"] = startOfWindow.ToString();
				chunkSize = response.ChunkSize;
				requestParameters["chunkSize"] = chunkSize.ToString();

				_progress.ProgressIndicator.PercentCompleted = startOfWindow * 100 / bundleSize;
				string eta = GetEstimatedTimeRemaining(bundleSize, chunkSize, startOfWindow);
				_progress.WriteStatus(string.Format("Received {0} of {1} bytes ({2} remaining)", startOfWindow, bundleSize, eta));

				loopCtr++;

			} while (startOfWindow < bundleSize || retryLoop);

			if (_repo.Unbundle(bundleHelper.BundlePath))
			{
				// TODO: we could avoid another network operation if we sent along the bundle's "tip" as chunk metadata
				LastKnownCommonBase = GetRemoteTip();

				_progress.WriteMessage("Pull operation completed successfully");
				_progress.ProgressIndicator.Finish();
				_progress.WriteStatus("Finished Receiving");
				bundleHelper.Cleanup();
				var response = FinishPull(transactionId);
				if (response == PullStatus.Reset)
				{
					/* Calling Pull recursively to finish up another pull will mess up the ProgressIndicator.  This case is
					// rare enough that it's not worth trying to get the progress indicator working for a recursive Pull()
					*/
					_progress.WriteMessage("Remote repo has changed.  Initiating additional pull operation");
					return Pull();
				}
				return true;
			}
			_progress.WriteError("Received all data but local unbundle operation failed or resulted in multiple heads!");
			_progress.ProgressIndicator.Finish();
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
			var apiResponse = _apiServer.Execute("finishPullBundle", new Dictionary<string, string> { { "transId", transactionId }, {"repoId", _apiServer.ProjectId} }, 20);
			switch (apiResponse.StatusCode)
			{
				case HttpStatusCode.OK:
					return PullStatus.OK;
				case HttpStatusCode.BadRequest:
					if (apiResponse.Headers["X-HgR-Status"] == "RESET")
					{
						return PullStatus.Reset;
					}
					return PullStatus.Fail;
				case HttpStatusCode.ServiceUnavailable:
					return PullStatus.NotAvailable;
			}
			return PullStatus.Fail;
		}

		private PullResponse PullOneChunk(Dictionary<string, string> parameters)
		{
			var pullResponse = new PullResponse(PullStatus.Fail);
			int chunkSize = Convert.ToInt32(parameters["chunkSize"]);
			try
			{
				parameters["chunkSize"] = chunkSize.ToString();
				var response = _apiServer.Execute("pullBundleChunk", parameters, timeoutInSeconds);
				_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
				/* API returns the following HTTP codes:
					* 200 OK (SUCCESS)
					* 304 Not Modified (NOCHANGE)
					* 400 Bad Request (FAIL, UNKNOWNID)
					*/
				if (response != null) // null means server timed out
				{
					if (response.StatusCode == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
					{
						var msg = String.Format("Server temporarily unavailable: {0}",
						Encoding.UTF8.GetString(response.Content));
						_progress.WriteWarning(msg);
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
						int actualChunkSize = Convert.ToInt32(response.Headers["X-HgR-ChunkSize"]);
						pullResponse.BundleSize = Convert.ToInt32(response.Headers["X-HgR-BundleSize"]);
						pullResponse.Status = PullStatus.OK;
						pullResponse.ChunkSize = CalculateChunkSize(chunkSize, response.ResponseTimeInMilliseconds);

						pullResponse.Chunk = response.Content;
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers["X-HgR-Status"] == "UNKNOWNID")
					{
						// this is not implemented currently (feb 2012 cjh)
						_progress.WriteWarning("The server {0} does not have repoId '{1}'", _targetLabel, _repo.Identifier);
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers["X-HgR-Status"] == "RESET")
					{
						pullResponse.Status = PullStatus.Reset;
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest)
					{
						if (response.Headers.ContainsKey("X-HgR-Error"))
						{
							if (response.Headers["X-HgR-Error"] == "invalid baseHash")
							{
								pullResponse.Status = PullStatus.InvalidHash;
							} else
							{
								_progress.WriteWarning("Server Error: {0}", response.Headers["X-HgR-Error"]);
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
		/// returns something like \%AppData%\Chorus\ChorusStorage\uniqueRepoId
		///</summary>
		public static string PathToLocalStorage(string id)
		{
			if (String.IsNullOrEmpty(id))
			{
				id = "0";
			}
			string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chorus");
			return Path.Combine(appDataPath,
								Path.Combine("ChorusStorage",
											 id)
								);
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

		public void Dispose()
		{
		}
	}
}