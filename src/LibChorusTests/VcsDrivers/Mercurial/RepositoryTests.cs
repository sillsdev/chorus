using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Ionic.Zip;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class RepositoryTests
	{
		[Test]
		public void AddingRepositoryWithinAnotherRepositoryFromDirectoryNameIsDifferentRepository()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var dirInfo = Directory.CreateDirectory(Path.Combine(parentFolder, "Child"));
				var childRepo = HgRepository.CreateOrUseExisting(dirInfo.FullName, new NullProgress());
				Assert.AreNotEqual(parentFolder, childRepo.PathToRepo);
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryFromFileNameIsDifferentRepository()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var dirInfo = Directory.CreateDirectory(Path.Combine(parentFolder, "Child"));
				var childPathname = Path.Combine(dirInfo.FullName, "Child.txt");
				File.WriteAllText(childPathname, "New child content.");
				var childRepo = HgRepository.CreateOrUseExisting(childPathname, new NullProgress());
				Assert.AreNotEqual(parentFolder, childRepo.PathToRepo);
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithNonexistantDirectoryThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var nonexistantDirectory = Path.Combine(parentFolder, "Child");
				Assert.Throws<InvalidOperationException>(() => HgRepository.CreateOrUseExisting(nonexistantDirectory, new NullProgress()));
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithNonexistantFileThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var nonexistantFile = Path.Combine(parentFolder, "bogusfile.txt");
				Assert.Throws<InvalidOperationException>(() => HgRepository.CreateOrUseExisting(nonexistantFile, new NullProgress()));
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithNullDirectoryThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				Assert.Throws<ArgumentNullException>(() => HgRepository.CreateOrUseExisting(null, new NullProgress()));
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithEmptyStringDirectoryThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				Assert.Throws<ArgumentNullException>(() => HgRepository.CreateOrUseExisting("", new NullProgress()));
			}
		}

		[Test]
		public void RepositoryRecoversFromIncompleteMerge()
		{
			using (var tempRepo = new TemporaryFolder("ChorusIncompleteMerge"))
			{
				var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
#if MONO
				baseDir = baseDir.Replace(@"file:", null);
#else
				baseDir = baseDir.Replace(@"file:\", null);
#endif
				var zipFile = new ZipFile(Path.Combine(baseDir, Path.Combine("VcsDrivers", Path.Combine("TestData", "incompletemergerepo.zip"))));
				zipFile.ExtractAll(tempRepo.Path);
				var hgRepo = new HgRepository(tempRepo.Path, new NullProgress());
				hgRepo.CheckAndUpdateHgrc();
				var parentFile = tempRepo.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				var exception = Assert.Throws<ApplicationException>(() => hgRepo.AddAndCheckinFile(parentFile.Path));
				Assert.That(exception.Message.Contains("Unable to recover") && !exception.Message.Contains("unresolved merge"),
					String.Format("Repository should have conflict in retrying the merge, but not have an incomplete merge: {0}", exception.Message));
			}
		}

		[Test]
		public void CloneToUsbWithoutUpdateFollowedByIdentifierDoesNotAffectHgrc()
		{
			using(var repo = new RepositorySetup("source"))
			using(var f = new TemporaryFolder("clonetest"))
			{
				// The MakeCloneFromLocalToLocal with false on alsoDoCheckout is the core of the usb clone operation.
				// We need to make sure that this clone is bare of extensions, and remains so after the identifier is checked.
				HgHighLevel.MakeCloneFromLocalToUsb(repo.ProjectFolder.Path, f.Path, new NullProgress());
				var cloneRepo = new HgRepository(f.Path, new NullProgress());
				var hgFolderPath = Path.Combine(f.Path, ".hg");
				Assert.IsTrue(Directory.Exists(hgFolderPath));
				var hgrcLines = File.ReadAllLines(Path.Combine(hgFolderPath, "hgrc"));
				//SUT
				CollectionAssert.DoesNotContain(hgrcLines, "[extensions]", "extensions section created in bare clone");
				var id = cloneRepo.Identifier;
				CollectionAssert.DoesNotContain(hgrcLines, "[extensions]", "extensions section created after Identifier property read");
			}
		}

		#region Testing UpdateToLongHash on HgRepository
		[Test]
		public void UpdateToLongHashOnEmptyRepoReturns_UpdateResults_NoCommitsInRepository()
		{
			using (var testRoot = new TemporaryFolder("RepositoryTests"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, new NullProgress());
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				Assert.That(repo.UpdateToLongHash("fakelonghash"), Is.EqualTo(HgRepository.UpdateResults.NoCommitsInRepository));
			}
		}

		[Test]
		public void UpdateToLongHashOnNonEmptyRepoReturns_UpdateResults_Success()
		{
			using (var testRoot = new TemporaryFolder("RepositoryTests"))
			{
				var progress = new NullProgress();
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, progress);
				// fileX and fileXRev are zero based indexing, since those local commit numbers in Hg are zero based indexing.
				using (var file0 = testRoot.GetNewTempFile(true))
				{
					var repo = new HgRepository(testRoot.Path, new NullProgress());

					repo.AddAndCheckinFile(file0.Path);
					var file0Rev = repo.GetRevisionWorkingSetIsBasedOn();
					using (var file1 = testRoot.GetNewTempFile(true))
					{
						// On same branch.
						repo.AddAndCheckinFile(file1.Path);
						var file1Rev = repo.GetRevisionWorkingSetIsBasedOn();
						using (var file2 = testRoot.GetNewTempFile(true))
						{
							// Make new branch.
							repo.BranchingHelper.Branch(progress, "newbranch");
							repo.AddAndCheckinFile(file2.Path);
							var file2Rev = repo.GetRevisionWorkingSetIsBasedOn();

							// Round 1: update from rev 0 to rev 2.
							// Switch back to rev 0, using old method.
							repo.Update("0");
							var testRev = repo.GetRevisionWorkingSetIsBasedOn();
							// It did move.
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file0Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo(string.Empty)); // default branch returns string.Empty in mercurial.
							// SUT
							Assert.That(repo.UpdateToLongHash(file2Rev.Number.LongHash), Is.EqualTo(HgRepository.UpdateResults.Success));
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file2Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo("newbranch"));

							// Round 2: update from rev 1 to rev 2.
							// Set up for another pass to update to file2Rev from file3Rev
							// Switch back to rev 1 (default branch), using old method.
							repo.Update("1");
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							// It did move.
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file1Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo(string.Empty)); // default branch returns string.Empty in mercurial.
							// SUT
							Assert.That(repo.UpdateToLongHash(file2Rev.Number.LongHash), Is.EqualTo(HgRepository.UpdateResults.Success));
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file2Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo("newbranch"));

							// Round 3: downgrade from rev 2 to rev 0.
							// Set up for another pass to update to file0Rev from file2Rev
							// Switch back to rev 2 (newbranch branch), using old method.
							repo.Update("2");
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							// It did move.
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file2Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo("newbranch"));
							// SUT
							Assert.That(repo.UpdateToLongHash(file0Rev.Number.LongHash), Is.EqualTo(HgRepository.UpdateResults.Success));
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file0Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo(string.Empty)); // default branch returns string.Empty in mercurial.

							// Round 4: downgrade from rev 2 to rev 1.
							// Set up for another pass to update to file2Rev from file1Rev
							// Switch back to rev 2 (newbranch branch), using old method.
							repo.Update("2");
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							// It did move.
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file2Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo("newbranch"));
							// SUT
							Assert.That(repo.UpdateToLongHash(file1Rev.Number.LongHash), Is.EqualTo(HgRepository.UpdateResults.Success));
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file1Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo(string.Empty)); // default branch returns string.Empty in mercurial.

							// Round 5: downgrade from rev 1 to rev 0.
							// Set up for another pass to update to file0Rev from file1Rev
							// Switch back to rev 0 (default branch), using old method.
							repo.Update("1");
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							// It did move.
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file1Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo(string.Empty)); // default branch returns string.Empty in mercurial.
							// SUT
							Assert.That(repo.UpdateToLongHash(file0Rev.Number.LongHash), Is.EqualTo(HgRepository.UpdateResults.Success));
							testRev = repo.GetRevisionWorkingSetIsBasedOn();
							Assert.That(testRev.Number.LongHash, Is.EqualTo(file0Rev.Number.LongHash));
							Assert.That(testRev.Branch, Is.EqualTo(string.Empty)); // default branch returns string.Empty in mercurial.
						}
					}
				}
			}
		}

		[Test]
		public void UpdateToLongHashOnSameLongHashReturns_UpdateResults_AlreadyOnIt()
		{
			using (var testRoot = new TemporaryFolder("RepositoryTests"))
			{
				var progress = new NullProgress();
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, progress);
				// fileX and fileXRev are zero based indexing, since those local commit numbers in Hg are zero based indexing.
				using (var file0 = testRoot.GetNewTempFile(true))
				{
					var repo = new HgRepository(testRoot.Path, new NullProgress());
					repo.AddAndCheckinFile(file0.Path);
					var file0Rev = repo.GetRevisionWorkingSetIsBasedOn();

					// SUT
					Assert.That(repo.UpdateToLongHash(file0Rev.Number.LongHash), Is.EqualTo(HgRepository.UpdateResults.AlreadyOnIt));
				}
			}
		}

		[Test]
		public void UpdateToLongHashOnNonExistantLongHashReturns_UpdateResults_NoSuchRevision()
		{
			using (var testRoot = new TemporaryFolder("RepositoryTests"))
			{
				var progress = new NullProgress();
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, progress);
				// fileX and fileXRev are zero based indexing, since those local commit numbers in Hg are zero based indexing.
				using (var file0 = testRoot.GetNewTempFile(true))
				{
					var repo = new HgRepository(testRoot.Path, new NullProgress());
					repo.AddAndCheckinFile(file0.Path);

					// SUT
					Assert.That(repo.UpdateToLongHash("12345678901234567890BAD0HASH000000000000"), Is.EqualTo(HgRepository.UpdateResults.NoSuchRevision));
				}
			}
		}
		#endregion Testing UpdateToLongHash on HgRepository

		#region Testing UpdateToBranchHead on HgRepository
		[Test]
		public void UpdateToBranchHeadCallsReturnExpected_UpdateResults()
		{
			using (var testRoot = new TemporaryFolder("RepositoryTests"))
			{
				var progress = new NullProgress();
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, progress);
				// fileX and fileXRev are zero based indexing, since those local commit numbers in Hg are zero based indexing.
				using (var file0 = testRoot.GetNewTempFile(true))
				{
					var repo = new HgRepository(testRoot.Path, new NullProgress());

					// SUT
					Assert.That(repo.UpdateToBranchHead("fakebranchname"), Is.EqualTo(HgRepository.UpdateResults.NoCommitsInRepository));

					repo.AddAndCheckinFile(file0.Path);
					var file0Rev = repo.GetRevisionWorkingSetIsBasedOn();

					// SUT
					Assert.That(repo.UpdateToBranchHead(file0Rev.Branch), Is.EqualTo(HgRepository.UpdateResults.AlreadyOnIt));

					// SUT
					Assert.That(repo.UpdateToBranchHead("NoSuchBranch"), Is.EqualTo(HgRepository.UpdateResults.NoSuchBranch));

					using (var file1 = testRoot.GetNewTempFile(true))
					{
						// Make new branch.
						repo.BranchingHelper.Branch(progress, "newbranch");
						repo.AddAndCheckinFile(file1.Path);
						var file1Rev = repo.GetRevisionWorkingSetIsBasedOn();
						Assert.That(repo.UpdateToBranchHead(file1Rev.Branch), Is.EqualTo(HgRepository.UpdateResults.AlreadyOnIt));

						// Go back to commit 0 and create another "newbranch", which should then be a two headed monster.
						repo.Update("0");
						repo.BranchingHelper.Branch(progress, "newbranch");
						File.WriteAllText(file1.Path, @"new contents");
						repo.Commit(true, "Force double headed branch");
						var heads = repo.GetHeads().ToList();
						Assert.That(heads.Count, Is.EqualTo(3));

						// SUT
						Assert.That(repo.UpdateToBranchHead(file1Rev.Branch), Is.EqualTo(HgRepository.UpdateResults.AlreadyOnIt));
						var testRev = repo.GetRevisionWorkingSetIsBasedOn();
						Assert.That(testRev.Branch, Is.EqualTo("newbranch"));
						Assert.That(testRev.Number.LocalRevisionNumber, Is.EqualTo("2"));

						// Switch to older head of 'newbranch'
						// (Goes from rev 2 back to rev 1, both of which are on the same branch (newbranch).)
						repo.Update("1");

						// SUT
						// Returns "Success", because we moved from rev 1 to rev 2 (higher rev number) in the same branch, which branch has two heads.)
						Assert.That(repo.UpdateToBranchHead(file1Rev.Branch), Is.EqualTo(HgRepository.UpdateResults.Success));
						testRev = repo.GetRevisionWorkingSetIsBasedOn();
						Assert.That(testRev.Branch, Is.EqualTo("newbranch"));
						Assert.That(testRev.Number.LocalRevisionNumber, Is.EqualTo("2"));

						// Switch to commit 0.
						repo.Update("0");
						testRev = repo.GetRevisionWorkingSetIsBasedOn();
						Assert.That(testRev.Branch, Is.EqualTo(string.Empty));
						Assert.That(testRev.Number.LocalRevisionNumber, Is.EqualTo("0"));

						// SUT
						Assert.That(repo.UpdateToBranchHead(file1Rev.Branch), Is.EqualTo(HgRepository.UpdateResults.Success));
						testRev = repo.GetRevisionWorkingSetIsBasedOn();
						Assert.That(testRev.Branch, Is.EqualTo("newbranch"));
						Assert.That(testRev.Number.LocalRevisionNumber, Is.EqualTo("2"));
					}
				}
			}
		}

		#endregion Testing UpdateToBranchHead on HgRepository
	}
}