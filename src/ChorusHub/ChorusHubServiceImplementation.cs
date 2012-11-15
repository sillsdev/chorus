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
	internal class ChorusHubServiceImplementation : IChorusHubService
	{
		//this is static because at the moment, I don't know how to construct or access
		//this class; the WCF service just does it for me.

		public static IProgress Progress = new ConsoleProgress();

		public IEnumerable<string> GetRepositoryNames()
		{
			Progress.WriteMessage("Client requested repository names.");

			foreach (var directory in Directory.GetDirectories(ChorusHubService.Parameters.RootDirectory))
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
			var directory = Path.Combine(ChorusHubService.Parameters.RootDirectory, name);
			Progress.WriteMessage("PrepareToReceiveRepository() is preparing a place for '" + name + "'");
			Directory.CreateDirectory(directory);
			HgRepository.CreateRepositoryInExistingDir(directory, new ConsoleProgress());
			return true;
		}

	}
}