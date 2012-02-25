using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Chorus.Utilities;
using Chorus.Utilities.UsbDrive;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;
using Palaso.Reporting;

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
		///
		/// </summary>
		/// <param name="uri">examples: http://sil.org/chorus, c:\work, UsbKey, //GregSmith/public/language projects</param>
		/// <param name="name">examples: SIL Language Depot, My Machine, USB Key, Greg</param>
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
			return uri.ToLower().Contains("languageforge.org") || uri.ToLower().Contains("resumable");
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
	}

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

	public class DirectoryRepositorySource : RepositoryAddress
	{

		public DirectoryRepositorySource(string name, string uri, bool readOnly)
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
			var path = GetPotentialRepoUri(localRepository.Identifier, projectName, progress);
			if (this.URI.StartsWith(@"\\"))
			{
				progress.WriteStatus("Checking to see if we can connect with {0}...", path);
			}
			return Directory.Exists(path);
		}

		public bool LooksLikeLocalDirectory
		{
			get { return !(this.URI.StartsWith(@"\\")); }
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

//        private string RootDirForUsbSourceDuringUnitTest
//        {
//            get {
//                if(_rootDirForAllSourcesDuringUnitTest ==null)
//                    return null;
//                return
//        }

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

			var drives = Chorus.Utilities.UsbDrive.UsbDriveInfo.GetDrives();
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
					catch (Exception e)
					{
						ErrorReport.ReportNonFatalExceptionWithMessage(e, "Error while processing USB folder '{0}'", path);
					}
				}
			}
			return string.Empty;
		}

		private static List<string> CollectPathsWithRepositories(string path)
		{
			return (from directory in DirectoryUtilities.GetSafeDirectories(path)
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