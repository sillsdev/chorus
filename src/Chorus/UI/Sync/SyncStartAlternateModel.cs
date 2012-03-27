using System;
using System.IO;
using System.Net.NetworkInformation;
using Chorus.Properties;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Sync
{
	class SyncStartAlternateModel : ISyncStartModel
	{
		private readonly HgRepository _repository;
		//private readonly string INTERNET_HOST = Resources.ksInternetVerificationSite;

		public SyncStartAlternateModel(HgRepository repository)
		{
			_repository = repository;
		}

		public bool GetInternetStatusLink(out string buttonLabel, out string message, out string tooltip)
		{
			buttonLabel = Resources.ksInternetButtonLabel;
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

				// But, the Internet might be down or the repo unreachable.
				if (!IsInternetRepositoryReachable(address))
				{
					message = Resources.ksNoInternetAccess;
					tooltip = string.Empty;
					return false;
				}
			}
			else
			{
				message = tooltip;
			}

			return ready;
		}

		private bool IsInternetRepositoryReachable(RepositoryAddress repoAddress)
		{
			return repoAddress.CanConnect(_repository, repoAddress.Name, new NullProgress());
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
				message = Resources.ksSharedFolderNotAssociated;
			else
				ready = IsSharedFolderRepositoryReachable(address);
			if (ready)
			{
				message = string.Empty;
				tooltip = address.URI;
			}
			else
			{
				if (address != null)
					message = Resources.ksSharedFolderInaccessible;
				tooltip = message;
			}

			return ready;
		}

		private bool IsSharedFolderRepositoryReachable(RepositoryAddress repoAddress)
		{
			// We want to know if we can connect, but we don't want to bother the user with extraneous information.
			return repoAddress.CanConnect(_repository, repoAddress.Name, new NullProgress());
		}

		bool ISyncStartModel.GetUsbStatusLink(IUsbDriveLocator usbDriveLocator, out string message)
		{
			return SyncStartModel.GetUsbStatusLinkInternal(usbDriveLocator, out message);
		}

		public void SetNewSharedNetworkAddress(HgRepository repository, string path)
		{
			SyncStartModel.SetNewSharedNetworkAddress(repository, path);
		}
	}
}
