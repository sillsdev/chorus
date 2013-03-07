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

		private const char OrChar = '|';
		private const string key1 = "fileExtension";
		private const string key2 = "repoID";

		// These are the keys found in the submitted url's search query.
		private string[] _searchKeys;
		// These functions tell the repository 'comber' how to determine matches for each of the search keys.
		private Func<string, IEnumerable<string>, bool>[] _keyFunctions;

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
				var ckeys = InitKeyAndFunctions();
				var parsedParams = new string[ckeys];
				for (var i = 0; i < ckeys; i++)
				{
					parsedParams[i] = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, _searchKeys[i], string.Empty);
				}
				return CombRepositoriesForMatchingNames(allDirectories, parsedParams).Select(Path.GetFileName);
			}
			catch (ApplicationException e)
			{
				// Url parser couldn't parse the url.
				Progress.WriteMessage("GetRepositoryNames(): " + e.Message);
				return new List<string>();
			}
		}

		private int InitKeyAndFunctions()
		{
			// To add a new search key, simply add the string to _searchKeys and the name of the
			// method that determines if a directory matches or not in _keyFunctions AT THE SAME PLACE IN THE ARRAYS!
			_searchKeys = new[] { key1, key2 };
			var ckeys = _searchKeys.Length;
			_keyFunctions = new Func<string, IEnumerable<string>, bool>[] { FindFileWithExtensionIn, FindRepoIDIn };
			return ckeys;
		}

		private IEnumerable<string> CombRepositoriesForMatchingNames(IEnumerable<string> allDirectories,
			string[] queries)
		{
			if (queries.All(string.IsNullOrEmpty))
			{
				Progress.WriteMessage("Client search string contained only unknown keys or empty values.");
				return allDirectories; // Well THAT was a waste of time!
			}

			var result = allDirectories.ToList();

			for (var i = 0; i < _searchKeys.Length; i++)
			{
				if (queries[i] == string.Empty)
				{
					continue;
				}
				// preprocessing changes to lowercase and splits 'or'd search values
				var thisKeysQueries = PreProcessQueriesForOneKey(queries[i]).ToArray();
				var reposToDiscard = result.Where(dirName => !_keyFunctions[i](dirName, thisKeysQueries));
				result.RemoveAll(reposToDiscard.Contains);
				if (result.Count == 0)
				{
					return result;
				}
			}
			return result;
		}

		private  IEnumerable<string> PreProcessQueriesForOneKey(string query)
		{
			// there could be several search terms 'or'd together
			return query.ToLowerInvariant().Split(OrChar);
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
			return FindExtensionMatch(internalHgFileNames, fileExtensions.Select(ext => ext + ".i"));
		}

		private bool FindExtensionMatch(IEnumerable<string> fileNames, IEnumerable<string> lcExtensions)
		{
			return fileNames.Any(fileName => lcExtensions.Any(
				fileExtension => fileName.ToLowerInvariant().EndsWith(fileExtension)));
		}

		private bool FindRepoIDIn(string dirName, IEnumerable<string> repoIdStrings)
		{
			// Currently unused. If you use it, add some tests!
			if (!File.Exists(Path.Combine(dirName, ".hg")))
			{
				return false;
			}
			var repo = HgRepository.CreateOrUseExisting(dirName, new ConsoleProgress());
			return repoIdStrings.Contains(repo.Identifier);
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