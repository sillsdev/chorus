using System;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.sync;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.merge
{
	[TestFixture]
	public class ConflictFileTests
	{
		[Test]
		public void ConflictFileIsCheckedIn()
		{
			using (UserWithFiles bob = new UserWithFiles("bob"))
			{
				using (UserWithFiles sally = new UserWithFiles("sally", bob))
				{
					bob.ReplaceSomething("bob");
					bob.Checkin();
					sally.ReplaceSomething("sally");
					sally.Pull(bob);

					string xmlConflictFile = XmlLogMergeEventListener.GetXmlConflictFilePath(sally._liftFile.Path);
					Assert.IsTrue(File.Exists(xmlConflictFile), "Conflict file should have been in working set");
					Assert.IsTrue(sally.Repo.GetFileExistsInRepo(xmlConflictFile),"Conflict file should have been in repository");

				}
			}
		}
	}

	/// <summary>
	/// Provides temporary directories,files, and repositories.  Provides operations on them, to simulate a user.
	/// </summary>
	/// <remarks>
	/// Any test doing high-level testing such that this is useful should expressly Not be interested in details of the files,
	/// so no methods are provided to control the contents of the files.
	/// </remarks>
	internal class UserWithFiles :IDisposable
	{
		private ProjectFolderConfiguration _project;
		private StringBuilderProgress _progress = new StringBuilderProgress();
		public TempFolder _rootFolder;
		public TempFolder _projectFolder;
		public TempLiftFile _liftFile;
		public RepositoryManager Repo;
		public RepositorySource RepoSource;


		public UserWithFiles(string userName)
		{
			string entriesXml = @"<entry id='one'>
						<lexical-unit>
							<form lang='a'>
								<text>original</text>
							</form>
						</lexical-unit>
					 </entry>";

			_rootFolder = new TempFolder("ChorusTest-"+userName);
			_projectFolder = new TempFolder(_rootFolder, "foo project");
			_liftFile = new TempLiftFile(_projectFolder, entriesXml, "0.00");

			RepositoryManager.MakeRepositoryForTest(_projectFolder.Path, userName);
			Init(userName);
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoPushToLocalSources = false;
			Repo.SyncNow(options, _progress);
		}

		private void Init(string userName)
		{
			_project = new ProjectFolderConfiguration(_projectFolder.Path);
			_project.IncludePatterns.Add(_liftFile.Path);
			_project.FolderPath = _projectFolder.Path;

			RepoSource = RepositorySource.Create(_rootFolder.Path, userName, false);//notice, does not include the actual project folder that is the repo itself
			Repo = RepositoryManager.FromRootOrChildFolder(_project);
		}

		public UserWithFiles(string userName, UserWithFiles cloneFromUser)
		{
			_rootFolder = new TempFolder("ChorusTest-"+userName);
			string pathToProject = _rootFolder.Combine(Path.GetFileName(cloneFromUser._projectFolder.Path));
			 cloneFromUser.Repo.MakeClone(pathToProject, true, _progress);
			_projectFolder = TempFolder.TrackExisting(_rootFolder.Combine("foo project"));
			string pathToOurLiftFile = _projectFolder.Combine(Path.GetFileName(cloneFromUser._liftFile.Path));
			_liftFile = TempLiftFile.TrackExisting(pathToOurLiftFile);

			Init(userName);
		}

		public void Dispose()
		{
			_liftFile.Dispose();
			_projectFolder.Dispose();
			_rootFolder.Dispose();
		}

		public void ReplaceSomething(string replacement)
		{
			File.WriteAllText(_liftFile.Path, File.ReadAllText(_liftFile.Path).Replace("original", replacement));
		}

		public void Pull(UserWithFiles syncWithUser)
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = true;
			options.DoPullFromOthers = true;
			options.DoPushToLocalSources = false;

			options.RepositorySourcesToTry.Add(syncWithUser.RepoSource);
			Repo.SyncNow(options, _progress);
		}


		public void Checkin()
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoPushToLocalSources = false;

			Repo.SyncNow(options, _progress);
		}
	}
}