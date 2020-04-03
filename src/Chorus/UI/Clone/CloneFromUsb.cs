using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SIL.UsbDrive;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;
using SIL.IO;
using SIL.Progress;

namespace Chorus.UI.Clone
{
	/// <summary>
	/// Use this class to make an initial clone from a USB drive or Internet repository.
	/// Note, most clients can instead use the GetCloneFromUsbDialog in Chorus.exe.
	/// </summary>
	public class CloneFromUsb
	{
		public CloneFromUsb()
		{
			DriveInfoRetriever = new RetrieveUsbDriveInfo();
		}

		/// <summary>
		/// Use this to insert an artificial drive info system for unit tests
		/// </summary>
		public IRetrieveUsbDriveInfo DriveInfoRetriever { get; set; }

		/// <summary>
		/// Use this to inject a custom filter, so that the only projects that can be chosen are ones
		/// you application is prepared to open.  The delegate is given the path to each mercurial project.
		/// </summary>
		public Func<string, bool> ProjectFilter = GetSharedProjectModel.DefaultProjectFilter;


		public bool GetHaveOneOrMoreUsbDrives()
		{
			return DriveInfoRetriever.GetDrives().Count > 0;
		}

		///<summary>
		/// Retrieves from all USB drives all the Mercurial Repositories at the root level
		///</summary>
		///<note>repositories in sub folders are not returned (There were issues writing synchronizing code for that case)</note>
		public IEnumerable<string> GetDirectoriesWithMecurialRepos()
		{
			foreach (var drive in DriveInfoRetriever.GetDrives())
			{
				string[] directories = new string[0];
				try
				{ // this is all complicated because the yield can't be inside the try/catch
					directories = DirectoryHelper.GetSafeDirectories(drive.RootDirectory.FullName);
				}
				catch (Exception error)
				{
					MessageBox.Show(
						string.Format(LocalizationManager.GetString("Messages.UsbError", "Error while looking at USB flash drive.  The drive root was {0}. The error was: {1}"),
									  drive.RootDirectory.FullName, error.Message), LocalizationManager.GetString("Common.Error", "Error"), MessageBoxButtons.OK,
						MessageBoxIcon.Error);
				}
				foreach (var dir in directories)
				{
					if (Directory.Exists(Path.Combine(dir, ".hg")) && ProjectFilter(dir))
					{
						yield return dir;
					}
				}
			}
		}

		///<summary>
		/// Makes a Mercurial clone of a repository from sourcePath to parentDirectoryToPutCloneIn
		///</summary>
		///<param name="sourcePath">Existing Hg repo</param>
		///<param name="parentDirectoryToPutCloneIn">Target folder for new clone</param>
		///<param name="progress">Progress indicator object</param>
		///<returns>Directory that clone was actually placed in (allows for renaming to avoid duplicates)</returns>
		public string MakeClone(string sourcePath, string parentDirectoryToPutCloneIn, IProgress progress)
		{
			return HgHighLevel.MakeCloneFromUsbToLocal(sourcePath,
				Path.Combine(parentDirectoryToPutCloneIn, Path.GetFileName(sourcePath)), progress);
		}

		public ListViewItem CreateListItemFor(string path)
		{
			var projectName = Path.GetFileName(path);
			var item = new ListViewItem(projectName);
			item.Tag = path;
			var last = File.GetLastWriteTime(path);
			item.SubItems.Add(last.ToShortDateString() + " " + last.ToShortTimeString());
			item.ImageIndex = 0;
			if (!IsValidRepository(path))
			{
				item.ToolTipText = string.Format(InvalidRepositoryTemplate, projectName);
				item.ForeColor = DisabledItemForeColor;
				item.ImageIndex = 1;
			}
			else
			{
				var projectWithExistingRepo = GetProjectWithExistingRepo(path);
				if (projectWithExistingRepo != null)
				{
					item.ToolTipText = string.Format(ProjectInUseTemplate, projectWithExistingRepo);
					item.ForeColor = DisabledItemForeColor;
					item.ImageIndex = 1;
				}
				else if (ExistingProjects != null && ExistingProjects.Contains(projectName))
				{
					item.ToolTipText = ProjectWithSameNameExists;
					item.ForeColor = DisabledItemForeColor;
					item.ImageIndex = 2;
				}
				else
				{
					item.ToolTipText = path;
				}
			}
			return item;
		}

		private bool IsValidRepository(string path)
		{
			var repo = new HgRepository(path, new NullProgress());
			return !string.IsNullOrEmpty(repo.Identifier);
		}

		private string GetProjectWithExistingRepo(string path)
		{
			if (ReposInUse == null)
				return null; // if it's in use, we don't know it.
			var repo = new HgRepository(path, new NullProgress());
			string projectWithExistingRepo;
			ReposInUse.TryGetValue(repo.Identifier, out projectWithExistingRepo);
			return projectWithExistingRepo;
		}

		/// <summary>
		/// Set this to the names of existing projects. Items on the USB with the same names will be disabled.
		/// </summary>
		public HashSet<string> ExistingProjects { get; set; }

		/// <summary>
		/// Set this to a dictionary which maps Repo identifiers to the name of the corresponding project.
		/// </summary>
		public Dictionary<string, string> ReposInUse { get; set; }

		/// <summary>
		/// This is a property rather than a constant string to facilitate eventual localization.
		/// It is the tooltip that shows when a repo is disabled because its name matches an existing one.
		/// </summary>
		public static string ProjectWithSameNameExists
		{
			get { return LocalizationManager.GetString("Messages.DuplicateName","A project with this name already exists on this computer.");}
		}

		/// <summary>
		/// This is a property rather than a constant string to facilitate eventual localization.
		/// It is the tooltip that shows when a repo is disabled because it is already in use.
		/// </summary>
		public static string ProjectInUseTemplate
		{
			get { return LocalizationManager.GetString("Messages.RepositoryInUse","The project {0} is already using this repository."); }
		}

		/// <summary>
		/// This is a property rather than a constant string to facilitate eventual localization.
		/// It is the tooltip that shows when a repo is disabled because it is already in use.
		/// </summary>
		public static string InvalidRepositoryTemplate
		{
			get { return LocalizationManager.GetString("Messages.InvalidRepository", "The folder {0} can not be used for Send/Receive. It may be corrupt."); }
		}

		/// <summary>
		/// The color we use to indicate that an item is disabled in list box (also in GetCloneFromChorusHubDialog).
		/// </summary>
		public static Color DisabledItemForeColor
		{
			get { return Color.FromKnownColor(KnownColor.GrayText); }
		}
	}
}