using System;
using System.IO;
using System.Xml;
using Chorus.merge.xml.generic;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.TestUtilities;
using SIL.Xml;

namespace LibChorus.TestUtilities
{
	/// <summary>
	/// Provides temporary directories,files, and repositories.  Provides operations on them, to simulate a user.
	/// </summary>
	/// <remarks>
	/// Any test doing high-level testing such that this is useful should expressly Not be interested in details of the files,
	/// so no methods are provided to control the contents of the files.
	/// </remarks>
	public class RepositoryWithFilesSetup :IDisposable
	{
		public ProjectFolderConfiguration ProjectConfiguration;
		private StringBuilderProgress _stringProgress = new StringBuilderProgress();
		public IProgress Progress;
		public TemporaryFolder RootFolder;
		public TemporaryFolder ProjectFolder;
		public TempFile UserFile;
		public Synchronizer Synchronizer;
		public RepositoryAddress RepoPath;
		private HgRepository _repository;

		public static RepositoryWithFilesSetup CreateWithLiftFile(string userName)
		{
		   string entriesXml = @"<entry id='one' guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
						<lexical-unit>
							<form lang='a'>
								<text>original</text>
							</form>
							<form lang='b'>
								<text>other</text>
							</form>
						</lexical-unit>
					 </entry>";
		   string liftContents = string.Format("<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>", "0.00", entriesXml);
			return new RepositoryWithFilesSetup(userName, "test.lift", liftContents);
		}
		public string ProgressString
		{
			get { return _stringProgress.Text; }
		}
		public HgRepository Repository
		{
			get { return _repository; }
		}

		public RepositoryWithFilesSetup(string userName, string fileName, string fileContents)
		{
			Progress = new MultiProgress(new IProgress[] { new ConsoleProgress(), _stringProgress });
			RootFolder = new TemporaryFolder("ChorusTest-" + userName + "-" + Guid.NewGuid());
			ProjectFolder = new TemporaryFolder(RootFolder, "foo project");
			Console.WriteLine("TestRepository Created: {0}", RootFolder.Path);
			var p = ProjectFolder.Combine(fileName);
			File.WriteAllText(p, fileContents);
			UserFile = TempFile.TrackExisting(p);

			RepositorySetup.MakeRepositoryForTest(ProjectFolder.Path, userName,Progress);
			Init(userName);
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoSendToOthers = false;
			Synchronizer.SyncNow(options);
		}

		public static RepositoryWithFilesSetup CreateByCloning(string userName, RepositoryWithFilesSetup cloneFromUser)
		{
			return new RepositoryWithFilesSetup(userName,cloneFromUser);
		}

		private RepositoryWithFilesSetup(string userName, RepositoryWithFilesSetup cloneFromUser)
		{
			Progress= new MultiProgress(new IProgress[] { new ConsoleProgress(), _stringProgress });
			RootFolder = new TemporaryFolder("ChorusTest-" + userName + "-" + Guid.NewGuid());
			Console.WriteLine("TestRepository Cloned: {0}", RootFolder.Path);
			string pathToProject = RootFolder.Combine(Path.GetFileName(cloneFromUser.ProjectFolder.Path));
			//cloneFromUser.Synchronizer.MakeClone(pathToProject, true);
			HgHighLevel.MakeCloneFromUsbToLocal(cloneFromUser.Repository.PathToRepo, pathToProject, Progress);

			ProjectFolder = TemporaryFolder.TrackExisting(RootFolder.Combine("foo project"));
			string pathToOurLiftFile = ProjectFolder.Combine(Path.GetFileName(cloneFromUser.UserFile.Path));
			UserFile = TempFile.TrackExisting(pathToOurLiftFile);

			Init(userName);
		}
		private void Init(string userName)
		{
			ProjectConfiguration = new ProjectFolderConfiguration(ProjectFolder.Path);
			ProjectConfiguration.IncludePatterns.Add(UserFile.Path);
			ProjectConfiguration.FolderPath = ProjectFolder.Path;
			_repository = new HgRepository(ProjectFolder.Path,Progress);

			RepoPath = RepositoryAddress.Create(userName, ProjectFolder.Path, false);
			Synchronizer = Synchronizer.FromProjectConfiguration(ProjectConfiguration, Progress);
			Synchronizer.Repository.SetUserNameInIni(userName,Progress);
		}

