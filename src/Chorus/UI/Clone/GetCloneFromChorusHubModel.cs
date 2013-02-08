using System;
using System.IO;
using System.Media;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using ChorusHub;
using Palaso.Progress;

namespace Chorus.UI.Clone
{
	///<summary>
	/// A class to handle the data passed in and out of the network respository selection dialog (GetCloneFromNetworkFolderDlg).
	///</summary>
	public class GetCloneFromChorusHubModel
	{
		public string RepositoryName { get; set; }

		///<summary>
		/// Flag indicating success or otherwise of MakeClone call
		///</summary>
		public bool CloneSucceeded { get; set; }

		/// <summary>
		/// After a successful clone, this will have the path to the folder that we just copied to the computer
		/// </summary>
		public string NewlyClonedFolder { get; private set; }

		// Parent folder to use for cloned repository:
		private readonly string _baseFolder;

		/// <summary>
		/// Use this to inject a custom filter, so that the only projects that can be chosen are ones
		/// you application is prepared to open. The delegate is given the path to each mercurial project.
		/// The default filter is simply true, in that it will accept any folder.
		/// </summary>
		public Func<string, bool> ProjectFilter = GetSharedProjectModel.DefaultProjectFilter;

		public GetCloneFromChorusHubModel(string pathToFolderWhichWillContainClonedFolder)
		{
			_baseFolder = pathToFolderWhichWillContainClonedFolder;
		}

		public void MakeClone(IProgress progress)
		{
			 var client = new ChorusHubClient();
			if(client.FindServer()==null)
			{
				progress.WriteError("The Chorus Server is no longer available.");
				CloneSucceeded = false;
				return;
			}

			var targetFolder = Path.Combine(_baseFolder, RepositoryName);
			try
			{
				NewlyClonedFolder= HgRepository.Clone(new HttpRepositoryPath(RepositoryName, client.GetUrl(RepositoryName), false), targetFolder, progress);
				CloneSucceeded = true;
			}
			catch (Exception)
			{
				NewlyClonedFolder = null;
				CloneSucceeded = false;
				throw;
			}
//
//			// These next two calls are fine in how they treat the hgrc update, as a bootstrap clone has no old stuff to fret about.
//			// SetKnownRepositoryAddresses blows away entire 'paths' section, including the "default" one that hg puts in, which we don't really want anyway.
//			repo.SetKnownRepositoryAddresses(new[] { address });
//			// SetIsOneDefaultSyncAddresses adds 'address' to another section (ChorusDefaultRepositories) in hgrc.
//			// 'true' then writes the "address.Name=" (section.Set(address.Name, string.Empty);).
//			// I (RandyR) think this then uses that address.Name as the new 'default' for that particular repo source type.
//			repo.SetIsOneDefaultSyncAddresses(address, true);
//
//
//            if (ActualClonedFolder.Length > 0)
//				CloneSucceeded = true;
//
//            return ActualClonedFolder;
		}

	}
}