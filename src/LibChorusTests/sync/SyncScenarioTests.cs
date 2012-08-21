using System.Diagnostics;
using System.IO;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace LibChorus.Tests.sync
{
	[TestFixture]
	[Category("Sync")]
	public class SyncScenarioTests
	{
		private string _pathToTestRoot;

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusSyncScenarioTests");
			Directory.CreateDirectory(_pathToTestRoot);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_pathToTestRoot, true);
		}


		#region Test Utilities

		public static void SetAdjunctModelVersion(Synchronizer synchronizer, string modelVersion)
		{
			synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct(modelVersion);
		}

		#endregion

		class BobSetup
		{
			public string _lexiconProjectPath;
			public  string _languageProjectPath;
			public ProjectFolderConfiguration _projectFolderConfiguration;
			public string _pathToLift;
			private ConsoleProgress _progress;
			public string PathToText;
			public string BobSourcePath;
			public const string ProjectFolderName= "LP";

			public BobSetup(ConsoleProgress progress, string pathToTestRoot)
			{
				_progress = progress;
				BobSourcePath = Path.Combine(pathToTestRoot, "Bob");
				Directory.CreateDirectory(BobSourcePath);
				string languageProjectPath = Path.Combine(BobSourcePath, ProjectFolderName);
				Directory.CreateDirectory(languageProjectPath);
				_languageProjectPath = languageProjectPath;
				_lexiconProjectPath = Path.Combine(_languageProjectPath, "lexicon");
				Directory.CreateDirectory(_lexiconProjectPath);

				PathToText = Path.Combine(_lexiconProjectPath, "foo.abc");
				File.WriteAllText(PathToText, "version one of my pretend txt");

				_pathToLift = Path.Combine(_lexiconProjectPath, "foo.lift");
				File.WriteAllText(_pathToLift, "<lift version='0.12'></lift>");

				string picturesPath = Path.Combine(_lexiconProjectPath, "pictures");
				Directory.CreateDirectory(picturesPath);
				File.WriteAllText(Path.Combine(picturesPath, "dog.jpg"), "Not really a picture");

				string cachePath = Path.Combine(_lexiconProjectPath, "cache");
				Directory.CreateDirectory(cachePath);
				File.WriteAllText(Path.Combine(cachePath, "cache.abc"), "Some cache stuff");

				_projectFolderConfiguration = new ProjectFolderConfiguration(languageProjectPath);
				_projectFolderConfiguration.IncludePatterns.Add(_lexiconProjectPath);
				_projectFolderConfiguration.ExcludePatterns.Add("**/cache");

				SyncOptions options = new SyncOptions();
				options.DoPullFromOthers = false;
				options.DoMergeWithOthers = false;
				options.CheckinDescription = "Added";

				RepositorySetup.MakeRepositoryForTest(languageProjectPath, "bob", _progress);

				//SyncManager bobManager = SyncManager.FromChildPath(_lexiconProjectPath, progress, "bob");
				SyncResults results = GetSynchronizer().SyncNow(options);
			}

			public string BobProjectPath
			{
				//get { return Path.Combine(BobSourcePath, RepositoryAddress.ProjectNameVariable); }
				get { return _languageProjectPath; }
		   }

			public void ChangeTextFile()
			{
				SyncOptions options = new SyncOptions();
				options.CheckinDescription = "a change to foo.abc";
				string bobsFooTextPath = Path.Combine(_lexiconProjectPath, "foo.abc");
				File.WriteAllText(bobsFooTextPath, "version two of my pretend txt");
				GetSynchronizer().SyncNow(options);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="targetPath">does not inclue the project folder dir</param>
			public string SetupClone(string targetPath)
			{
				//return GetSynchronizer().MakeClone(Path.Combine(targetPath, BobSetup.ProjectFolderName), true);
				return HgHighLevel.MakeCloneFromLocalToLocal(_languageProjectPath,
														Path.Combine(targetPath, BobSetup.ProjectFolderName), true,
														_progress);
			}

			public Synchronizer GetSynchronizer()
			{
				ProjectFolderConfiguration project = new ProjectFolderConfiguration(_lexiconProjectPath);
				project.IncludePatterns.Add("**.abc");
				 project.IncludePatterns.Add("**.lift");
			   Synchronizer repo= Synchronizer.FromProjectConfiguration(project, _progress);
				repo.Repository.SetUserNameInIni("bob", _progress);
				return repo;
			}
		}

//        [Test]
//        public void CloneShouldHaveSameDirectoryName()
//        {
//            ConsoleProgress progress = new ConsoleProgress();
//            BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);
//
//            Synchronizer repo = Synchronizer.FromProjectConfiguration(bobSetup._projectFolderConfiguration);
//            string usbPath = Path.Combine(_pathToTestRoot, "USB-A");
//            repo.MakeCloneFromLocalToLocal(usbPath, false, progress);
//            Assert.IsTrue(Directory.Exists(Path.Combine(usbPath, BobSetup.ProjectFolderName)));
//        }

		[Test]
		public void CanGetNewFileFromAnotherRep()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			bobSetup.ChangeTextFile();
			string usbPath = Path.Combine(_pathToTestRoot, "USB-A");
			Directory.CreateDirectory(usbPath);
			bobSetup.SetupClone(usbPath);

			RepositoryAddress otherDirPath = RepositoryAddress.Create("USBA", Path.Combine(usbPath, RepositoryAddress.ProjectNameVariable), false);
			Synchronizer bob = bobSetup.GetSynchronizer();
			bob.ExtraRepositorySources.Add(otherDirPath);

			//now stick a new file over in the "usb", so we can see if it comes back to us
			File.WriteAllText(Path.Combine(otherDirPath.GetPotentialRepoUri(bob.Repository.Identifier, BobSetup.ProjectFolderName, progress), "incoming.abc"), "this would be a file coming in");
			SyncOptions options = new SyncOptions();
			ProjectFolderConfiguration usbProject = new ProjectFolderConfiguration(Path.Combine(usbPath, BobSetup.ProjectFolderName));
			usbProject.IncludePatterns.Add("**.abc");
			options.CheckinDescription = "adding a file to the usb for some reason";
			var synchronizer = Synchronizer.FromProjectConfiguration(usbProject, progress);
			synchronizer.Repository.SetUserNameInIni("usba", progress);
			synchronizer.SyncNow(options);


			//now we should get that file

			options.DoPullFromOthers = true;
			options.DoMergeWithOthers = false;
			options.DoSendToOthers = false;
			options.CheckinDescription = "test getting new file from usb";
			options.RepositorySourcesToTry.Add(otherDirPath);
			bob.SyncNow(options);
			Assert.IsTrue(File.Exists(Path.Combine(bobSetup._languageProjectPath, "incoming.abc")));
		}

		[Test]
		public void TestNewVersion_SallyUpgrades_BobNotYet()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			bobSetup.ChangeTextFile();

			//Ok, this is unrealistic, but we just clone Bob onto Sally
			string sallyMachineRoot = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallyMachineRoot);
			string sallyProjectRoot = bobSetup.SetupClone(sallyMachineRoot);
			ProjectFolderConfiguration sallyProject = new ProjectFolderConfiguration(sallyProjectRoot);
			sallyProject.IncludePatterns.Add("**.abc");
			sallyProject.IncludePatterns.Add("**.lift");

			var repository = HgRepository.CreateOrLocate(sallyProject.FolderPath, progress);
			repository.SetUserNameInIni("sally", progress);

			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, "<lift version='0.12'><entry id='dog' guid='c1ed1fa9-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>dog</text></form></lexical-unit></entry></lift>");
			var bobOptions = new SyncOptions
			{
				CheckinDescription = "added 'dog'",
				DoMergeWithOthers = false, // just want a fast checkin
				DoSendToOthers = false, // just want a fast checkin
				DoPullFromOthers = false // just want a fast checkin
			};
			var bobSyncer = bobSetup.GetSynchronizer();
			SetAdjunctModelVersion(bobSyncer, ""); // Bob is on 'default' branch
			bobSyncer.SyncNow(bobOptions);

			//now Sally modifies the original file, not having seen Bob's changes yet
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, "lexicon/foo.lift");
			File.WriteAllText(sallyPathToLift, "<lift version='0.13'><entry id='cat' guid='c1ed1faa-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>cat</text></form></lexical-unit></entry></lift>");

			//Sally syncs, pulling in Bob's change, and encountering a need to merge (no conflicts)
			var sallyOptions = new SyncOptions
			{
				CheckinDescription = "adding cat",
				DoPullFromOthers = true,
				DoSendToOthers = true,
				DoMergeWithOthers = true
			};
			var bobAddress = RepositoryAddress.Create("bob's machine", bobSetup.BobProjectPath, false);
			sallyOptions.RepositorySourcesToTry.Add(bobAddress);

			var synchronizer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			const string sallyNewVersion = "LIFT0.13";
			SetAdjunctModelVersion(synchronizer, sallyNewVersion);
			synchronizer.SyncNow(sallyOptions);

			// Verification
			var bobContents = File.ReadAllText(bobSetup._pathToLift);
			Assert.IsFalse(bobContents.Contains("cat"), "'cat' should only be on Sally's branch.");
			Assert.IsTrue(bobContents.Contains("dog"));
			var sallyContents = File.ReadAllText(sallyPathToLift);
			//Debug.WriteLine("sally's: " + sallyContents);
			Assert.IsTrue(sallyContents.Contains("cat"));
			Assert.IsFalse(sallyContents.Contains("dog"), "'dog' should only be in Bob's repo.");

			// Verify Bob is still on the default branch (empty string)
			string dummy;
			var result = bobSyncer.Repository.BranchingHelper.IsLatestBranchDifferent("", out dummy);
			Assert.IsFalse(result, "Bob should be on default branch.");

			// Verify Sally is on the right branch
			result = synchronizer.Repository.BranchingHelper.IsLatestBranchDifferent(sallyNewVersion, out dummy);
			Assert.IsFalse(result, "Sally should be on LIFT0.13 branch.");
		}

		[Test]
		public void TestNewVersion_SallyUpgradesToBobVersion()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			bobSetup.ChangeTextFile();

			//Ok, this is unrealistic, but we just clone Bob onto Sally
			string sallyMachineRoot = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallyMachineRoot);
			string sallyProjectRoot = bobSetup.SetupClone(sallyMachineRoot);
			ProjectFolderConfiguration sallyProject = new ProjectFolderConfiguration(sallyProjectRoot);
			sallyProject.IncludePatterns.Add("**.abc");
			sallyProject.IncludePatterns.Add("**.lift");

			var repository = HgRepository.CreateOrLocate(sallyProject.FolderPath, progress);
			repository.SetUserNameInIni("sally", progress);

			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, "<lift version='0.13'><entry id='dog' guid='c1ed1fa9-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>dog</text></form></lexical-unit></entry></lift>");
			var bobOptions = new SyncOptions
			{
				CheckinDescription = "added 'dog'",
				DoMergeWithOthers = false, // just want a fast checkin
				DoSendToOthers = false, // just want a fast checkin
				DoPullFromOthers = false // just want a fast checkin
			};
			var bobsyncer = bobSetup.GetSynchronizer();
			SetAdjunctModelVersion(bobsyncer, "LIFT0.13"); // Bob is still on an older branch
			bobsyncer.SyncNow(bobOptions);

			// bob makes another change and syncs
			File.WriteAllText(bobSetup._pathToLift, "<lift version='0.14'><entry id='dog' guid='c1ed1fa9-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>dog</text></form></lexical-unit></entry><entry id='herring' guid='d194fdff-707e-42ef-a70f-4e91db2dffd8'><lexical-unit><form lang='en'><text>herring</text></form></lexical-unit></entry></lift>");
			var newBobOptions = new SyncOptions
			{
				CheckinDescription = "added 'herring'",
				DoMergeWithOthers = false, // just want a fast checkin
				DoSendToOthers = false, // just want a fast checkin
				DoPullFromOthers = false // just want a fast checkin
			};
			SetAdjunctModelVersion(bobsyncer, "LIFT0.14"); // Bob is now on a new branch
			bobsyncer.SyncNow(newBobOptions);

			//now Sally modifies the original file, not having seen Bob's changes yet
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, "lexicon/foo.lift");
			File.WriteAllText(sallyPathToLift, "<lift version='0.13'><entry id='cat' guid='c1ed1faa-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>cat</text></form></lexical-unit></entry></lift>");

			//Sally syncs, pulling in Bob's 1st change, and encountering a need to merge (no conflicts)
			var sallyOptions = new SyncOptions
			{
				CheckinDescription = "adding cat",
				DoPullFromOthers = true,
				DoSendToOthers = true,
				DoMergeWithOthers = true
			};
			sallyOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("bob's machine", bobSetup.BobProjectPath, false));

			var synchronizer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			SetAdjunctModelVersion(synchronizer, "LIFT0.13"); // Sally is still on the initial branch
			synchronizer.SyncNow(sallyOptions);

			// So what's supposed to happen?
			var bobContents = File.ReadAllText(bobSetup._pathToLift);
			Assert.IsFalse(bobContents.Contains("cat"), "'cat' should only be on Sally's branch.");
			Assert.IsTrue(bobContents.Contains("dog"));
			var sallyContents = File.ReadAllText(sallyPathToLift);
			Debug.WriteLine("sally's: " + sallyContents);
			Assert.IsTrue(sallyContents.Contains("cat"));
			Assert.IsTrue(sallyContents.Contains("dog"), "Sally should have merged in older branch to hers.");
			Assert.IsFalse(sallyContents.Contains("herring"), "The red herring is only in Bob's repo; 2nd branch.");

			// Now Sally upgrades her LIFT-capable program to Bob's version!
			File.WriteAllText(sallyPathToLift, "<lift version='0.14'><entry id='pig' guid='f6a02b2b-f501-4433-93a6-a1aa40146f63'><lexical-unit><form lang='en'><text>pig</text></form></lexical-unit></entry><entry id='dog' guid='c1ed1fa9-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>dog</text></form></lexical-unit></entry><entry id='cat' guid='c1ed1faa-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>cat</text></form></lexical-unit></entry></lift>");

			//Sally syncs, pulling in Bob's change, and encountering a need to merge (no conflicts)
			sallyOptions = new SyncOptions
			{
				CheckinDescription = "adding pig",
				DoPullFromOthers = true,
				DoSendToOthers = true,
				DoMergeWithOthers = true
			};

			const string lift14version = "LIFT0.14";
			SetAdjunctModelVersion(synchronizer, lift14version); // Sally updates to the new version (branch)
			synchronizer.SyncNow(sallyOptions);
			newBobOptions.DoPullFromOthers = true;
			newBobOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("sally's machine", sallyProjectRoot, false));

			bobsyncer.SyncNow(newBobOptions);
			// Verification
			bobContents = File.ReadAllText(bobSetup._pathToLift);
			//Debug.Print("Bob's: " + bobContents);
			Assert.IsTrue(bobContents.Contains("cat"), "'cat' survived the upgrade to Bob's repo.");
			Assert.IsTrue(bobContents.Contains("dog"));
			Assert.IsTrue(bobContents.Contains("pig"), "'pig' survived the upgrade to Bob's repo.");
			sallyContents = File.ReadAllText(sallyPathToLift);
			//Debug.Print("Sally's: " + sallyContents);
			Assert.IsTrue(sallyContents.Contains("cat"));
			Assert.IsTrue(sallyContents.Contains("dog"), "'dog' should be from Bob's older repo.");
			Assert.IsTrue(sallyContents.Contains("herring"), "Now we should have everything from Bob's repo.");
			Assert.IsTrue(sallyContents.Contains("pig"), "'pig' should have survived the upgrade.");

			// Verify Bob is on the latest branch
			string dummy;
			var result = bobsyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift14version, out dummy);
			Assert.IsFalse(result, "Bob should be on the latest LIFT0.14 branch.");

			// Verify Sally is on the right branch
			result = synchronizer.Repository.BranchingHelper.IsLatestBranchDifferent(lift14version, out dummy);
			Assert.IsFalse(result, "Sally should be on the latest LIFT0.14 branch.");
		}

