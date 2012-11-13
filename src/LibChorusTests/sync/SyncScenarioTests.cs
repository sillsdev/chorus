using System.IO;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;
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

				RepositorySetup.MakeRepositoryForTest(languageProjectPath, "bob",_progress);

				//SyncManager bobManager = SyncManager.FromChildPath(_lexiconProjectPath, progress, "bob");
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
				get { return Path.Combine(BobSourcePath, RepositoryAddress.ProjectNameVariable); }
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
				ProjectFolderConfiguration project = CreateFolderConfig(_languageProjectPath);
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
			ProjectFolderConfiguration usbProject = BobSetup.CreateFolderConfig(Path.Combine(usbPath, BobSetup.ProjectFolderName));
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

			var repository = HgRepository.CreateOrReconstitute(sallyProject.FolderPath, progress);
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
			var sallyPathToLift = Path.Combine(sallyProject.FolderPath, Path.Combine("lexicon", "foo.lift"));
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