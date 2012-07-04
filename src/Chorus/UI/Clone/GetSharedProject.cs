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
					var cloneModel = new GetCloneFromInternetModel(baseProjectDir) { LocalFolderName = preferredClonedFolderName };
					using (var internetCloneDlg = new GetCloneFromInternetDialog(cloneModel))
					{
						switch (internetCloneDlg.ShowDialog(parent))
						{
							default:
								return new CloneResult(null, CloneStatus.NotCreated);
							case DialogResult.Cancel:
								return new CloneResult(null, CloneStatus.Cancelled);
							case DialogResult.OK:
								// It made a clone, but maybe in the wrong name, grab the project name.
								actualCloneLocation = internetCloneDlg.PathToNewProject;
								break;
						}
					}
					break;
				case ExtantRepoSource.LocalNetwork:
					var cloneFromNetworkFolderModel = new GetCloneFromNetworkFolderModel(baseProjectDir)
					{
						ProjectFilter = projectFilter ?? DefaultProjectFilter
					};

					using (var openFileDlg = new GetCloneFromNetworkFolderDlg())
					{
						// We don't have a GetCloneFromNetworkFolderDlg constructor that takes the model because
						// it would inexplicably mess up Visual Studio's designer view of the dialog:
						openFileDlg.LoadFromModel(cloneFromNetworkFolderModel);

						switch (openFileDlg.ShowDialog(parent))
						{
							default:
								return new CloneResult(null, CloneStatus.NotCreated);
							case DialogResult.Cancel:
								return new CloneResult(null, CloneStatus.Cancelled);
							case DialogResult.OK:
								actualCloneLocation = cloneFromNetworkFolderModel.ActualClonedFolder;
								break;
						}
					}
					break;
				case ExtantRepoSource.Usb:
					using (var usbCloneDlg = new GetCloneFromUsbDialog(baseProjectDir))
					{
						usbCloneDlg.Model.ProjectFilter = projectFilter ?? DefaultProjectFilter;
						switch (usbCloneDlg.ShowDialog(parent))
						{
							default:
								return new CloneResult(null, CloneStatus.NotCreated);
							case DialogResult.Cancel:
								return new CloneResult(null, CloneStatus.Cancelled);
							case DialogResult.OK:
								// It made a clone, grab the project name.
								actualCloneLocation = usbCloneDlg.PathToNewProject;
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