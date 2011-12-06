using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class DummyApiServerForTest : IApiServer, IDisposable
	{
		private readonly Queue<HgResumeApiResponse> _responseQueue = new Queue<HgResumeApiResponse>();

		public DummyApiServerForTest(string identifier = "DummyApiServerForTest")
		{
			Identifier = identifier;
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout)
		{
			return Execute(method, parameters, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] contentToSend, int secondsBeforeTimeout)
		{
			HgResumeApiResponse response;
			if (_responseQueue.Count > 0)
			{
				response = _responseQueue.Dequeue();
				if (response.StatusCode == HttpStatusCode.RequestTimeout)
				{
					throw new WebException("ApiServerForTest: timeout!");
				}
			} else
			{
				response = new HgResumeApiResponse {StatusCode = HttpStatusCode.InternalServerError};
			}
			return response;
		}

		public string Identifier { get; private set; }

		public void AddResponse(HgResumeApiResponse response)
		{
			_responseQueue.Enqueue(response);
		}

		public void AddTimeOut()
		{
			// we are hijacking the HTTP 408 request timeout to mean a client-side networking timeout...
			// it works for our testing purposes even though that's not what the status code means
			_responseQueue.Enqueue(new HgResumeApiResponse {StatusCode = HttpStatusCode.RequestTimeout});
		}

		public void Dispose()
		{
		}
	}

	public class PushHandlerApiServerForTest : IApiServer, IDisposable
	{
		private readonly PullStorageManager _helper;  // yes, we DO want to use the PullBundleHelper for the PushHandler (this is the other side of the API, so it's opposite)
		private readonly HgRepository _repo;
		public List<string> Revisions;
		private TemporaryFolder _localStorage;

		public PushHandlerApiServerForTest(HgRepository repo, string identifier = "PushHandlerApiServerForTest")
		{
			_localStorage = new TemporaryFolder("PushHandlerApiServerForTest");
			_repo = repo;
			_helper = new PullStorageManager(_localStorage.Path, identifier);
			Revisions = new List<string>();
			Identifier = identifier;
		}
		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout)
		{
			return Execute(method, parameters, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] bytesToWrite, int secondsBeforeTimeout)
		{
			if (method == "getRevisions")
			{
				string revisions = string.Join("|", Revisions.ToArray());
				return CannedResponses.Revisions(revisions);
			}
			if (method == "finishPushBundle")
			{
				return CannedResponses.PushComplete();
			}
			if (method == "pushBundleChunk")
			{
				_helper.AppendChunk(bytesToWrite);
				int bundleSize = Convert.ToInt32(parameters["bundleSize"]);
				int offset = Convert.ToInt32(parameters["offset"]);
				int chunkSize = bytesToWrite.Length;
				if (offset + chunkSize == bundleSize)
				{
					if (_repo.Unbundle(_helper.BundlePath))
					{
						return CannedResponses.PushComplete();
					}
					return CannedResponses.PushUnbundleFailedOnServer();
				}
				if (offset + chunkSize < bundleSize)
				{
					return CannedResponses.PushAccepted(offset + chunkSize);
				}
				return CannedResponses.Failed("offset + chunkSize > bundleSize !");
			}
			return CannedResponses.Custom(HttpStatusCode.InternalServerError);
		}

		public string Identifier { get; private set; }

		public void Dispose()
		{
			_localStorage.Dispose();
		}
	}

	public class PullHandlerApiServerForTest : IApiServer, IDisposable
	{
		private readonly PushStorageHelper _helper;
		private readonly HgRepository _repo;
		private int _executeCount;
		private List<int> _badChecksumList;
		private List<int> _timeoutList;

		public PullHandlerApiServerForTest(HgRepository repo, string identifier = "PullHandlerApiServerForTest")
		{
			_repo = repo;
			_helper = new PushStorageHelper();
			Identifier = identifier;
			_executeCount = 0;
			_badChecksumList = new List<int>();
			_timeoutList = new List<int>();
		}

		public void PrepareBundle(string revHash)
		{
			if(File.Exists(_helper.BundlePath))
			{
				File.Delete(_helper.BundlePath);
			}
			_repo.MakeBundle(revHash, _helper.BundlePath);
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout)
		{
			return Execute(method, parameters, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] bytesToWrite, int secondsBeforeTimeout)
		{
			_executeCount++;
			if (method == "finishPullBundle")
			{
				return CannedResponses.Custom(HttpStatusCode.OK);
			}
			if (method == "pullBundleChunk")
			{
				if (_timeoutList.Contains(_executeCount))
				{
					throw new WebException("ApiServerForTest: timeout!");
				}
				int offset = Convert.ToInt32(parameters["offset"]);
				int chunkSize = Convert.ToInt32(parameters["chunkSize"]);
				var bundleFile = new FileInfo(_helper.BundlePath);
				if (offset >= bundleFile.Length)
				{
					return CannedResponses.Failed("offset greater than bundleSize");
				}
				var chunk = _helper.GetChunk(offset, chunkSize);
				if (_badChecksumList.Contains(_executeCount))
				{
					return CannedResponses.PullOkWithBadChecksum(Convert.ToInt32(bundleFile.Length), chunk);
				}
				return CannedResponses.PullOk(Convert.ToInt32(bundleFile.Length), chunk);
			}
			return CannedResponses.Custom(HttpStatusCode.InternalServerError);
		}

		public string Identifier { get; private set; }

		public void Dispose()
		{
			_helper.Dispose();
		}

		public void AddBadChecksumResponse(int executeCount)
		{
			if (!_badChecksumList.Contains(executeCount))
			{
				_badChecksumList.Add(executeCount);
			}
		}

		public void AddTimeoutResponse(int executeCount)
		{
			if (!_timeoutList.Contains(executeCount))
			{
				_timeoutList.Add(executeCount);
			}
		}
	}


	public class ProgressForTest : IProgress, IDisposable
	{
		public List<string> Statuses = new List<string>();
		public List<string> Messages = new List<string>();
		public List<string> Warnings = new List<string>();
		public List<Exception> Exceptions = new List<Exception>();
		public List<string> Errors = new List<string>();
		public List<string> Verbose = new List<string>();

		public void WriteStatus(string message, params object[] args)
		{
			Statuses.Add(message);
		}

		public void WriteMessage(string message, params object[] args)
		{
			Messages.Add(message);
		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			Messages.Add(message);
		}

		public void WriteWarning(string message, params object[] args)
		{
			Warnings.Add(message);
		}

		public void WriteException(Exception error)
		{
			Exceptions.Add(error);
		}

		public void WriteError(string message, params object[] args)
		{
			Errors.Add(message);
		}

		public void WriteVerbose(string message, params object[] args)
		{
			Verbose.Add(message);
		}

		public bool ShowVerbose
		{
			set { }
		}

		public List<string> AllMessages
		{
			get
			{
				var all = new List<string>();
				all.AddRange(Statuses);
				all.AddRange(Messages);
				all.AddRange(Warnings);
				all.AddRange(Errors);
				all.AddRange(Verbose);
				return all;
			}
		}

		public bool CancelRequested
		{
			get { return false; }
			set { }
		}

		public bool ErrorEncountered
		{
			get { return false; }
			set { }
		}

		public IProgressIndicator ProgressIndicator
		{
			get;
			set;
		}

		public void Dispose()
		{
		}
	}

	public class CannedResponses
	{
		public static HgResumeApiResponse PushComplete()
		{
			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.OK,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}

		public static HgResumeApiResponse PushAccepted(int startOfWindow)
		{
			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.Accepted,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "RECEIVED"},
											 {"X-HgR-Version", "1"},
											 {"X-HgR-sow", startOfWindow.ToString()}
										 }
			};
		}



		public static HgResumeApiResponse BadChecksum()
		{
			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.PreconditionFailed,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "RESEND"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}

		public static HgResumeApiResponse PushUnbundleFailedOnServer()
		{
			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.BadRequest,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "RESET"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}


		public static HgResumeApiResponse Failed(string message = "")
		{
			var response = new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.BadRequest,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "FAIL"},
											 {"X-HgR-Version", "1"}
										 }
			};
			if (!String.IsNullOrEmpty(message))
			{
				response.Headers.Add("X-HgR-Error", message);
			}
			return response;
		}

		public static HgResumeApiResponse Custom(HttpStatusCode status)
		{
			return new HgResumeApiResponse
			{
				StatusCode = status,
				Headers = new Dictionary<string, string>()
			};
		}

		public static HgResumeApiResponse Revisions(string revisions)
		{
			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.OK,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"}
										 },
				Content = Encoding.UTF8.GetBytes(revisions)
			};
		}

		public static HgResumeApiResponse PullOk(int bundleSize, byte[] contentToSend)
		{
			if (contentToSend.Length > bundleSize)
			{
				throw new ArgumentException("bundleSize must be larger than the size of the content you are sending");
			}

			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.OK,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"},
											 {"X-HgR-checksum", HgResumeTransport.CalculateChecksum(contentToSend)},
											 {"X-HgR-bundleSize", bundleSize.ToString()},
											 {"X-HgR-chunkSize", contentToSend.Length.ToString()}
										 },
				Content = contentToSend
			};
		}

		public static HgResumeApiResponse PullOkWithBadChecksum(int bundleSize, byte[] contentToSend)
		{
			if (contentToSend.Length > bundleSize)
			{
				throw new ArgumentException("bundleSize must be larger than the size of the content you are sending");
			}

			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.OK,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"},
											 {"X-HgR-checksum", "boguschecksum"},
											 {"X-HgR-bundleSize", bundleSize.ToString()},
											 {"X-HgR-chunkSize", contentToSend.Length.ToString()}
										 },
				Content = contentToSend
			};
		}

		public static HgResumeApiResponse PullNoChange()
		{
			return new HgResumeApiResponse
			{
				StatusCode = HttpStatusCode.NotModified,
				Headers = new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "NOCHANGE"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}
	}
}