using System;
using System.Windows.Forms;
using Palaso.Code;

namespace Chorus.UI.Clone
{
	/// <summary>
	/// Implementation of IGetSharedProject interface that can be used by client code to get a newly cloned project.
	/// </summary>
	public class GetSharedProject : IGetSharedProject
	{
		#region Implementation of IGetSharedProject

		/// <summary>
		/// Get a teammate's shared project from the specified source.
		/// </summary>
		/// <returns>
		/// A CloneResult that provides the clone results (e.g., success or failure) and the desired and actual clone locations.
		/// </returns>
		public CloneResult GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, Func<string, bool> projectFilter, string baseProjectDir, string preferredClonedFolderName)
		{
			Guard.AgainstNull(parent, "parent");
			Guard.Against(string.IsNullOrEmpty(baseProjectDir), "'baseProjectDir' is null or an empty string.");
			if (preferredClonedFolderName == string.Empty)
				preferredClonedFolderName = null;

			// Make clone from some source.
			string actualCloneLocation = null;
			switch (extantRepoSource)
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
								return new CloneResult(null, CloneStatus.NotCreated);
							case DialogResult.Cancel:
								return new CloneResult(null, CloneStatus.Cancelled);
							case DialogResult.OK:
								// It made a clone, but maybe in the wrong name, grab the project name.
								actualCloneLocation = cloneFromInternetDialog.PathToNewlyClonedFolder;
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
								return new CloneResult(null, CloneStatus.NotCreated);
							case DialogResult.Cancel:
								return new CloneResult(null, CloneStatus.Cancelled);
							case DialogResult.OK:
								actualCloneLocation = cloneFromNetworkFolderDlg.PathToNewlyClonedFolder;
								break;
						}
					}
					break;
				case ExtantRepoSource.Usb:
					using (var cloneFromUsbDialog = new GetCloneFromUsbDialog(baseProjectDir))
					{
						cloneFromUsbDialog.Model.ProjectFilter = projectFilter ?? DefaultProjectFilter;
						switch (cloneFromUsbDialog.ShowDialog(parent))
						{
							default:
								return new CloneResult(null, CloneStatus.NotCreated);
							case DialogResult.Cancel:
								return new CloneResult(null, CloneStatus.Cancelled);
							case DialogResult.OK:
								// It made a clone, grab the project name.
								actualCloneLocation = cloneFromUsbDialog.PathToNewlyClonedFolder;
								break;
						}
					}
					break;
			}
			return new CloneResult(actualCloneLocation, CloneStatus.Created);
		}

		#endregion

		internal static bool DefaultProjectFilter(string path)
		{
			return true;
		}
	}
}