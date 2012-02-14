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
		[Test]
		public void Push_SingleResponse_OK()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			{
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				e.Transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_SuccessfulPush_PushDataCacheDestroyed()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.LocalCheckInLargeFile();
				e.Transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
				var dirInfo = new DirectoryInfo(Path.Combine(HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier), "pushData"));
				Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(0));
			}
		}

		[Test]
		public void Push_UnknownServerResponse_Fails()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			{
				e.LocalCheckIn();
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.Custom(HttpStatusCode.SeeOther));
				e.ApiServer.AddResponse(ApiResponses.Custom(HttpStatusCode.SeeOther));
				Assert.That(() => e.Transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
			}
		}

		[Test]
		public void Push_SomeServerTimeOuts_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			{
				e.LocalCheckIn();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalCheckIn();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				e.Transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_LargeFileSizeBundle_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.LocalCheckInLargeFile();
				e.Transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}


		[Test]
		public void Push_MultiChunkBundleAndUnBundleFails_Fail()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			{
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(2));
				e.ApiServer.AddResponse(ApiResponses.Reset());
				Assert.That(() => e.Transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
			}
		}

		[Test]
		public void Push_RemoteRepoDbNotExistsAndSetsCorrectlyWithRevHash_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			{
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());

				string dbStoragePath = HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier);
				string dbFilePath = Path.Combine(dbStoragePath, "remoteRepo.db");
				Assert.That(File.Exists(dbFilePath), Is.False);

				var tipHash = e.Local.Repository.GetTip().Number.Hash;
				e.Transport.Push();
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
			{
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash));
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var tipHash = e.Local.Repository.GetTip().Number.Hash;
				e.Transport.Push();

				string dbStoragePath = HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier);
				string dbFilePath = Path.Combine(dbStoragePath, "remoteRepo.db");
				string dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(e.ApiServer.Host + "|" + tipHash));

				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));

				e.LocalCheckIn();
				e.LocalCheckIn();
				var tipHash2 = e.Local.Repository.GetTip().Number.Hash;
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				e.Transport.Push();
				dbContents = File.ReadAllText(dbFilePath).Trim();
				Assert.That(dbContents, Is.EqualTo(e.ApiServer.Host + "|" + tipHash2));
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_2DifferentApiServers_HgRepoFileUpdatedWithBothEntries()
		{
			using (var e1 = new TestEnvironment("api1", ApiServerType.Dummy))
			{
				var api2 = new DummyApiServerForTest("api2");
				var progress2 =
					new MultiProgress(new IProgress[] {new ConsoleProgress {ShowVerbose = true}, e1.Progress});
				var transport2 = new HgResumeTransport(e1.Local.Repository, "api2", api2, progress2);

				// push to ApiServer 1
				e1.LocalCheckIn();
				var revisionResponse = ApiResponses.Revisions(e1.Local.Repository.GetTip().Number.Hash);
				e1.ApiServer.AddResponse(revisionResponse);
				e1.LocalCheckIn();
				e1.ApiServer.AddResponse(ApiResponses.PushComplete());
				var tipHash1 = e1.Local.Repository.GetTip().Number.Hash;
				e1.Transport.Push();
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
				e1.LocalCheckIn();
				e1.LocalCheckIn();
				string tipHash2 = e1.Local.Repository.GetTip().Number.Hash;
				e1.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e1.ApiServer.AddResponse(ApiResponses.PushComplete());
				e1.Transport.Push();

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
			{
				e.LocalCheckIn();
				e.ApiServer.AddResponse(ApiResponses.Custom(HttpStatusCode.ServiceUnavailable));
				Assert.That(() => e.Transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
			}
		}

		[Test]
		public void Pull_NoChangesInRepo_NoChanges()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				Assert.That(e.Local.Repository.GetTip().Number.Hash, Is.EqualTo(e.Remote.Repository.GetTip().Number.Hash));
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("No changes"));
			}
		}

		[Test]
		public void Pull_BundleInOneChunk_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckIn();
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_SucessfulPull_PullBundleDataIsRemoved()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckIn();
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
				var dirInfo = new DirectoryInfo(Path.Combine(HgResumeTransport.PathToLocalStorage(e.Local.Repository.Identifier), "pullData"));
				Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(0));
			}
		}


		[Test]
		public void Pull_BundleInMultipleChunks_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckInLargeFile();
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_SomeTimeOuts_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckInLargeFile();
				e.ApiServer.AddTimeoutResponse(2);
				e.ApiServer.AddTimeoutResponse(3);
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_PullOperationFailsMidwayAndStartsAgainWithSeparatePull_Resumes()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckInLargeFile();
				e.ApiServer.AddFailResponse(3);
				Assert.That(() => e.Transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.ApiServer.AddFailResponse(5);
				Assert.That(() => e.Transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming pull operation at 4KB received"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming pull operation at 9KB received"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Push_PushOperationFailsMidwayAndBeginsAgainWithAdditionalPush_Resumes()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.LocalCheckInLargeFile();
				e.ApiServer.AddFailResponse(3);
				Assert.That(() => e.Transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.ApiServer.AddFailResponse(6);
				Assert.That(() => e.Transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.Transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming push operation at 126KB sent"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming push operation at 249KB sent"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Push_PushFailsMidwayThenRepoChanges_PushDoesNotResume()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.LocalCheckInLargeFile();
				e.ApiServer.AddFailResponse(2);
				Assert.That(() => e.Transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.RemoteCheckIn();
				e.Transport.Push();
				Assert.That(e.Progress.AllMessagesString().Contains("Resuming push operation at"), Is.Not.True);
				Assert.That(e.Progress.AllMessages, Contains.Item("Push operation completed successfully"));
			}
		}

		[Test]
		public void Pull_PullFailsMidwayTheRemoteRepoChanges_PullFinishesThenStartsSecondPullToGetNewChanges()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckInLargeFile();
				e.ApiServer.AddFailResponse(3);
				Assert.That(() => e.Transport.Pull(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
				e.RemoteCheckIn();
				e.Transport.Pull();
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
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.LocalCheckInLargeFile();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddServerUnavailableResponse(4, serverMessage);
				e.Transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("Push operation completed successfully"));
			}
		}

		[Test]
		public void Pull_ServerNotAvailableMidTransaction_NotAvailableMessage()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckInLargeFile();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddServerUnavailableResponse(2, serverMessage);
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_InitialServerResponseServerNotAvailable_NotAvailableMessage()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			{
				e.LocalCheckIn();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddResponse(ApiResponses.NotAvailable(serverMessage));
				e.ApiServer.AddResponse(ApiResponses.PullNoChange());
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("The pull operation completed successfully"));
			}
		}

		[Test]
		public void Push_InitialServerResponseServerNotAvailable_NotAvailableMessage()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			{
				e.LocalCheckIn();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddResponse(ApiResponses.NotAvailable(serverMessage));
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				e.Transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("The pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_LocalRepoAndRemoteRepoUpdatedIndependently_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.LocalCheckIn();
				e.RemoteCheckIn();
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Clone_LocalRepoEmpty_ReposAreIdentical()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.RemoteCheckIn();
				e.RemoteCheckIn();
				e.RemoteCheckIn();
				e.Transport.Clone();
				Assert.That(e.Local.Repository.GetTip().Number.Hash, Is.EqualTo(e.Remote.Repository.GetTip().Number.Hash));
			}
		}

		[Test]
		public void Pull_InvalidBaseHashFromServer_ClientRecoversSuccessfully()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.RemoteCheckIn();
				e.Transport.Pull();
				// at this point the local server has cached the tip of the repo
				e.Remote.Repository.RollbackWorkingDirectoryToLastCheckin();
				e.Transport.Pull();
				Assert.That(e.Progress.AllMessages, Has.No.Member("Pull operation failed"));
			}
		}

		[Test]
		public void Push_InvalidBaseHashFromServer_ClientRecoversSuccessfully()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			{
				e.LocalCheckIn();
				e.CloneRemoteFromLocal();
				e.LocalCheckIn();
				e.Transport.Push();
				// at this point the transport has cached the tip of the remote repo
				e.Remote.Repository.RollbackWorkingDirectoryToLastCheckin();
				e.LocalCheckIn();
				Assert.That(() => e.Transport.Push(), Throws.Nothing);
			}
		}
	}
}
