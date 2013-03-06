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

		const char OrChar = '|';

		public IEnumerable<string> GetRepositoryNames(string searchUrl)
		{
			Progress.WriteMessage("Client requested repository names.");

			var allDirectories = GetAllDirectories();
			if (string.IsNullOrEmpty(searchUrl))
			{
				return allDirectories.Select(Path.GetFileName);
			}
			try
			{
				var fileExtensions = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, "fileExtension", string.Empty);
				var repoID = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, "repoID", string.Empty);
				return CombRepositoriesForMatchingNames(allDirectories, fileExtensions, repoID).Select(Path.GetFileName);
			}
			catch (ApplicationException e)
			{
				// Url parser couldn't parse the url.
				Progress.WriteMessage("GetRepositoryNames(): " + e.Message);
				return new List<string>();
			}
		}

		private IEnumerable<string> CombRepositoriesForMatchingNames(IEnumerable<string> allDirectories,
			string fileExtensionQuery, string repoID)
		{
			if (fileExtensionQuery == string.Empty && repoID == string.Empty)
			{
				return allDirectories; // Well THAT was a waste of time!
			}

			var result = allDirectories.ToList();

			if (fileExtensionQuery != string.Empty)
			{
				// preprocessing changes to lowercase and appends .i to the extensions
				var fileExtensions = PreProcessExtensions(fileExtensionQuery).ToArray();
				// Remove repositories that don't contain a file with one of these fileExtensions
				var intermediateResult =
					allDirectories.Where(dirName => !FindFileWithExtensionIn(dirName, fileExtensions));
				result.RemoveAll(intermediateResult.Contains);
				if (result.Count == 0)
				{
					return result;
				}
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

		private  IEnumerable<string> PreProcessExtensions(string fileExtensionQuery)
		{
			return fileExtensionQuery.ToLowerInvariant().Split(OrChar).Select(extension => extension + ".i");
		}

		private bool FindFileWithExtensionIn(string dirName, IEnumerable<string> fileExtensions)
		{
			// If we still haven't found a match, try looking in .hg/store/data
			// Check that the internal directory exists first!
			var internalDirectory = Path.Combine(dirName, ".hg", "store", "data");
			if (!Directory.Exists(internalDirectory))
			{
				return false;
			}
			var internalHgFileNames = Directory.GetFiles(internalDirectory);
			return FindExtensionMatch(internalHgFileNames, fileExtensions);
		}

		private bool FindExtensionMatch(IEnumerable<string> fileNames, IEnumerable<string> lcExtensions)
		{
			return fileNames.Any(fileName => lcExtensions.Any(
				fileExtension => fileName.ToLowerInvariant().EndsWith(fileExtension)));
		}

		private bool FindRepoIDIn(string dirName, string repoId)
		{
			// Currently unused. If you use it, add some tests!
			if (!File.Exists(Path.Combine(dirName, ".hg")))
			{
				return false;
			}
			var repo = HgRepository.CreateOrUseExisting(dirName, new ConsoleProgress());
			return repoId == repo.Identifier;
		}

		private static IEnumerable<string> GetAllDirectories()
		{
			return Directory.GetDirectories(ChorusHubService.Parameters.RootDirectory);
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