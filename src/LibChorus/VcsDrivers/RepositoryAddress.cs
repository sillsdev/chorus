using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Chorus.Utilities;
using SIL.UsbDrive;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using SIL.Progress;
using SIL.Reporting;

namespace Chorus.VcsDrivers
{
	public abstract class RepositoryAddress
	{
		/// <summary>
		/// Can be a file path or an http address
		/// </summary>
		public string URI { get; private set; }

		/// <summary>
		/// This can be used in place of the project name, so that path can be specified which will work
		/// with multiple projects.  For example, you could specify a backup location like this:
		/// Path.Combine("e:/chorusBackups/", ProjectNameVariable), which would become e:/chorusBackups/%projectName%.
		/// </summary>
		public const string ProjectNameVariable = "%projectName%";

		/// <summary>
		/// This can be used in place of the name of the medium (USB flash drive, Chorus Hub server, etc)
		/// </summary>
		public const string MediumVariable = "%syncMedium%";

		/// <summary>
		/// This message is displayed when the user tries to create a new repository with the same name as an
		/// unrelated, existing repository.  Because of the way this string is used, two customizations are necessary:
		///  - replace <see cref="MediumVariable"/> with the name of the syncronization medium
		///  - replace {0} and {0} with the URI's of the existing and new repositories, respectively
		/// </summary>
		public const string DuplicateWarningMessage = "Warning: There is a project repository on the " + MediumVariable
			+ " which has the right name ({0}), but it is not related to your project.  So, the program created a"
			+ " separate repository for your project at {1}. This happens when the project on your computer was not"
			+ " created from the repository on the " + MediumVariable + ".\n\nYou can continue working on your own"
			+ " project, but you will need expert help so you can work together without losing your work.\n\nTo work"
			+ " together, only one person can create the shared repository, and all other collaborators must use"
			+ " 'Get Project from Colleague...' to receive and create the project on their computers. Then, all"
			+ " projects will be related to each other and will be able to Send/Receive to the same repository on the "
			+ MediumVariable+ ".";


		/// <summary>
		/// In the case of a repo sitting on the user's machine, this will be a person's name.
		/// It might also be the name of the web-based repo. It also gets the "alias" name, in the case of hg.
		/// </summary>
		public string Name{get; private set;}

		public enum HardWiredSources { UsbKey };

		/// <summary>
		/// THis will be false for, say, usb-keys or shared internet repos
		/// but true for other people on LANs (maybe?)
		/// </summary>
		private bool _readOnly;


		public static RepositoryAddress Create(string name,string uri)
		{
			return Create(name,uri, false);
		}

		/// <summary>
		/// Creates a new HttpRepositoryPath or DirectoryRepositorySource.
		/// </summary>
		/// <param name="name">examples: SIL Language Depot, My Machine, USB Key, Greg</param>
		/// <param name="uri">
		/// examples: http://sil.org/chorus, c:\work, UsbKey, //GregSmith/public/language projects
		/// Note: does not work for ChorusHub
		/// </param>
		/// <param name="readOnly">normally false for local repositories (usb, hard drive) and true for other people's repositories</param>
		/// <returns></returns>
		public static RepositoryAddress Create(string name, string uri, bool readOnly)
		{
			if (uri.Trim().StartsWith("http"))
			{
				return new HttpRepositoryPath(name, uri, readOnly);
			}
			else
			{
				return new DirectoryRepositorySource(name, uri, readOnly);
			}

		}

		public static RepositoryAddress Create(HardWiredSources hardWiredSource, string name, bool readOnly)
		{
			switch (hardWiredSource)
			{
				case HardWiredSources.UsbKey:
					return new UsbKeyRepositorySource(name, HardWiredSources.UsbKey.ToString(), readOnly);
				default:
					throw new ArgumentException("RepositoryAddress does not recognize this kind of source (" + HardWiredSources.UsbKey.ToString() + ")");
			}
		}

