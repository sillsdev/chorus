using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Chorus.Utilities;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace Chorus.VcsDrivers.Mercurial
{


	public class HgResumeTransport : IHgTransport
	{
		private IProgress _progress;
		private HgRepository _repo;
		private string _targetLabel;
		private IApiServer _apiServer;

		private const int DefaultChunkSize = 10000;

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
		///</summary>
		private string LastKnownCommonBase
		{
			get
			{
				string filePath = Path.Combine(_repo.PathToRepo, "remoteRepo.db");
				if (File.Exists(filePath))
				{
					string[] dbFileContents = File.ReadAllLines(filePath);
					string remoteId = _apiServer.Identifier;
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
				if (String.IsNullOrEmpty(value)) return;

				string filePath = Path.Combine(_repo.PathToRepo, "remoteRepo.db");
				string remoteId = _apiServer.Identifier;
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
			int secondsBeforeTimeout = 5;
			const int totalNumOfAttempts = 5;
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
															  { "repoId", _repo.Identifier },
															  {"offset", offset.ToString()},
															  {"quantity", quantity.ToString()}
														  },
													  secondsBeforeTimeout);
					// API returns either 200 OK or 400 Bad Request
					// HgR status can be: SUCCESS (200), FAIL (400) or UNKNOWNID (400)
					if (response.StatusCode == HttpStatusCode.OK)
					{
						string revString = Encoding.UTF8.GetString(response.Content);
						return revString.Split('|').ToList();
					}
					if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers["X-HgR-Status"] == "UNKNOWNID")
					{
						_progress.WriteWarning("The remote server {0} does not have repoId '{1}'", _targetLabel, _repo.Identifier);
					}
					_progress.WriteWarning("Failed to get remote revisions for {0}", _repo.Identifier);
					return new List<string>();
				}
				catch (WebException e)
				{
					if (attempt < 5)
					{
						_progress.WriteWarning("Unable to contact server.  Retrying... ({0} of {1} attempts).", attempt + 1, totalNumOfAttempts);
						secondsBeforeTimeout += 3; // increment by 3 seconds
					}
					else
					{
						_progress.WriteWarning("Failed to contact server.");
					}
				}
			}
			return new List<string>();
		}

		public void Push()
		{
			string baseRevision = GetCommonBaseHashWithRemoteRepo();
			if (String.IsNullOrEmpty(baseRevision))
			{
				_progress.WriteError("Push operation failed");
				return;
			}


			// create a bundle to push
			var bundleHelper = new PushBundleHelper();
			bool bundleExists = _repo.MakeBundle(baseRevision, bundleHelper.BundlePath);
			if (!bundleExists)
			{
				// TODO: how should I report problems???  Using WriteError, WriteException, or WriteWarning????
				// Should errors be "user-friendly" or contain technical details?

				_progress.WriteError("Unable to create bundle for Push");
				_progress.WriteError("Push operation failed");
				return;
			}

			string transactionId = Guid.NewGuid().ToString();
			int startOfWindow = 0;
			int chunkSize = DefaultChunkSize; // size in bytes
			var bundleFileInfo = new FileInfo(bundleHelper.BundlePath);
			long bundleSize = bundleFileInfo.Length;

			do // loop until finished... or until the user cancels
			{
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				byte[] bundleChunk = bundleHelper.GetChunk(startOfWindow, chunkSize);
				/* API parameters
				 * $repoId, $baseHash, $bundleSize, $checksum, $offset, $data, $transId
				 * */
				var requestParameters = new Dictionary<string, string>
											{
												{"repoId", _repo.Identifier},
												{"baseHash", baseRevision},
												{"bundleSize", bundleSize.ToString()},
												{"checksum", CalculateChecksum(bundleChunk)},
												{"offset", startOfWindow.ToString()},
												{"transId", transactionId}
											};
				var response = PushOneChunk(requestParameters, bundleChunk);
				chunkSize = response.ChunkSize;
				startOfWindow = response.StartOfWindow;
				if (response.Status == PushStatus.Complete)
				{
					// TODO: update progress bar to 100% ?
					_progress.WriteVerbose("Push operation completed successfully");

					// update our local knowledge of what the server has
					LastKnownCommonBase = _repo.GetTip().Number.Hash;
					FinishPush(transactionId);
					return;
				}
				if (response.Status == PushStatus.Fail)
				{
					_progress.WriteError("Push operation failed");
					return;
				}
			} while (startOfWindow < bundleSize);
		}

		private void FinishPush(string transactionId)
		{
			_apiServer.Execute("finishPushBundle", new Dictionary<string, string> {{"transId", transactionId}}, 20);
		}

		private PushResponse PushOneChunk(Dictionary<string, string> parameters, byte[] dataToPush)
		{
			var secondsBeforeTimeout = 15;
			const int totalNumOfAttempts = 5;
			int chunkSize = dataToPush.Length;
			var pushResponse = new PushResponse();

			for (var attempt = 1; attempt <= totalNumOfAttempts; attempt++)
			{
				pushResponse.ChunkSize = chunkSize;
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				try
				{
					string retryMsg = (attempt > 1) ? "Retrying s" : "S";
					_progress.WriteStatus("{0}ending {1}+{2} of {3} bytes", retryMsg, Convert.ToInt32(parameters["offset"]), chunkSize, parameters["bundleSize"]);
					var response = _apiServer.Execute("pushBundleChunk", parameters, dataToPush, secondsBeforeTimeout);
					/* API returns the following HTTP codes:
						* 200 OK (SUCCESS)
						* 202 Accepted (RECEIVED)
						* 412 Precondition Failed (RESEND)
						* 400 Bad Request (FAIL, UNKNOWNID, and RESET)
						*/

					// the chunk was received successfully
					if (response.StatusCode == HttpStatusCode.Accepted)
					{
						pushResponse.StartOfWindow = Convert.ToInt32(response.Headers["X-HgR-sow"]);
						pushResponse.Status = PushStatus.Received;
						return pushResponse;
					}

					// the final chunk was received successfully
					if (response.StatusCode == HttpStatusCode.OK)
					{
						pushResponse.Status = PushStatus.Complete;
						return pushResponse;
					}

					// checksum failed
					if (response.StatusCode == HttpStatusCode.PreconditionFailed)
					{
						// resend the data
						_progress.WriteWarning("Checksum failed while pushing {0} bytes of data at offset {1}", chunkSize, parameters["offset"]);
						continue;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest)
					{
						if (response.Headers["X-HgR-Status"] == "UNKNOWNID")
						{
							_progress.WriteWarning("The server {0} does not have repoId '{1}'", _targetLabel, _repo.Identifier);
							pushResponse.Status = PushStatus.Fail;
							return pushResponse;
						}
						if (response.Headers["X-HgR-Status"] == "RESET")
						{
							_progress.WriteWarning("All chunks were pushed to the server, but the unbundle operation failed.  Restarting the push operation...");
							pushResponse.Status = PushStatus.Reset;
							pushResponse.StartOfWindow = 0;
							return pushResponse;
						}
					}
					_progress.WriteWarning("Invalid Server Response '{0}'", response.StatusCode);
				}
				catch (WebException e)
				{
					if (attempt < 5)
					{
						_progress.WriteWarning("Unable to contact server.  Retrying... ({0} of {1} attempts).", attempt + 1, totalNumOfAttempts);
						secondsBeforeTimeout += 3; // increment by 3 seconds
						chunkSize = chunkSize/2; // reduce the chunksize by half and try again
					}
				}
			}
			_progress.WriteWarning("The push operation failed on the server");
			pushResponse.Status = PushStatus.Fail;
			return pushResponse;
		}

		internal static string CalculateChecksum(byte[] textBytes)
		{

			// lifted from http://www.spiration.co.uk/post/1203/MD5-in-C%23---works-like-php-md5%28%29-example
			System.Security.Cryptography.MD5CryptoServiceProvider cryptHandler;
			cryptHandler = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] hash = cryptHandler.ComputeHash(textBytes);
			string ret = "";
			foreach (byte a in hash)
			{
				if (a < 16)
					ret += "0" + a.ToString("x");
				else
					ret += a.ToString("x");
			}
			return ret;
		}

		public bool Pull()
		{
			var bundleHelper = new PullBundleHelper();
			string localTip = _repo.GetTip().Number.Hash;

			string transactionId = Guid.NewGuid().ToString();
			int startOfWindow = 0;
			int chunkSize = 10000; // size in bytes
			int bundleSize;

			/* API parameters
			 * $repoId, $baseHash, $offset, $chunkSize, $transId
			 * */
			var requestParameters = new Dictionary<string, string>
											{
												{"repoId", _repo.Identifier},
												{"baseHash", localTip},
												{"offset", startOfWindow.ToString()},
												{"chunkSize", chunkSize.ToString()},
												{"transId", transactionId}
											};
			do {
				var response = PullOneChunk(requestParameters);
				bundleSize = response.BundleSize;
				chunkSize = response.ChunkSize;
				if (response.Status == PullStatus.NoChange)
				{
					_progress.WriteVerbose("Pull operation reported no changes");
					return false;
				}
				if (response.Status == PullStatus.OK)
				{
					bundleHelper.WriteChunk(response.Chunk);
					startOfWindow = startOfWindow + response.Chunk.Length;
					requestParameters["offset"] = startOfWindow.ToString();
				}
				else
				{
					_progress.WriteError("Pull operation failed");
					return false;
				}
			} while (startOfWindow < bundleSize);

			if (_repo.Unbundle(bundleHelper.BundlePath))
			{
				_progress.WriteMessage("Pull operation completed successfully");
				return true;
			}
			_progress.WriteError("Received all data but local unbundle operation failed!");
			_progress.WriteError("Pull operation failed");
			return false;
		}

		private PullResponse PullOneChunk(Dictionary<string, string> parameters)
		{
			var secondsBeforeTimeout = 15;
			const int totalNumOfAttempts = 5;
			var pullResponse = new PullResponse();
			int chunkSize = DefaultChunkSize;
			for (var attempt = 1; attempt <= totalNumOfAttempts; attempt++)
			{
				if (_progress.CancelRequested)
				{
					throw new UserCancelledException();
				}
				try
				{
					if (attempt > 1)
					{
						_progress.WriteStatus("Retrying pull operation...");
					}

					var response = _apiServer.Execute("pullBundleChunk", parameters, secondsBeforeTimeout);
					/* API returns the following HTTP codes:
						* 200 OK (SUCCESS)
						* 304 Not Modified (NOCHANGE)
						* 400 Bad Request (FAIL, UNKNOWNID)
						*/

					// chunk pulled OK
					if (response.StatusCode == HttpStatusCode.OK)
					{
						int actualChunkSize = Convert.ToInt32(response.Headers["X-HgR-chunkSize"]);
						_progress.WriteStatus("Received {0}+{1} of {2} bytes", parameters["offset"], actualChunkSize, response.Headers["X-HgR-bundleSize"]);
						pullResponse.Checksum = response.Headers["X-HgR-checksum"];
						pullResponse.BundleSize = Convert.ToInt32(response.Headers["X-HgR-bundleSize"]);
						pullResponse.Status = PullStatus.OK;
						pullResponse.ChunkSize = chunkSize;

						// verify checksum
						if (CalculateChecksum(response.Content) != response.Headers["X-HgR-checksum"])
						{
							_progress.WriteWarning("Checksum failed while pulling {0} bytes of data at offset {1}", actualChunkSize, parameters["offset"]);
							continue;
						}

						pullResponse.Chunk = response.Content;
						return pullResponse;
					}
					if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers["X-HgR-Status"] == "UNKNOWNID")
					{
						_progress.WriteWarning("The server {0} does not have repoId '{1}'", _targetLabel, _repo.Identifier);
						pullResponse.Status = PullStatus.Fail;
						return pullResponse;
					}
					_progress.WriteWarning("Invalid Server Response '{0}'", response.StatusCode);
				}
				catch (WebException e)
				{
					if (attempt < 5)
					{
						_progress.WriteWarning("Unable to contact server.  Retrying... ({0} of {1} attempts).", attempt + 1, totalNumOfAttempts);
						secondsBeforeTimeout += 3; // increment by 3 seconds
						chunkSize = chunkSize / 2; // reduce the chunksize by half and try again
					}
				}
			}
			_progress.WriteWarning("The pull operation failed on the server");
			pullResponse.Status = PullStatus.Fail;
			return pullResponse;
		}

		public void Dispose()
		{
		}
	}
}