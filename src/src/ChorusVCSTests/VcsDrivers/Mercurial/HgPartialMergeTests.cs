using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HgPartialMergeTests
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


		[Test]
		public void GetVersion()
		{
			ExecutionResult result =  WrapShellCallRunner.Run("hg version");
			Console.WriteLine(result.StandardOutput);
			Console.WriteLine(result.StandardError);
		}

		[Test]
		public void LoneUserCanModifyAndCheckIn()
		{
			string filePath;
			HgPartialMerge bob = MakeBobAndHisDataFileAndCheckItIn(out filePath);
			AssertLineOfFile(filePath, 7, "adj");
			ChangeTestFileAndCheckin(bob, "data.txt", 7, "noun");
			AssertLineOfFile(filePath, 7, "noun");

			//now delete it and make sure the system noticed
			File.Delete(filePath);
			List<string> files = bob.GetChangedFiles();
			Assert.AreEqual(1,files.Count);
			Assert.AreEqual("data.txt", files[0]);

			//now get it back
			bob.Update();
			AssertLineOfFile(filePath, 7, "noun");

		}

		[Test]
		public void NoConflictSharing()
		{
			string filePath;
			HgPartialMerge bob = MakeBobAndHisDataFileAndCheckItIn(out filePath);
			HgPartialMerge sally =  HgPartialMerge.CreateNewByCloning(bob, _pathToTestRoot, "sally");
			AssertTestFile(bob, 7, "adj");
			AssertTestFile(sally, 7, "adj");

			//here, sally makes a change but bob makes none, not even a non-conflicting one
			ChangeTestFileAndCheckin(sally, 7, "noun");
			sally.Sync(bob); // get her work in locally
			ChangeTestFileAndCheckin(bob, 3, "I (bob) think it should be this");
			bob.Sync(sally); // now bob gets it
			AssertTestFile(bob, 7, "noun");
			AssertTestFile(sally, 7, "noun");

			//here, sally and bob make non-conflicting changes

			ChangeTestFileAndCheckin(sally, 7, "prep");
			sally.Sync(bob); // get her work in locally
			ChangeTestFileAndCheckin(bob, 3, "I (bob) think it should be this");
			bob.Sync(sally); // now bob gets it
			AssertTestFile(bob, 7, "prep");
			AssertTestFile(bob, 3, "I (bob) think it should be this");
			AssertTestFile(sally, 7, "prep");
			//she hasn"t seen his work yet

			sally.Sync(bob);
			AssertTestFile(sally,  3, "I (bob) think it should be this");
			bob.Sync(sally);
			AssertTestFile(bob, 7, "prep");
		}

		[Test]
		public void SimpleConflict()
		{
			string filePath;
			HgPartialMerge bob = MakeBobAndHisDataFileAndCheckItIn(out filePath);
			HgPartialMerge sally =  HgPartialMerge.CreateNewByCloning(bob, _pathToTestRoot, "sally");

			ChangeTestFileAndCheckin(bob, 7, "noun");
			ChangeTestFileAndCheckin(sally, 7, "verb");
			ChangeTestFileAndCheckin(sally, 5, "Sally was here");
			bob.Sync(sally);
			AssertTestFile(bob, 7, "noun");
			AssertTestFile(bob, 5, "Sally was here");
			sally.Sync(bob);
			AssertTestFile(bob, 7, "noun");
			AssertTestFile(bob, 5, "Sally was here");
			AssertTestFile(sally, 5, "Sally was here");
			AssertTestFile(sally, 7, "verb");
		}

		/// <summary>
		/// The idea here is, what happens when you encounter a stub left by someone for you,
		/// but you have conflicts with *it*?
		/// </summary>
		[Test, Ignore("Current algorithm can't handle this")]
		public void ConflictWithStub()
		{
			string filePath;
			HgPartialMerge bob = MakeBobAndHisDataFileAndCheckItIn(out filePath);
			HgPartialMerge sally = HgPartialMerge.CreateNewByCloning(bob, _pathToTestRoot, "sally");

			ChangeTestFileAndCheckin(bob, 7, "noun");
			ChangeTestFileAndCheckin(sally, 7, "verb");
			//ok, the key thing about this change is that it's not *yet* a conflict with sally,
			//but it's also not in their common ancestor
			ChangeTestFileAndCheckin(bob, 5, "b");
			bob.Sync(sally);

			//at this point, there should be stub waiting for sally in bob's repo, and it will
			//contain bob's change to line 5. Meanwhile, sally also changes line 5

			ChangeTestFileAndCheckin(sally, 5, "s");

			//And now, the merge with the stub *should* do something new; it should simply take
			//sally's side in this conflict.
			sally.Sync(bob);
			AssertTestFile(sally, 5, "s");
			AssertTestFile(sally, 7, "verb");
			bob.Sync(sally);
			AssertTestFile(bob, 7, "noun");
			AssertTestFile(bob, 5, "b");
		}


		[Test]
		public void CanGetContentsRequestedRevisionOfFile()
		{
			string filePath;
			HgPartialMerge bob = MakeBobAndHisDataFileAndCheckItIn(out filePath);
			ChangeTestFileAndCheckin(bob, "data.txt", 7, "noun");
			AssertLineOfFile(filePath, 7, "noun");

			using (TempFile f = new TempFile())
			{
				bob.GetRevisionOfFile("data.txt", "1", f.Path);
				AssertLineOfFile(f.Path, 7, "adj"); //the original value
			}
		}

		private void AssertTestFile(HgPartialMerge repo, int line, string expectedContents)
		{
			Debug.WriteLine("Checking that " + repo.UserName + " has '" + expectedContents + "' in line " + line);
			AssertLineOfFile(repo.GetFilePath("data.txt"), line, expectedContents);
		}

		private HgPartialMerge MakeBobAndHisDataFileAndCheckItIn(out string filePath)
		{
			HgPartialMerge bob = InitializeFirstPerson("bob");
			filePath = bob.GetFilePath("data.txt");
			File.WriteAllText(filePath,
							  @"This is a test file
Bob is known to make changes in this section
foo
Sally is known to make them in this section
blah
And the tend to have conflicting ideas in this secion
adj
This is the end of the test file
");

/*@"This is a test file
I am here to provide Sync
adj
and final Sync data
the end");
			*/
			bob.AddAndCheckinFile(filePath);
			return bob;
		}

		private string PathToUserDirectory(string userName)
		{
			return Path.Combine(_pathToTestRoot, userName);
		}

		public void AssertLineOfFile(string filePath, int lineNumber1Based, string shouldEqual)
		{
			string[] lines = File.ReadAllLines(filePath);
			Assert.AreEqual(shouldEqual, lines[lineNumber1Based-1]);
		}

		private HgPartialMerge InitializeFirstPerson(string userName)
		{
			return  HgPartialMerge.CreateNewDirectoryAndRepository(_pathToTestRoot, userName);
		}

		public void ChangeTestFileAndCheckin(HgPartialMerge repo, int lineNumber1Based, string newText)
		{
			ChangeTestFileAndCheckin(repo, "data.txt", lineNumber1Based, newText);
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