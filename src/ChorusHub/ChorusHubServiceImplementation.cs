using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
		/// The two names are "name" and "id".
		/// </summary>
		/// <example>searchUrl: "scheme://path?filePattern=*.lift|*.CustomProperties"</example>
		/// <example>returned repo info string: {"name": "someProject", "id": "123abc"}</example>
		/// <param name="searchUrl"></param>
		/// <returns></returns>
		public IEnumerable<string> GetRepositoryInformation(string searchUrl)
		{
			Progress.WriteMessage("Client requested repository information.");

			var allDirectoryTuples = GetAllDirectoriesWithRepos();
			if (string.IsNullOrEmpty(searchUrl))
			{
				return allDirectoryTuples.Select(dirInfo => dirInfo.Item2); // return the JSON strings
			}
			try
			{
				var searchPatternString = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, FilePattern, string.Empty);
				Progress.WriteMessage("Client requested repositories matching {0}.", searchPatternString);
				return CombRepositoriesForMatchingNames(allDirectoryTuples, searchPatternString);
			}
			catch (ApplicationException e)
			{
				// Url parser couldn't parse the url.
				Progress.WriteMessage("GetRepositoryInformation(): " + e.Message);
				return new List<string>();
			}
		}

		private IEnumerable<string> CombRepositoriesForMatchingNames(
			IEnumerable<Tuple<string, string>> allDirectories, string queries)
		{
			if (string.IsNullOrEmpty(queries))
			{
				Progress.WriteMessage("Client search string contained only unknown keys or empty values.");
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

		private bool FindFileWithExtensionIn(string dirName, IEnumerable<string> fileExtensions)
		{
			// Look in .hg/store/data
			// Check that the internal directory exists first!
			var internalDirectory = Path.Combine(dirName, HgFolder, Store, Data);
			if (!Directory.Exists(internalDirectory))
			{
				return false;
			}
			foreach (var ext in fileExtensions)
			{
				var result = Directory.GetFiles(internalDirectory, ext + InternalExt,
												SearchOption.TopDirectoryOnly);
				if (result.Length != 0)
				{
					return true;
				}
			}
			return false;
			//return fileExtensions.Select(extension =>
			//    Directory.GetFiles(internalDirectory, extension + InternalExt, SearchOption.TopDirectoryOnly))
			//    .Any(fileList => fileList.Any());
		}

		private static IEnumerable<Tuple<string, string>> GetAllDirectoriesWithRepos()
		{
			var dirs = Directory.GetDirectories(ChorusHubService.Parameters.RootDirectory);
			foreach (var fullDirName in dirs)
			{
				string jsonRepoInfo;
				if (ToTheBestOfOurKnowledgeANonEmptyHgRepoExists(fullDirName, out jsonRepoInfo))
				{
					yield return new Tuple<string, string>(fullDirName, jsonRepoInfo);
				}
			}
		}

		private static bool ToTheBestOfOurKnowledgeANonEmptyHgRepoExists(string dirName, out string jsonRepoInfo)
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
			if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
			{
				return false;
			}
			jsonRepoInfo = ImitationHubJSONService.MakeJsonString(name, id);
			return true;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="name"></param>
		/// <returns>true if client should wait for hg to notice</returns>
		public bool PrepareToReceiveRepository(string name)
		{
			// Enhance GJM: If it turns out that the caller has the repoID, it'd be better to use that here!
			var jsonStrings = GetRepositoryInformation(string.Empty);
			var hubInfo = ImitationHubJSONService.ParseJsonStringsToChorusHubRepoInfos(jsonStrings);
			if (hubInfo.Any(info => info.RepoName == name))
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