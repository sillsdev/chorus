using System;
using System.IO;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.Tests.sync
{
	[TestFixture]
	[Category("Sync")]
	public class SyncScenarioTests
	{
		private string _pathToTestRootBase;
		private string _pathToTestRoot;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_pathToTestRootBase = Path.Combine(Path.GetTempPath(), "ChorusSyncScenarioTests");
			if (Directory.Exists(_pathToTestRootBase))
				Directory.Delete(_pathToTestRootBase, true); // Just in case is has lingered from the last test run.
			Directory.CreateDirectory(_pathToTestRootBase);
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			Directory.Delete(_pathToTestRootBase, true);
		}

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(_pathToTestRootBase, Guid.NewGuid().ToString());
			Directory.CreateDirectory(_pathToTestRoot);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_pathToTestRoot, true);
		}


		#region Test Utilities

		public static void SetAdjunctModelVersion(Synchronizer synchronizer, string branchName)
		{
			synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct(branchName);
		}

		/// <summary>
		/// Returns a new SyncOptions object with checkin description filled and all flags set to true
		/// (DoPullFromOthers, DoSendToOthers, DoMergeWithOthers)
		/// </summary>
		/// <param name="checkinComment"></param>
		/// <returns></returns>
		public SyncOptions GetFullSyncOptions(string checkinComment)
		{
			return new SyncOptions
			{
				CheckinDescription = checkinComment,
				DoPullFromOthers = true,
				DoSendToOthers = true,
				DoMergeWithOthers = true
			};
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

				_projectFolderConfiguration = CreateFolderConfig(languageProjectPath);

				SyncOptions options = new SyncOptions();
				options.DoPullFromOthers = false;
				options.DoMergeWithOthers = false;
				options.CheckinDescription = "Added";

				RepositorySetup.MakeRepositoryForTest(languageProjectPath, "bob", _progress);

				SyncResults results = GetSynchronizer().SyncNow(options);
			}

			public static ProjectFolderConfiguration CreateFolderConfig(string baseDir)
			{
				var config = new ProjectFolderConfiguration(baseDir);
				config.ExcludePatterns.Add(Path.Combine("**", "cache"));
				config.IncludePatterns.Add("**.abc");
				config.IncludePatterns.Add("**.lift");
				return config;
			}

			public string BobProjectPath
			{
				//get { return Path.Combine(BobSourcePath, RepositoryAddress.ProjectNameVariable); }
				get { return _languageProjectPath; }
			}

			public void ChangeTextFile()
			{
				ChangeTextFile(GetSynchronizer());
			}

			public void ChangeTextFile(Synchronizer sync)
			{
				SyncOptions options = new SyncOptions();
				options.CheckinDescription = "a change to foo.abc";
				string bobsFooTextPath = Path.Combine(_lexiconProjectPath, "foo.abc");
				File.WriteAllText(bobsFooTextPath, "version two of my pretend txt");
				sync.SyncNow(options);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="targetPath">does not inclue the project folder dir</param>
			public string SetupClone(string targetPath)
			{
				//return GetSynchronizer().MakeClone(Path.Combine(targetPath, BobSetup.ProjectFolderName), true);
				return HgHighLevel.MakeCloneFromUsbToLocal(_languageProjectPath,
					Path.Combine(targetPath, BobSetup.ProjectFolderName), _progress);
			}

			public Synchronizer GetSynchronizer()
			{
				ProjectFolderConfiguration project = CreateFolderConfig(_languageProjectPath);
				Synchronizer repo= Synchronizer.FromProjectConfiguration(project, _progress);
				repo.Repository.SetUserNameInIni("bob", _progress);
				return repo;
			}
		}

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
			var options = GetFullSyncOptions("adding a file to the usb for some reason");
			ProjectFolderConfiguration usbProject = BobSetup.CreateFolderConfig(Path.Combine(usbPath, BobSetup.ProjectFolderName));
			var synchronizer = Synchronizer.FromProjectConfiguration(usbProject, progress);
			synchronizer.Repository.SetUserNameInIni("usba", progress);
			synchronizer.SyncNow(options);

			//now we should get that file
			options = GetFullSyncOptions("test getting new file from usb");
			options.DoMergeWithOthers = false;
			options.DoSendToOthers = false;
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

			var repository = HgRepository.CreateOrUseExisting(sallyProject.FolderPath, progress);
			repository.SetUserNameInIni("sally", progress);

			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift12Dog);

			var bobOptions = GetFullSyncOptions("added dog");
			bobOptions.DoMergeWithOthers = false; // just want a fast checkin
			bobOptions.DoSendToOthers = false; // just want a fast checkin
			bobOptions.DoPullFromOthers = false; // just want a fast checkin

			var bobSyncer = bobSetup.GetSynchronizer();
			SetAdjunctModelVersion(bobSyncer, ""); // Bob is on 'default' branch
			bobSyncer.SyncNow(bobOptions);

			//now Sally modifies the original file, not having seen Bob's changes yet
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, "lexicon/foo.lift");
			File.WriteAllText(sallyPathToLift, LiftFileStrings.lift12Cat);

			//Sally syncs, pulling in Bob's change, and encountering a need to merge (no conflicts)
			var sallyOptions = GetFullSyncOptions("adding cat");
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
		[Category("KnownMonoIssue")] // Actually, it is an unknown mono issue.
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

			var repository = HgRepository.CreateOrUseExisting(sallyProject.FolderPath, progress);
			repository.SetUserNameInIni("sally", progress);

			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift12Dog);
			var bobOptions = GetFullSyncOptions("added dog");
			bobOptions.DoMergeWithOthers = false; // just want a fast checkin
			bobOptions.DoSendToOthers = false; // just want a fast checkin
			bobOptions.DoPullFromOthers = false; // just want a fast checkin

			var bobsyncer = bobSetup.GetSynchronizer();
			SetAdjunctModelVersion(bobsyncer, "LIFT0.12"); // Bob is still on an older branch
			bobsyncer.SyncNow(bobOptions);

			// bob makes another change and syncs
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift13DogHerring);
			bobOptions.CheckinDescription = "added 'herring'"; // still just want a fast checkin

			SetAdjunctModelVersion(bobsyncer, "LIFT0.13"); // Bob is now on a new branch
			bobsyncer.SyncNow(bobOptions);

			//now Sally modifies the original file, not having seen Bob's changes yet
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, "lexicon/foo.lift");
			File.WriteAllText(sallyPathToLift, LiftFileStrings.lift12Cat);

			//Sally syncs, pulling in Bob's 1st change, and encountering a need to merge (no conflicts)
			var sallyOptions = GetFullSyncOptions("adding cat");
			sallyOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("bob's machine", bobSetup.BobProjectPath, false));

			var synchronizer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			SetAdjunctModelVersion(synchronizer, "LIFT0.12"); // Sally is still on the initial branch

			// SUT
			synchronizer.SyncNow(sallyOptions);

			// Verification stage 1
			var bobContents = File.ReadAllText(bobSetup._pathToLift);
			Assert.IsFalse(bobContents.Contains("cat"), "'cat' should only be on Sally's branch.");
			Assert.IsTrue(bobContents.Contains("dog"));
			var sallyContents = File.ReadAllText(sallyPathToLift);
			Assert.IsTrue(sallyContents.Contains("cat"));
			Assert.IsTrue(sallyContents.Contains("dog"), "Sally should have merged in older branch to hers.");
			Assert.IsFalse(sallyContents.Contains("herring"), "The red herring is only in Bob's repo; 2nd branch.");

			// Now Sally upgrades her LIFT-capable program to Bob's version!
			File.WriteAllText(sallyPathToLift, LiftFileStrings.lift13PigDogCat);

			//Sally syncs, pulling in Bob's change, and encountering a need to merge (no conflicts)
			sallyOptions = GetFullSyncOptions("adding pig");

			const string lift13version = "LIFT0.13";
			SetAdjunctModelVersion(synchronizer, lift13version); // Sally updates to the new version (branch)
			synchronizer.SyncNow(sallyOptions);
			bobOptions.DoPullFromOthers = true;
			bobOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("sally's machine", sallyProjectRoot, false));

			// SUT
			bobsyncer.SyncNow(bobOptions);

			// Verification stage 2
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
			var result = bobsyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsFalse(result, "Bob should be on the latest LIFT0.13 branch.");

			// Verify Sally is on the right branch
			result = synchronizer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsFalse(result, "Sally should be on the latest LIFT0.13 branch.");
		}

		[Test]
		public void TestNewVersion_SallyAndBobUpgradeButFredDelays()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			// clone Bob onto Sally
			string sallyMachineRoot = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallyMachineRoot);
			string sallyProjectRoot = bobSetup.SetupClone(sallyMachineRoot);
			ProjectFolderConfiguration sallyProject = new ProjectFolderConfiguration(sallyProjectRoot);
			sallyProject.IncludePatterns.Add("**.abc");
			sallyProject.IncludePatterns.Add("**.lift");

			// clone Bob onto Fred
			string fredMachineRoot = Path.Combine(_pathToTestRoot, "fred");
			Directory.CreateDirectory(fredMachineRoot);
			string fredProjectRoot = bobSetup.SetupClone(fredMachineRoot);
			ProjectFolderConfiguration fredProject = new ProjectFolderConfiguration(fredProjectRoot);
			fredProject.IncludePatterns.Add("**.abc");
			fredProject.IncludePatterns.Add("**.lift");

			// Setup Sally and Fred repositories
			var sallyRepository = HgRepository.CreateOrUseExisting(sallyProject.FolderPath, progress);
			sallyRepository.SetUserNameInIni("sally", progress);
			var fredRepository = HgRepository.CreateOrUseExisting(fredProject.FolderPath, progress);
			fredRepository.SetUserNameInIni("fred", progress);
			var sallyRepoAddress = RepositoryAddress.Create("sally's machine", sallyProjectRoot, false);
			var fredRepoAddress = RepositoryAddress.Create("fred's machine", fredProjectRoot, false);
			var bobRepoAddress = RepositoryAddress.Create("bob's machine", bobSetup.BobProjectPath, false);

			// bob makes a change and syncs to everybody
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift12Dog);
			var bobOptions = GetFullSyncOptions("added 'dog'");
			bobOptions.RepositorySourcesToTry.Add(sallyRepoAddress);
			bobOptions.RepositorySourcesToTry.Add(fredRepoAddress);

			var bobSyncer = bobSetup.GetSynchronizer();
			bobSyncer.SyncNow(bobOptions); // Bob syncs with everybody on 'default' branch

			// Verification Step 1
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, "lexicon/foo.lift");
			var fredPathToLift = Path.Combine(fredProject.FolderPath, "lexicon/foo.lift");
			var sallyContents = File.ReadAllText(sallyPathToLift);
			Assert.IsTrue(sallyContents.Contains("dog"), "'dog' should be in Sally repo.");
			var fredContents = File.ReadAllText(fredPathToLift);
			Assert.IsTrue(fredContents.Contains("dog"), "'dog' should be in Fred repo.");

			// bob makes another change and syncs to new version
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift13DogCat);
			var newBobOptions = GetFullSyncOptions("added 'cat'");
			newBobOptions.DoMergeWithOthers = false; // just want a fast checkin
			newBobOptions.DoPullFromOthers = false; // just want a fast checkin
			newBobOptions.DoSendToOthers = false; // just want a fast checkin
			const string lift13version = "LIFT0.13";

			SetAdjunctModelVersion(bobSyncer, lift13version); // Bob is now on the new version of LIFT
			bobSyncer.SyncNow(newBobOptions);

			// now Fred modifies default branch to add 'ant'
			File.WriteAllText(fredPathToLift, LiftFileStrings.lift12DogAnt);
			var fredOptions = GetFullSyncOptions("added 'ant'");
			fredOptions.RepositorySourcesToTry.Add(bobRepoAddress);
			fredOptions.RepositorySourcesToTry.Add(sallyRepoAddress);
			var fredSyncer = Synchronizer.FromProjectConfiguration(fredProject, progress);
			fredSyncer.SyncNow(fredOptions);

			// Verification Step 2
			fredContents = File.ReadAllText(fredPathToLift);
			Assert.IsFalse(fredContents.Contains("cat"), "'cat' should only be on Bob's branch.");
			Assert.IsTrue(fredContents.Contains("ant"));
			sallyContents = File.ReadAllText(sallyPathToLift);
			Assert.IsTrue(sallyContents.Contains("ant"), "'ant' was propogated to Sally's branch.");
			Assert.IsFalse(sallyContents.Contains("cat"), "'cat' should only be on Bob's branch.");
			var bobContents = File.ReadAllText(bobSetup._pathToLift);
			Assert.IsFalse(bobContents.Contains("ant"), "'ant' is only on 'default' branch.");
			// Verify Bob is on the latest branch
			string dummy;
			var result = bobSyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsFalse(result, "Bob should be on the new LIFT0.13 branch.");
			// And Fred isn't
			result = fredSyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsTrue(result, "Fred should still be on the 'default' branch.");

			// Now Sally modifies the file, not having seen Bob's changes yet, but having seen Fred's changes.
			// She adds 'herring' and has upgraded to Bob's version of LIFT
			File.WriteAllText(sallyPathToLift, LiftFileStrings.lift13DogAntHerring);

			//Sally syncs, pulling in Bob's 1st change, and encountering a need to merge (no conflicts)
			var sallyOptions = GetFullSyncOptions("adding 'herring'");
			sallyOptions.RepositorySourcesToTry.Add(bobRepoAddress);
			sallyOptions.RepositorySourcesToTry.Add(fredRepoAddress); // Why not? Even though he's still on 'default' branch

			var sallySyncer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			SetAdjunctModelVersion(sallySyncer, lift13version); // Sally is now on the Bob's later version
			// Below is the line with the hg error
			sallySyncer.SyncNow(sallyOptions);

			// Verification Step 3
			bobContents = File.ReadAllText(bobSetup._pathToLift);
			Assert.IsTrue(bobContents.Contains("herring"), "'herring' should be pulled in from Sally's branch.");
			Assert.IsTrue(bobContents.Contains("ant"), "'ant' should be pulled in from Sally's branch.");
			sallyContents = File.ReadAllText(sallyPathToLift);
			Assert.IsTrue(sallyContents.Contains("cat"), "'cat' should be pulled in from Bob's branch.");
			Assert.IsTrue(sallyContents.Contains("dog"), "Everybody should have 'dog' from before.");
			fredContents = File.ReadAllText(fredPathToLift);
			Assert.IsFalse(fredContents.Contains("herring"), "The red herring is only in the new version for now.");
			Assert.IsFalse(fredContents.Contains("cat"), "'cat' is only in the new version for now.");
			// Verify Sally is now on the latest branch
			result = sallySyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsFalse(result, "Sally should be on the new LIFT0.13 branch.");
			// And Fred still shouldn't be
			result = fredSyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsTrue(result, "Fred should still be on the 'default' branch.");

			// Now Fred checks in 'pig' to the 'default' branch
			File.WriteAllText(fredPathToLift, LiftFileStrings.lift12DogAntPig);

			// Fred syncs, not finding anybody else's changes
			fredOptions.CheckinDescription = "adding 'pig'";
			fredSyncer.SyncNow(fredOptions);

			// Verification Step 4
			bobContents = File.ReadAllText(bobSetup._pathToLift);
			Assert.IsFalse(bobContents.Contains("pig"), "'pig' should only be on 'default' branch.");
			sallyContents = File.ReadAllText(sallyPathToLift);
			Assert.IsFalse(sallyContents.Contains("pig"), "'pig' should only be on 'default' branch.");
			fredContents = File.ReadAllText(fredPathToLift);
			Assert.IsFalse(fredContents.Contains("herring"), "'herring' should still only be in the new version.");
			// Just check Fred hasn't changed branches
			result = fredSyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsTrue(result, "Fred should still be on the 'default' branch.");

			// Now Bob checks in 'deer' in the new version
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift13DogCatHerringAntDeer);
			bobOptions.CheckinDescription = "adding 'deer'";
			bobSyncer.SyncNow(bobOptions);

			// Verification Step 5
			// Check that Fred hasn't changed
			fredContents = File.ReadAllText(fredPathToLift);
			Assert.IsFalse(fredContents.Contains("deer"), "'deer' should only be on new version.");
			result = fredSyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsTrue(result, "Fred should still be on the 'default' branch.");
			// Check that Sally got her 'deer'
			sallyContents = File.ReadAllText(sallyPathToLift);
			Assert.IsTrue(sallyContents.Contains("deer"), "'deer' should have propagated to Sally.");
			// Make sure that 'pig' hasn't migrated to the new version
			Assert.IsFalse(sallyContents.Contains("pig"), "'pig' should still only be on 'default' branch.");

			// Now Fred has finally upgraded and will check in 'fox' -- LAST CHECK-IN FOR THIS TEST!
			File.WriteAllText(fredPathToLift, LiftFileStrings.lift13DogAntPigFox);
			fredOptions.CheckinDescription = "adding 'fox'";
			SetAdjunctModelVersion(fredSyncer, lift13version); // Fred finally updates to the new version (branch)
			fredSyncer.SyncNow(fredOptions);

			// Verification Step 6 (Last)
			bobContents = File.ReadAllText(bobSetup._pathToLift);
			Assert.IsTrue(bobContents.Contains("cat"), "'cat' should survive the big hairy test in Bob's repo.");
			Assert.IsTrue(bobContents.Contains("dog"), "'dog' should survive the big hairy test in Bob's repo.");
			Assert.IsTrue(bobContents.Contains("pig"), "'pig' should survive the big hairy test in Bob's repo.");
			Assert.IsTrue(bobContents.Contains("herring"), "'herring' should survive the big hairy test in Bob's repo.");
			Assert.IsTrue(bobContents.Contains("deer"), "'deer' should survive the big hairy test in Bob's repo.");
			Assert.IsTrue(bobContents.Contains("ant"), "'ant' should survive the big hairy test in Bob's repo.");
			Assert.IsTrue(bobContents.Contains("fox"), "'fox' should survive the big hairy test in Bob's repo.");
			sallyContents = File.ReadAllText(sallyPathToLift);
			Assert.IsTrue(sallyContents.Contains("cat"), "'cat' should survive the big hairy test in Sally's repo.");
			Assert.IsTrue(sallyContents.Contains("dog"), "'dog' should survive the big hairy test in Sally's repo.");
			Assert.IsTrue(sallyContents.Contains("herring"), "'herring' should survive the big hairy test in Sally's repo.");
			Assert.IsTrue(sallyContents.Contains("pig"), "'pig' should survive the big hairy test in Sally's repo.");
			Assert.IsTrue(sallyContents.Contains("deer"), "'deer' should survive the big hairy test in Sally's repo.");
			Assert.IsTrue(sallyContents.Contains("ant"), "'ant' should survive the big hairy test in Sally's repo.");
			Assert.IsTrue(sallyContents.Contains("fox"), "'fox' should survive the big hairy test in Sally's repo.");
			fredContents = File.ReadAllText(fredPathToLift);
			Assert.IsTrue(fredContents.Contains("cat"), "'cat' should survive the big hairy test in Fred's repo.");
			Assert.IsTrue(fredContents.Contains("dog"), "'dog' should survive the big hairy test in Fred's repo.");
			Assert.IsTrue(fredContents.Contains("herring"), "'herring' should survive the big hairy test in Fred's repo.");
			Assert.IsTrue(fredContents.Contains("pig"), "'pig' should survive the big hairy test in Fred's repo.");
			Assert.IsTrue(fredContents.Contains("deer"), "'deer' should survive the big hairy test in Fred's repo.");
			Assert.IsTrue(fredContents.Contains("ant"), "'ant' should survive the big hairy test in Fred's repo.");
			Assert.IsTrue(fredContents.Contains("fox"), "'fox' should survive the big hairy test in Fred's repo.");

			// Verify Bob is on the latest branch
			result = bobSyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsFalse(result, "Bob should be on the new LIFT0.13 branch.");

			// Verify Sally is on the right branch
			result = sallySyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsFalse(result, "Sally should be on the new LIFT0.13 branch.");

			// Verify Fred is finally on the new branch
			result = fredSyncer.Repository.BranchingHelper.IsLatestBranchDifferent(lift13version, out dummy);
			Assert.IsFalse(result, "Fred should finally be on the new LIFT0.13 branch.");
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
			Synchronizer usbRepo = Synchronizer.FromProjectConfiguration(BobSetup.CreateFolderConfig(usbProjectPath), progress);

			Synchronizer bobSynchronizer =  bobSetup.GetSynchronizer();

			//Sally gets the usb and uses it to clone herself a repository
			string sallySourcePath = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallySourcePath);
			//string sallyRepoPath = usbRepo.MakeClone(Path.Combine(sallySourcePath, BobSetup.ProjectFolderName), true);
			string sallyRepoPath = HgHighLevel.MakeCloneFromUsbToLocal(usbRepo.Repository.PathToRepo, Path.Combine(sallySourcePath, BobSetup.ProjectFolderName), progress);


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

			ProjectFolderConfiguration sallyProject = BobSetup.CreateFolderConfig(sallyRepoPath);

			Synchronizer sallySynchronizer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			sallySynchronizer.Repository.SetUserNameInIni("sally", new NullProgress());

			//now she modifies a file
			File.WriteAllText(Path.Combine(sallyRepoPath, Path.Combine("lexicon", "foo.abc")), "Sally was here");

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
			File.WriteAllText(Path.Combine(sallyRepoPath, Path.Combine("lexicon", "foo.abc")), "Sally changed her mind");
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
			ProjectFolderConfiguration sallyProject = BobSetup.CreateFolderConfig(sallyProjectRoot);

			var repository = HgRepository.CreateOrUseExisting(sallyProject.FolderPath, progress);
			repository.SetUserNameInIni("sally",progress);

			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift12Dog);
			var bobOptions = new SyncOptions
										{
											CheckinDescription = "added 'dog'",
											DoMergeWithOthers = false, // just want a fast checkin
											DoSendToOthers = false, // just want a fast checkin
											DoPullFromOthers = false // just want a fast checkin
										};
			bobSetup.GetSynchronizer().SyncNow(bobOptions);

			//now Sally modifies the original file, not having seen Bob's changes yet
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, Path.Combine("lexicon", "foo.lift"));
			File.WriteAllText(sallyPathToLift, LiftFileStrings.lift12Cat);

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

		/// <summary>
		/// If one collaborator does two S/R cycles including a merge and a further commit.
		/// A collaborator "who has not made any changes" receiving those changes needs the tip updated
		/// </summary>
		[Test]
		public void TipUpdatedPostMerge()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);
			var bobSynchronizer = bobSetup.GetSynchronizer();
			//set up two branches to trigger issue
			SetAdjunctModelVersion(bobSynchronizer, "notdefault"); // Bob is on 'default' branch
			bobSetup.ChangeTextFile(bobSynchronizer);

			//Ok, this is unrealistic, but we just clone Bob onto Sally
			var hubRoot = Path.Combine(_pathToTestRoot, "Hub");
			var sallyMachineRoot = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallyMachineRoot);
			Directory.CreateDirectory(hubRoot);
			var sallyProjectRoot = bobSetup.SetupClone(sallyMachineRoot);
			var hubProjectRoot = bobSetup.SetupClone(hubRoot);
			var sallyProject = BobSetup.CreateFolderConfig(sallyProjectRoot);
			var hubProject = BobSetup.CreateFolderConfig(hubProjectRoot);

			var repository = HgRepository.CreateOrUseExisting(sallyProject.FolderPath, progress);
			repository.SetUserNameInIni("sally", progress);

			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift12Dog);
			var bobOptions = new SyncOptions
			{
				CheckinDescription = "added 'dog'",
				DoMergeWithOthers = true,
				DoSendToOthers = true,
				DoPullFromOthers = true
			};
			bobOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("Hub", hubProject.FolderPath, false));

			//now Sally modifies the original file, not having seen Bob's changes yet
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, Path.Combine("lexicon", "foo.lift"));
			File.WriteAllText(sallyPathToLift, LiftFileStrings.lift12Cat);

			//Sally syncs, pulling in Bob's change, and encountering a need to merge (no conflicts)
			var sallyOptions = new SyncOptions
			{
				CheckinDescription = "adding cat",
				DoPullFromOthers = true,
				DoSendToOthers = true,
				DoMergeWithOthers = true
			};
			sallyOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("Hub", hubProject.FolderPath, false));

			var sallySyncer = Synchronizer.FromProjectConfiguration(sallyProject, progress);
			SetAdjunctModelVersion(sallySyncer, "notdefault");
			sallySyncer.SyncNow(sallyOptions);
			bobSynchronizer.SyncNow(bobOptions);
			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, LiftFileStrings.lift12DogAnt);
			bobSynchronizer.SyncNow(bobOptions);
			sallyOptions.DoSendToOthers = false;
			sallySyncer.SyncNow(sallyOptions);
			//Debug.WriteLine("bob's: " + File.ReadAllText(bobSetup._pathToLift));
			var contents = File.ReadAllText(sallyPathToLift);
			//Debug.WriteLine("sally's: " + contents);
			Assert.IsTrue(contents.Contains("ant"));
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

	public static class LiftFileStrings
	{
		public static string liftClosing = "</lift>";
		public static string lift12Header = "<lift version='0.12'>";
		public static string lift13Header = "<lift version='0.13'>";

		public static string dogEntry =
			@"<entry id='dog' guid='c1ed1fa9-e382-11de-8a39-0800200c9a66'>
				<lexical-unit>
					<form lang='en'><text>dog</text></form>
				</lexical-unit>
			</entry>";

		public static string catEntry =
			@"<entry id='cat' guid='c1ed1faa-e382-11de-8a39-0800200c9a66'>
				<lexical-unit>
					<form lang='en'><text>cat</text></form>
				</lexical-unit>
			</entry>";

		public static string antEntry =
			@"<entry id='ant' guid='3525f996-c711-42ac-896e-d7fef8296e1d'>
				<lexical-unit>
					<form lang='en'><text>ant</text></form>
				</lexical-unit>
			</entry>";

		public static string herringEntry =
			@"<entry id='herring' guid='d194fdff-707e-42ef-a70f-4e91db2dffd8'>
				<lexical-unit>
					<form lang='en'><text>herring</text></form>
				</lexical-unit>
			</entry>";

		public static string pigEntry =
			@"<entry id='pig' guid='f6a02b2b-f501-4433-93a6-a1aa40146f63'>
				<lexical-unit>
					<form lang='en'><text>pig</text></form>
				</lexical-unit>
			</entry>";

		public static string deerEntry =
			@"<entry id='deer' guid='7a3c7608-f33d-40f3-9000-36d97021f140'>
				<lexical-unit>
					<form lang='en'><text>deer</text></form>
				</lexical-unit>
			</entry>";

		public static string foxEntry =
			@"<entry id='fox' guid='76f4f768-d64f-4bc9-a236-acd0618064ff'>
				<lexical-unit>
					<form lang='en'><text>fox</text></form>
				</lexical-unit>
			</entry>";

		public static string lift12Dog = lift12Header + dogEntry + liftClosing;
		public static string lift12Cat = lift12Header + catEntry + liftClosing;
		public static string lift12DogAnt = lift12Header + dogEntry + antEntry + liftClosing;
		public static string lift12DogAntPig = lift12Header + dogEntry + antEntry + pigEntry + liftClosing;
		public static string lift13DogHerring = lift13Header + dogEntry + herringEntry + liftClosing;
		public static string lift13DogAntHerring = lift13Header + dogEntry + antEntry + herringEntry + liftClosing;
		public static string lift13DogCatHerringAntDeer = lift13Header + dogEntry + catEntry + herringEntry + antEntry + deerEntry + liftClosing;
		public static string lift13DogAntPigFox = lift13Header + dogEntry + antEntry + pigEntry + foxEntry + liftClosing;
		public static string lift13DogCat = lift13Header + dogEntry + catEntry + liftClosing;
		public static string lift13PigDogCat = lift13Header + pigEntry + dogEntry + catEntry + liftClosing;
	}
}