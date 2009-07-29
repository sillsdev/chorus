using System;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.merge
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
		public TempFolder RootFolder;
		public TempFolder ProjectFolder;
		public TempFile UserFile;
		public Synchronizer Synchronizer;
		public RepositoryAddress RepoPath;

		public static RepositoryWithFilesSetup CreateWithLiftFile(string userName)
		{
		   string entriesXml = @"<entry id='one' guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
						<lexical-unit>
							<form lang='a'>
								<text>original</text>
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
			get { return Synchronizer.Repository; }
		}

		public RepositoryWithFilesSetup(string userName, string fileName, string fileContents)
		{
			Progress = new MultiProgress(new IProgress[] { new ConsoleProgress(), _stringProgress });
			RootFolder = new TempFolder("ChorusTest-"+userName);
			ProjectFolder = new TempFolder(RootFolder, "foo project");
			var p = ProjectFolder.Combine(fileName);
			File.WriteAllText(p, fileContents);
			UserFile = TempFile.TrackExisting(p);

			EmptyRepositorySetup.MakeRepositoryForTest(ProjectFolder.Path, userName);
			Init(userName);
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoPushToLocalSources = false;
			Synchronizer.SyncNow(options, Progress);
		}

		public static RepositoryWithFilesSetup CreateByCloning(string userName, RepositoryWithFilesSetup cloneFromUser)
		{
			return new RepositoryWithFilesSetup(userName,cloneFromUser);
		}

		private RepositoryWithFilesSetup(string userName, RepositoryWithFilesSetup cloneFromUser)
		{
			Progress= new MultiProgress(new IProgress[] { new ConsoleProgress(), _stringProgress });
			RootFolder = new TempFolder("ChorusTest-"+userName);
			string pathToProject = RootFolder.Combine(Path.GetFileName(cloneFromUser.ProjectFolder.Path));
			cloneFromUser.Synchronizer.MakeClone(pathToProject, true, Progress);
			ProjectFolder = TempFolder.TrackExisting(RootFolder.Combine("foo project"));
			string pathToOurLiftFile = ProjectFolder.Combine(Path.GetFileName(cloneFromUser.UserFile.Path));
			UserFile = TempFile.TrackExisting(pathToOurLiftFile);

			Init(userName);
		}
		private void Init(string userName)
		{
			ProjectConfiguration = new ProjectFolderConfiguration(ProjectFolder.Path);
			ProjectConfiguration.IncludePatterns.Add(UserFile.Path);
			ProjectConfiguration.FolderPath = ProjectFolder.Path;

			RepoPath = RepositoryAddress.Create(userName, ProjectFolder.Path, false);
			Synchronizer = Synchronizer.FromProjectConfiguration(ProjectConfiguration, new NullProgress());
			Synchronizer.Repository.SetUserNameInIni(userName,Progress);
		}

		public void Dispose()
		{
			UserFile.Dispose();
			ProjectFolder.Dispose();
			RootFolder.Dispose();
		}

		public void ReplaceSomething(string replacement)
		{
			File.WriteAllText(UserFile.Path, File.ReadAllText(UserFile.Path).Replace("original", replacement));
		}

		public void CheckinAndPullAndMerge(RepositoryWithFilesSetup syncWithUser)
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = true;
			options.DoPullFromOthers = true;
			options.DoPushToLocalSources = false;

			options.RepositorySourcesToTry.Add(syncWithUser.RepoPath);
			Synchronizer.SyncNow(options, Progress);
		}


		public void AddAndCheckIn()
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoPushToLocalSources = false;

			Synchronizer.SyncNow(options, Progress);
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

// this would be cool, but we don't yet de-persist the conflicts        public void AssertSingleConflict(Func<IConflict, bool> assertion)

		public void AssertSingleConflictType<TConflict>()
		{
			string xmlConflictFile = XmlLogMergeEventListener.GetXmlConflictFilePath(UserFile.Path);
			Assert.IsTrue(File.Exists(xmlConflictFile), "Conflict file should have been in working set");
			Assert.IsTrue(Synchronizer.Repository.GetFileIsInRepositoryFromFullPath(xmlConflictFile), "Conflict file should have been in repository");

			XmlDocument doc = new XmlDocument();
			doc.Load(xmlConflictFile);
			Assert.AreEqual(1, doc.SafeSelectNodes("conflicts/conflict").Count);

//            var x = typeof (TConflict).GetCustomAttributes(true);
//            var y = x[0] as TypeGuidAttribute;
//            Assert.AreEqual(1, doc.SafeSelectNodes("conflicts/conflict[@typeGuid='{0}']", y.GuidString).Count);

		}

		public HgRepository GetRepository()
		{
			return Synchronizer.Repository;
		}

		public void AssertNoErrorsReported()
		{
			Assert.IsFalse(ProgressString.ToLower().Contains("error"));
		}

		public void AssertFileExists(string relativePath)
		{
			Assert.IsTrue(File.Exists(ProjectFolder.Combine(relativePath)));
		}

		public void AssertFileContents(string relativePath, string expectedContents)
		{
			Assert.AreEqual(expectedContents,File.ReadAllText(ProjectFolder.Combine(relativePath)));
		}
	}


}