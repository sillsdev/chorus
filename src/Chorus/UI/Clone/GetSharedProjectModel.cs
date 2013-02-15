using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Code;
using Palaso.Progress;

namespace Chorus.UI.Clone
{
	/// <summary>
	/// A model that lets a user select a source repository to clone locally.
	/// </summary>
	public class GetSharedProjectModel
	{
		public ExtantRepoSource RepositorySource { get; set; }

		/// <summary>
		/// Get a teammate's shared project from the specified source.
		/// </summary>
		/// <returns>
		/// A CloneResult that provides the clone results (e.g., success or failure) and the actual clone location (null if not created).
		/// </returns>
		public CloneResult GetSharedProjectUsing(Form parent, Dictionary<string, string> existingRepositories, HashSet<string> existingProjectNames, Func<string, bool> projectFilter, string baseProjectDir, string preferredClonedFolderName)
		{
			Guard.AgainstNull(parent, "parent");
			Guard.AgainstNull(existingRepositories, "existingRepositories");
			Guard.Against(string.IsNullOrEmpty(baseProjectDir), "'baseProjectDir' is null or an empty string.");
			if (preferredClonedFolderName == string.Empty)
				preferredClonedFolderName = null;

			// "existingRepositoryIdentifiers" is currently not used, but the expectation is that the various models/views could use it how they see fit.
			// "Seeing fit' may mean to warn the user they already have some repository, or as a filter to not show ones that already exist.
			// Waht to do with the list of extant repos is left up to a view+model pair.

			// Select basic source type.
			using (var getSharedProjectDlg = new GetSharedProjectDlg())
			{
				getSharedProjectDlg.InitFromModel(this);
				getSharedProjectDlg.ShowDialog(parent);
				if (getSharedProjectDlg.DialogResult != DialogResult.OK)
				{
					return new CloneResult(null, CloneStatus.NotCreated);
				}
			}

			// Make clone from some source.
			string actualCloneLocation = null;
			var cloneStatus = CloneStatus.NotCreated;
			switch (RepositorySource)
			{
				case ExtantRepoSource.Internet:
					var cloneFromInternetModel = new GetCloneFromInternetModel(baseProjectDir)
						{
							LocalFolderName = preferredClonedFolderName
						};
					using (var cloneFromInternetDialog = new GetCloneFromInternetDialog(cloneFromInternetModel))
					{
						switch (cloneFromInternetDialog.ShowDialog(parent))
						{
							default:
								cloneStatus = CloneStatus.NotCreated;
								break;
							case DialogResult.Cancel:
								cloneStatus = CloneStatus.Cancelled;
								break;
							case DialogResult.OK:
								actualCloneLocation = cloneFromInternetDialog.PathToNewlyClonedFolder;
								cloneStatus = CloneStatus.Created;
								break;
						}
					}
					break;

				case ExtantRepoSource.LocalNetwork:
					var cloneFromNetworkFolderModel = new GetCloneFromNetworkFolderModel(baseProjectDir)
						{
							ProjectFilter = projectFilter ?? DefaultProjectFilter
						};

					using (var cloneFromNetworkFolderDlg = new GetCloneFromNetworkFolderDlg())
					{
						// We don't have a GetCloneFromNetworkFolderDlg constructor that takes the model because
						// it would inexplicably mess up Visual Studio's designer view of the dialog:
						cloneFromNetworkFolderDlg.LoadFromModel(cloneFromNetworkFolderModel);

						switch (cloneFromNetworkFolderDlg.ShowDialog(parent))
						{
							default:
								cloneStatus = CloneStatus.NotCreated;
								break;
							case DialogResult.Cancel:
								cloneStatus = CloneStatus.Cancelled;
								break;
							case DialogResult.OK:
								actualCloneLocation = cloneFromNetworkFolderDlg.PathToNewlyClonedFolder;
								cloneStatus = CloneStatus.Created;
								break;
						}
					}
					break;

				case ExtantRepoSource.ChorusHub:
					var getCloneFromChorusHubModel = new GetCloneFromChorusHubModel(baseProjectDir)
					{
						ProjectFilter = projectFilter ?? DefaultProjectFilter,
						ExistingProjects = existingProjectNames
					};

					using (var getCloneFromChorusHubDialog = new GetCloneFromChorusHubDialog(getCloneFromChorusHubModel))
					{
						switch (getCloneFromChorusHubDialog.ShowDialog(parent))
						{
							default:
								cloneStatus = CloneStatus.NotCreated;
								break;
							case DialogResult.Cancel:
								cloneStatus = CloneStatus.Cancelled;
								break;
							case DialogResult.OK:
								if (getCloneFromChorusHubModel.CloneSucceeded)
								{
									actualCloneLocation = getCloneFromChorusHubDialog.PathToNewlyClonedFolder;
									cloneStatus = CloneStatus.Created;
								}
								else
								{
									cloneStatus = CloneStatus.NotCreated;
								}
								break;
						}
					}
					break;

				case ExtantRepoSource.Usb:
					using (var cloneFromUsbDialog = new GetCloneFromUsbDialog(baseProjectDir))
					{
						cloneFromUsbDialog.Model.ProjectFilter = projectFilter ?? DefaultProjectFilter;
						cloneFromUsbDialog.Model.ReposInUse = existingRepositories;
						cloneFromUsbDialog.Model.ExistingProjects = existingProjectNames;
						switch (cloneFromUsbDialog.ShowDialog(parent))
						{
							default:
								cloneStatus = CloneStatus.NotCreated;
								break;
							case DialogResult.Cancel:
								cloneStatus = CloneStatus.Cancelled;
								break;
							case DialogResult.OK:
								actualCloneLocation = cloneFromUsbDialog.PathToNewlyClonedFolder;
								cloneStatus = CloneStatus.Created;
								break;
						}
					}
					break;

			}
			// Warn the user if they already have this by another name. Not currently possible if USB.
			if (RepositorySource != ExtantRepoSource.Usb && cloneStatus == CloneStatus.Created)
			{
				var repo = new HgRepository(actualCloneLocation, new NullProgress());
				string projectWithExistingRepo;
				if (existingRepositories.TryGetValue(repo.Identifier, out projectWithExistingRepo))
				{
					MessageBox.Show(string.Format("The project {0} is already using this repository. Using Send/Receive in both will combine all the changes you make in either.", projectWithExistingRepo),
						"Multiple Projects", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

			}
			return new CloneResult(actualCloneLocation, cloneStatus);
		}

		internal static bool DefaultProjectFilter(string path)
		{
			return true;
		}
	}

	/// <summary>
	/// The results of a clone attempt.
	/// </summary>
	public class CloneResult
	{
		/// <summary>Constructor</summary>
		public CloneResult(string actualLocation, CloneStatus cloneStatus)
		{
			ActualLocation = actualLocation;
			CloneStatus = cloneStatus;
		}

		/// <summary>Get the actual location of a clone. (May, or may not, be the same as the desired location.)</summary>
		public string ActualLocation { get; private set; }
		/// <summary>Get the status of the clone attempt.</summary>
		public CloneStatus CloneStatus { get; private set; }
	}

	/// <summary>
	/// An enumeration of the possible repository sources.
	/// </summary>
	public enum ExtantRepoSource
	{
		/// <summary>Get a clone from the internet</summary>
		Internet,
		/// <summary>Get a clone from a USB drive</summary>
		Usb,
		/// <summary>Get a clone from a shared network folder</summary>
		LocalNetwork,
		/// <summary>Get a clone from ChorusHub</summary>
		ChorusHub
	}

	/// <summary>
	/// An indication of the success/failure of the clone attempt.
	/// </summary>
	public enum CloneStatus
	{
		/// <summary>The clone was made</summary>
		Created,
		/// <summary>The clone operation was cancelled</summary>
		Cancelled,
		/// <summary>The clone could not be created</summary>
		NotCreated
	}
}