using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Chorus.Utilities;
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

		public IEnumerable<string> GetRepositoryNames(string searchUrl)
		{
			Progress.WriteMessage("Client requested repository names.");

			var allDirectories = GetAllDirectories();
			if (string.IsNullOrEmpty(searchUrl))
			{
				return allDirectories;
			}
			var filteredDirectories = new List<string>();
			try
			{
				var fileExtensions = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, "fileExtension", string.Empty);

			}
			catch (ApplicationException e)
			{
				// Url parser couldn't parse the url.
				Progress.WriteMessage("GetRepositoryNames(): " + e.Message);
				return filteredDirectories;
			}
			//Progress.WriteMessage("GetRepositoryNames(): Client sent unknown search parameter '" + param + "'");
			return new List<string>();
		}

		private static IEnumerable<string> GetAllDirectories()
		{
			return Directory.GetDirectories(ChorusHubService.Parameters.RootDirectory).Select(
				directory => Path.GetFileName(directory));
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="name"></param>
		/// <returns>true if client should wait for hg to notice</returns>
		public bool PrepareToReceiveRepository(string name)
		{
			if (GetRepositoryNames(string.Empty).Contains(name))
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