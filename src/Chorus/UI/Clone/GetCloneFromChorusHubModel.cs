using System;
using System.Collections.Generic;
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
		/// your application is prepared to open. The usual method of passing a filter delegate doesn't
		/// work with ChorusHub's cross-process communication, so our ProjectFilter is a string which
		/// gets parsed by the server to determine whether a given mercurial project can be chosen or not.
		/// The default filter is simply empty string, which returns any folder name.
		/// </summary>
		/// <example>Set this to "fileExtension=.lift" to get LIFT repos, but not Bloom ones, for instance.
		/// The server looks in the project's .hg/store/data folder for a file ending in .lift.i</example>
		public string ProjectFilter = string.Empty;

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
		}

		/// <summary>
		/// Set this to the names of existing projects. Items on the Hub with the same names will be disabled.
		/// </summary>
		public HashSet<string> ExistingProjects { get; set; }

		/// <summary>
		/// Set this to the IDs of existing projects. Items on the Hub with the same IDs will be disabled.
		/// </summary>
		public Dictionary<string, string> ExistingRepositoryIdentifiers { get; set; }

	}
}