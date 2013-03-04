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
			try
			{
				var fileExtensions = UrlHelper.GetMultipleValuesFromQueryStringOfRef(searchUrl, "fileExtension", string.Empty);
				var repoID = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, "repoID", string.Empty);
				return CombRepositoriesForMatchingNames(allDirectories, fileExtensions, repoID);
			}
			catch (ApplicationException e)
			{
				// Url parser couldn't parse the url.
				Progress.WriteMessage("GetRepositoryNames(): " + e.Message);
				return new List<string>();
			}
		}

		private IEnumerable<string> CombRepositoriesForMatchingNames(IEnumerable<string> allDirectories,
			string[] fileExtensions, string repoID)
		{
			if (fileExtensions.Length == 1 && fileExtensions[0] == string.Empty && repoID == string.Empty)
			{
				return allDirectories; // Well THAT was a waste of time!
			}

			var result = new List<string>();

			if (fileExtensions.Length > 1 || fileExtensions[0] != string.Empty)
			{
				// Include repositories that contain files with these fileExtensions
				result.AddRange(allDirectories.Where(dirName => FindFileWithExtensionIn(dirName, fileExtensions)));
				if (result.Count == 0)
				{
					return result;
				}
			}
			else
			{
				// No fileExtension filter; include them all and try filtering out by repoID
				result.AddRange(allDirectories);
			}

			if (repoID != string.Empty)
			{
				// Filter out repositories that don't match the given 'repoID'
				foreach (var dirName in result.Where(dirName => !FindRepoIDIn(dirName, repoID)))
				{
					result.Remove(dirName);
				}
			}
			return result;
		}

		private bool FindFileWithExtensionIn(string dirName, IEnumerable<string> fileExtensions)
		{
			// Try files in the directory itself first
			var topLevelFiles = Directory.GetFiles(dirName);

			// Compare as lowercase

			// TODO: Keep working on this one....

			// If we still haven't found a match, try looking in .hg/store/data
			// for a file with the extension + .i
			return false;
		}

		private bool FindRepoIDIn(string dirName, string repoId)
		{
			// Presently does nothing
			return true;
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