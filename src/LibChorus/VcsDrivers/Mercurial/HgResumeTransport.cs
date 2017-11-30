using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Chorus.Properties;
using Chorus.Utilities;
using SIL.Progress;

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
		private readonly IProgress _progress;
		private readonly HgRepository _repo;
		private readonly string _targetLabel;
		private readonly IApiServer _apiServer;

		private const int InitialChunkSize = 5000;
		private const int MaximumChunkSize = 20000000; // 20MB
		private const int TimeoutInSeconds = 30;
		private const int TargetTimeInSeconds = 7;
		internal const string RevisionCacheFilename = "revisioncache.db";
		internal int RevisionRequestQuantity = 200;

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
		private List<Revision> LastKnownCommonBases
		{
			get
			{
				string storagePath = PathToLocalStorage;
				if (!Directory.Exists(storagePath))
				{
					Directory.CreateDirectory(storagePath);
				}
				string filePath = Path.Combine(storagePath, RevisionCacheFilename);
				if (File.Exists(filePath))
				{
					List<ServerRevision> revisions = ReadServerRevisionCache(filePath);
					if(revisions != null)
					{
						var remoteId = _apiServer.Host;
						var result = revisions.Where(x => x.RemoteId == remoteId);
						if (result.Count() > 0)
						{
							var results = new List<Revision>(result.Count());
							foreach (var rev in result)
							{
								results.Add(rev.Revision);
							}
							return results;
						}
					}
				}
				return new List<Revision>();
			}
			set
			{
				string remoteId = _apiServer.Host;
				var serverRevs = new List<ServerRevision>();
				foreach (var revision in value)
				{
					serverRevs.Add(new ServerRevision(remoteId, revision));
				}
				var storagePath = PathToLocalStorage;
				if (!Directory.Exists(storagePath))
				{
					Directory.CreateDirectory(storagePath);
				}

				var filePath = Path.Combine(storagePath, RevisionCacheFilename);
				var fileContents = ReadServerRevisionCache(filePath);
				fileContents.RemoveAll(x => x.RemoteId == remoteId);
				serverRevs.AddRange(fileContents);
				using(Stream stream = File.Open(filePath, FileMode.Create))
				{
					var bFormatter = new BinaryFormatter();
					bFormatter.Serialize(stream, serverRevs);
					stream.Close();
				}
				return;
			}
		}

		/// <summary>
		/// Used to retrieve the cache of revisions for each server and branch
		/// </summary>
		/// <returns>The contents of the cache file if it exists, or an empty list</returns>
		internal static List<ServerRevision> ReadServerRevisionCache(string filePath)
		{
			if (File.Exists(filePath))
			{
				using (Stream stream = File.Open(filePath, FileMode.Open))
				{
					var bFormatter = new BinaryFormatter();
					var revisions = bFormatter.Deserialize(stream) as List<ServerRevision>;
					stream.Close();
					return revisions;
				}
			}
			else
			{
				return new List<ServerRevision>();
			}
		}

		[Serializable]
		internal class ServerRevision
		{
			public readonly string RemoteId;
			public readonly Revision Revision;

			public ServerRevision(string id, Revision rev)
			{
				RemoteId = id;
				Revision = rev;
			}
		}

		private List<Revision> GetCommonBaseHashesWithRemoteRepo()
		{
			return GetCommonBaseHashesWithRemoteRepo(true);
		}

		private List<Revision> GetCommonBaseHashesWithRemoteRepo(bool useCache)
		{
			if (useCache && LastKnownCommonBases.Count > 0)
			{
				return LastKnownCommonBases;
			}
			int offset = 0;
			var localRevisions = new MultiMap<string, Revision>();
			foreach (var rev in  _repo.GetAllRevisions())
			{
				localRevisions.Add(rev.Branch, rev);
			}

			//The goal here is to to return the first common revision of each branch.
			var commonBases = new List<Revision>();
			var localBranches = new List<string>(localRevisions.Keys);
			while (commonBases.Count < localRevisions.Keys.Count())
			{
				var remoteRevisions = GetRemoteRevisions(offset, RevisionRequestQuantity);
				if (remoteRevisions.Keys.Count() == 1 && remoteRevisions[remoteRevisions.Keys.First()].First().Split(':')[0] == "0")
				{
					// special case when remote repo is empty (initialized with no changesets)
					return new List<Revision>();
				}
				var branchesMatched = new List<string>(); // track branches that we've already found a common revision for
				foreach (var branch in localBranches)
				{
					var localList = localRevisions[branch];
					var remoteList = remoteRevisions[branch];
					foreach (var revision in remoteList)
					{
						var remoteRevision = revision; //copy to local for use in predicate
						var commonRevision = localList.Find(localRev => localRev.Number.Hash == remoteRevision);
						if (commonRevision != null)
						{
							commonBases.Add(commonRevision);
							branchesMatched.Add(branch); // found a common revision for this branch
							break;
						}
					}
				}
				localBranches.RemoveAll(branchesMatched.Contains); // stop looking for common revisions in branches we matched
				if(remoteRevisions.Count() < RevisionRequestQuantity)
				{
					//we did not find a common revision for each branch, but we ran out of revisions from the repo
					break;
				}
				offset += RevisionRequestQuantity;
			}

			// If we have found no common revisions at this point, the remote repo is unrelated
			if (commonBases.Count == 0)
			{
				return null;
			}

			LastKnownCommonBases = commonBases;
			return commonBases;
		}


		private MultiMap<string, string> GetRemoteRevisions(int offset, int quantity)
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
																		  TimeoutInSeconds);
					_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
					// API returns either 200 OK or 400 Bad Request
					// HgR status can be: SUCCESS (200), FAIL (400) or UNKNOWNID (400)

					if (response != null) // null means server timed out
					{
						if (response.HttpStatus == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
						{
							var msg = String.Format("Server temporarily unavailable: {0}",
							Encoding.UTF8.GetString(response.Content));
							_progress.WriteError(msg);
							//Server returned unavailable
							break;
						}
						if (response.HttpStatus == HttpStatusCode.OK && response.Content.Length > 0)
						{
							//The expected response from API version 3 follows the format of
							//revisionhash:branch|revisionhash:branch|...
							var revString = Encoding.UTF8.GetString(response.Content);
							var pairs = revString.Split('|').ToList();
							var revisions = new MultiMap<string, string>();
							foreach (var pair in pairs)
							{
								var hashRevCombo = pair.Split(':');
								if(hashRevCombo.Length < 2)
								{
									throw new HgResumeOperationFailed("Failed to get remote revisions. Server/Client API format mismatch.");
								}
								revisions.Add(hashRevCombo[1], hashRevCombo[0]);
							}
							return revisions;
						}
						if (response.HttpStatus == HttpStatusCode.OK && response.Content.Length == 0)
						{
							//There were no more revisions in the range requested [edge case]
							return new MultiMap<string, string>();
						}
						if (response.HttpStatus == HttpStatusCode.BadRequest && response.ResumableResponse.Status == "UNKNOWNID")
						{
							_progress.WriteWarning("The remote server {0} does not have repoId '{1}'", _targetLabel, _apiServer.ProjectId);
						}
						else if (response.HttpStatus == HttpStatusCode.Unauthorized)
						{
							_progress.WriteWarning(Resources.ksHgTransptUnauthorized);
						}
						else
						{
							_progress.WriteWarning("Invalid Server Response '{0}'", response.HttpStatus);
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

		/// <summary>
		/// Gets a Bundle ID based on the hashes of the available base revisions (branches) and the Tip.
		/// This Bundle ID is presently used exclusively for the name of a file in which BundleStorageManager stores the Transaction ID (GUID)
		/// LT-18093: Because this is a filename, it should maintain a fixed length.
		/// </summary>
		private static string GetBundleIdFilenameBase(string direction, IEnumerable<string> baseRevisions, string tip)
		{
			return string.Format("{0}{1}{2}{3}", direction, string.Join(string.Empty, baseRevisions).GetHashCode(), '-', tip);
		}

		public void Push()
		{
			var baseRevisions = GetCommonBaseHashesWithRemoteRepo();
			if (baseRevisions == null)
			{
				const string errorMessage = "Push failed: A common revision could not be found with the server.";
				_progress.WriteError(errorMessage);
				throw new HgResumeOperationFailed(errorMessage);
			}

			// create a bundle to push
			var bundleHelper = new PushStorageManager(PathToLocalStorage,
				GetBundleIdFilenameBase("push", baseRevisions.Select(rev => rev.Number.Hash), _repo.GetTip().Number.Hash));
			var bundleFileInfo = new FileInfo(bundleHelper.BundlePath);
			if (bundleFileInfo.Length == 0)
			{
				_progress.WriteStatus("Preparing data to send");
				bool bundleCreatedSuccessfully = _repo.MakeBundle(GetHashStringsFromRevisions(baseRevisions), bundleHelper.BundlePath);
				if (!bundleCreatedSuccessfully)
				{
					// try again after clearing revision cache
					LastKnownCommonBases = new List<Revision>();
					baseRevisions = GetCommonBaseHashesWithRemoteRepo();
					bundleCreatedSuccessfully = _repo.MakeBundle(GetHashStringsFromRevisions(baseRevisions), bundleHelper.BundlePath);
					if (!bundleCreatedSuccessfully)
					{
						const string errorMessage = "Push failed: Unable to create local bundle.";
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

			var req = new HgResumeApiParameters
					  {
						  RepoId = _apiServer.ProjectId,
						  TransId = bundleHelper.TransactionId,
						  StartOfWindow = 0,
						  BundleSize = (int) bundleFileInfo.Length
					  };
			req.ChunkSize = (req.BundleSize < InitialChunkSize) ? req.BundleSize : InitialChunkSize;
//            req.BaseHash = baseRevisions; <-- Unless I'm not reading the php right we don't need to set this on push.  JLN Aug-12
			_progress.ProgressIndicator.Initialize();

			_progress.WriteStatus("Sending data");
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
//                    req.BaseHash = GetCommonBaseHashesWithRemoteRepo(false); <-- Unless I'm misreading the php we don't need to set this on a push. JLN Aug-12
					continue;
				}
				if (response.Status == PushStatus.Fail)
				{
					// This 'Fail' intentionally aborts the push attempt.  I think we can continue to go around the loop and retry. See Pull also. CP 2012-06
					continue;
					//var errorMessage = "Push operation failed";
					//_progress.WriteError(errorMessage);
					//throw new HgResumeOperationFailed(errorMessage);
				}
				if (response.Status == PushStatus.Reset)
				{
					FinishPush(req.TransId);
					bundleHelper.Cleanup();
					const string errorMessage = "Push failed: Server reset.";
					_progress.WriteError(errorMessage);
					throw new HgResumeOperationFailed(errorMessage);
				}

				if (response.Status == PushStatus.Complete || req.StartOfWindow >= req.BundleSize)
				{
					if (response.Status == PushStatus.Complete)
					{
						_progress.WriteMessage("Finished sending");
					} else
					{
						_progress.WriteMessage("Finished sending. Server unpacking data");
					}
					_progress.ProgressIndicator.Finish();

					// update our local knowledge of what the server has
					LastKnownCommonBases = new List<Revision>(_repo.BranchingHelper.GetBranches()); // This may be a little optimistic, the server may still be unbundling the data.
					//FinishPush(req.TransId);  // We can't really tell when the server has finished processing our pulled data.  The server cleans up after itself. CP 2012-07
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

		internal static string CalculateEstimatedTimeRemaining(int bundleSize, int chunkSize, int startOfWindow)
		{
			if (startOfWindow < 80000)
			{
				return ""; // wait until we've transferred 80K before calculating an ETA
			}
			int secondsRemaining = (bundleSize - startOfWindow)/chunkSize*TargetTimeInSeconds;
			if (secondsRemaining < 60)
			{
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
			catch(OverflowException) // I'm not sure why I would get an overflow exception, but I did once and so I swallow it here.
			{
				return "...";
			}
		}

		private PushStatus FinishPush(string transactionId)
		{
			var apiResponse = _apiServer.Execute("finishPushBundle", new HgResumeApiParameters { TransId = transactionId, RepoId = _apiServer.ProjectId }, 20);
			_progress.WriteVerbose("API URL: {0}", _apiServer.Url);
			switch (apiResponse.HttpStatus)
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
				HgResumeApiResponse response = _apiServer.Execute("pushBundleChunk", request, dataToPush, TimeoutInSeconds);
				if (response == null)
				{
					_progress.WriteVerbose("API REQ: {0} Timeout");
					pushResponse.Status = PushStatus.Timeout;
					return pushResponse;
				}
				/* API returns the following HTTP codes:
				 * 200 OK (SUCCESS)
				 * 202 Accepted (RECEIVED)
				 * 412 Precondition Failed (RESEND)
				 * 400 Bad Request (FAIL, UNKNOWNID, and RESET)
				 */
				_progress.WriteVerbose("API REQ: {0} RSP: {1} in {2}ms", _apiServer.Url, response.HttpStatus, response.ResponseTimeInMilliseconds);
				if (response.HttpStatus == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
				{
					var msg = String.Format("Server temporarily unavailable: {0}",
											Encoding.UTF8.GetString(response.Content));
					_progress.WriteError(msg);
					pushResponse.Status = PushStatus.NotAvailable;
					return pushResponse;
				}
				if (response.ResumableResponse.HasNote)
				{
					_progress.WriteWarning(String.Format("Server replied: {0}", response.ResumableResponse.Note));
				}
				// the chunk was received successfully
				if (response.HttpStatus == HttpStatusCode.Accepted)
				{
					pushResponse.StartOfWindow = response.ResumableResponse.StartOfWindow;
					pushResponse.Status = PushStatus.Received;
					pushResponse.ChunkSize = CalculateChunkSize(request.ChunkSize, response.ResponseTimeInMilliseconds);
					return pushResponse;
				}

				// the final chunk was received successfully
				if (response.HttpStatus == HttpStatusCode.OK)
				{
					pushResponse.Status = PushStatus.Complete;
					return pushResponse;
				}

				if (response.HttpStatus == HttpStatusCode.BadRequest)
				{
					if (response.ResumableResponse.Status == "UNKNOWNID")
					{
						_progress.WriteError("The server {0} does not have the project '{1}'", _targetLabel, _apiServer.ProjectId);
						return pushResponse;
					}
					if (response.ResumableResponse.Status == "RESET")
					{
						_progress.WriteError("Push failed: All chunks were pushed to the server, but the unbundle operation failed.  Try again later.");
						pushResponse.Status = PushStatus.Reset;
						return pushResponse;
					}
					if (response.ResumableResponse.HasError)
					{
						if (response.ResumableResponse.Error == "invalid baseHash")
						{
							pushResponse.Status = PushStatus.InvalidHash;
						}
						else
						{
							_progress.WriteError("Server Error: {0}", response.ResumableResponse.Error);
						}
						return pushResponse;
					}

				}
				_progress.WriteWarning("Invalid Server Response '{0}'", response.HttpStatus);
				return pushResponse;
			}
			catch (WebException e)
			{
				_progress.WriteWarning(String.Format("Push data chunk failed: {0}", e.Message));
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
			long newChunkSize = (long) ((float) chunkSize / responseTimeInMilliseconds * TargetTimeInSeconds * 1000);

			if (newChunkSize > MaximumChunkSize)
			{
				newChunkSize = MaximumChunkSize;
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
			var baseHashes = GetCommonBaseHashesWithRemoteRepo();
			if (baseHashes == null || baseHashes.Count == 0)
			{
				// a null or empty list indicates that the server has an empty repo
				// in this case there is no reason to Pull
				return false;
			}
			return Pull(GetHashStringsFromRevisions(baseHashes));
		}

		public bool Pull(string[] baseRevisions)
		{
			var tipRevision = _repo.GetTip();
			var localTip = "0";
			string errorMessage;
			if (tipRevision != null)
			{
				localTip = tipRevision.Number.Hash;
			}

			if (baseRevisions.Length == 0)
			{
				errorMessage = "Pull failed: No base revision.";
				_progress.WriteError(errorMessage);
				throw new HgResumeOperationFailed(errorMessage);
			}

			var bundleHelper = new PullStorageManager(PathToLocalStorage, GetBundleIdFilenameBase("pull", baseRevisions, localTip));
			var req = new HgResumeApiParameters
					  {
						  RepoId = _apiServer.ProjectId,
						  BaseHashes = baseRevisions,
						  TransId = bundleHelper.TransactionId,
						  StartOfWindow = bundleHelper.StartOfWindow,
						  ChunkSize = InitialChunkSize
					  };
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
				if (response.Status == PullStatus.Unauthorized)
				{
					throw new UnauthorizedAccessException();
				}
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
				if (response.Status == PullStatus.InProgress)
				{
					_progress.WriteStatus("Preparing data on server");
					retryLoop = true;
					// advance the progress bar 2% to show that something is happening
					_progress.ProgressIndicator.PercentCompleted = _progress.ProgressIndicator.PercentCompleted + 2;
					continue;
				}
				if (response.Status == PullStatus.InvalidHash)
				{
					// this should not happen...but sometimes it gets into a state where it remembers the wrong basehash of the server (CJH Feb-12)
					retryLoop = true;
					_progress.WriteVerbose("Invalid basehash response received from server... clearing cache and retrying");
					req.BaseHashes = GetHashStringsFromRevisions(GetCommonBaseHashesWithRemoteRepo(false));
					continue;
				}
				if (response.Status == PullStatus.Fail)
				{
					// No need to abort the attempts just because .Net web request says so. See Push also. CP 2012-06
					_progress.WriteWarning("Pull data chunk failed");
					retryLoop = true;
					req.StartOfWindow = bundleHelper.StartOfWindow;
					continue;
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
					_progress.WriteStatus(string.Format("Preparing data on server (>{0})", GetHumanReadableByteSize(bundleSize)));
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
					GetCommonBaseHashesWithRemoteRepo(false); //Rebuild cache with current server information
					return Pull();
				}

				// REVIEW: I'm not sure why this was set to the server tip before, if we just pulled then won't our head
				// be the correct common base? Maybe not if a merge needs to happen,
				LastKnownCommonBases = new List<Revision>(_repo.BranchingHelper.GetBranches());
				return true;
			}
			_progress.WriteError("Received all data but local unbundle operation failed or resulted in multiple heads!");
			_progress.ProgressIndicator.Finish();
			bundleHelper.Cleanup();
			errorMessage = "Pull operation failed";
			_progress.WriteError(errorMessage);
			throw new HgResumeOperationFailed(errorMessage);
		}

		internal static string[] GetHashStringsFromRevisions(IEnumerable<Revision> branchHeadRevisions)
		{
			var hashes = new string[branchHeadRevisions.Count()];
			for(var index = 0; index < branchHeadRevisions.Count(); ++index)
			{
				hashes[index] = branchHeadRevisions.ElementAt(index).Number.Hash;
			}
			return hashes;
		}

		private PullStatus FinishPull(string transactionId)
		{
			var apiResponse = _apiServer.Execute("finishPullBundle", new HgResumeApiParameters {TransId  = transactionId, RepoId = _apiServer.ProjectId }, 20);
			switch (apiResponse.HttpStatus)
			{
				case HttpStatusCode.OK:
					return PullStatus.OK;
				case HttpStatusCode.BadRequest:
					if (apiResponse.ResumableResponse.Status == "RESET")
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

				HgResumeApiResponse response = _apiServer.Execute("pullBundleChunk", request, TimeoutInSeconds);
				if (response == null)
				{
					_progress.WriteVerbose("API REQ: {0} Timeout", _apiServer.Url);
					pullResponse.Status = PullStatus.Timeout;
					return pullResponse;
				}
				/* API returns the following HTTP codes:
				 * 200 OK (SUCCESS)
				 * 304 Not Modified (NOCHANGE)
				 * 400 Bad Request (FAIL, UNKNOWNID)
				 */
				_progress.WriteVerbose("API REQ: {0} RSP: {1} in {2}ms", _apiServer.Url, response.HttpStatus, response.ResponseTimeInMilliseconds);
				if (response.ResumableResponse.HasNote)
				{
					_progress.WriteMessage(String.Format("Server replied: {0}", response.ResumableResponse.Note));
				}

				if (response.HttpStatus == HttpStatusCode.ServiceUnavailable && response.Content.Length > 0)
				{
					var msg = String.Format("Server temporarily unavailable: {0}",
											Encoding.UTF8.GetString(response.Content));
					_progress.WriteError(msg);
					pullResponse.Status = PullStatus.NotAvailable;
					return pullResponse;
				}
				if (response.HttpStatus == HttpStatusCode.NotModified)
				{
					pullResponse.Status = PullStatus.NoChange;
					return pullResponse;
				}
				if (response.HttpStatus == HttpStatusCode.Accepted)
				{
					pullResponse.Status = PullStatus.InProgress;
					return pullResponse;
				}

				// chunk pulled OK
				if (response.HttpStatus == HttpStatusCode.OK)
				{
					pullResponse.BundleSize = response.ResumableResponse.BundleSize;
					pullResponse.Status = PullStatus.OK;
					pullResponse.ChunkSize = CalculateChunkSize(request.ChunkSize, response.ResponseTimeInMilliseconds);

					pullResponse.Chunk = response.Content;
					return pullResponse;
				}
				if (response.HttpStatus == HttpStatusCode.BadRequest && response.ResumableResponse.Status == "UNKNOWNID")
				{
					// this is not implemented currently (feb 2012 cjh)
					_progress.WriteError("The server {0} does not have the project '{1}'", _targetLabel, request.RepoId);
					return pullResponse;
				}
				if (response.HttpStatus == HttpStatusCode.BadRequest && response.ResumableResponse.Status == "RESET")
				{
					pullResponse.Status = PullStatus.Reset;
					return pullResponse;
				}
				if (response.HttpStatus == HttpStatusCode.BadRequest)
				{
					if (response.ResumableResponse.HasError)
					{
						if (response.ResumableResponse.Error == "invalid baseHash")
						{
							pullResponse.Status = PullStatus.InvalidHash;
						} else
						{
							_progress.WriteWarning("Server Error: {0}", response.ResumableResponse.Error);
						}
					}
					return pullResponse;
				}
				if (response.HttpStatus == HttpStatusCode.Unauthorized)
				{
					_progress.WriteWarning(Resources.ksHgTransptUnauthorized);
					pullResponse.Status =  PullStatus.Unauthorized;
				}
				_progress.WriteWarning("Invalid Server Response '{0}'", response.HttpStatus);
				return pullResponse;
			}
			catch (WebException e)
			{
				_progress.WriteWarning(String.Format("Pull data chunk failed: {0}", e.Message));
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
				Pull(new []{"0"});
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