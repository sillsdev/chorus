using System;
using System.Collections.Generic;
using System.IO;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Clone
{
	///<summary>
	/// A class to handle the data passed in and out of the network respository selection dialog (GetCloneFromNetworkFolderDlg).
	///</summary>
	public class GetCloneFromNetworkFolderModel
	{
		/// <summary>
		/// This serves as both the initial folder to present/search, and also the user's last-selected folder:
		/// </summary>
		public string FolderPath { get; set; }

		/// <summary>
		/// The path of the Hg repository selected by the user:
		/// </summary>
		public string UserSelectedRepositoryPath { get; set; }

		private string _actualClonedFolder;
		///<summary>
		/// The path to the local copy of a cloned repository.
		///</summary>
		public string ActualClonedFolder { get { return _actualClonedFolder; } }

		// Parent folder to use for cloned repository:
		private readonly string _baseFolder;

		/// <summary>
		/// Use this to inject a custom filter, so that the only projects that can be chosen are ones
		/// you application is prepared to open.  The delegate is given the path to each mercurial project.
		/// The default filter is simply that there must be a .hg subfolder.
		/// </summary>
		public Func<string, bool> ProjectFilter = path => Directory.Exists(path + Path.DirectorySeparatorChar + @".hg");

		///<summary>
		/// Constructor
		///</summary>
		///<param name="baseFolder"></param>
		public GetCloneFromNetworkFolderModel(string baseFolder)
		{
			_baseFolder = baseFolder;
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
			return HgHighLevel.MakeCloneFromLocalToLocal(sourcePath,
														 Path.Combine(parentDirectoryToPutCloneIn, Path.GetFileName(sourcePath)),
														 true,
														 progress);
		}

		/// <summary>
		/// Returns true if the given folder path represents a repository that this model (or a derivative)
		/// is interested in.
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		public bool IsValidRepository(string folderPath)
		{
			try
			{
				return ProjectFilter(folderPath);
			}
			catch // Typically because we do not have permission to read the folderPath's contents
			{
				return false;
			}
		}

		///<summary>
		/// Recursively searches folders for valid repositories. Depth of recursion can be limited,
		/// in which case the next folders to be searched are passed out via nextLevelFolderPaths.
		///</summary>
		///<param name="folderPaths">Paths of folders to be searched, or null to use our own FolderPath</param>
		///<param name="nextLevelFolderPaths">[out] Folders next in line to be searched after recursion halted</param>
		///<param name="maxRecursionDepth">Maximum number of levels of subfolders to recurse into. Any negative number will cause recursion all the way to the end.</param>
		///<param name="exceptionThrown">Gets set to true if an exception was thrown during this call.</param>
		///<returns>List of folder paths that are valid repositories (according to ProjectFilter member)</returns>
		public List<string> GetRepositoriesAndNextLevelSearchFolders(List<string> folderPaths, out List<string> nextLevelFolderPaths, int maxRecursionDepth, out bool exceptionThrown)
		{
			if (folderPaths == null)
				folderPaths = new List<string> { FolderPath };

			var repositoryPaths = new List<string>();
			nextLevelFolderPaths = new List<string>();
			exceptionThrown = false;

			if (folderPaths.Count == 0)
				return repositoryPaths; // Base case

			foreach (var folderPath in folderPaths)
			{
				if (IsValidRepository(folderPath))
					repositoryPaths.Add(folderPath);
				else
				{
					try
					{
						nextLevelFolderPaths.AddRange(Directory.GetDirectories(folderPath));
					}
					catch // Typically because we do not have permission to read the folderPath's contents
					{
						exceptionThrown = true;
					}
				}
			}

			if (maxRecursionDepth != 0)
				repositoryPaths.AddRange(GetRepositoriesAndNextLevelSearchFolders(nextLevelFolderPaths, out nextLevelFolderPaths, maxRecursionDepth - 1, out exceptionThrown));

			return repositoryPaths;
		}

		/// <summary>
		/// Save the settings in the folder's .hg, creating the folder and settings if necessary.
		/// This is only available if you previously called InitFromProjectPath().  It isn't used
		/// in the GetCloneFromInternet scenario.
		/// </summary>
		public void SaveSettings()
		{
			// Sanity check - the GetCloneFromNetworkFolderDlg should ensure we should never get into this problem:
			if (!IsValidRepository(UserSelectedRepositoryPath))
				return;

			_actualClonedFolder = MakeClone(UserSelectedRepositoryPath, _baseFolder, new StatusProgress());

			var repo = new HgRepository(_actualClonedFolder, new NullProgress());
			var address = RepositoryAddress.Create("Shared NetWork", UserSelectedRepositoryPath);
			// These next two calls are fine in how they treat the hgrc update, as a bootstrap clone has no old stuff to fret about.
			// SetKnownRepositoryAddresses blows away entire 'paths' section, including the "default" one that hg puts in, which we don't really want anyway.
			repo.SetKnownRepositoryAddresses(new[] { address });
			// SetIsOneDefaultSyncAddresses adds 'address' to another section (ChorusDefaultRepositories) in hgrc.
			// 'true' then writes the "address.Name=" (section.Set(address.Name, string.Empty);).
			// I (RandyR) think this then uses that address.Name as the new 'default' for that particular repo source type.
			repo.SetIsOneDefaultSyncAddresses(address, true);
		}
	}
}
