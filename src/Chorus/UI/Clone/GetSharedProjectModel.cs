using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.Utilities.Help;
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
		/// <param name="parent">Window that will be parent of progress window</param>
		/// <param name="projectFilter">Function taking a directory path and telling whether it contains the right sort of repo</param>
		/// <param name="baseProjectDir">The directory which contains projects we already have, and where the result should go</param>
		/// <param name="otherRepoPath">Optionally specifies another place to look for existing repos: look in this subfolder of each folder in baseProjectDir.
		/// This is used in FLEx (passing "OtherRepositories") so existing LIFT repos linked to FW projects can be found. Pass null if not used.</param>
		/// <param name="preferredClonedFolderName"></param>
		/// <param name="howToSendReceiveMessageText">This string is appended to the message we build when we have received a repo and can't keep it, because
		/// it has the same hash as an existing project. We think it is likely the user actually intended to Send/Receive that project rather than obtaining
		/// a duplicate. This message thus typically tells him how to do so, in the particular client program. May also be empty.</param>
		/// <returns>
		/// A CloneResult that provides the clone results (e.g., success or failure) and the actual clone location (null if not created).
		/// </returns>
		public CloneResult GetSharedProjectUsing(Form parent, Func<string, bool> projectFilter, string baseProjectDir, string otherRepoPath, string preferredClonedFolderName,
			string howToSendReceiveMessageText)
		{
			Guard.AgainstNull(parent, "parent");
			Guard.Against(string.IsNullOrEmpty(baseProjectDir), "'baseProjectDir' is null or an empty string.");
			if (preferredClonedFolderName == string.Empty)
				preferredClonedFolderName = null;
			var existingRepositories = ExtantRepoIdentifiers(baseProjectDir, otherRepoPath);
			var existingProjectNames = new HashSet<string>(from dir in Directory.GetDirectories(baseProjectDir) select Path.GetFileName(dir));

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
					using (var warningDlg = new DuplicateProjectWarningDialog())
						warningDlg.Run(projectWithExistingRepo, howToSendReceiveMessageText);
					Directory.Delete(actualCloneLocation, true);
					actualCloneLocation = "";
					cloneStatus = CloneStatus.Cancelled;
				}

			}
			return new CloneResult(actualCloneLocation, cloneStatus);
		}

		internal static bool DefaultProjectFilter(string path)
		{
			return true;
		}

		public static Dictionary<string, string> ExtantRepoIdentifiers(string baseProjectDir, string otherRepoLocation)
		{
			var extantRepoIdentifiers = new Dictionary<string, string>();

			foreach (var mainFwProjectFolder in Directory.GetDirectories(baseProjectDir, "*", SearchOption.TopDirectoryOnly))
			{
				var hgfolder = Path.Combine(mainFwProjectFolder, ".hg");
				if (Directory.Exists(hgfolder))
				{
					CheckForMatchingRepo(mainFwProjectFolder, extantRepoIdentifiers);
				}

				if (string.IsNullOrWhiteSpace(otherRepoLocation))
					continue;
				var otherRepoFolder = Path.Combine(mainFwProjectFolder, otherRepoLocation);
				if (!Directory.Exists(otherRepoFolder))
					continue;

				foreach (var sharedFolder in Directory.GetDirectories(otherRepoFolder, "*", SearchOption.TopDirectoryOnly))
				{
					hgfolder = Path.Combine(sharedFolder, ".hg");
					if (Directory.Exists(hgfolder))
					{
						CheckForMatchingRepo(sharedFolder, extantRepoIdentifiers);
					}
				}
			}
			return extantRepoIdentifiers;
		}

		private static void CheckForMatchingRepo(string repoContainingFolder, Dictionary<string, string> extantRepoIdentifiers)
		{
			var repo = new HgRepository(repoContainingFolder, new NullProgress());
			var identifier = repo.Identifier;
			// Pathologically we may already have a duplicate. If so we can only record one name; just keep the last encountered.
			if (identifier != null)
				extantRepoIdentifiers[identifier] = Path.GetFileName(repoContainingFolder);
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