using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Sync
{
	internal class SyncStartModel
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

//			if (address == null)
//			{
//				message = "This project is not yet associated with an internet server";
//				tooltip = string.Empty;
//				linkText = string.Empty;
//				return ready;
//			}
//			else
//			{
//				message = address.Name;
//				linkText = string.Empty;
//				tooltip = address.URI;
//				return true;
//			}
		}
	}
}
