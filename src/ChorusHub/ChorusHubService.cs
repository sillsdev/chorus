using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Chorus.ChorusHub;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress;

namespace ChorusHub
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	public class ChorusHubService : IChorusHubService
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
		private const string TipIdFolder = "tipIds";

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
			return GetRepositoryInformation(searchUrl, true);
		}

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
		public string GetRepositoryInformationWithoutIds(string searchUrl)
		{
			return GetRepositoryInformation(searchUrl, false);
		}

		/// <summary>
		/// Returns information about the Hg repositories that the ChorusHub knows about.
		/// 
		/// Similar to GetRepositoryInformation except the value of the id field is the id
		/// of the tip revision of the repository.
		/// 
		/// For the returned tip to be accurate, clients using this method have to call
		/// PutFile(IChorusHubService.tipIdFolder, projectName, tipId) after doing a push.
		/// </summary>
		public string GetRepositoryInformationWithTipIds(string searchUrl)
		{
			List<RepositoryInformation> results = new List<RepositoryInformation>();
			foreach (RepositoryInformation repoInfo in
				ImitationHubJSONService.ParseJsonStringsToChorusHubRepoInfos(GetRepositoryInformation(searchUrl, false)))
			{
				string tipId = GetFileAsText(TipIdFolder, repoInfo.RepoName);
				if (tipId == null)
				{
					tipId = GetTipId(repoInfo.RepoName);
					PutFileFromText(TipIdFolder, repoInfo.RepoName, tipId);
				}
				results.Add(new RepositoryInformation(repoInfo.RepoName, tipId));
			}

			return string.Join("/", results.Select(r => ImitationHubJSONService.MakeJsonString(r.RepoName, r.RepoID)));
		}

		///  <summary>
		///  Returns information about the Hg repositories that the ChorusHub knows about.
		/// 
		///  The search can be trimmed by use of the 'searchUrl' parameter.
		///  Everything about the searchUrl except the query string is fabricated
		///  by the ChorusHubClient. The query string is fed in by the application.
		/// 
		///  The possible search values are:
		///  filePattern -- This key can have multiple values separated by the '|' character
		/// 
		///  Each repository generates a JSON string consisting of two name/value pairs.
		///  The two names are "name" and "id". The JSON strings are concatenated with / between.
		///  (An earlier version returned an enumeration of json strings. But Mono could not
		///  marshal this.)
		///  </summary>
		///  <example>searchUrl: "scheme://path?filePattern=*.lift|*.CustomProperties"</example>
		///  <example>returned repo info string: {"name": "someProject", "id": "123abc"}</example>
		/// <remarks>A new (empty repo) will hav the folder name as 'name', and the id as 'newRepo'</remarks>
		///  <returns></returns>
		private string GetRepositoryInformation(string searchUrl, bool getIds)
		{
			//EventLog.WriteEntry("Application", "Client requested repository information.", EventLogEntryType.Information);

			var allDirectoryTuples = GetAllDirectoriesWithRepos(getIds);
			if (string.IsNullOrEmpty(searchUrl))
			{
				return string.Join("/", allDirectoryTuples.Select(dirInfo => dirInfo.Item2)); // return the JSON strings
			}
			try
			{
				var searchPatternString = UrlHelper.GetValueFromQueryStringOfRef(searchUrl, FilePattern, string.Empty);
				//EventLog.WriteEntry("Application", string.Format("Client requested repositories matching {0}.", searchPatternString), EventLogEntryType.Information);
				return string.Join("/", CombRepositoriesForMatchingNames(allDirectoryTuples, searchPatternString).ToArray());
			}
			catch (ApplicationException e)
			{
				// Url parser couldn't parse the url.
				//EventLog.WriteEntry("Application", "GetRepositoryInformation(): " + e.Message, EventLogEntryType.Warning);
				return "";
			}
		}

		private IEnumerable<string> CombRepositoriesForMatchingNames(
			IEnumerable<Tuple<string, string>> allDirectories, string queries)
		{
			if (string.IsNullOrEmpty(queries))
			{
				//EventLog.WriteEntry("Application", "Client search string contained only unknown keys or empty values.", EventLogEntryType.Warning);
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

		private static IEnumerable<Tuple<string, string>> GetAllDirectoriesWithRepos(bool getIds)
		{
			var dirs = Directory.GetDirectories(ChorusHubOptions.RootDirectory);
			foreach (var fullDirName in dirs)
			{
				string jsonRepoInfo;
				if (HasRepo(fullDirName, getIds, out jsonRepoInfo))
				{
					yield return new Tuple<string, string>(fullDirName, jsonRepoInfo);
				}
			}
		}

		private static bool HasRepo(string dirName, bool getId, out string jsonRepoInfo)
		{
			jsonRepoInfo = null;
			var hgDir = Path.Combine(dirName, HgFolder);
			if (!Directory.Exists(hgDir))
			{
				return false;
			}

			string id = "";
			if (getId)
			{
				var repo = HgRepository.CreateOrUseExisting(dirName, new ConsoleProgress());
				id = repo.Identifier ?? RepositoryInformation.NEW_REPO;
			}
			string name = Path.GetFileName(dirName);
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
			if (!string.IsNullOrEmpty(id))
			{
				var jsonStrings = GetRepositoryInformation(string.Empty);
				var hubInfo = ImitationHubJSONService.ParseJsonStringsToChorusHubRepoInfos(jsonStrings);
				if (hubInfo.Any(info => info.RepoID == id))
				{
					return false;
				}
			}

			// since the repository doesn't exist, create it
			var directory = Path.Combine(ChorusHubOptions.RootDirectory, name);
			var uniqueDir = DirectoryUtilities.GetUniqueFolderPath(directory);
			//EventLog.WriteEntry("Application", string.Format("PrepareToReceiveRepository() is preparing a place for '{0}'.", name), EventLogEntryType.Information);
			if (uniqueDir != directory)
			{
				//EventLog.WriteEntry("Application", string.Format("{0} already exists! Creating repository for {1} at {2}.", directory, name, uniqueDir), EventLogEntryType.Warning);
			}
			Directory.CreateDirectory(uniqueDir);
			HgRepository.CreateRepositoryInExistingDir(uniqueDir, new ConsoleProgress());
			return true;
		}

		public byte[] GetFileRevision(string repositoryName, string fileRelativePath, string revisionStr)
		{
			string directory = Path.Combine(ChorusHubOptions.RootDirectory, repositoryName);
			HgRepository repo = new HgRepository(directory, new NullProgress());
			Revision revision = repo.GetRevision(revisionStr);
			if (revision == null)
				return null;

			string tempPath = null;
			try
			{
				tempPath = repo.RetrieveHistoricalVersionOfFile(fileRelativePath, revisionStr);
				return File.ReadAllBytes(tempPath);
			}
			catch (ApplicationException)
			{
				return null; // file cannot be found in revision
			}
			finally
			{
				if (!string.IsNullOrEmpty(tempPath))
					File.Delete(tempPath);
			}
		}

		public string Verify(string repositoryName)
		{
			string directory = Path.Combine(ChorusHubOptions.RootDirectory, repositoryName);
			HgRepository repo = new HgRepository(directory, new NullProgress());
			return repo.Verify();
		}

		public bool Rename(string repositoryName, string newName)
		{
			string directory = Path.Combine(ChorusHubOptions.RootDirectory, repositoryName);
			string newDirectory = Path.Combine(ChorusHubOptions.RootDirectory, newName);

			if (Directory.Exists(newDirectory))
				return false;

			try
			{
				Directory.Move(directory, newDirectory);
			}
			catch
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Writes the given contents to the specified file - the folder will be created if it doesn't
		/// exist.
		/// </summary>
		public void PutFileFromText(string folder, string fileName, string contents)
		{
			string folderPath = Path.Combine(ChorusHubOptions.RootDirectory, folder);
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);
			string filePath = Path.Combine(folderPath, fileName);
			File.WriteAllText(filePath, contents);
		}

		/// <summary>
		/// Writes the given contents to the specified file - the folder will be created if it doesn't
		/// exist.
		/// </summary>
		public void PutFileFromBytes(string folder, string fileName, byte[] contents)
		{
			string folderPath = Path.Combine(ChorusHubOptions.RootDirectory, folder);
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);
			string filePath = Path.Combine(folderPath, fileName);
			File.WriteAllBytes(filePath, contents);
		}

		/// <summary>
		/// Gets the contents of the specified file as a string.
		/// </summary>
		/// <returns>contents of file or null if file doesn't exist</returns>
		public string GetFileAsText(string folder, string fileName)
		{
			string filePath = Path.Combine(ChorusHubOptions.RootDirectory, folder, fileName);
			return File.Exists(filePath) ? File.ReadAllText(filePath) : null;
		}

		/// <summary>
		/// Gets the contents of the specified file as a byte array.
		/// </summary>
		/// <returns>contents of file or null if file doesn't exist</returns>
		public byte[] GetFileAsBytes(string folder, string fileName)
		{
			string filePath = Path.Combine(ChorusHubOptions.RootDirectory, folder, fileName);
			return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
		}

		private string GetTipId(string repositoryName)
		{
			string directory = Path.Combine(ChorusHubOptions.RootDirectory, repositoryName);
			HgRepository repo = new HgRepository(directory, new NullProgress());
			Revision tip = repo.GetTip();
			return tip != null ? tip.Number.Hash : "";
		}
	}
}