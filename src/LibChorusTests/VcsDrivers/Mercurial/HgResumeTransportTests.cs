using System;
using System.IO;
using System.Net;
using System.Text;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HgResumeTransportTests
	{
		[Test]
		public void Push_SingleResponse_OK()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress {ShowVerbose=true}, progressForTest}))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				string revHash = setup.Repository.GetTip().Number.Hash;
				var revisionResponse = CannedResponses.Revisions(revHash);
				setup.AddAndCheckinFile("sample2", "second checkin");
				var tipHash = setup.Repository.GetTip().Number.Hash;
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				apiServer.AddResponse(revisionResponse);
				apiServer.AddResponse(CannedResponses.PushComplete());
				string dbFilePath = Path.Combine(setup.Repository.PathToRepo, "remoteRepo.db");
				Assert.That(File.Exists(dbFilePath), Is.False);
				transport.Push();
				Assert.That(File.Exists(dbFilePath), Is.True);
				string dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(apiServer.Identifier + "|" + tipHash));
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_2PushesAndRemoteRepoDbFileUpdated_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
				Assert.That(dbContents, Is.EqualTo(apiServer.Identifier + "|" + tipHash));
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));

				// second push
				setup.AddAndCheckinFile("sample3", "third checkin");

				setup.AddAndCheckinFile("sample4", "fourth checkin");
				string tipHash2 = setup.Repository.GetTip().Number.Hash;
				apiServer.AddResponse(CannedResponses.PushAccepted(1));
				apiServer.AddResponse(CannedResponses.PushComplete());
				transport.Push();
				dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(apiServer.Identifier + "|" + tipHash2));
				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_2DifferentApiServers_HgRepoFileUpdatedWithBothEntries()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer1 = new DummyApiServerForTest("apiServer1"))
			using (var apiServer2 = new DummyApiServerForTest("apiServer2"))
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
			{
				string dbFilePath = Path.Combine(setup.Repository.PathToRepo, "remoteRepo.db");

				var transport1 = new HgResumeTransport(setup.Repository, "test repo", apiServer1, progress);
				// first push to apiServer1
				setup.AddAndCheckinFile("sample1", "first checkin");
				string revHash1 = setup.Repository.GetTip().Number.Hash;
				setup.AddAndCheckinFile("sample2", "second checkin");
				string tipHash1 = setup.Repository.GetTip().Number.Hash;
				var revisionResponse = CannedResponses.Revisions(revHash1);
				apiServer1.AddResponse(revisionResponse);
				apiServer1.AddResponse(CannedResponses.PushComplete());
				transport1.Push();

				// first push to apiServer2
				var transport2 = new HgResumeTransport(setup.Repository, "test repo", apiServer2, progress);
				apiServer2.AddResponse(revisionResponse);
				apiServer2.AddResponse(CannedResponses.PushComplete());
				transport2.Push();

				// check contents of remoteRepoDb
				string[] dbContents = File.ReadAllLines(dbFilePath);
				Assert.That(dbContents, Contains.Item(apiServer1.Identifier + "|" + tipHash1));
				Assert.That(dbContents, Contains.Item(apiServer2.Identifier + "|" + tipHash1));

				// second push
				setup.AddAndCheckinFile("sample3", "third checkin");
				setup.AddAndCheckinFile("sample4", "fourth checkin");
				string tipHash2 = setup.Repository.GetTip().Number.Hash;
				apiServer1.AddResponse(CannedResponses.PushAccepted(1));
				apiServer1.AddResponse(CannedResponses.PushComplete());
				transport1.Push();

				dbContents = File.ReadAllLines(dbFilePath);
				Assert.That(dbContents, Contains.Item(apiServer1.Identifier + "|" + tipHash2));
				Assert.That(dbContents, Contains.Item(apiServer2.Identifier + "|" + tipHash1));

				Assert.That(progressForTest.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Pull_UnknownServerResponse_Fails()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
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
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
			{
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				setup.AddAndCheckinFile("sample1", "first checkin");
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				transport.Pull();
				Assert.That(progressForTest.AllMessages, Contains.Item("Pull operation failed"));
			}
		}

		[Test]
		public void Pull_BundleInOneChunk_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new PullHandlerApiServerForTest(setup.Repository))
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
			{
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				setup.AddAndCheckinFile("sample1", "first checkin");
				string revHash = setup.Repository.GetTip().Number.Hash;
				setup.AddAndCheckinFile("sample2", "second checkin");
				apiServer.PrepareBundle(revHash);
				transport.Pull();
				Assert.That(progressForTest.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_BundleInMultipleChunks_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new PullHandlerApiServerForTest(setup.Repository))
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
			{
				setup.AddAndCheckinFile("sample1", "first checkin");
				string revHash = setup.Repository.GetTip().Number.Hash;

				// just pick a file larger than 10K for use as a test... any file will do
				string sourcePathOfLargeFile = Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly,
					String.Format("..{0}..{0}lib{0}Debug{0}icu.net.dll", Path.DirectorySeparatorChar));

				string largeFilePath = setup.ProjectFolder.GetNewTempFile(false).Path;
				File.Copy(sourcePathOfLargeFile, largeFilePath);
				setup.Repository.AddAndCheckinFile(largeFilePath);

				apiServer.PrepareBundle(revHash);

				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				transport.Pull();
				Assert.That(progressForTest.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_SomeTimeOuts_Success()
		{
			var progressForTest = new ProgressForTest();
			using (var setup = new RepositorySetup("hgresumetest"))
			using (var apiServer = new DummyApiServerForTest())
			using (var progress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, progressForTest }))
			{
				var transport = new HgResumeTransport(setup.Repository, "test repo", apiServer, progress);
				setup.AddAndCheckinFile("sample1", "first checkin");
				apiServer.AddTimeOut();
				apiServer.AddResponse(CannedResponses.PullOk(200, Encoding.UTF8.GetBytes("data")));
				apiServer.AddTimeOut();
				apiServer.AddTimeOut();
				transport.Pull();
				Assert.That(progressForTest.AllMessages, Contains.Item("Pull operation failed"));
			}
		}

		[Test]
		public void Pull_SomeBadChecksum_Success()
		{
			throw new NotImplementedException();
		}
	}
}
