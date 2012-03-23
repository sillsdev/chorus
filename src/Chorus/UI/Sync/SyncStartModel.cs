using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;

namespace Chorus.UI.Sync
{
	internal class SyncStartModel : ISyncStartModel
	{
		private readonly HgRepository _repository;

		public SyncStartModel(HgRepository repository)
		{
			_repository = repository;
		}

		public bool GetInternetStatusLink(out string buttonLabel, out string message, out string tooltip)
		{
			buttonLabel = "Internet";

			RepositoryAddress address;
			try
			{
				address = _repository.GetDefaultNetworkAddress<HttpRepositoryPath>();
			}
			catch (Exception error)//probably, hgrc is locked
			{
				message = error.Message;
				tooltip = string.Empty;
				return false;
			}

			bool ready = _repository.GetIsReadyForInternetSendReceive(out tooltip);
			if (ready)
			{
				buttonLabel = address.Name;
				message = string.Empty;
			}
			else
			{
				message = tooltip;
			}

			return ready;
		}

		public bool GetNetworkStatusLink(out string message, out string tooltip)
		{
			RepositoryAddress address;
			var ready = false;
			message = string.Empty;

			try
			{
				address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
			}
			catch (Exception error)//probably, hgrc is locked
			{
				message = error.Message;
				tooltip = string.Empty;
				return false;
			}
			if (address == null)
				message = "This project is not yet associated with a shared folder";
			else
				ready = Directory.Exists(Path.Combine(address.URI, ".hg"));

			if (ready)
			{
				message = string.Empty;
				tooltip = address.URI;
			}
			else
			{
				if (address != null)
					message = "File not found.";
				tooltip = message;
			}

			return ready;
		}

		bool ISyncStartModel.GetUsbStatusLink(IUsbDriveLocator usbDriveLocator, out string message)
		{
			return GetUsbStatusLinkInternal(usbDriveLocator, out message);
		}

		internal static bool GetUsbStatusLinkInternal(IUsbDriveLocator usbDriveLocator, out string message)
		{
			var ready = false;
			if (!usbDriveLocator.UsbDrives.Any())
			{
				message = "First insert a USB flash drive.";
			}
			else if (usbDriveLocator.UsbDrives.Count() > 1)
			{
				message = "More than one USB drive detected. Please remove one.";
			}
			else
			{
				try
				{
					var first = usbDriveLocator.UsbDrives.First();
#if !MONO
					message = first.RootDirectory + " " + first.VolumeLabel + " (" +
										   Math.Floor(first.TotalFreeSpace / 1024000.0) + " Megs Free Space)";
#else
					message = first.VolumeLabel;
					//RootDir & volume label are the same on linux.  TotalFreeSpace is, like, maxint or something in mono 2.0
#endif
					ready = true;
				}
				catch (Exception error)
				{
					message = error.Message;
					ready = false;
				}
			}
			return ready;
		}

		void ISyncStartModel.SetNewSharedNetworkAddress(HgRepository repository, string path)
		{
			// Needed because this is a static method
			SetNewSharedNetworkAddress(repository, path);
		}

		public static void SetNewSharedNetworkAddress(HgRepository repository, string path)
		{
			if (string.IsNullOrEmpty(path))
				return;
			try
			{
				if (!Directory.Exists(Path.Combine(path, ".hg")))
				{
					if (DirectoryUtilities.GetSafeDirectories(path).Length > 0 || Directory.GetFiles(path).Length > 0)
					{
						Palaso.Reporting.ErrorReport.NotifyUserOfProblem(
							"The folder you chose doesn't have a repository. Chorus cannot make one there, because the folder is not empty.  Please choose a folder that is already being used for send/receive, or create and choose a new folder to hold the repository.");
						return;
					}

					var result = MessageBox.Show("A new repository will be created in " + path + ".", "Create new repository?",
									MessageBoxButtons.OKCancel);
					if (result != DialogResult.OK)
						return;

				}
				string alias = HgRepository.GetAliasFromPath(path);
				repository.SetTheOnlyAddressOfThisType(RepositoryAddress.Create(alias, path));
			}
			catch (Exception e)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e, "There was a problem setting the network path.");
				throw;
			}
		}
	}

	public interface ISyncStartModel
	{
		bool GetInternetStatusLink(out string buttonLabel, out string message, out string tooltip);

		bool GetNetworkStatusLink(out string message, out string tooltip);

		bool GetUsbStatusLink(IUsbDriveLocator usbDriveLocator, out string message);

		void SetNewSharedNetworkAddress(HgRepository repository, string path);
	}
}