#if forscreenshot
		string LiftExportPath = @"C:\foo\foo.lift";
	   string OurUsersName="sally";
	   ConsoleProgress Progress = new ConsoleProgress();

		public void TypicalUsage()
		{
			UpdateLiftFile(LiftExportPath);

			ProjectSyncInfo projectInfo = new ProjectSyncInfo();
			projectInfo.IncludePatterns.Add(@"**/*.lift");

			SyncOptions options = new SyncOptions();
			options.CheckinDescription = "Some dictionary work.";
			options.DoPullFromOthers = false;
			options.DoMergeWithOthers = false;

			SyncManager chorus = SyncManager.FromChildPath(LiftExportPath,
				Progress, OurUsersName);
			chorus.SyncNow(projectInfo, options);
			ImportLiftFile(LiftExportPath);
		}

		private void UpdateLiftFile(string s)
		{


		}

		private void ImportLiftFile(string s)
		{
			throw new NotImplementedException();
		}
#endif

		[Test]
		public void CanShareConflictingChangeViaUsb()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			bobSetup.ChangeTextFile();
			string usbSourcePath = Path.Combine(_pathToTestRoot, "USB-A");
			Directory.CreateDirectory(usbSourcePath);
			string usbProjectPath = bobSetup.SetupClone(usbSourcePath);
			Synchronizer usbRepo = Synchronizer.FromProjectConfiguration(new ProjectFolderConfiguration(usbProjectPath), progress);

			Synchronizer bobSynchronizer =  bobSetup.GetSynchronizer();

			//Sally gets the usb and uses it to clone herself a repository
			string sallySourcePath = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallySourcePath);
			//string sallyRepoPath = usbRepo.MakeClone(Path.Combine(sallySourcePath, BobSetup.ProjectFolderName), true);
			string sallyRepoPath = HgHighLevel.MakeCloneFromLocalToLocal(usbRepo.Repository.PathToRepo, Path.Combine(sallySourcePath, BobSetup.ProjectFolderName), true, progress);


			//Now bob sets up the conflict

			File.WriteAllText(bobSetup.PathToText, "Bob's new idea");
			SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "changed my mind";
			bobOptions.DoMergeWithOthers = false; // pretend the usb key isn't there
			bobOptions.DoPullFromOthers = false; // pretend the usb key isn't there
			bobOptions.DoSendToOthers = false;
			RepositoryAddress usbPath = RepositoryAddress.Create( "usba source", Path.Combine(usbSourcePath, RepositoryAddress.ProjectNameVariable),false);
			bobOptions.RepositorySourcesToTry.Add(usbPath);
			bobSynchronizer.SyncNow(bobOptions);

			ProjectFolderConfiguration sallyProject = new ProjectFolderConfiguration(sallyRepoPath);
			sallyProject.IncludePatterns.Add("**.abc");

			Synchronizer sallySynchronizer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			sallySynchronizer.Repository.SetUserNameInIni("sally", new NullProgress());

			//now she modifies a file
			File.WriteAllText(Path.Combine(sallyRepoPath, "lexicon/foo.abc"), "Sally was here");

			//and syncs, which pushes back to the usb key
			SyncOptions sallyOptions = new SyncOptions();
			sallyOptions.CheckinDescription = "making sally's mark on foo.abc";
			 sallyOptions.RepositorySourcesToTry.Add(usbPath);
			sallyOptions.DoPullFromOthers = true;
			sallyOptions.DoMergeWithOthers = true;
			sallyOptions.DoSendToOthers = true;
			sallySynchronizer.SyncNow(sallyOptions);

			//bob still doesn't have direct access to sally's repo... it's in some other city
			// but now the usb comes back to him
			// SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "Getting from sally, i hope";
			bobOptions.DoPullFromOthers = true;
			bobOptions.DoSendToOthers = true;
			bobOptions.DoMergeWithOthers = true;
			bobSynchronizer.SyncNow(bobOptions);

			// With sync set as 'WeWin'
			Assert.AreEqual("Bob's new idea", File.ReadAllText(bobSetup.PathToText));
			var notesPath = Path.Combine(Path.Combine(Path.Combine(usbSourcePath, BobSetup.ProjectFolderName), "lexicon"), "foo.abc.ChorusNotes");
			AssertThatXmlIn.File(notesPath).HasSpecifiedNumberOfMatchesForXpath("//notes/annotation[@class='mergeConflict']", 1);

			//The conflict has now been created, in the merge with Bob, make a new conflict and make sure that when Sally does the next sync both conflicts are
			//present in the ChorusNotes.
			File.WriteAllText(Path.Combine(sallyRepoPath, "lexicon/foo.abc"), "Sally changed her mind");
			File.WriteAllText(bobSetup.PathToText, "Bob changed his mind.");
			bobOptions.CheckinDescription = "Bob makes conflicting change.";
			bobSynchronizer.SyncNow(bobOptions);
			sallyOptions.CheckinDescription = "Sally makes conflicting change.";
			sallySynchronizer.SyncNow(sallyOptions);
			AssertThatXmlIn.File(notesPath).HasSpecifiedNumberOfMatchesForXpath("//notes/annotation[@class='mergeConflict']", 2);

		}

		[Test]
		public void CanCollaborateOnLift()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			bobSetup.ChangeTextFile();

			//Ok, this is unrealistic, but we just clone Bob onto Sally
			string sallyMachineRoot = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallyMachineRoot);
			string sallyProjectRoot = bobSetup.SetupClone(sallyMachineRoot);
			ProjectFolderConfiguration sallyProject = new ProjectFolderConfiguration(sallyProjectRoot);
			sallyProject.IncludePatterns.Add("**.abc");
			sallyProject.IncludePatterns.Add("**.lift");

			var repository = HgRepository.CreateOrLocate(sallyProject.FolderPath, progress);
			repository.SetUserNameInIni("sally",progress);


			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, "<lift version='0.12'><entry id='dog' guid='c1ed1fa9-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>dog</text></form></lexical-unit></entry></lift>");
			var bobOptions = new SyncOptions
										{
											CheckinDescription = "added 'dog'",
											DoMergeWithOthers = false, // just want a fast checkin
											DoSendToOthers = false, // just want a fast checkin
											DoPullFromOthers = false // just want a fast checkin
										};
			bobSetup.GetSynchronizer().SyncNow(bobOptions);

			//now Sally modifies the original file, not having seen Bob's changes yet
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, "lexicon/foo.lift");
			File.WriteAllText(sallyPathToLift, "<lift version='0.12'><entry id='cat' guid='c1ed1faa-e382-11de-8a39-0800200c9a66'><lexical-unit><form lang='en'><text>cat</text></form></lexical-unit></entry></lift>");

			//Sally syncs, pulling in Bob's change, and encountering a need to merge (no conflicts)
			var sallyOptions = new SyncOptions
										{
											CheckinDescription = "adding cat",
											DoPullFromOthers = true,
											DoSendToOthers = true,
											DoMergeWithOthers = true
										};
			sallyOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("bob's machine", bobSetup.BobProjectPath, false));

			var synchronizer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			synchronizer.SyncNow(sallyOptions);

			//Debug.WriteLine("bob's: " + File.ReadAllText(bobSetup._pathToLift));
			var contents = File.ReadAllText(sallyPathToLift);
			//Debug.WriteLine("sally's: " + contents);
			Assert.IsTrue(contents.Contains("cat"));
			Assert.IsTrue(contents.Contains("dog"));
		}



		public void AssertLineOfFile(string filePath, int lineNumber1Based, string shouldEqual)
		{
			string[] lines = File.ReadAllLines(filePath);
			Assert.AreEqual(shouldEqual, lines[lineNumber1Based - 1]);
		}
//
//        public void ChangeTestFileAndCheckin(HgPartialMerge repo, string fileName, int lineNumber1Based, string newText)
//        {
//            Debug.WriteLine(repo.UserName+" changing line "+lineNumber1Based +" to "+ newText);
//            string[] lines = File.ReadAllLines(repo.GetFilePath(fileName));
//            lines[lineNumber1Based-1] = newText;
//            using (StreamWriter stream = File.CreateText(repo.GetFilePath(fileName)))
//            {
//                foreach (string line in lines)
//                {
//                    stream.Write(line + System.Environment.NewLine);
//                }
//                stream.Close();
//            }
//
//            repo.Commit(false, "Change line " + lineNumber1Based + " of " + fileName /*repo.GetFilePath(fileName)*/ + " to " + newText);
//        }

	}
}