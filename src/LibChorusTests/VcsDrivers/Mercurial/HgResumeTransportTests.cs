using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.Tests.sync;
using NUnit.Framework;
using SIL.Progress;

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
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
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
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
				var dirInfo = new DirectoryInfo(Path.Combine(transport.PathToLocalStorage, "pushData"));
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
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch));
				e.LocalAddAndCommit();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddTimeOut();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
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
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}


		[Test]
		public void Push_MultiChunkBundleAndUnBundleFails_Fail()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(2));
				e.ApiServer.AddResponse(ApiResponses.Reset());
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<HgResumeOperationFailed>());
			}
		}

		[Test]
		public void Push_RemoteRevisionCacheWorksWhenReceivingMoreThanRequestedRevisions()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				provider.Transport.RevisionRequestQuantity = 2;
				var first2revs = "";
				var changingFile = e.LocalAddAndCommit();
				first2revs += e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch + "|";
				e.LocalChangeAndCommit(changingFile);
				first2revs += e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch;
				var second2revs = "";
				e.LocalChangeAndCommit(changingFile);
				second2revs += e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch + "|";
				e.LocalChangeAndCommit(changingFile);
				second2revs += e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch;

				e.ApiServer.AddResponse(ApiResponses.Revisions(first2revs));
				e.ApiServer.AddResponse(ApiResponses.Revisions(second2revs));
				e.ApiServer.AddResponse(ApiResponses.Revisions(""));
				e.SetLocalAdjunct(new ProgrammableSynchronizerAdjunct("newbranch"));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());

				var transport = provider.Transport;

				var dbStoragePath = transport.PathToLocalStorage;
				var dbFilePath = Path.Combine(dbStoragePath, HgResumeTransport.RevisionCacheFilename);
				Assert.That(File.Exists(dbFilePath), Is.False);

				var tipHash = e.Local.Repository.GetTip().Number.Hash;
				var cacheContents = HgResumeTransport.ReadServerRevisionCache(dbFilePath);
				transport.Push();
				Assert.That(File.Exists(dbFilePath), Is.True);
				cacheContents = HgResumeTransport.ReadServerRevisionCache(dbFilePath);
				Assert.True(cacheContents.Count == 2, "should be 2 entries in the cache at this point.");
				Assert.True(cacheContents.FirstOrDefault().RemoteId == e.ApiServer.Host
						 && cacheContents.FirstOrDefault().Revision.Number.Hash == tipHash, "Cache contents incorrect");
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}

		[Test]
		public void Push_RemoteRepoDbNotExistsAndSetsCorrectlyWithRevHash_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());

				var transport = provider.Transport;

				string dbStoragePath = transport.PathToLocalStorage;
				string dbFilePath = Path.Combine(dbStoragePath, HgResumeTransport.RevisionCacheFilename);
				Assert.That(File.Exists(dbFilePath), Is.False);

				var tipHash = e.Local.Repository.GetTip().Number.Hash;
				transport.Push();
				Assert.That(File.Exists(dbFilePath), Is.True);
				var cacheContents = HgResumeTransport.ReadServerRevisionCache(dbFilePath);
				Assert.True(cacheContents.Count == 1, "should only be one entry in the cache.");
				Assert.True(cacheContents.FirstOrDefault().RemoteId == e.ApiServer.Host
						 && cacheContents.FirstOrDefault().Revision.Number.Hash == tipHash, "Cache contents incorrect");
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}

		[Test]
		public void Push_2PushesAndRemoteRepoDbFileUpdated_Success()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.Revisions(e.Local.Repository.GetTip().Number.Hash + ':' + e.Local.Repository.GetTip().Branch));
				e.LocalAddAndCommit();
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				var tipHash = e.Local.Repository.GetTip().Number.Hash;
				var transport = provider.Transport;
				transport.Push();

				string dbStoragePath = transport.PathToLocalStorage;
				string dbFilePath = Path.Combine(dbStoragePath, HgResumeTransport.RevisionCacheFilename);

				var cacheContents = HgResumeTransport.ReadServerRevisionCache(dbFilePath);
				Assert.True(cacheContents.Count == 1, "should only be one entry in the cache.");
				Assert.True(cacheContents.FirstOrDefault().RemoteId == e.ApiServer.Host
						 && cacheContents.FirstOrDefault().Revision.Number.Hash == tipHash, "Cache contents incorrect");

				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));

				e.LocalAddAndCommit();
				e.LocalAddAndCommit();
				var tipHash2 = e.Local.Repository.GetTip().Number.Hash;
				e.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e.ApiServer.AddResponse(ApiResponses.PushComplete());
				transport.Push();
				cacheContents = HgResumeTransport.ReadServerRevisionCache(dbFilePath);
				Assert.True(cacheContents.Count == 1, "should only be one entry in the cache.");
				Assert.True(cacheContents.FirstOrDefault().RemoteId == e.ApiServer.Host
						 && cacheContents.FirstOrDefault().Revision.Number.Hash == tipHash2, "Cache contents incorrect");
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}

		[Test]
		public void Push_2DifferentApiServers_HgRepoFileUpdatedWithBothEntries()
		{
			using (var e1 = new TestEnvironment("api1", ApiServerType.Dummy))
			using (var provider = GetTransportProviderForTest(e1))
			{
				var api2 = new DummyApiServerForTest("api3");
				var transport2 = new HgResumeTransport(e1.Local.Repository, "api3", api2, e1.MultiProgress);

				// push to ApiServer 1
				e1.LocalAddAndCommit();
				var revisionResponse = ApiResponses.Revisions(e1.Local.Repository.GetTip().Number.Hash + ':' + e1.Local.Repository.GetTip().Branch);
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
				string dbStoragePath = transport.PathToLocalStorage;
				string dbFilePath = Path.Combine(dbStoragePath, HgResumeTransport.RevisionCacheFilename);
				var cacheContents = HgResumeTransport.ReadServerRevisionCache(dbFilePath);
				Assert.True(cacheContents.Count == 2, "should be two api server entries in the cache.");
				Assert.True(cacheContents.SingleOrDefault(x => x.RemoteId == e1.ApiServer.Host).Revision.Number.Hash == tipHash1, "Cache contents incorrect");
				Assert.True(cacheContents.SingleOrDefault(x => x.RemoteId == api2.Host).Revision.Number.Hash == tipHash1, "Cache contents incorrect");

				// second push
				e1.LocalAddAndCommit();
				e1.LocalAddAndCommit();
				string tipHash2 = e1.Local.Repository.GetTip().Number.Hash;
				e1.ApiServer.AddResponse(ApiResponses.PushAccepted(1));
				e1.ApiServer.AddResponse(ApiResponses.PushComplete());
				transport.Push();

				cacheContents = HgResumeTransport.ReadServerRevisionCache(dbFilePath);
				Assert.True(cacheContents.Count == 2, "should be two api server entries in the cache.");
				Assert.True(cacheContents.SingleOrDefault(x => x.RemoteId == e1.ApiServer.Host).Revision.Number.Hash == tipHash2, "Cache contents incorrect");
				Assert.True(cacheContents.SingleOrDefault(x => x.RemoteId == api2.Host).Revision.Number.Hash == tipHash1, "Cache contents incorrect");

				Assert.That(e1.Progress.AllMessages, Contains.Item("Finished sending"));
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
				var dirInfo = new DirectoryInfo(Path.Combine(transport.PathToLocalStorage, "pullData"));
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
		public void Pull_PullOperationFailsMidway_ContinuesToCompletion()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommitLargeFile();
				e.ApiServer.AddFailResponse(3);
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Push_UserCancelsMidwayAndBeginsAgainWithAdditionalPush_Resumes()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommitLargeFile();
				e.ApiServer.AddCancelResponse(3);
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<UserCancelledException>());
				e.Progress.CancelRequested = false;
				e.ApiServer.AddCancelResponse(6);
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<UserCancelledException>());
				e.Progress.CancelRequested = false;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming push operation at 175KB sent"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Resuming push operation at 346KB sent"));
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}

		[Test]
		public void Push_UserCancelsMidwayThenRepoChanges_PushDoesNotResume()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommitLargeFile();
				e.ApiServer.AddCancelResponse(2);
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Exception.TypeOf<UserCancelledException>());
				e.Progress.CancelRequested = false;
				e.RemoteAddAndCommit();
				transport.Push();
				Assert.That(e.Progress.AllMessagesString().Contains("Resuming push operation at"), Is.Not.True);
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}

		[Test]
		public void Pull_UserCancelsMidwayTheRemoteRepoChanges_PullFinishesSecondPull()
		{
			using (var e = new TestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.RemoteAddAndCommitLargeFile();
				e.ApiServer.AddCancelResponse(2);
				var transport = provider.Transport;
				Assert.That(() => transport.Pull(), Throws.Exception.TypeOf<UserCancelledException>());
				e.Progress.CancelRequested = false;
				e.RemoteAddAndCommit();
				transport.Pull();
				IEnumerable<string> msgs = e.Progress.Messages.Where(x => x == "Pull operation completed successfully");
				Assert.That(msgs.ToList(), Has.Count.EqualTo(1));
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
				e.ApiServer.AddServerUnavailableResponse(2, serverMessage);
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("Finished sending"));
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
				var revisionResponse = ApiResponses.Custom(HttpStatusCode.OK);
				revisionResponse.Content = Encoding.UTF8.GetBytes((e.Local.Repository.GetTip().Number.Hash + ":").ToArray());
				e.ApiServer.AddResponse(revisionResponse);
				e.ApiServer.AddResponse(ApiResponses.NotAvailable(serverMessage));
				e.ApiServer.AddResponse(ApiResponses.PullNoChange());
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("The pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_GetRevisionServerResponseServerNotAvailable_NotAvailableMessageAndFails()
		{
			using(var e = new TestEnvironment("hgresumetest", ApiServerType.Dummy))
			using(var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				var serverMessage = "The server is down for scheduled maintenance";
				e.ApiServer.AddResponse(ApiResponses.NotAvailable(serverMessage));
				e.ApiServer.AddResponse(ApiResponses.NotAvailable(serverMessage));
				var revisionResponse = ApiResponses.Custom(HttpStatusCode.OK);
				revisionResponse.Content = Encoding.UTF8.GetBytes((e.Local.Repository.GetTip().Number.Hash + ":").ToArray());
				e.ApiServer.AddResponse(revisionResponse);
				e.ApiServer.AddResponse(ApiResponses.PullNoChange());
				var transport = provider.Transport;
				Assert.Throws(typeof(HgResumeOperationFailed), () => transport.Pull());
				Assert.That(e.Progress.AllMessages, Contains.Item("Server temporarily unavailable: " + serverMessage));
				Assert.That(e.Progress.AllMessages, Has.No.Member("No changes"));
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
				var revisionResponse = ApiResponses.Custom(HttpStatusCode.OK);
				revisionResponse.Content = Encoding.UTF8.GetBytes("0:default").ToArray();
				e.ApiServer.AddResponse(revisionResponse);
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
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}

		[Test]
		public void Push_RemoteOnNewBranch_DoesNotThrow()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.SetRemoteAdjunct(new BranchTestAdjunct() { BranchName = "newRemoteBranch"});
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Nothing);
			}
		}

		[Test]
		public void Push_RemoteOnNewBranch_SendsData()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.SetRemoteAdjunct(new BranchTestAdjunct() { BranchName = "newRemoteBranch" });
				e.RemoteAddAndCommit();
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Nothing);
				Assert.That(e.Progress.AllMessages, !Contains.Item("No changes to send.  Push operation completed"));
			}
		}

		[Test]
		public void Push_LocalOnNewBranch_DoesNotThrow()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.SetLocalAdjunct(new BranchTestAdjunct() { BranchName = "newLocalBranch" });
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Nothing);
			}
		}

		[Test]
		public void Push_LocalOnNewBranch_SendsData()
		{
			using (var e = new BranchingTestEnvironment("localonnewbranch", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.SetLocalAdjunct(new BranchTestAdjunct() { BranchName = "newLocalBranch" });
				e.RemoteAddAndCommit();
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Nothing);
				Assert.That(e.Progress.AllMessages, !Contains.Item("No changes to send.  Push operation completed"));
			}
		}

		[Test]
		public void Push_LocalOnNewBranchRevisionsExceedQuantity_SendsData()
		{
			using (var e = new BranchingTestEnvironment("localonnewbranch", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.SetLocalAdjunct(new BranchTestAdjunct() { BranchName = "newLocalBranch" });
				e.RemoteAddAndCommit();
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.Nothing);
				Assert.That(e.Progress.AllMessages, !Contains.Item("No changes to send.  Push operation completed"));
			}
		}

		[Test]
		public void Pull_LocalOnNewBranch_Success()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.SetLocalAdjunct(new BranchTestAdjunct { BranchName = "localBranch"});
				e.LocalAddAndCommit();
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_RemoteOnNewBranch_Success()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.SetRemoteAdjunct(new BranchTestAdjunct { BranchName = "remoteBranch" });
				e.LocalAddAndCommit();
				e.RemoteAddAndCommit();
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("Pull operation completed successfully"));
			}
		}

		[Test]
		public void Pull_OneLocalChangeNothingToPullOn2BranchRemote_Success()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Pull))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.SetLocalAdjunct(new BranchTestAdjunct { BranchName = "branch2" });
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				transport.Pull();
				Assert.That(e.Progress.AllMessages, Contains.Item("No changes"));
			}
		}

		[Test]
		public void Push_OneLocalChangeNothingToPullOn2BranchRemote_Success()
		{
			using (var e = new BranchingTestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e))
			{
				e.LocalAddAndCommit();
				e.SetLocalAdjunct(new BranchTestAdjunct { BranchName = "branch2" });
				e.LocalAddAndCommit();
				e.CloneRemoteFromLocal();
				e.LocalAddAndCommit();
				var transport = provider.Transport;
				transport.Push();
				Assert.That(e.Progress.AllMessages, Contains.Item("Finished sending"));
			}
		}

		[Test]
		public void Push_RemoteRepoIsUnrelated_Throws()
		{
			using (var e1 = new TestEnvironment("hgresumetest", ApiServerType.Push))
			using (var provider = GetTransportProviderForTest(e1))
			{
				e1.LocalAddAndCommit();
				e1.RemoteAddAndCommit();
				var transport = provider.Transport;
				Assert.That(() => transport.Push(), Throws.TypeOf<HgResumeOperationFailed>());
			}
		}

        // regression test for bug http://jira.palaso.org/issues/browse/CHR-30
	    [Test]
	    public void CalculateEstimatedTimeRemaining_VeryLargeBundleSize_DoesNotThrow()
	    {
	        const int largeBundleSize = 2147000000; // 2 GB
	        const int chunkSize = 5000;
	        const int startOfWindow = 100000;
            Assert.DoesNotThrow(() => HgResumeTransport.CalculateEstimatedTimeRemaining(largeBundleSize, chunkSize, startOfWindow));
	    }

        [Test]
        public void CalculateEstimatedTimeRemaining_ExpectedTimeRemainingString()
        {
            const int bundleSize = 5000000;
            const int chunkSize = 100000;
            const int startOfWindow = 100000;
            Assert.That(HgResumeTransport.CalculateEstimatedTimeRemaining(bundleSize, chunkSize, startOfWindow), Is.EqualTo("(about 5 minutes)"));
        }



		private class BranchTestAdjunct : ISychronizerAdjunct
		{
			public bool WasUpdated { get; private set; }
			public string BranchName { get; set; }
			public void PrepareForInitialCommit(IProgress progress)
			{}

			public void SimpleUpdate(IProgress progress, bool isRollback)
			{
				WasUpdated = true;
			}

			public void PrepareForPostMergeCommit(IProgress progress)
			{
				WasUpdated = true;
			}

			public void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress)
			{}
		}
	}
}
