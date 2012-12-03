using System;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Code;
using Palaso.Progress;

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
		/// Property for setting the MessageBoxService (to aid unit testing.)
		/// </summary>
		public IMessageBoxService MessageBoxService { get; set; }

		/// <summary>
		/// Sets the data for the repository path in the hgrc file.
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="path"></param>
		private void SetNewSharedNetworkAddress(HgRepository repository, string path)
		{
			//if the path is empty, or only contains the ProjectNameVariable then this isn't a valid path so ignore it.
			if (string.IsNullOrEmpty(path) || path.Equals(RepositoryAddress.ProjectNameVariable))
				return;
			var projectDir = path.EndsWith(RepositoryAddress.ProjectNameVariable)
							? path.Replace(RepositoryAddress.ProjectNameVariable, "")
							: path;
			var projectName = Path.GetFileName(repository.PathToRepo);

			// If the user picked the .hg folder itsself, be kind and try and use the parent folder
			if(projectDir.EndsWith(".hg" + Path.DirectorySeparatorChar))
			{
				projectDir = path = path.Substring(0, path.LastIndexOf(".hg"));
			}

			string projectInFolder = "";
			if(!Directory.Exists(projectDir))
			{
				var result = MessageBoxService.Show("Create the folder and make a new repository?", "The directory does not exist.",
												MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (result != DialogResult.OK)
					return;
				Directory.CreateDirectory(projectDir);
				projectInFolder = path.Replace(RepositoryAddress.ProjectNameVariable, projectName);
			}
			else
			{
				var isProjFolderSelected = projectName != null && projectDir.EndsWith(projectName + Path.DirectorySeparatorChar);
				if (!isProjFolderSelected && !Directory.Exists(Path.Combine(projectDir, ".hg")))
				{
					if (projectName != null)
					{
						projectInFolder = path.Replace(RepositoryAddress.ProjectNameVariable, projectName);
					}
					if (string.IsNullOrEmpty(projectInFolder) || !Directory.Exists(projectInFolder))
					{
						var result = MessageBoxService.Show("The repository will be created in " + projectDir + ".",
															"Create new repository?",
															MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
						if (result != DialogResult.OK)
							return;
						Directory.CreateDirectory(projectInFolder);
					}
				}
				else
				{
					projectInFolder = projectDir;
				}
			}
			try
			{
				// This section will test for any existing repo in the selected folder, if it is a correct repo it will save the selection
				// otherwise it complains
				if (Directory.Exists(Path.Combine(projectInFolder, ".hg")))
				{
					var root = HgRepository.CreateOrUseExisting(projectInFolder, new NullProgress());
					if (repository.Identifier == root.Identifier)
					{
						path = projectInFolder;
					}
					else
					{
						MessageBoxService.Show("You selected a repository for a different project. The Shared Network Folder setting was not saved.", "Unrelated repository selected.",
											   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}

				}
				else
				{
					//The user has apparently chosen an existing project which does not match their project repo, move up a folder
					path = projectInFolder;
				}
				string alias = HgRepository.GetAliasFromPath(projectDir);
				repository.SetTheOnlyAddressOfThisType(RepositoryAddress.Create(alias, path));
			}
			catch (Exception e)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e, "There was a problem setting the path to the Network Folder.");
			}
		}

		/// <summary>
		/// Initialize the model with a repo location.
		/// </summary>
		/// <param name="repositoryLocation"></param>
		public void InitFromProjectPath(string repositoryLocation)
		{
			RequireThat.Directory(repositoryLocation).Exists();

			_repo = HgRepository.CreateOrUseExisting(repositoryLocation, new NullProgress());

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

		public interface IMessageBoxService
		{
			DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon);
		}
	}
}
