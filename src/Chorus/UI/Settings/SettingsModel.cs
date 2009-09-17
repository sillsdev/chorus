using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.UI.Settings
{
	public class SettingsModel
	{
		private readonly HgRepository _repository;

		public SettingsModel(HgRepository repository)
		{
			_repository = repository;
		}

		public void SetUserName(string name, IProgress progress)
		{
			if (name.Trim() != string.Empty)
			{
				_repository.SetUserNameInIni(name, progress);
			}
		}

		public string GetUserName(IProgress progress)
		{
			return _repository.GetUserNameFromIni(progress);
		}

		public string GetAddresses()
		{
			var b = new StringBuilder();
			foreach (var repo in _repository.GetRepositoryPathsInHgrc())
			{
				b.AppendLine(repo.Name + " = " + repo.URI);
			}
			return b.ToString();
		}

		public void SetAddresses(string text, IProgress progress)
		{
			var oldPaths = _repository.GetRepositoryPathsInHgrc();
			var aliases = new List<RepositoryAddress>();
			var lines = text.Split('\n');
			foreach (var line in lines)
			{
				var parts = line.Split('=');
				if(parts.Length != 2)
					continue;
				aliases.Add( RepositoryAddress.Create(parts[0].Trim(), parts[1].Trim()));
			}

			if (oldPaths.Count() > 0 && aliases.Count() == 0)
			{
				var response = MessageBox.Show(
					"Repository Paths is being cleared.  If you did that on purpose, fine, click 'Yes'.  If not, please click 'No' and report this to issues@wesay.org (we're trying to track down a bug).",
					"Pleaes confirm", MessageBoxButtons.YesNo);
				if(response == DialogResult.No)
					return;
			}
			_repository.SetKnownRepositoryAddresses(aliases);
		}
	}
}