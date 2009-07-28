using System;
using System.Collections.Generic;
using System.Text;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Baton.Settings
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
			var aliases = new List<RepositoryAddress>();
			var lines = text.Split('\n');
			foreach (var line in lines)
			{
				var parts = line.Split('=');
				if(parts.Length != 2)
					continue;
				aliases.Add( RepositoryAddress.Create(parts[0].Trim(), parts[1].Trim()));
			}

			_repository.SetKnownRepositoryAddresses(aliases);
		}
	}
}
