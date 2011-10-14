using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

		public HgResumeTransport(HgRepository repo, string targetLabel, IApiServer apiServer, IProgress progress)
		{
			_repo = repo;
			_targetLabel = targetLabel;
			_apiServer = apiServer;
			_progress = progress;
		}

		private string GetRemoteTip()
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
					using (HttpWebResponse response = _apiServer.Execute("getTip", new Dictionary<string, string> {{ "repoId", _repo.Identifier }}, secondsBeforeTimeout))
					{
						// API returns either 200 OK or 400 Bad Request
						// HgR status can be: SUCCESS (200), FAIL (400) or UNKNOWNID (400)
						if (response.StatusCode == HttpStatusCode.OK)
						{
							string tip = response.GetResponseHeader("X-HgR-Tip");
							_progress.WriteVerbose("Got remote tip: {0}", tip);
							return tip.Trim();
						}
						if (response.GetResponseHeader("X-HgR-Status") == "UNKNOWNID")
						{
							_progress.WriteWarning("The remote server {0} does not have repoId '{1}'", _targetLabel, _repo.Identifier);
						}
						_progress.WriteWarning("Failed to get remote tip.", _repo.Identifier);
						return "";
					}
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
			return "";
		}

		public void Push()
		{
			string tip = GetRemoteTip();
			if (String.IsNullOrEmpty(tip))
			{
				return;
			}


			// create a bundle to push
			var bundleHelper = new PushBundleHelper();
			bool bundleExists = _repo.MakeBundle(tip, bundleHelper.BundlePath);
			if (!bundleExists)
			{
				// TODO: how should I report problems???  Using WriteError, WriteException, or WriteWarning????
				// Should errors be "user-friendly" or contain technical details?

				_progress.WriteError("Unable to create bundle for Push");
				return;
			}

			string transactionId = Guid.NewGuid().ToString();
			int startOfWindow = 0;
			int chunkSize = 10000; // size in bytes
			var bundleFileInfo = new FileInfo(bundleHelper.BundlePath);
			long bundleSize = bundleFileInfo.Length;

			while (startOfWindow < bundleSize) // loop until finished... or until the user cancels
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
												{"baseHash", tip},
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

					FinishPush(transactionId);
					return;
				}
				if (response.Status == PushStatus.Fail)
				{
					_progress.WriteError("Push operation failed.");
					return;
				}
			}
		}

		private void FinishPush(string transactionId)
		{
			_apiServer.Execute("pushBundleChunk", new Dictionary<string, string> {{"transId", transactionId}}, 20);
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
					_progress.WriteStatus("Sending {0} of {1} bytes", parameters["offset"], parameters["bundleSize"]);
					using (HttpWebResponse response = _apiServer.Execute("pushBundleChunk", parameters, secondsBeforeTimeout))
					{
						/* API returns the following HTTP codes:
						 * 200 OK (SUCCESS)
						 * 202 Accepted (RECEIVED)
						 * 412 Precondition Failed (RESEND)
						 * 400 Bad Request (FAIL, UNKNOWNID, and RESET)
						 */

						// the chunk was received successfully
						if (response.StatusCode == HttpStatusCode.Accepted)
						{
							pushResponse.StartOfWindow = Convert.ToInt32(response.GetResponseHeader("X-HgR-offset"));
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
						if (response.GetResponseHeader("X-HgR-Status") == "UNKNOWNID")
						{
							_progress.WriteWarning("The server {0} does not have repoId '{1}'", _targetLabel, _repo.Identifier);
							pushResponse.Status = PushStatus.Fail;
							return pushResponse;
						}
						if (response.GetResponseHeader("X-HgR-Status") == "RESET")
						{
							_progress.WriteWarning("All chunks were pushed to the server, but the unbundle operation failed.  Restarting the push operation...");
							pushResponse.Status = PushStatus.Reset;
							pushResponse.StartOfWindow = 0;
							return pushResponse;
						}
					}
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

		private static string CalculateChecksum(byte[] textBytes)
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
			var bundleHelper = new BundleHelper();
			string localTip = _repo.GetTip().Number.Hash;

			string transactionId = Guid.NewGuid().ToString();
			int startOfWindow = 0;
			int chunkSize = 10000; // size in bytes
			int bundleSize = 0;

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
			var response = PullOneChunk(requestParameters);

			if (response.Status == PullStatus.NoChange)
			{
				_progress.WriteVerbose("Pull operation reported no changes");
				return false;
			}

			bundleSize = response.BundleSize;

			while (startOfWindow < bundleSize)
			{
				response = PullOneChunk(requestParameters);

				if (response.Status == PullStatus.OK)
				{
					bundleHelper.WriteChunk(response.Chunk);
					startOfWindow = startOfWindow + response.Chunk.Length;
				}


				if (_repo.Unbundle(bundleHelper.BundlePath))
				{
					_progress.WriteMessage("Pull operation completed successfully");
					return true;
				}
				_progress.WriteError("Received all data but local unbundle operation failed!");
				return false;
			}
			_progress.WriteError("Pull operation failed.");
			return false;
		}

		private PullResponse PullOneChunk(Dictionary<string, string> requestParameters)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}


}