		public void Dispose()
		{
			if (Repository != null)
			{
				Assert.That(Repository.GetHasLocks(), Is.False, "A lock was left over, after the test.");
			}

			if (DoNotDispose)
			{
				Console.WriteLine("TestRepository not deleted in {0}", RootFolder.Path);
			}
			else
			{
				Console.WriteLine("TestRepository deleted {0}", RootFolder.Path);
				UserFile.Dispose();
				ProjectFolder.Dispose();
				RootFolder.Dispose();
			}
		}

		public bool DoNotDispose { get; set; }

		public void ReplaceSomething(string replacement)
		{
			File.WriteAllText(UserFile.Path, File.ReadAllText(UserFile.Path).Replace("original", replacement));
		}

		/// <summary>
		/// replaces all occurrences of "other" with replacement
		/// </summary>
		/// <param name="replacement"></param>
		public void ReplaceSomethingElse(string replacement)
		{
			File.WriteAllText(UserFile.Path, File.ReadAllText(UserFile.Path).Replace("other", replacement));
		}

		public void WriteNewContentsToTestFile(string replacement)
		{
			File.WriteAllText(UserFile.Path, replacement);
		}

		public SyncResults SyncWithOptions(SyncOptions options)
		{
			return SyncWithOptions(options, Synchronizer);
		}

		public SyncResults SyncWithOptions(SyncOptions options, Synchronizer synchronizer)
		{
			return synchronizer.SyncNow(options);
		}

		public SyncResults CheckinAndPullAndMerge(RepositoryWithFilesSetup syncWithUser)
		{
			var options = new SyncOptions
							{
								DoMergeWithOthers = true,
								DoPullFromOthers = true,
								DoSendToOthers = false
							};

			options.RepositorySourcesToTry.Add(syncWithUser.RepoPath);

			return SyncWithOptions(options);
		}


		public void AddAndCheckIn()
		{
			var options = new SyncOptions
							{
								DoMergeWithOthers = false,
								DoPullFromOthers = false,
								DoSendToOthers = false
							};

			SyncWithOptions(options);
		}

		public void WriteIniContents(string s)
		{
			var p = Path.Combine(Path.Combine(ProjectConfiguration.FolderPath, ".hg"), "hgrc");
			File.WriteAllText(p, s);
		}

		public void EnsureNoHgrcExists()
		{
			var p = Path.Combine(Path.Combine(ProjectConfiguration.FolderPath, ".hg"), "hgrc");
			if(File.Exists(p))
				File.Delete(p);
		}

		public void AssertSingleHead()
		{
			var actual = Synchronizer.Repository.GetHeads().Count;
			Assert.AreEqual(1, actual, "There should be on only one head, but there are "+actual.ToString());
		}

		public void AssertHeadCount(int count)
		{
			var actual = Synchronizer.Repository.GetHeads().Count;
			Assert.AreEqual(count, actual, "Wrong number of heads");
		}


		public void AssertSingleConflictType<TConflict>()
		{
			string cmlFile = ChorusNotesMergeEventListener.GetChorusNotesFilePath(UserFile.Path);
			Assert.That(cmlFile, Does.Exist, "ChorusNotes file should have been in working set");
			Assert.That(Synchronizer.Repository.GetFileIsInRepositoryFromFullPath(cmlFile), Is.True, "ChorusNotes file should have been in repository");

			XmlDocument doc = new XmlDocument();
			doc.Load(cmlFile);
			Assert.AreEqual(1, doc.SafeSelectNodes("notes/annotation").Count);

		}

		public HgRepository GetRepository()
		{
			return Synchronizer.Repository;
		}

		public void AssertNoErrorsReported()
		{
			Assert.That(ProgressString, Does.Not.Contain("error").IgnoreCase);
		}

		public void AssertFileExists(string relativePath)
		{
			Assert.That(ProjectFolder.Combine(relativePath), Does.Exist);
		}

		public void AssertFileContents(string relativePath, string expectedContents)
		{
			Assert.AreEqual(expectedContents,File.ReadAllText(ProjectFolder.Combine(relativePath)));
		}

		/// <summary>
		/// Obviously, don't leave this in a unit test... it's only for debugging
		/// </summary>
		public void ShowInTortoise()
		{
			var start = new System.Diagnostics.ProcessStartInfo("hgtk", "log");
			start.WorkingDirectory = ProjectFolder.Path;
			System.Diagnostics.Process.Start(start);
		}
	}


}