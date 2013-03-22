using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Code;
using Palaso.Progress;
using Palaso.IO;

namespace Chorus.UI.Clone
{
	///<summary>
	/// A class to handle the data passed in and out of the network respository selection dialog (GetCloneFromNetworkFolderDlg).
	///</summary>
	public class GetCloneFromNetworkFolderModel
	{
		// This is a win32 error code, it can indicate several failures but here it is probably a lack of connections or
		// an unauthorized exception that surprised windows when we were working with a network folder
		private const Int32 ErrorNetNameDeleted = 64;

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
		/// Flag indicating success or otherwise of MakeClone call
		///</summary>
		public bool CloneSucceeded { get; set; }

		///<summary>
		/// The path to the local copy of a cloned repository.
		///</summary>
		public string ActualClonedFolder { get { return _actualClonedFolder; } }

		// Parent folder to use for cloned repository:
		private readonly string _baseFolder;

		/// <summary>
		/// Use this to inject a custom filter, so that the only projects that can be chosen are ones
		/// you application is prepared to open. The delegate is given the path to each mercurial project.
		/// The default filter is simply true, in that it will accept any folder.
		/// </summary>
		public Func<string, bool> ProjectFilter = GetSharedProjectModel.DefaultProjectFilter;

		///<summary>
		/// Constructor
		///</summary>
		///<param name="baseFolder">Parent folder to use for cloning Mercurial repository into.</param>
		public GetCloneFromNetworkFolderModel(string baseFolder)
		{
			_baseFolder = baseFolder;
		}

		///<summary>
		/// Makes a Mercurial clone of a repository, reading from the user-selected path, writing to the
		/// folder passed into our constructor.
		///</summary>
		///<param name="progress">Progress indicator object</param>
		///<returns>Directory that clone was actually placed in (allows for renaming to avoid duplicates)</returns>
		public string MakeClone(IProgress progress)
		{
			var targetPath = Path.Combine(_baseFolder, Path.GetFileName(UserSelectedRepositoryPath));
			_actualClonedFolder = HgHighLevel.MakeCloneFromLocalToLocal(UserSelectedRepositoryPath,
														 targetPath,
														 true,
														 progress);

			var repo = new HgRepository(_actualClonedFolder, new NullProgress());
			var address = RepositoryAddress.Create("Shared Network", UserSelectedRepositoryPath);

			// These next two calls are fine in how they treat the hgrc update, as a bootstrap clone has no old stuff to fret about.
			// SetKnownRepositoryAddresses blows away entire 'paths' section, including the "default" one that hg puts in, which we don't really want anyway.
			repo.SetKnownRepositoryAddresses(new[] { address });
			// SetIsOneDefaultSyncAddresses adds 'address' to another section (ChorusDefaultRepositories) in hgrc.
			// 'true' then writes the "address.Name=" (section.Set(address.Name, string.Empty);).
			// I (RandyR) think this then uses that address.Name as the new 'default' for that particular repo source type.
			repo.SetIsOneDefaultSyncAddresses(address, true);

			// TODO: is there really no better way to detect success other than via the returned clone path?
			if (_actualClonedFolder.Length > 0)
				CloneSucceeded = true;

			return _actualClonedFolder;
		}

		///<summary>
		/// Searches folders for valid repositories. The subfolders of the given list are are passed out via nextLevelFolderPaths.
		///</summary>
		///<param name="folderPaths">Paths of folders to be searched</param>
		///<param name="nextLevelFolderPaths">[out] Folders next in line to be searched</param>
		///<returns>List of folder paths that are valid repositories (according to ProjectFilter member)</returns>
		public HashSet<string> GetRepositoriesAndNextLevelSearchFolders(List<string> folderPaths, out List<string> nextLevelFolderPaths)
		{
			Guard.AgainstNull(folderPaths, "folderPaths");

			var repositoryPaths = new HashSet<string>();
			nextLevelFolderPaths = new List<string>();

			if (folderPaths.Count == 0)
				return repositoryPaths; // Nothing to do

			if (folderPaths.Count == 1)
			{
				var singleFolder = folderPaths.First();
				if (singleFolder.EndsWith(".hg"))
				{
					var folderParent = Directory.GetParent(singleFolder).FullName;
					if (ProjectFilter(folderParent))
					{
						repositoryPaths.Add(folderParent);
						return repositoryPaths;
					}
				}
			}

			foreach (var folderPath in folderPaths)
			{
				if (Directory.Exists(Path.Combine(folderPath, ".hg")) && ProjectFilter(folderPath))
				{
					repositoryPaths.Add(folderPath);
				}
				else
				{
					try
					{
						nextLevelFolderPaths.AddRange(DirectoryUtilities.GetSafeDirectories(folderPath));
					}
					catch (UnauthorizedAccessException)
					{
						// We don't care if we can't read a folder; we'll just go on to the next one.
					}
					catch(IOException io)
					{
						var errorCode = Marshal.GetHRForException(io) & 0xFFFF;
						if (errorCode == ErrorNetNameDeleted)
						{
							//This error could mean signal insufficient network connections so we will try again later
							nextLevelFolderPaths.Add(folderPath);
						}
						//Otherwise we can't read the folder for some other reason, if we can't read it, then we can't use
						//a repository within it
					}
				}
			}

			return repositoryPaths;
		}
	}
}