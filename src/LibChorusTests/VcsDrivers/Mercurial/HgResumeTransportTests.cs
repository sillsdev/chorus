using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HgResumeTransportTests
	{
		private static HgResumeTransportProvider GetTransportProviderForTest(TestEnvironment e)
		{
			return new HgResumeTransportProvider(new HgResumeTransport(e.Local.Repository, e.Label, e.ApiServer, e.MultiProgress));
		}

		[Test]
		public void Push_SingleResponse_OK()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_SuccessfulPush_PushDataCacheDestroyed()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommitLargeFile();
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
				var dirInfo = new DirectoryInfo(Path.Combine(HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier), "pushData"));
				Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(0));
			}
		}

		[Test]
		public void Push_UnknownServerResponse_Fails()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Custom(HttpStatusCode.SeeOther));
				e.ApiServer.AddResponse(ApiResponses.Custom(HttpStatusCode.SeeOther));
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
			}
		}

		[Test]
		public void Push_SomeServerTimeOuts_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalAddAndCommit();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_LargeFileSizeBundle_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommitLargeFile();
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}


		[Test]
		public void Push_MultiChunkBundleAndUnBundleFails_Fail()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(2));
				e.ApiServer.AddResponse(ApiResponses.Reset());
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
			}
		}

		[Test]
		public void Push_RemoteRepoDbNotExistsAndSetsCorrectlyWithRevHash_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());

				string dbStoragePath = HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier);
				string dbFilePath = Path.Combine(dbStoragePath, "remoteRepo.db");
				Assert.That(File.Exists(dbFilePath), Is.False);

				var tipHash = e.Local.Repository.GetTip().Number.Hash;
				var transport = provider.Transport;
				transport.Push();
				Assert.That(File.Exists(dbFilePath), Is.True);
				string dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(e.ApiServer.Host + "|" + tipHash));
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_2PushesAndRemoteRepoDbFileUpdated_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var tipHash = e.Local.Repository.GetTip().Number.Hash;
				var transport = provider.Transport;
				transport.Push();

				string dbStoragePath = HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier);
				string dbFilePath = Path.Combine(dbStoragePath, "remoteRepo.db");
				string dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(e.ApiServer.Host + "|" + tipHash));

				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));

				e.LocalAddAndCommit();
				e.LocalAddAndCommit();
				var tipHash2 = e.Local.Repository.GetTip().Number.Hash;
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				transport.Push();
				dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(e.ApiServer.Host + "|" + tipHash2));
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_2DifferentApiServers_HgRepoFileUpdatedWithBothEntries()
		{
			using (var e1 = new TestEnvironment("api1", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e1))
			{
				var api2 = new DummyApiServerForTest("api2");
				var transport2 = new HgResumeTransport(e1.Local.Repository, "api2", api2, e1.MultiProgress);

				// push to ApiServer 1
				e1.LocalAddAndCommit();
				var revisionResponse = ApiResponses.Revisions(e1.Local.Repository.GetTip().Number.Hash);
				e1.ApiServer.AddResponse(revisionResponse);
				e1.LocalAddAndCommit();
				e1.ApiServer.AddResponse(ApiResponses.PushComplete());
				var tipHash1 = e1.Local.Repository.GetTip().Number.Hash;
				var transport = provider.Transport;
				transport.Push();
				e1.ApiServer.AddResponse(ApiResponses.PushComplete());  // finishPushBundle

				// push to ApiServer 2
				api2.AddResponse(revisionResponse);
				api2.AddResponse(ApiResponses.PushComplete());
				transport2.Push();

				// check contents of remoteRepoDb
				string dbStoragePath = HgResumeTransport.PathToLocalStorage(e1.Local.Repository.Identifier);
				string dbFilePath = Path.Combine(dbStoragePath, "remoteRepo.db");
				string[] dbContents = File.ReadAllLines(dbFilePath);
				Assert.That(dbContents, Contains.Item(e1.ApiServer.Host + "|" + tipHash1));
				Assert.That(dbContents, Contains.Item(api2.Host + "|" + tipHash1));

				// second push
				e1.LocalAddAndCommit();
				e1.LocalAddAndCommit();
				string tipHash2 = e1.Local.Repository.GetTip().Number.Hash;
				e1.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e1.ApiServer.AddResponse(ApiResponses.PushComplete());
				transport.Push();

				dbContents = File.ReadAllLines(dbFilePath);
				Assert.That(dbContents, Contains.Item(e1.ApiServer.Host + "|" + tipHash2));
				Assert.That(dbContents, Contains.Item(api2.Host + "|" + tipHash1));

				Assert.That(e1.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Pull_UnknownServerResponse_Fails()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Custom(HttpStatusCode.ServiceUnavailable));
				var transport = provider.Transport;
				Assert.That(() => transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
			}
		}

		[Test]
		public void Pull_NoChangesInRepo_NoChanges()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				Assert.That(e.Local.Repository.GetTip().Number.Hash, Is.EqualTo(e.Remote.Repository.GetTip().Number.Hash));
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("No changes"));
			}
		}

		[Test]
		public void Pull_BundleInOneChunk_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_SucessfulPull_PullBundleDataIsRemoved()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
				var dirInfo = new DirectoryInfo(Path.Combine(HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier), "pullData"));
				Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(0));
			}
		}


		[Test]
		public void Pull_BundleInMultipleChunks_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommitLargeFile();
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_SomeTimeOuts_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommitLargeFile();
				e.ApiServer.AddTimeoutResponse(2);
				e.ApiServer.AddTimeoutResponse(3);
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_PullOperationFailsMidwayAndStartsAgainWithSeparatePull_Resumes()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommitLargeFile();
				e.ApiServer.AddFailResponse(3);
				var transport = provider.Transport;
				Assert.That(() => transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.ApiServer.AddFailResponse(5);
				Assert.That(() => transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming pull operation at 4KB received"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming pull operation at 9KB received"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Push_PushOperationFailsMidwayAndBeginsAgainWithAdditionalPush_Resumes()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommitLargeFile();
				e.ApiServer.AddFailResponse(3);
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.ApiServer.AddFailResponse(6);
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming push operation at 126KB sent"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming push operation at 249KB sent"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_PushFailsMidwayThenRepoChanges_PushDoesNotResume()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommitLargeFile();
				e.ApiServer.AddFailResponse(2);
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.RemoteAddAndCommit();
				transport.Push();
				Assert.That(e.Progress.AllMessagesString().Contains("Resuming push operation at"), Is.Not.True);
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Pull_PullFailsMidwayTheRemoteRepoChanges_PullFinishesThenStartsSecondPullToGetNewChanges()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommitLargeFile();
				e.ApiServer.AddFailResponse(3);
				var transport = provider.Transport;
				Assert.That(() => transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.RemoteAddAndCommit();
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming pull operation at 4KB received"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Remote repo has changed.  Initiating additional pull operation"));
				IEnumerable<string> msgs = e.Progress.Messages.Where(x => x == "Pull operation completed successfully");
				Assert.That(msgs.ToList(), Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void Push_ServerNotAvailableMidTransaction_NotAvailableMessage()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommitLargeFile();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddServerUnavailableResponse(4, serverMessage);
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("Push operation completed successfully"));
			}
		}

		[Test]
		public void Pull_ServerNotAvailableMidTransaction_NotAvailableMessage()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommitLargeFile();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddServerUnavailableResponse(2, serverMessage);
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_InitialServerResponseServerNotAvailable_NotAvailableMessage()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddResponse(ApiResponses.NotAvailable(serverMessage));
				e.ApiServer.AddResponse(ApiResponses.PullNoChange());
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("The pull operation completed successfully"));
			}
		}

		[Test]
		public void Push_InitialServerResponseServerNotAvailable_NotAvailableMessage()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddResponse(ApiResponses.NotAvailable(serverMessage));
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("The pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_LocalRepoAndRemoteRepoUpdatedIndependently_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommit();
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Clone_LocalRepoEmpty_ReposAreIdentical()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.RemoteAddAndCommit();
				e.RemoteAddAndCommit();
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				transport.Clone();
				Assert.That(e.Local.Repository.GetTip().Number.Hash, Is.EqualTo(e.Remote.Repository.GetTip().Number.Hash));
			}
		}

		[Test]
		public void Pull_InvalidBaseHashFromServer_ClientRecoversSuccessfully()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				transport.Pull();
				// at this point the local server has cached the tip of the repo
				e.Remote.Repository.RollbackWorkingDirectoryToLastCheckin();
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Has.No.Member("Pull operation failed"));
			}
		}

		[Test]
		public void Push_InvalidBaseHashFromServer_ClientRecoversSuccessfully()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				transport.Push();
				// at this point the transport has cached the tip of the remote repo
				e.Remote.Repository.RollbackWorkingDirectoryToLastCheckin();
				e.LocalAddAndCommit();
				Assert.That(() => transport.Push(), Throws.Nothing);
			}
		}

		[Test]
		public void Push_RemoteRepoIsEmptyRepo_PushesBundleSuccessfully()
		{
			// TODO: this test succeeds but for the wrong reason.
			// Make sure that a inited repo can have a bundle applied to it; currently there are "transaction abort" and "rollback completed" messages
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}
	}
}
