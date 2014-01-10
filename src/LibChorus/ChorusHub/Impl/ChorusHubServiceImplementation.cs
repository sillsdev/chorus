using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress;

namespace Chorus.ChorusHub.Impl
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	internal class ChorusHubServiceImplementation : IChorusHubService
	{
		private const char OrChar = '|';
		private const char UnderScore = '_';
		private const char Asterisk = '*';
		private const char Period = '.';
		private const string FilePattern = "filePattern";
		private const string HgFolder = ".hg";
		private const string Store = "store";
		private const string Data = "data";
		private const string InternalExt = ".i";

		/// <summary>
		/// Returns information about the Hg repositories that the ChorusHub knows about.
		///
		/// The search can be trimmed by use of the 'searchUrl' parameter.
		/// Everything about the searchUrl except the query string is fabricated
		/// by the ChorusHubClient. The query string is fed in by the application.
		///
		/// The possible search values are:
		/// filePattern -- This key can have multiple values separated by the '|' character
		///
		/// Each repository generates a JSON string consisting of two name/value pairs.
		/// The two names are "name" and "id". The JSON strings are concatenated with / between.
		/// (An earlier version returned an enumeration of json strings. But Mono could not
		/// marshal this.)
		/// </summary>
		/// <example>searchUrl: "scheme://path?filePattern=*.lift|*.CustomProperties"</example>
		/// <example>returned repo info string: {"name": "someProject", "id": "123abc"}</example>
		/// <param name="searchUrl"></param>
		/// <remarks>A new (empty repo) will hav the folder name as 'name', and the id as 'newRepo'</remarks>
		/// <returns></returns>
		public string GetRepositoryInformation(string searchUrl)
		{
			EventLog.WriteEntry("Application", "Client requested repository information.", EventLogEntryType.Information);

			var allDirectoryTuples = GetAllDirectoriesWithRepos();
			if (string.IsNullOrEmpty(searchUrl))
			{
				return string.Join("/", allDirectoryTuples.Select(dirInfo => dirInfo.Item2)); // return the JSON strings
			}
			try
			{
				var searchPatternString = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, FilePattern, string.Empty);
				EventLog.WriteEntry("Application", string.Format("Client requested repositories matching {0}.", searchPatternString), EventLogEntryType.Information);
				return string.Join("/", CombRepositoriesForMatchingNames(allDirectoryTuples, searchPatternString).ToArray());
			}
			catch (ApplicationException e)
			{
				// Url parser couldn't parse the url.
				EventLog.WriteEntry("Application", "GetRepositoryInformation(): " + e.Message, EventLogEntryType.Warning);
				return "";
			}
		}

		private IEnumerable<string> CombRepositoriesForMatchingNames(
			IEnumerable<Tuple<string, string>> allDirectories, string queries)
		{
			if (string.IsNullOrEmpty(queries))
			{
				EventLog.WriteEntry("Application", "Client search string contained only unknown keys or empty values.", EventLogEntryType.Warning);
				return allDirectories.Select(dir => dir.Item2); // Well THAT was a waste of time!
			}

			var result = allDirectories.ToList();

			// preprocessing changes uppercase to underscore + lowercase and splits 'or'd search values
			var processedQueries = PreProcessQueries(queries).ToArray();
			var reposToDiscard = result.Where(dirTuple => !FindFileWithExtensionIn(dirTuple.Item1, processedQueries));
			result.RemoveAll(reposToDiscard.Contains);
			return result.Select(dir => dir.Item2);
		}

		private  IEnumerable<string> PreProcessQueries(string query)
		{
			// there could be several search terms 'or'd together
			// need to munge Uppercase -> _lowercase and _ -> __
			var sb = new StringBuilder();
			foreach (var ch in query)
			{
				switch (ch)
				{
					case OrChar:
					case Asterisk:
					case Period:
						sb.Append(ch);
						break;
					case UnderScore:
						sb.Append(UnderScore);
						sb.Append(UnderScore);
						break;
					default:
						if (ch == Char.ToUpper(ch, CultureInfo.CurrentCulture) && !Char.IsDigit(ch))
						{
							sb.Append(UnderScore);
							sb.Append(Char.ToLower(ch, CultureInfo.CurrentCulture));
						}
						else
						{
							sb.Append(ch);
						}
						break;
				}
			}
			return sb.ToString().Split(OrChar);
		}

		private static bool FindFileWithExtensionIn(string dirName, IEnumerable<string> fileExtensions)
		{
			// Look in .hg/store/data
			// Check that the internal directory exists first!
			var internalDirectory = Path.Combine(dirName, HgFolder, Store, Data);
			return Directory.Exists(internalDirectory)
				&& fileExtensions
				.Select(ext => Directory.GetFiles(internalDirectory, ext + InternalExt, SearchOption.TopDirectoryOnly))
				.Any(result => result.Length != 0);
		}

		private static IEnumerable<Tuple<string, string>> GetAllDirectoriesWithRepos()
		{
			var dirs = Directory.GetDirectories(ChorusHubParameters.RootDirectory);
			foreach (var fullDirName in dirs)
			{
				string jsonRepoInfo;
				if (HasRepo(fullDirName, out jsonRepoInfo))
				{
					yield return new Tuple<string, string>(fullDirName, jsonRepoInfo);
				}
			}
		}

		private static bool HasRepo(string dirName, out string jsonRepoInfo)
		{
			jsonRepoInfo = null;
			var hgDir = Path.Combine(dirName, HgFolder);
			if (!Directory.Exists(hgDir))
			{
				return false;
			}
			var repo = HgRepository.CreateOrUseExisting(dirName, new ConsoleProgress());
			var id = repo.Identifier;
			var name = Path.GetFileName(dirName);
			if (id == null)
			{
				id = RepositoryInformation.NEW_REPO;
			}
			jsonRepoInfo = ImitationHubJSONService.MakeJsonString(name, id);
			return true;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="name"></param>
		/// <param name="id"></param>
		/// <returns>true if client should wait for hg to notice</returns>
		public bool PrepareToReceiveRepository(string name, string id)
		{
			var jsonStrings = GetRepositoryInformation(string.Empty);
			var hubInfo = ImitationHubJSONService.ParseJsonStringsToChorusHubRepoInfos(jsonStrings);
			if (hubInfo.Any(info => info.RepoID == id))
			{
				return false;
			}

			// since the repository doesn't exist, create it
			var directory = Path.Combine(ChorusHubParameters.RootDirectory, name);
			var uniqueDir = DirectoryUtilities.GetUniqueFolderPath(directory);
			EventLog.WriteEntry("Application", string.Format("PrepareToReceiveRepository() is preparing a place for '{0}'.", name), EventLogEntryType.Information);
			if (uniqueDir != directory)
			{
				EventLog.WriteEntry("Application", string.Format("{0} already exists! Creating repository for {1} at {2}.", directory, name, uniqueDir), EventLogEntryType.Warning);
			}
			Directory.CreateDirectory(uniqueDir);
			HgRepository.CreateRepositoryInExistingDir(uniqueDir, new ConsoleProgress());
			return true;
		}
	}
}