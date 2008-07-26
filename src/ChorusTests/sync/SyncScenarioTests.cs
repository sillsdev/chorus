using System;
using System.Diagnostics;
using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.sync
{
	[TestFixture]
	public class SyncScenarioTests
	{
		private string _pathToTestRoot;

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if(Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);
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

				PathToText = Path.Combine(_lexiconProjectPath, "foo.txt");
				File.WriteAllText(PathToText, "version one of my pretend txt");

				_pathToLift = Path.Combine(_lexiconProjectPath, "foo.lift");
				File.WriteAllText(_pathToLift, "<lift version='0.12'></lift>");

				string picturesPath = Path.Combine(_lexiconProjectPath, "pictures");
				Directory.CreateDirectory(picturesPath);
				File.WriteAllText(Path.Combine(picturesPath, "dog.jpg"), "Not really a picture");

				string cachePath = Path.Combine(_lexiconProjectPath, "cache");
				Directory.CreateDirectory(cachePath);
				File.WriteAllText(Path.Combine(cachePath, "cache.txt"), "Some cache stuff");

				_projectFolderConfiguration = new ProjectFolderConfiguration(languageProjectPath);
				_projectFolderConfiguration.IncludePatterns.Add(_lexiconProjectPath);
				_projectFolderConfiguration.ExcludePatterns.Add("**/cache");

				SyncOptions options = new SyncOptions();
				options.DoPullFromOthers = false;
				options.DoMergeWithOthers = false;
				options.CheckinDescription = "initial";

				RepositoryManager.MakeRepositoryForTest(languageProjectPath, "bob");

				//SyncManager bobManager = SyncManager.FromChildPath(_lexiconProjectPath, progress, "bob");
				SyncResults results = GetManager().SyncNow(options,progress);
			}

			public void ChangeTextFile()
			{
				SyncOptions options = new SyncOptions();
				options.CheckinDescription = "a change to foo.txt";
				string bobsFooTextPath = Path.Combine(_lexiconProjectPath, "foo.txt");
				File.WriteAllText(bobsFooTextPath, "version two of my pretend txt");
				GetManager().SyncNow(options,_progress);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="sourcePath">does not inclue the project folder dir</param>
			public string SetupClone(string sourcePath)
			{
				return GetManager().MakeClone(Path.Combine(sourcePath, BobSetup.ProjectFolderName), true, _progress);
			}

			public RepositoryManager GetManager()
			{
				ProjectFolderConfiguration project = new ProjectFolderConfiguration(_lexiconProjectPath);
				project.IncludePatterns.Add("**.txt");
				 project.IncludePatterns.Add("**.lift");
			   RepositoryManager repo= RepositoryManager.FromRootOrChildFolder(project);
				repo.SetUserId("bob");
				return repo;
			}
		}

//        [Test]
//        public void CloneShouldHaveSameDirectoryName()
//        {
//            ConsoleProgress progress = new ConsoleProgress();
//            BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);
//
//            RepositoryManager repo = RepositoryManager.FromRootOrChildFolder(bobSetup._projectFolderConfiguration);
//            string usbPath = Path.Combine(_pathToTestRoot, "USB-A");
//            repo.MakeClone(usbPath, false, progress);
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

			RepositorySource otherDirSource = RepositorySource.Create(usbPath, "USBA", false);
			RepositoryManager bob = bobSetup.GetManager();
			bob.KnownRepositorySources.Add(otherDirSource);

			//now stick a new file over in the "usb", so we can see if it comes back to us
			File.WriteAllText(Path.Combine(otherDirSource.PotentialRepoUri(BobSetup.ProjectFolderName, progress), "incoming.txt"), "this would be a file coming in");
			SyncOptions options = new SyncOptions();
			ProjectFolderConfiguration usbProject = new ProjectFolderConfiguration(Path.Combine(usbPath, BobSetup.ProjectFolderName));
			usbProject.IncludePatterns.Add("**.txt");
			options.CheckinDescription = "adding a file to the usb for some reason";
			RepositoryManager usbManager = RepositoryManager.FromRootOrChildFolder(usbProject);
			usbManager.SetUserId("usba");
			usbManager.SyncNow(options,progress);


			//now we should get that file

			options.DoPullFromOthers = true;
			options.DoMergeWithOthers = false;
			options.DoPushToLocalSources = false;
			options.CheckinDescription = "test getting new file from usb";
			options.RepositorySourcesToTry.Add(otherDirSource);
			bob.SyncNow(options, progress);
			Assert.IsTrue(File.Exists(Path.Combine(bobSetup._languageProjectPath, "incoming.txt")));
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
			RepositoryManager usbRepo = RepositoryManager.FromRootOrChildFolder(new ProjectFolderConfiguration(usbProjectPath));

			RepositoryManager bobRepo =  bobSetup.GetManager();

			//Sally gets the usb and uses it to clone herself a repository
			string sallySourcePath = Path.Combine(_pathToTestRoot, "sally");
			Directory.CreateDirectory(sallySourcePath);
			string sallyRepoPath = usbRepo.MakeClone(Path.Combine(sallySourcePath, BobSetup.ProjectFolderName), true, progress);

			//Now bob sets up the conflict

			File.WriteAllText(bobSetup.PathToText, "Bob's new idea");
			SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "changed my mind";
			bobOptions.DoMergeWithOthers = false; // pretend the usb key isn't there
			bobOptions.DoPullFromOthers = false; // pretend the usb key isn't there
			bobOptions.DoPushToLocalSources = false;
			RepositorySource usbSource = RepositorySource.Create(usbSourcePath, "usba source", false);
			bobOptions.RepositorySourcesToTry.Add(usbSource);
			bobRepo.SyncNow(bobOptions, progress);

			ProjectFolderConfiguration sallyProject = new ProjectFolderConfiguration(sallyRepoPath);
			sallyProject.IncludePatterns.Add("**.txt");

			RepositoryManager sally = RepositoryManager.FromRootOrChildFolder(sallyProject);
			sally.SetUserId("sally");

			//now she modifies a file
			File.WriteAllText(Path.Combine(sallyRepoPath, "lexicon/foo.txt"), "Sally was here");

			//and syncs, which pushes back to the usb key
			SyncOptions sallyOptions = new SyncOptions();
			sallyOptions.CheckinDescription = "making sally's mark on foo.txt";
			 sallyOptions.RepositorySourcesToTry.Add(usbSource);
			sallyOptions.DoPullFromOthers = true;
			sallyOptions.DoMergeWithOthers = true;
			sallyOptions.DoPushToLocalSources = true;
			sally.SyncNow(sallyOptions, progress);

			//bob still doesn't have direct access to sally's repo... it's in some other city
			// but now the usb comes back to him
			// SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "Getting from sally, i hope";
			bobOptions.DoPullFromOthers = true;
			bobOptions.DoPushToLocalSources = true;
			bobOptions.DoMergeWithOthers = true;
			bobRepo.SyncNow(bobOptions, progress);


			Assert.AreEqual("Sally was here", File.ReadAllText(bobSetup.PathToText));

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
			sallyProject.IncludePatterns.Add("**.txt");
			sallyProject.IncludePatterns.Add("**.lift");

			RepositoryManager sallyRepo = RepositoryManager.FromRootOrChildFolder(sallyProject);
			sallyRepo.SetUserId("sally");


			// bob makes a change and syncs
			File.WriteAllText(bobSetup._pathToLift, "<lift version='0.12'><entry id='dog'><lexical-unit><form lang='en'><text>dog</text></form></lexical-unit></entry></lift>");
			SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "added 'dog'";
			bobOptions.DoMergeWithOthers = false; // just want a fast checkin
			bobOptions.DoPushToLocalSources = false; // just want a fast checkin
			bobOptions.DoPullFromOthers = false; // just want a fast checkin
			bobSetup.GetManager().SyncNow(bobOptions, progress);


			//now Sally modifies the original file, not having seen Bob's changes yet
			string sallyPathToLift = Path.Combine(sallyProject.FolderPath, "lexicon/foo.lift");
			File.WriteAllText(sallyPathToLift, "<lift version='0.12'><entry id='cat'><lexical-unit><form lang='en'><text>cat</text></form></lexical-unit></entry></lift>");

			//Salyy syncs, pulling in Bob's change, and encountering a need to merge (no conflicts)
			SyncOptions sallyOptions = new SyncOptions();
			sallyOptions.CheckinDescription = "adding cat";
			sallyOptions.DoPullFromOthers = true;
			sallyOptions.DoPushToLocalSources = true;
			sallyOptions.DoMergeWithOthers = true;
			sallyOptions.RepositorySourcesToTry.Add(RepositorySource.Create(bobSetup.BobSourcePath, "bob's machine", false));
			sallyRepo.SyncNow(sallyOptions, progress);

			Debug.WriteLine(File.ReadAllText(bobSetup._pathToLift));
			string contents = File.ReadAllText(sallyPathToLift);
			Assert.IsTrue(contents.Contains("cat"));
			Assert.IsTrue(contents.Contains("dog"));
		}


		private void AssertTestFile(HgPartialMerge repo, int line, string expectedContents)
		{
			Debug.WriteLine("Checking that " + repo.UserName + " has '" + expectedContents + "' in line " + line);
			AssertLineOfFile(repo.GetFilePath("data.txt"), line, expectedContents);
		}

		public void AssertLineOfFile(string filePath, int lineNumber1Based, string shouldEqual)
		{
			string[] lines = File.ReadAllLines(filePath);
			Assert.AreEqual(shouldEqual, lines[lineNumber1Based - 1]);
		}

		public void ChangeTestFileAndCheckin(HgPartialMerge repo, string fileName, int lineNumber1Based, string newText)
		{
			Debug.WriteLine(repo.UserName+" changing line "+lineNumber1Based +" to "+ newText);
			string[] lines = File.ReadAllLines(repo.GetFilePath(fileName));
			lines[lineNumber1Based-1] = newText;
			using (StreamWriter stream = File.CreateText(repo.GetFilePath(fileName)))
			{
				foreach (string line in lines)
				{
					stream.Write(line + System.Environment.NewLine);
				}
				stream.Close();
			}

			repo.Commit(false, "Change line " + lineNumber1Based + " of " + fileName /*repo.GetFilePath(fileName)*/ + " to " + newText);
		}

	}
}