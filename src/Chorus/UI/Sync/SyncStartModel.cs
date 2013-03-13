using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress;

namespace Chorus.UI.Sync
{
	internal class SyncStartModel
	{
		private readonly HgRepository _repository;

		private const string _noInternetMsg = "The computer does not have internet access.";
		internal string NoInternetMessage
		{
			get { return _noInternetMsg; }
		}

		private const string _noSharedFolderMsg = "The computer does not have access to the specified network folder.";
		internal string NoSharedFolderMessage
		{
			get { return _noSharedFolderMsg; }
		}

		public SyncStartModel(HgRepository repository)
		{
			_repository = repository;
		}

		public bool GetInternetStatusLink(out string buttonLabel, out string message, out string tooltip, out string diagnosticNotes)
		{
			buttonLabel = "Internet";
			RepositoryAddress address;
			diagnosticNotes = string.Empty;

			try
			{
				address = _repository.GetDefaultNetworkAddress<HttpRepositoryPath>();
			}
			catch (Exception error)//probably, hgrc is locked
			{
				diagnosticNotes = error.Message;
				message = error.Message;
				tooltip = string.Empty;
				return false;
			}

			bool ready = _repository.GetIsReadyForInternetSendReceive(out tooltip);
			if (ready)
			{
				buttonLabel = address.Name;
				message = string.Empty;

				// But, the Internet might be down or the repo unreachable.
				if (!IsInternetRepositoryReachable(address, out diagnosticNotes))
				{
					message = NoInternetMessage;
					tooltip = message;
					return false;
				}
			}
			else
			{
				message = tooltip;
			}

			return ready;
		}

		private bool IsInternetRepositoryReachable(RepositoryAddress repoAddress, out string logString)
		{
			logString = string.Empty;
			var progress = new StringBuilderProgress(){ShowVerbose = true};
			var result = repoAddress.CanConnect(_repository, repoAddress.Name, progress);
			if (!result)
				logString = progress.Text;
			return result;
		}

		public bool GetNetworkStatusLink(out string message, out string tooltip, out string diagnosticNotes)
		{
			RepositoryAddress address;
			var ready = false;
			message = string.Empty;
			diagnosticNotes = string.Empty;

			try
			{
				address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
			}
			catch (Exception error)//probably, hgrc is locked
			{
				diagnosticNotes = error.Message;
				message = error.Message;
				tooltip = string.Empty;
				return false;
			}
			if (address == null)
				message = "No Chorus Hub found on this network.";//" and this project is not yet associated with a shared folder.";
			else
			{
				ready = IsSharedFolderRepositoryReachable(address, out diagnosticNotes);
				if (!ready)
				{
					message = NoSharedFolderMessage;
					tooltip = message;
					return false;
				}
			}
			if (ready)
			{
				message = tooltip = address.GetPotentialRepoUri(address.URI, "", new NullProgress());
			}
			else
			{
				tooltip = message;
			}

			return ready;
		}

		internal bool HasASharedFolderAddressBeenSetUp()
		{
			try
			{
				var address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
				return address != null;
			}
			catch (Exception error)//probably, hgrc is locked
			{
				return false;
			}
		}

		private bool IsSharedFolderRepositoryReachable(RepositoryAddress repoAddress, out string logString)
		{
			// We want to know if we can connect, but we don't want to bother the user with extraneous information.
			// But we DO want the diagnostic information available.
			logString = string.Empty;
			var progress = new StringBuilderProgress() { ShowVerbose = true };
			var result = repoAddress.CanConnect(_repository, "", progress);
			if (!result)
				logString = progress.Text;
			return result;
		}

		internal bool GetUsbStatusLink(IUsbDriveLocator usbDriveLocator, out string message)
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
	}
}
