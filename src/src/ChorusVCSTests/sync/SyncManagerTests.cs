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
	public class SyncManagerTests
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
			public ProjectDescriptor _projectDescriptor;
			public string _pathToLift;
			private ConsoleProgress _progress;
			public string _pathToText;

			public BobSetup(ConsoleProgress progress, string pathToTestRoot)
			{
				_progress = progress;
				string bobPath = Path.Combine(pathToTestRoot, "Bob");
				Directory.CreateDirectory(bobPath);
				string languageProjectPath = Path.Combine(bobPath, "LP");
				Directory.CreateDirectory(languageProjectPath);
				_languageProjectPath = languageProjectPath;
				_lexiconProjectPath = Path.Combine(_languageProjectPath, "lexicon");
				Directory.CreateDirectory(_lexiconProjectPath);

				_pathToText = Path.Combine(_lexiconProjectPath, "foo.txt");
				File.WriteAllText(_pathToText, "version one of my pretend txt");

				_pathToLift = Path.Combine(_lexiconProjectPath, "foo.lift");
				File.WriteAllText(_pathToLift, "<lift version='0.12'></lift>");

				string picturesPath = Path.Combine(_lexiconProjectPath, "pictures");
				Directory.CreateDirectory(picturesPath);
				File.WriteAllText(Path.Combine(picturesPath, "dog.jpg"), "Not really a picture");

				string cachePath = Path.Combine(_lexiconProjectPath, "cache");
				Directory.CreateDirectory(cachePath);
				File.WriteAllText(Path.Combine(cachePath, "cache.txt"), "Some cache stuff");

				_projectDescriptor = new ProjectDescriptor();
				_projectDescriptor.IncludePatterns.Add(_lexiconProjectPath);
				_projectDescriptor.ExcludePatterns.Add("**/cache");

				SyncOptions options = new SyncOptions();
				options.DoPullFromOthers = false;
				options.DoMergeWithOthers = false;
				options.CheckinDescription = "initial";

				SyncManager.LocationToMakeRepositoryDuringTest = languageProjectPath;

				SyncManager bobManager = SyncManager.FromChildPath(_lexiconProjectPath, progress, "bob");
				SyncResults results = bobManager.SyncNow(_projectDescriptor, options);
			}

			public void ChangeTextFile()
			{
				SyncOptions options = new SyncOptions();
				options.CheckinDescription = "a change to foo.txt";
				string bobsFooTextPath = Path.Combine(_lexiconProjectPath, "foo.txt");
				File.WriteAllText(bobsFooTextPath, "version two of my pretend txt");
				GetManager().SyncNow(_projectDescriptor, options);
			}

			public void SetupClone(string path, string name)
			{
				GetManager().MakeClone(path);
			}

			public SyncManager GetManager()
			{
				return SyncManager.FromChildPath(_lexiconProjectPath, _progress, "bob");
			}
		}

		[Test]
		public void CanGetNewFileFromAnotherRep()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			bobSetup.ChangeTextFile();
			string usbPath = Path.Combine(_pathToTestRoot, "USB-A");
			bobSetup.SetupClone(usbPath, "USBA");

			RepositorySource usbRepo = RepositorySource.Create(usbPath, "USBA", false);
			SyncManager bob = bobSetup.GetManager();
			bob.KnownRepositories.Add(usbRepo);

			//now stick a new file over in the "usb", so we can see if it comes back to us
			ProjectDescriptor usbProject = new ProjectDescriptor();
			File.WriteAllText(Path.Combine(usbPath, "incoming.txt"), "this would be a file coming in");
			SyncOptions options = new SyncOptions();
			options.CheckinDescription = "adding a file to the usb for some reason";
			SyncManager usbManager = SyncManager.FromChildPath(usbPath, progress, "usbA");
			usbManager.SyncNow(usbProject, options);


			//now we should get that file

			options.DoPullFromOthers = true;
			options.DoMergeWithOthers = false;
			options.CheckinDescription = "test getting new file from usb";
			bob.SyncNow(bobSetup._projectDescriptor, options);
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
			string usbPath = Path.Combine(_pathToTestRoot, "USB-A");
			bobSetup.SetupClone(usbPath, "USBA");
			SyncManager usb = SyncManager.FromChildPath(usbPath, progress, "USBA");

			RepositorySource usbRepo = RepositorySource.Create(usbPath, "USBA", false);
			SyncManager bob =  bobSetup.GetManager();
			bob.KnownRepositories.Add(usbRepo);

			//Sally gets the usb and starts a repository
			string sallyRoot = Path.Combine(_pathToTestRoot, "sally");
			usb.MakeClone(sallyRoot);

			//Now bob sets up the conflict

			File.WriteAllText(bobSetup._pathToText, "Bob's new idea");
			SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "changed my mind";
			bobOptions.DoMergeWithOthers = false; // pretend the usb key isn't there
			bobOptions.DoPullFromOthers = false; // pretend the usb key isn't there
			bob.SyncNow(bobSetup._projectDescriptor, bobOptions);

			SyncManager sally = SyncManager.FromChildPath(sallyRoot, progress,"sally");
			sally.KnownRepositories.Add(RepositorySource.Create(usbRepo.URI, usbRepo.SourceName, false));

			//now she modifies a file
			File.WriteAllText(Path.Combine(sallyRoot, "lexicon/foo.txt"), "Sally was here");
			ProjectDescriptor sallyProject = new ProjectDescriptor();
			SyncOptions sallyOptions = new SyncOptions();
			sallyOptions.CheckinDescription = "making sally's mark on foo.txt";
			sallyOptions.DoPullFromOthers = false;
			sallyOptions.DoMergeWithOthers = false;
			sally.SyncNow(sallyProject, sallyOptions);

			//now she syncs again with the usb key
			sallyOptions.DoPullFromOthers = true;
			sallyOptions.DoMergeWithOthers = true;
			sally.SyncNow(sallyProject, sallyOptions);

			//bob still doesn't have direct access to sally's repo... it's in some other city
			// but now the usb comes back to him
			// SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "Getting from sally, i hope";
			bobOptions.DoPullFromOthers = true;
			bobOptions.DoMergeWithOthers = true;
			bob.SyncNow(bobSetup._projectDescriptor, bobOptions);


			Assert.AreEqual("Sally was here", File.ReadAllText(bobSetup._pathToText));

		}

		[Test]
		public void CanCollaborateOnLift()
		{
			ConsoleProgress progress = new ConsoleProgress();
			BobSetup bobSetup = new BobSetup(progress, _pathToTestRoot);

			bobSetup.ChangeTextFile();

			//Sally gets the usb and starts a repository
			string sallyRoot = Path.Combine(_pathToTestRoot, "sally");
			bobSetup.SetupClone(sallyRoot,"sally");

			File.WriteAllText(bobSetup._pathToLift, "<lift version='0.12'><entry id='dog'><lexical-unit><form lang='en'><text>dog</text></form></lexical-unit></entry></lift>");
			SyncOptions bobOptions = new SyncOptions();
			bobOptions.CheckinDescription = "added 'dog'";
			bobOptions.DoMergeWithOthers = false; // just want a fast checkin
			bobOptions.DoPullFromOthers = false; // just want a fast checkin
			bobSetup.GetManager().SyncNow(bobSetup._projectDescriptor, bobOptions);

			SyncManager sally = SyncManager.FromChildPath(sallyRoot, progress, "sally");
			sally.KnownRepositories.Add(RepositorySource.Create(bobSetup._languageProjectPath, "bob", false));

			//now she modifies a file
			string sallyPathToLift = Path.Combine(sallyRoot, "lexicon/foo.lift");
			File.WriteAllText(sallyPathToLift, "<lift version='0.12'><entry id='cat'><lexical-unit><form lang='en'><text>cat</text></form></lexical-unit></entry></lift>");
			ProjectDescriptor sallyProject = new ProjectDescriptor();
			SyncOptions sallyOptions = new SyncOptions();
			sallyOptions.CheckinDescription = "adding cat";
			sallyOptions.DoPullFromOthers = true;
			sallyOptions.DoMergeWithOthers = true;
			sally.SyncNow(sallyProject, sallyOptions);

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