		protected RepositoryAddress(string name, string uri, bool readOnly)
		{
			URI = uri;
			Name = name;
			ReadOnly = readOnly;
			IsResumable = IsKnownResumableRepository(uri);
		}

		public static bool IsKnownResumableRepository(string uri)
		{
			return uri.ToLower().Contains("hg-test.languageforge.org") || uri.ToLower().Contains("resumable");
		}



		/// <summary>
		/// THis will be false for, say, usb-keys or shared internet repos
		/// but true for other people on LANs (maybe?)
		/// </summary>
		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}

		public bool IsResumable { get; private set; }

		/// <summary>
		/// Does the user want us to try to sync with this one?
		/// </summary>
		public bool Enabled { get; set; }

		public string Password
		{
			get { return UrlHelper.GetPassword(URI); }
		}

		public string UserName
		{
			get { return UrlHelper.GetUserName(URI); }
		}

		public abstract bool CanConnect(HgRepository localRepository, string projectName, IProgress progress);

		/// <summary>
		/// Gets what the uri of the named repository would be, on this source. I.e., gets the full path.
		/// </summary>
		public abstract string GetPotentialRepoUri(string repoIdentifier, string projectName, IProgress progress);

		public virtual List<string> GetPossibleCloneUris(string repoIdentifier, string name, IProgress progress)
		{
			return null;
		}


		public override string ToString()
		{
			return Name;
		}

