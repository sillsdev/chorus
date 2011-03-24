using System;
using System.IO;
using System.Windows.Forms;
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

		public void SetNewSharedNetworkAddress(string path)
		{
			if (string.IsNullOrEmpty(path))
				return;
			try
			{
				if (!Directory.Exists(Path.Combine(path, ".hg")))
				{
					if (Directory.GetDirectories(path).Length > 0 || Directory.GetFiles(path).Length > 0)
					{
						Palaso.Reporting.ErrorReport.NotifyUserOfProblem(
							"The folder you chose doesn't have a repository. Chorus cannot make one there, because the folder is not empty.  Please choose a folder that is already being used for send/recieve, or create and choose a new folder to hold the repository.");
						return;
	}

					var result = MessageBox.Show("A new repository will be created in " + path + ".", "Create new repository?",
									MessageBoxButtons.OKCancel);
					if (result != DialogResult.OK)
						return;

}
				string alias = path;
				_repository.SetTheOnlyAddressOfThisType(RepositoryAddress.Create(alias, path));
			}
			catch (Exception e)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e,"There was a problem setting the network path.");
				throw;
			}

		}
	}
}
