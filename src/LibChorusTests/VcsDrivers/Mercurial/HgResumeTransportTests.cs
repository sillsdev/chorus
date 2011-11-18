using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class DummyApiServerForTest : IApiServer, IDisposable
	{
		private Queue<HgResumeApiResponse> _responseQueue = new Queue<HgResumeApiResponse>();

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
				response = new HgResumeApiResponse() {StatusCode = HttpStatusCode.InternalServerError};
			}
			return response;
		}

		public string GetIdentifier()
		{
			return "DummyApiServerForTest";
		}

		public void AddResponse(HgResumeApiResponse response)
		{
			_responseQueue.Enqueue(response);
		}

		public void AddTimeOut()
		{
			// we are hijacking the HTTP 408 request timeout to mean a client-side networking timeout...
			// it works for our testing purposes even though that's not what the status code means
			_responseQueue.Enqueue(new HgResumeApiResponse() {StatusCode = HttpStatusCode.RequestTimeout});
		}

		public void Dispose()
		{
		}
	}

	public class PushHandlerApiServerForTest : IApiServer, IDisposable
	{
		private PullBundleHelper _helper;
		private HgRepository _repo;
		public List<string> Revisions;

		public PushHandlerApiServerForTest(HgRepository repo)
		{
			_repo = repo;
			_helper = new PullBundleHelper();
			Revisions = new List<string>();
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
			_helper.WriteChunk(bytesToWrite);
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

		public string GetIdentifier()
		{
			return "PushHandlerApiServerForTest";
		}

		public void Dispose()
		{
			_helper.Dispose();
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

		public void Dispose()
		{
		}
	}

	public class CannedResponses
	{
		public static HgResumeApiResponse PushComplete()
		{
			return new HgResumeApiResponse()
					   {
						   StatusCode = HttpStatusCode.OK,
						   Headers = new Dictionary<string, string>()
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"}
										 }
					   };
		}

		public static HgResumeApiResponse PushAccepted(int startOfWindow)
		{
			return new HgResumeApiResponse()
			{
				StatusCode = HttpStatusCode.Accepted,
				Headers = new Dictionary<string, string>()
										 {
											 {"X-HgR-Status", "RECEIVED"},
											 {"X-HgR-Version", "1"},
											 {"X-HgR-sow", startOfWindow.ToString()}
										 }
			};
		}



		public static HgResumeApiResponse BadChecksum()
		{
			return new HgResumeApiResponse()
			{
				StatusCode = HttpStatusCode.PreconditionFailed,
				Headers = new Dictionary<string, string>()
										 {
											 {"X-HgR-Status", "RESEND"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}

		public static HgResumeApiResponse PushUnbundleFailedOnServer()
		{
			return new HgResumeApiResponse()
			{
				StatusCode = HttpStatusCode.BadRequest,
				Headers = new Dictionary<string, string>()
										 {
											 {"X-HgR-Status", "RESET"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}


		public static HgResumeApiResponse Failed(string message = "")
		{
			var response = new HgResumeApiResponse()
			{
				StatusCode = HttpStatusCode.BadRequest,
				Headers = new Dictionary<string, string>()
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
			return new HgResumeApiResponse()
					   {
						   StatusCode = status,
						   Headers = new Dictionary<string, string>()
					   };
		}

		public static HgResumeApiResponse Revisions(string revisions)
		{
			return new HgResumeApiResponse()
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

		public static HgResumeApiResponse PullComplete()
		{
			return new HgResumeApiResponse()
			{
				StatusCode = HttpStatusCode.OK,
				Headers = new Dictionary<string, string>()
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}

		public static HgResumeApiResponse PullNoChange()
		{
			return new HgResumeApiResponse()
			{
				StatusCode = HttpStatusCode.NotModified,
				Headers = new Dictionary<string, string>()
										 {
											 {"X-HgR-Status", "NOCHANGE"},
											 {"X-HgR-Version", "1"}
										 }
			};
		}
	}

	[TestFixture]
	public class HgResumeTransportTests
	{
		[Test]
		public void Push_SingleResponse_OK()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress(){ShowVerbose=true}, progressForTest}))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				var revisionResponse = CannedResponses.Revisions(setup.Repository.GetTip().Number.Hash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddResponse(revisionResponse);
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_UnknownServerResponse_Fails()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				apiServer.AddResponse(CannedResponses.Custom(HttpStatusCode.ServiceUnavailable));
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation failed"));
			}
		}

		[Test]
		public void Push_SomeServerTimeOuts_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				var revisionResponse = CannedResponses.Revisions(setup.Repository.GetTip().Number.Hash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddTimeOut();
				apiServer.AddResponse(revisionResponse);
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_TooManyServerTimeouts_Fails()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				var revisionResponse = CannedResponses.Revisions(setup.Repository.GetTip().Number.Hash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddTimeOut();
				apiServer.AddResponse(revisionResponse);
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation failed"));
			}
		}

		[Test]
		public void Push_LargeFileSizeBundle_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new PushHandlerApiServerForTest(setup.Repository))
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				apiServer.Revisions.Add(setup.Repository.GetTip().Number.Hash);

				// just pick a file larger than 10K for use as a test... any file will do
				string sourcePathOfLargeFile = Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly,
					String.Format("..{0}..{0}lib{0}Debug{0}icu.net.dll", Path.DirectorySeparatorChar));

				string largeFilePath = setup.ProjectFolder.GetNewTempFile(false).Path;
				File.Copy(sourcePathOfLargeFile, largeFilePath);
				setup.Repository.AddAndCheckinFile(largeFilePath);
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_BadChecksumInOneChunk_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				var revisionResponse = CannedResponses.Revisions(setup.Repository.GetTip().Number.Hash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddResponse(revisionResponse);
				apiServer.AddResponse(CannedResponses.PushAccepted(5));
				apiServer.AddResponse(CannedResponses.PushAccepted(10));
				apiServer.AddResponse(CannedResponses.BadChecksum());
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_RepeatedBadChecksum_Fail()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				var revisionResponse = CannedResponses.Revisions(setup.Repository.GetTip().Number.Hash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddResponse(revisionResponse);
				apiServer.AddResponse(CannedResponses.BadChecksum());
				apiServer.AddResponse(CannedResponses.BadChecksum());
				apiServer.AddResponse(CannedResponses.BadChecksum());
				apiServer.AddResponse(CannedResponses.BadChecksum());
				apiServer.AddResponse(CannedResponses.BadChecksum());
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation failed"));
			}
		}

		[Test]
		public void Push_MultiChunkBundleAndUnBundleFails_Fail()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				var revisionResponse = CannedResponses.Revisions(setup.Repository.GetTip().Number.Hash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddResponse(revisionResponse);
				apiServer.AddResponse(CannedResponses.PushAccepted(1));
				apiServer.AddResponse(CannedResponses.PushAccepted(2));
				apiServer.AddResponse(CannedResponses.PushUnbundleFailedOnServer());
				transport.Push();
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation failed"));
			}
		}

		[Test]
		public void Push_RemoteRepoDbNotExistsAndSetsCorrectlyWithRevHash_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				string revHash = setup.Repository.GetTip().Number.Hash;
				var revisionResponse = CannedResponses.Revisions(revHash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddResponse(revisionResponse);
				apiServer.AddResponse(CannedResponses.PushComplete());
				string dbFilePath = Path.Combine(setup.Repository.PathToRepo, "remoteRepo.db");
				Assert.That(File.Exists(dbFilePath), Is.False);
				transport.Push();
				Assert.That(File.Exists(dbFilePath), Is.True);
				string dbContents = File.ReadAllText(dbFilePath);
				Assert.That(dbContents, Is.EqualTo(apiServer.GetIdentifier() + "|" + revHash));
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_2PushesAndRemoteRepoDbFileUpdated_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				string dbFilePath = Path.Combine(setup.Repository.PathToRepo, "remoteRepo.db");
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);

				// first push
				setup.AddAndCheckinFile("sample1", "first checkin");
				string revHash1 = setup.Repository.GetTip().Number.Hash;
				setup.AddAndCheckinFile("sample2", "second checkin");
				string tipHash = setup.Repository.GetTip().Number.Hash;
				var revisionResponse = CannedResponses.Revisions(revHash1);
				apiServer.AddResponse(revisionResponse);
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				string dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(apiServer.GetIdentifier() + "|" + tipHash));
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));

				// second push
				setup.AddAndCheckinFile("sample3", "third checkin");

				setup.AddAndCheckinFile("sample4", "fourth checkin");
				string tipHash2 = setup.Repository.GetTip().Number.Hash;
				apiServer.AddResponse(CannedResponses.PushAccepted(1));
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(apiServer.GetIdentifier() + "|" + tipHash2));
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Pull_UnknownServerResponse_Fails()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, progressForTest }))
			{
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				setup.AddAndCheckinFile("sample1", "first checkin");
				apiServer.AddResponse(CannedResponses.Custom(HttpStatusCode.ServiceUnavailable));
				transport.Pull();
				Assert.That(progressForTest.AllMessages, Contains.Item("Pull operation failed"));
			}
		}

		[Test]
		public void Pull_ServerTimeOut_Fails()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Pull_BundleInOneChunk_Success()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Pull_BundleInMultipleChunks_Success()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Pull_SomeTimeOuts_Success()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Pull_SomeBadChecksum_Success()
		{
			throw new NotImplementedException();
		}
	}
}
