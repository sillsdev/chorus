using System;
using System.IO;
using System.Windows.Forms;
using Chorus.Utilities.code;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Misc
{
	public class NetworkFolderSettingsModel
	{
		private HgRepository _repo;

		/// <summary>
		/// Property for the SharedFolder path string
		/// </summary>
		public string SharedFolder
		{
			get; set;
		}

		/// <summary>
		/// Sets the data for the repository path in the hgrc file.
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="path"></param>
		/// <returns>Return false if were not able to create the repository and want to give the user another option.</returns>
		private static bool SetNewSharedNetworkAddress(HgRepository repository, string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			var projectDir = path.EndsWith(RepositoryAddress.ProjectNameVariable)
							? path.Replace(RepositoryAddress.ProjectNameVariable, "")
							: path;
			try
			{
				if (!Directory.Exists(Path.Combine(projectDir, ".hg")))
				{
					if(!Directory.Exists(projectDir))
					{
						var result = MessageBox.Show("Create the folder and make a new repository?", "The directory does not exist.",
													 MessageBoxButtons.OKCancel);
						if (result != DialogResult.OK)
							return false;
						Directory.CreateDirectory(projectDir);
					}
					else
					{
						var result = MessageBox.Show("The repository will be created in " + projectDir + ".", "Create new repository?",
										MessageBoxButtons.OKCancel);
						if (result != DialogResult.OK)
							return false;
					}
				}
				else
				{
					//The user has apparently chosen an existing project, presume they did this on purpose we can't determine here
					//if this is the correct repo.
					path = projectDir;
				}
				string alias = HgRepository.GetAliasFromPath(projectDir);
				repository.SetTheOnlyAddressOfThisType(RepositoryAddress.Create(alias, path));
			}
			catch (Exception e)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e, "There was a problem setting the path to the Network Folder.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Initialize the model with a repo location.
		/// </summary>
		/// <param name="repositoryLocation"></param>
		public void InitFromProjectPath(string repositoryLocation)
		{
			RequireThat.Directory(repositoryLocation).Exists();

			_repo = HgRepository.CreateOrLocate(repositoryLocation, new NullProgress());

			var address = _repo.GetDefaultNetworkAddress<DirectoryRepositorySource>();

			if (address != null)
			{
				if(address.URI.EndsWith(RepositoryAddress.ProjectNameVariable))
				{
					SharedFolder = address.URI.Substring(0, address.URI.Length - RepositoryAddress.ProjectNameVariable.Length);
				}
				else
				{
					SharedFolder = address.URI;
				}
			}
			else
			{
				SharedFolder = String.Empty;
			}
		}

		/// <summary>
		/// Save the settings in the folder's .hg, creating the folder and settings if necessary.
		/// </summary>
		public void SaveSettings()
		{
			if (_repo == null)
			{
				throw new ArgumentException("SaveSettings() only works if you InitFromProjectPath()");
			}

			SetNewSharedNetworkAddress(_repo, Path.Combine(SharedFolder, RepositoryAddress.ProjectNameVariable));
		}
	}
}
