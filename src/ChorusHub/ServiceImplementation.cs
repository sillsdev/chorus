using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;

namespace ChorusHub
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	internal class ServiceImplementation : IChorusHubService
	{
		public IProgress Progress = new ConsoleProgress();

		public IEnumerable<string> GetRepositoryNames()
		{
			Progress.WriteMessage("Client requested repository names.");

			foreach (var directory in Directory.GetDirectories(ChorusHubService._rootPath))
			{
				yield return Path.GetFileName(directory);
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="name"></param>
		/// <returns>true if client should wait for hg to notice</returns>
		public bool PrepareToReceiveRepository(string name)
		{
			if (GetRepositoryNames().Contains(name))
			{
				return false;
			}
			var directory = Path.Combine(ChorusHubService._rootPath, name);
			Progress.WriteMessage("PrepareToReceiveRepository() is preparing a place for '" + name + "'");
			Directory.CreateDirectory(directory);
			HgRepository.CreateRepositoryInExistingDir(directory, new ConsoleProgress());
			return true;
		}

	}
}