		public virtual string GetFullName(string uri)
		{
			return Name;
		}
	} // end class RepositoryAddress

	public class HttpRepositoryPath : RepositoryAddress
	{
		public HttpRepositoryPath(string name, string uri, bool readOnly)
			: base(name, uri, readOnly)
		{
		}

		/// <summary>
		/// Gets what the uri of the named repository would be, on this source. I.e., gets the full path.
		/// </summary>
		public override string GetPotentialRepoUri(string repoIdentifier, string projectName, IProgress progress)
		{
			return URI.Replace(ProjectNameVariable, projectName);
		}

		public override bool CanConnect(HgRepository localRepository, string projectName, IProgress progress)
		{
			//review: i don't know how long this is going to cut it...
			// we really want to know more, like is that repo actually related to us?
			return localRepository.GetCanConnectToRemote(GetPotentialRepoUri(localRepository.Identifier, projectName, progress), progress);
		}

		public override List<string> GetPossibleCloneUris(string repoIdentifier, string projectName, IProgress progress)
		{
			return new List<string>(new string[] { GetPotentialRepoUri(repoIdentifier, projectName, progress) });
		}
	}

	public class ChorusHubRepositorySource : RepositoryAddress
	{
		private readonly List<RepositoryInformation> _sourceRepositoryInformation;

		public ChorusHubRepositorySource(string name, string uri, bool readOnly, IEnumerable<RepositoryInformation> repositoryInformations)
			: base(name, uri, readOnly)
		{
			_sourceRepositoryInformation = new List<RepositoryInformation>(repositoryInformations);
		}

		/// <summary>
		/// Determines whether a repository is a potential match for the project name. The repository must not be null.
		/// </summary>
		/// <param name="repoInfo"></param>
		/// <param name="projectName"></param>
		/// <returns>
		/// true iff the repository's name begins with the project name followed by only a number
		/// </returns>
		private static bool IsMatchingName(RepositoryInformation repoInfo, string projectName)
		{
			int dummy;
			return repoInfo.RepoName.StartsWith(projectName)
				   && int.TryParse(repoInfo.RepoName.Substring(projectName.Length), out dummy);
		}

		private string FormatWarningMessage(string projectName, string repoName)
		{
			return String.Format(DuplicateWarningMessage.Replace(MediumVariable, "Chorus Hub server"),
				URI.Replace(ProjectNameVariable, projectName), URI.Replace(ProjectNameVariable, repoName));
		}

		/// <summary>
		/// Determines a the proper repository name for a project on Chorus Hub, and whether that repository has been
		/// created.  It does not matter if the repository has been initialized or not; it could be an empty hg
		/// repository with an 'id' (within Chorus) of 'newRepo'.
		///
		/// First checks whether a repository has already been initialized for this project (by the hg repository ID).
		/// If not, checks for an uninitialized repository whose name is <paramref name="projectName"/>, or begins with
		/// <paramref name="projectName"/> followed by a number.  If none of the following are found, returns false.
		/// </summary>
		/// <param name="repoIdentifier">the unique repository identifier</param>
		/// <param name="projectName">the name of the project on the user's computer</param>
		/// <param name="matchName">(out) the name or potential name of the project repository on Chorus Hub</param>
		/// <param name="warningMessage">
		/// (out) a warning message if we cannot connect or if we are creating a new repository with a name other than the project name
		/// (because the project name has been taken by an unrelated repository);
		/// null otherwise (subsequent syncs, or we are creating a new repository with the correct name)
		/// </param>
		/// <returns>true iff a repository named <paramref name="matchName"/> exists on the Chorus Hub server</returns>
		private bool TryGetBestRepoMatch(string repoIdentifier, string projectName, out string matchName, out string warningMessage)
		{
			// default repository name is projectName
			matchName = projectName;
			// default is no warning message
			warningMessage = null;

			// Our first (most likely) choice is an existing repository with the correct ID
			var match = _sourceRepositoryInformation.FirstOrDefault(repoInfo => repoInfo.RepoID == repoIdentifier);
			if (match != null)
			{
				matchName = match.RepoName;
				return true;
			}

			// Our next choice is a new repository with the projetct name
			// (which should already exist with the ID "newRepo")
			match = _sourceRepositoryInformation.FirstOrDefault(repoInfo => repoInfo.RepoName == projectName);
			if (match != null && match.IsNew()) // the repository exists and has not been initialized
			{
				matchName = match.RepoName;
				return true;
			}

			// Our next choice is an existing "newRepo" whose name begins with the project name, followed by numbers.
			// The user will be notified of the naming conflict.
			// Enhance pH: check for multiple matches, which may indicate either
			// a race condition or an incomplete creation in the past.
			match = _sourceRepositoryInformation.FirstOrDefault(repoInfo =>
				(repoInfo.IsNew() && IsMatchingName(repoInfo, projectName)));
			if (match != null)
			{
				// We found a repository we can use
				matchName = match.RepoName;
				warningMessage = FormatWarningMessage(projectName, matchName);
				return true;
			}

			// No repository has been created for this project, so we will be unable to connect now. If Chorus Hub and
			// Mercurial are taking longer than we waited to create the repository, the user can try again later.
			warningMessage = new ProjectLabelErrorException(URI.Replace(ProjectNameVariable, projectName)).Message;
			return false;
		}

		/// <summary>
		/// Gets what the uri of the named repository would be, on this source. I.e., gets the full path.
		/// </summary>
		public override string GetPotentialRepoUri(string repoIdentifier, string projectName, IProgress progress)
		{
			string matchName;
			string warningMessage;
			TryGetBestRepoMatch(repoIdentifier, projectName, out matchName, out warningMessage);
			if (warningMessage != null)
			{
				progress.WriteWarning(warningMessage);
			}
			return URI.Replace(ProjectNameVariable, matchName);
		}

		/// <summary>
		/// Find out if ChorusHub can connect or not.
		/// </summary>
		public override bool CanConnect(HgRepository localRepository, string projectName, IProgress progress)
		{
			// It can connect for either of these reasons:
			//	1. 'localRepository' Identifier matches one of the ids of _sourceRepositoryInformation. (Name may not be the same as 'projectName')
			//	2. The name of one of _sourceRepositoryInformation matches or begins with 'projectName' AND the id is 'newRepo'.
			//     (A clone of this isn't useful.)
			string dummy;
			return TryGetBestRepoMatch(localRepository.Identifier, projectName, out dummy, out dummy);
		}

		public override List<string> GetPossibleCloneUris(string repoIdentifier, string projectName, IProgress progress)
		{
			return new List<string>(new[] { GetPotentialRepoUri(repoIdentifier, projectName, progress) });
		}
	}

	/// <summary>
	/// This class was created to support the now-obsolete option of using a shared network folder as a repository source.
	/// Although this did not prove reliable enough to keep using (at least with Mercurial 1.5), DirectoryRepositorySource
	/// continues to have a marginal usefulness in supporting some tests that would otherwise be difficult to do without
	/// a USB stick or ChorusHub available.
	/// </summary>
	public class DirectoryRepositorySource : RepositoryAddress
	{
		private readonly string _networkMachineSpecifier;
		private readonly string _alternativeMachineSpecifier;

		public DirectoryRepositorySource(string name, string uri, bool readOnly)
			: base(name, uri, readOnly)
		{
			_networkMachineSpecifier = new string(Path.DirectorySeparatorChar, 2);
			_alternativeMachineSpecifier = new string(Path.AltDirectorySeparatorChar, 2);
		}

		/// <summary>
		/// Gets what the uri of the named repository would be, on this source. I.e., gets the full path.
		/// </summary>
		public override string GetPotentialRepoUri(string repoIdentifier, string projectName, IProgress progress)
		{
			return URI.Replace(ProjectNameVariable, projectName).TrimEnd(Path.DirectorySeparatorChar);
		}

		public override bool CanConnect(HgRepository localRepository, string projectName, IProgress progress)
		{
			var path = GetPotentialRepoUri(localRepository.Identifier, projectName, progress);
			if (URI.StartsWith(_networkMachineSpecifier) || URI.StartsWith(_alternativeMachineSpecifier))
			{
				progress.WriteStatus("Checking to see if we can connect with {0}...", path);
				if (!NetworkInterface.GetIsNetworkAvailable())
				{
					progress.WriteWarning("This machine does not have a live network connection.");
					return false;
				}
			}

			var result = Directory.Exists(path);
			if (!result)
			{
				progress.WriteWarning("Cannot find the specified file folder.");
			}
			return result;
		}

		public bool LooksLikeLocalDirectory
		{
			get { return !(this.URI.StartsWith(_networkMachineSpecifier)); }
		}

		public override List<string> GetPossibleCloneUris(string repoIdentifier, string projectName, IProgress progress)
		{
			return new List<string>(new string[]{GetPotentialRepoUri(repoIdentifier, projectName, progress)});
		}
	}

	public class UsbKeyRepositorySource : RepositoryAddress
	{
		private static string _rootDirForAllSourcesDuringUnitTest;

		/// <summary>
		/// also creates the directory
		/// </summary>
		/// <param name="pathToRootForAllSources"></param>
		static public void SetRootDirForAllSourcesDuringUnitTest(string pathToRootForAllSources)
		{
			_rootDirForAllSourcesDuringUnitTest = pathToRootForAllSources;
			Directory.CreateDirectory(RootDirForUsbSourceDuringUnitTest);
		}
		static public string RootDirForUsbSourceDuringUnitTest
		{
			get
			{
				if(_rootDirForAllSourcesDuringUnitTest ==null)
					return null;
				return Path.Combine(_rootDirForAllSourcesDuringUnitTest, "usb");
			}
		}

		public UsbKeyRepositorySource(string sourceLabel, string uri, bool readOnly)
			: base(sourceLabel, uri, readOnly)
		{

		}

		public override string GetFullName(string address)
		{
			var root = Path.GetPathRoot(address);
			foreach (DriveInfo drive in DriveInfo.GetDrives())
			{
				if(drive.RootDirectory.Name == root)
					return Name + "(" +  drive.VolumeLabel + ")";
			}
			Debug.Fail("Why didn't we find the drive?");
			return Name;
	   }

		/// <summary>
		/// Get a path to use with the version control
		/// </summary>
		/// <returns>null if can't find a usb key</returns>
		public override string GetPotentialRepoUri(string repoIdentifier, string projectName, IProgress progress)
		{
			if (RootDirForUsbSourceDuringUnitTest != null)
			{
				return Path.Combine(RootDirForUsbSourceDuringUnitTest, projectName);
			}

			var drives = UsbDriveInfo.GetDrives();
			if (drives.Count == 0)
				return null;

			//first try to find this repository on one of the usb keys
			foreach (var drive in drives)
			{
				// Look at all root directories, matching or not,
				// since Lift Bridge may be trying to sync to a project with a different name.
				// Also look for a root folder called "Shared-Dictionaries",
				// which would contain one or more directories with repos in them.
				var foldersWithRepos = CollectPathsWithRepositories(drive.RootDirectory.FullName);
				var sharedFolderPath = Path.Combine(drive.RootDirectory.FullName, "Shared-Dictionaries");
				if (Directory.Exists(sharedFolderPath))
					foldersWithRepos.AddRange(CollectPathsWithRepositories(sharedFolderPath));

				foreach (var path in foldersWithRepos)
				{
					try
					{
						if (File.Exists(Path.Combine(path, "SharedRepositoryInfo.xml")))
						{
							progress.WriteVerbose("Not checking folder that looks like paratext project: "+path);
							continue;
						}

						// Need to create an HgRepository for each so we can get its Id.
						var usbRepo = new HgRepository(path, progress);
						if (usbRepo.Identifier == null)
						{
							// Null indicates a new repo, with no commits yet.
							continue;
						}
						else if (repoIdentifier.ToLowerInvariant() == usbRepo.Identifier.ToLowerInvariant())
						{
							return path;
						}
					}
					catch (UserCancelledException )
					{   // deal with this separately to avoid an error report - the user didn't ask for that.
						throw; // if not thrown now, more folders could be searched in FLExBridge
					}
					catch (Exception e)
					{
						if(e.Message.Contains("not supported"))
						{
							progress.WriteWarning("Could not check the repository at {0} because it has some unsupported feature (e.g. made with a new versin of this or some other software?). Error was: {1}", path, e.Message);
							continue;
						}
						ErrorReport.ReportNonFatalExceptionWithMessage(e, "Error while processing USB folder '{0}'", path);
					}
				}
			}
			return string.Empty;
		}

		private static List<string> CollectPathsWithRepositories(string path)
		{
			return (from directory in DirectoryHelper.GetSafeDirectories(path)
					where Directory.Exists(Path.Combine(directory, ".hg"))
									select directory).ToList();
		}

		public override List<string> GetPossibleCloneUris(string repoIdentifier, string projectName, IProgress progress)
		{
			progress.WriteVerbose("Looking for USB flash drives to receive clone...");
			List<string>  urisToTryCreationAt = new List<string>();

			if (RootDirForUsbSourceDuringUnitTest != null)
			{
				string path = Path.Combine(RootDirForUsbSourceDuringUnitTest, projectName);
				//     Debug.Assert(Directory.Exists(path));

				urisToTryCreationAt.Add(path);
				return urisToTryCreationAt;
			}

			var drives = UsbDriveInfo.GetDrives();

			if (drives.Count == 0)
				return null;

			// didn't find an existing one, so just create on on the first usb key we can
			foreach (var drive in drives)
			{
				urisToTryCreationAt.Add(Path.Combine(drive.RootDirectory.FullName, projectName));
			}
			return urisToTryCreationAt;
		}

		public override bool CanConnect(HgRepository localRepository, string projectName, IProgress progress)
		{
		   // progress.WriteStatus("Looking for USB flash drives with existing repositories...");
			string path= GetPotentialRepoUri(localRepository.Identifier, projectName, progress);
			return (path != null) && Directory.Exists(path);
		}

	}
}
