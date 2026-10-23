using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Chorus.Properties;
using Chorus.Utilities;
using Newtonsoft.Json;
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

	public class HgResumeTransport : IHgTransport, IDisposable
	{
		private readonly IProgress _progress;
		private readonly HgRepository _repo;
		private readonly string _targetLabel;
		private readonly IApiServer _apiServer;

		// Deliberately small: the first chunk is sent before we know anything about the link, and it has
		// to complete inside TimeoutInSeconds even on the slow connections this protocol exists for.
		// CalculateChunkSize ramps from here within a couple of round trips, so there is little to gain
		// by starting bigger and real risk on a bad link.
		private const int InitialChunkSize = 5000;
		private const int MaximumChunkSize = 20000000; // 20MB
		private const int TimeoutInSeconds = 30;
		private const int TargetTimeInSeconds = 7;
		internal const string RevisionCacheFilename = "revisioncache.json";
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

		/// <summary>Disposes the api server (and so its HttpClient), which this transport owns.</summary>
		public void Dispose()
		{
			(_apiServer as IDisposable)?.Dispose();
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
			get => GetLastKnownCommonBases(PathToLocalStorage, _apiServer.Host);
			set => SetLastKnownCommonBases(value, PathToLocalStorage, _apiServer.Host);
		}

		public static List<Revision> GetLastKnownCommonBases(string storagePath, string remoteId)
		{
			if (!Directory.Exists(storagePath))
			{
				Directory.CreateDirectory(storagePath);
			}

			string filePath = Path.Combine(storagePath, RevisionCacheFilename);
			if (File.Exists(filePath))
			{
				List<ServerRevision> revisions = ReadServerRevisionCache(filePath);
				if (revisions != null)
				{
					return revisions.Where(x => x.RemoteId == remoteId).Select(r => r.Revision).ToList();
				}
			}

			return new List<Revision>();
		}

		public static void SetLastKnownCommonBases(List<Revision> revisions, string storagePath, string remoteId)
		{
			var serverRevs = new List<ServerRevision>();
			foreach (var revision in revisions)
			{
				serverRevs.Add(new ServerRevision(remoteId, revision));
			}

			if (!Directory.Exists(storagePath))
			{
				Directory.CreateDirectory(storagePath);
			}

			var filePath = Path.Combine(storagePath, RevisionCacheFilename);
			var fileContents = ReadServerRevisionCache(filePath);
			fileContents.RemoveAll(x => x.RemoteId == remoteId);
			serverRevs.AddRange(fileContents);
			var jsonSerializer = JsonSerializer.CreateDefault();
			using (var stream = File.Open(filePath, FileMode.Create))
			using (var jsonWriter = new JsonTextWriter(new StreamWriter(stream)))
			{
				jsonSerializer.Serialize(jsonWriter, serverRevs);
				jsonWriter.Flush();
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
				var jsonSerializer = JsonSerializer.CreateDefault();
				using (Stream stream = File.Open(filePath, FileMode.Open))
				using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
				{
					var revisions = jsonSerializer.Deserialize<List<ServerRevision>>(jsonReader);
					return revisions;
				}
			}
			else
			{
				return new List<ServerRevision>();
			}
		}

		internal class ServerRevision
		{
			public readonly string RemoteId;
			public readonly Revision Revision;

			//parameter names must be the same as the field names for json serialization to work
			public ServerRevision(string remoteId, Revision revision)
			{
				RemoteId = remoteId;
				Revision = revision;
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
				catch (Exception e) when (e is WebException || e is HttpRequestException)
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

		private static string GetHumanReadableByteSize(long length)
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
			using var activity = LibChorusActivitySource.Value.StartActivity();
			activity?.TagResumableParameters("push", request);

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
						_progress.WriteVerbose(string.Format("Additional error info: {0}", response.ResumableResponse.Error));
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
			catch (Exception e) when (e is WebException || e is HttpRequestException)
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
						  TransId = bundleHelper.TransactionId
					  };
			int bundleSize = 0; // as last reported by the server; it grows while the server is still making the bundle
			var reporter = new PullProgressReporter(_progress);
			bool firstData = true;
			bool retryLoop;

			_progress.ProgressIndicator.Initialize();
			do
			{
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				retryLoop = false;
				// Each request picks up from the end of what is on disk already: what an earlier run left
				// behind, or what arrived before the last request was cut off.
				req.StartOfWindow = bundleHelper.StartOfWindow;
				req.ChunkSize = ChunkSizeToAskFor(req.StartOfWindow, bundleSize);
				var response = PullBundle(req, bundleHelper, reporter);
				if (response.BundleSize > bundleSize)
				{
					bundleSize = response.BundleSize;
				}
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
					// Whatever arrived before the failure is in the bundle file, so the retry carries on from there.
					_progress.WriteWarning("Pull failed after receiving {0}.  Retrying...", GetHumanReadableByteSize(bundleHelper.StartOfWindow));
					retryLoop = true;
					continue;
				}
				if (response.Status == PullStatus.Reset)
				{
					retryLoop = true;
					bundleHelper.Reset();
					bundleSize = 0; // the server will make the bundle afresh
					_progress.WriteVerbose("Server's bundle cache has expired.  Restarting pull...");
					continue;
				}

				if (firstData)
				{
					firstData = false;
					if (req.StartOfWindow != 0)
					{
						string message = String.Format("Resuming pull operation at {0} received", GetHumanReadableByteSize(req.StartOfWindow));
						_progress.WriteVerbose(message);
					}
				}

				// The data went straight into the bundle file as it arrived, so there is nothing to store here.
				reporter.Report(bundleHelper.StartOfWindow, bundleSize);
			} while (bundleHelper.StartOfWindow < bundleSize || retryLoop);

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

		/// <summary>
		/// How much to ask for in the next pullBundleChunk request, given the bundle size the server last
		/// reported (0 if it has not yet). The server answers a request at once when it can satisfy it from
		/// the bundle file as it stands; otherwise, while it is still making the bundle, it waits up to 14
		/// seconds before answering "in progress" (a bundle it has finished is served whole, at once). So:
		/// with nothing known yet, ask for a single byte, which the server can hand over as soon as the
		/// bundle file has started, and which brings back its size so far; while the server has more than
		/// we do, ask for all of that but the last byte, which it serves at once even while still adding to
		/// the file; once we have caught up, ask for the rest of the bundle in one go. If the bundle is
		/// finished that is one streamed response, and if not, this is where waiting for the server is
		/// unavoidable.
		/// </summary>
		internal static int ChunkSizeToAskFor(int offset, int lastReportedBundleSize)
		{
			if (lastReportedBundleSize > offset + 1)
			{
				return lastReportedBundleSize - offset - 1;
			}
			if (lastReportedBundleSize == 0)
			{
				return ProbeChunkSize;
			}
			return RestOfBundleChunkSize(offset);
		}

		private const int ProbeChunkSize = 1;

		/// <summary>
		/// The chunkSize to ask for so that one pullBundleChunk response carries everything after
		/// <paramref name="offset"/>: the server sends the smaller of chunkSize and what is left of the
		/// bundle. The sum offset + chunkSize is kept within int because the server works it out in 32 bits
		/// (to decide whether a bundle it is still building already has enough to serve the request from);
		/// if it wrapped negative, the server would hand out pieces of the unfinished bundle.
		/// Note that the server reads the whole response into memory before sending it, so this holds a
		/// copy of the bundle on the server for as long as the download takes; cap the result here if that
		/// becomes a problem, since the rest of the pull works the same for any size.
		/// </summary>
		private static int RestOfBundleChunkSize(int offset)
		{
			return int.MaxValue - offset;
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

		/// <summary>
		/// One pullBundleChunk request for the bundle from request.StartOfWindow on, written into the bundle
		/// file as it arrives. If the connection is lost part-way, the bytes that did arrive stay in the file
		/// and the status is Fail (or Timeout), so the caller's next request picks up from the new end of the
		/// file rather than from where this one started.
		/// </summary>
		private PullResponse PullBundle(HgResumeApiParameters request, PullStorageManager bundleHelper, PullProgressReporter reporter)
		{
			using var activity = LibChorusActivitySource.Value.StartActivity();
			activity?.TagResumableParameters("pull", request);

			var pullResponse = new PullResponse(PullStatus.Fail);
			int offset = request.StartOfWindow;
			bool probe = request.ChunkSize == ProbeChunkSize; // its one byte says nothing worth a progress report
			try
			{
				HgResumeApiResponse response;
				using (var bundleFile = bundleHelper.OpenForWritingAt(offset))
				{
					response = _apiServer.Execute("pullBundleChunk", request, bundleFile,
						(received, bodyLength) =>
						{
							// the body is the bundle from the offset to (at least) the end the server had
							// when it answered, so its length tells us that much about the bundle's size
							// even if the transfer is cut short
							if (bodyLength >= 0)
							{
								pullResponse.BundleSize = (int) Math.Max(pullResponse.BundleSize, offset + bodyLength);
							}
							if (!probe)
							{
								reporter.Report(offset + received, pullResponse.BundleSize);
							}
						},
						TimeoutInSeconds);
				}
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

				// bundle data pulled OK (it is already in the bundle file)
				if (response.HttpStatus == HttpStatusCode.OK)
				{
					pullResponse.BundleSize = Math.Max(pullResponse.BundleSize, response.ResumableResponse.BundleSize);
					pullResponse.Status = PullStatus.OK;
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
			catch (Exception e) when (e is WebException || e is HttpRequestException)
			{
				_progress.WriteWarning(String.Format("Pull data transfer failed: {0}", e.Message));
				return pullResponse;
			}
		}

		/// <summary>
		/// Progress for a pull whose data arrives as a stream rather than in chunks: the indicator and the
		/// status line are updated at most once a second, with an ETA from the rate measured so far.
		/// </summary>
		private sealed class PullProgressReporter
		{
			private const int ReportIntervalInMilliseconds = 1000;
			private const int MinimumBytesForEta = 80000; // wait until we've transferred 80K before calculating an ETA
			private readonly IProgress _progress;
			private readonly Stopwatch _clock = Stopwatch.StartNew();
			private long _total;
			private long _firstReportBytes = -1;
			private long _firstReportMs;
			private long _lastReportMs = long.MinValue;

			public PullProgressReporter(IProgress progress)
			{
				_progress = progress;
			}

			/// <param name="received">bytes of the bundle received so far, counting earlier requests</param>
			/// <param name="total">the bundle size, if known; the largest value seen wins, since a request
			/// that asks for the rest of the bundle only knows its own length</param>
			public void Report(long received, long total)
			{
				if (total > _total)
				{
					_total = total;
				}
				if (_total <= 0)
				{
					return;
				}
				long now = _clock.ElapsedMilliseconds;
				if (_firstReportBytes < 0)
				{
					_firstReportBytes = received;
					_firstReportMs = now;
				}
				if (received < _total && now - _lastReportMs < ReportIntervalInMilliseconds)
				{
					return;
				}
				_lastReportMs = now;
				_progress.ProgressIndicator.PercentCompleted = (int) (received * 100 / _total);
				string eta = CalculateEstimatedTimeRemaining(_total - received, BytesPerSecond(received, now));
				_progress.WriteStatus(string.Format("Receiving {0} {1}", GetHumanReadableByteSize(_total), eta));
			}

			private double BytesPerSecond(long received, long nowMs)
			{
				long bytes = received - _firstReportBytes;
				long ms = nowMs - _firstReportMs;
				if (bytes < MinimumBytesForEta || ms < ReportIntervalInMilliseconds)
				{
					return 0; // too little to measure yet
				}
				return bytes * 1000.0 / ms;
			}
		}

		internal static string CalculateEstimatedTimeRemaining(long bytesRemaining, double bytesPerSecond)
		{
			if (bytesPerSecond <= 0)
			{
				return "";
			}
			double secondsRemaining = bytesRemaining / bytesPerSecond;
			if (secondsRemaining < 60)
			{
				return "(less than 1 minute)";
			}
			int minutesRemaining = (int) (secondsRemaining / 60);
			string minutesString = (minutesRemaining > 1) ? "minutes" : "minute";
			return String.Format("(about {0} {1})", minutesRemaining, minutesString);
		}

		///<summary>
		/// returns something like:
		/// FLEX:	C:\ProgramData\SIL\FieldWorks\Projects\<ProjectName>\OtherRepositories\<ProjectName>_LIFT\Chorus\ChorusStorage
		/// WeSay:	%USERPROFILE%\Documents\WeSay\<ProjectName>\Chorus\ChorusStorage
		///</summary>
		public string PathToLocalStorage
		{
			get
			{
				return Path.Combine(_repo.PathToRepo, "Chorus", "ChorusStorage");
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