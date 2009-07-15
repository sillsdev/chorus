using System;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.sync;
using Chorus.Utilities;
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
		private ProjectFolderConfiguration _project;
		public IProgress Progress = new ConsoleProgress();// new StringBuilderProgress();
		public TempFolder RootFolder;
		public TempFolder ProjectFolder;
		public TempFile UserFile;
		public RepositoryManager RepoMan;
		public RepositoryPath RepoPath;

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

		public RepositoryWithFilesSetup(string userName, string fileName, string fileContents)
		{
			RootFolder = new TempFolder("ChorusTest-"+userName);
			ProjectFolder = new TempFolder(RootFolder, "foo project");
			var p = ProjectFolder.Combine(fileName);
			File.WriteAllText(p, fileContents);
			UserFile = TempFile.TrackExisting(p);

			RepositoryManager.MakeRepositoryForTest(ProjectFolder.Path, userName);
			Init(userName);
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoPushToLocalSources = false;
			RepoMan.SyncNow(options, Progress);
		}

		public static RepositoryWithFilesSetup CreateByCloning(string userName, RepositoryWithFilesSetup cloneFromUser)
		{
			return new RepositoryWithFilesSetup(userName,cloneFromUser);
		}

		private RepositoryWithFilesSetup(string userName, RepositoryWithFilesSetup cloneFromUser)
		{
			RootFolder = new TempFolder("ChorusTest-"+userName);
			string pathToProject = RootFolder.Combine(Path.GetFileName(cloneFromUser.ProjectFolder.Path));
			cloneFromUser.RepoMan.MakeClone(pathToProject, true, Progress);
			ProjectFolder = TempFolder.TrackExisting(RootFolder.Combine("foo project"));
			string pathToOurLiftFile = ProjectFolder.Combine(Path.GetFileName(cloneFromUser.UserFile.Path));
			UserFile = TempFile.TrackExisting(pathToOurLiftFile);

			Init(userName);
		}
		private void Init(string userName)
		{
			_project = new ProjectFolderConfiguration(ProjectFolder.Path);
			_project.IncludePatterns.Add(UserFile.Path);
			_project.FolderPath = ProjectFolder.Path;

			RepoPath = RepositoryPath.Create(ProjectFolder.Path, userName, false);
			RepoMan = RepositoryManager.FromRootOrChildFolder(_project);
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
			RepoMan.SyncNow(options, Progress);
		}


		public void Checkin()
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoPushToLocalSources = false;

			RepoMan.SyncNow(options, Progress);
		}

		public void WriteIniContents(string s)
		{
			var p = Path.Combine(Path.Combine(_project.FolderPath, ".hg"), "hgrc");
			File.WriteAllText(p, s);
		}

		public void EnsureNoHgrcExists()
		{
			var p = Path.Combine(Path.Combine(_project.FolderPath, ".hg"), "hgrc");
			if(File.Exists(p))
				File.Delete(p);
		}

		public void AssertSingleHead()
		{
			var actual = RepoMan.GetRepository(Progress).GetHeads().Count;
			Assert.AreEqual(1, actual, "There should be on only one head, but there are "+actual.ToString());
		}

// this would be cool, but we don't yet de-persist the conflicts        public void AssertSingleConflict(Func<IConflict, bool> assertion)

		public void AssertSingleConflictType<TConflict>()
		{
			string xmlConflictFile = XmlLogMergeEventListener.GetXmlConflictFilePath(UserFile.Path);
			Assert.IsTrue(File.Exists(xmlConflictFile), "Conflict file should have been in working set");
			Assert.IsTrue(RepoMan.GetFileExistsInRepo(xmlConflictFile), "Conflict file should have been in repository");

			XmlDocument doc = new XmlDocument();
			doc.Load(xmlConflictFile);
			Assert.AreEqual(1, doc.SafeSelectNodes("conflicts/conflict").Count);
			Assert.AreEqual(1, doc.SafeSelectNodes("conflicts/conflict").Count);
			var x = typeof (TConflict).GetCustomAttributes(true);
			var y = x[0] as TypeGuidAttribute;
			Assert.AreEqual(1, doc.SafeSelectNodes("conflicts/conflict[@typeGuid='{0}']", y.GuidString).Count);

		}
	}


}