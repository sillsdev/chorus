using System;
using System.IO;
using System.Net.NetworkInformation;
using Chorus.Properties;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Sync
{
	class SyncStartAlternateModel : ISyncStartModel
	{
		private readonly HgRepository _repository;
		private readonly string INTERNET_HOST = Resources.ksInternetVerificationSite;

		public SyncStartAlternateModel(HgRepository repository)
		{
			_repository = repository;
		}

		public bool GetInternetStatusLink(out string buttonLabel, out string message, out string tooltip)
		{
			buttonLabel = Resources.ksInternetButtonLabel;
			if (!IsInternetPingable(INTERNET_HOST))
			{
				message = Resources.ksNoInternetAccess;
				tooltip = string.Empty;
				return false;
			}
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

		private bool IsInternetPingable(string host)
		{
			var myPing = new Ping();
			var buffer = new byte[32];
			const int timeout = 2000;
			var pingOptions = new PingOptions();
			try
			{
				var reply = myPing.Send(host, timeout, buffer, pingOptions);
				return reply != null && reply.Status == IPStatus.Success;
			}
			catch (PingException)
			{
			}
			return false;
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
				ready = Directory.Exists(address.URI);

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
