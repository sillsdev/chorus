using System;
using System.Collections.Generic;
using System.IO;
using Chorus.Utilities;

namespace Chorus.sync
{
	public abstract class RepositoryPath
	{
		/// <summary>
		/// Can be a file path or an http address
		/// </summary>
		public string URI { get; set; }

		/// <summary>
		/// This can be used in place of the project name, so that path can be specified which will work
		/// with multiple projects.  For example, you could specify a backup location like this:
		/// Path.Combine("e:/chorusBackups/", RepositoryNameVariable), which would become e:/chorusBackups/%repoName%.
		/// </summary>
		public const string RepositoryNameVariable = "%repoName%";

		/// <summary>
		/// In the case of a repo sitting on the user's machine, this will be a person's name.
		/// It might also be the name of the web-based repo. It also gets the "alias" name, in the case of hg.
		/// </summary>
		public string Name{get;set;}

		public enum HardWiredSources { UsbKey };

		/// <summary>
		/// THis will be false for, say, usb-keys or shared internet repos
		/// but true for other people on LANs (maybe?)
		/// </summary>
		private bool _readOnly;

		/// <summary>
		///
		/// </summary>
		/// <param name="uri">examples: http://sil.org/chorus, c:\work, UsbKey, //GregSmith/public/language projects</param>
		/// <param name="name">examples: SIL Language Depot, My Machine, USB Key, Greg</param>
		/// <param name="readOnly">normally false for local repositories (usb, hard drive) and true for other people's repositories</param>
		/// <returns></returns>
		public static RepositoryPath Create(string uri, string name, bool readOnly)
		{

			if (uri.Trim().StartsWith("http"))
			{
				return new HttpRepositoryPath(uri, name, readOnly);
			}
			else
			{
				return new DirectoryRepositorySource(uri, name, readOnly);
			}

		}

		public static RepositoryPath Create(HardWiredSources hardWiredSource, string name, bool readOnly)
		{
			switch (hardWiredSource)
			{
				case HardWiredSources.UsbKey:
					return new UsbKeyRepositorySource(HardWiredSources.UsbKey.ToString(), name, readOnly);
				default:
					throw new ArgumentException("RepositoryPath does not recognize this kind of source (" + HardWiredSources.UsbKey.ToString() + ")");
			}
		}

		protected RepositoryPath(string uri, string name, bool readOnly)
		{
			URI = uri;
			Name = name;
			ReadOnly = readOnly;
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

		/// <summary>
		/// Does the user want us to try to sync with this one?
		/// </summary>
		public bool Enabled { get; set; }

		public abstract bool CanConnect(string repoName, IProgress progress);

		/// <summary>
		/// Gets what the uri of the named repository would be, on this source. I.e., gets the full path.
		/// </summary>
		public abstract string PotentialRepoUri(string repoName, IProgress progress);

		 public virtual List<string> GetPossibleCloneUris(string name, IProgress progress)
		{
			return null;
		}


		public override string ToString()
		{
			return Name;
		}
	}

	public class HttpRepositoryPath : RepositoryPath
	{
		public HttpRepositoryPath(string uri, string name, bool readOnly)
			: base(uri, name, readOnly)
		{

		}

		/// <summary>
		/// Gets what the uri of the named repository would be, on this source. I.e., gets the full path.
		/// </summary>
		public override string PotentialRepoUri(string repoName, IProgress progress)
		{
			return URI.Replace(RepositoryNameVariable, repoName);
		}

		public override bool CanConnect(string repoName, IProgress progress)
		{
			return false;//todo
		}

		public override List<string> GetPossibleCloneUris(string repoName, IProgress progress)
		{
			return new List<string>(new string[] { PotentialRepoUri(repoName, progress) });
		}
	}

	public class DirectoryRepositorySource : RepositoryPath
	{

		public DirectoryRepositorySource(string uri, string sourceLabel, bool readOnly)
			: base(uri, sourceLabel, readOnly)
		{

		}

		/// <summary>
		/// Gets what the uri of the named repository would be, on this source. I.e., gets the full path.
		/// </summary>
		public override string PotentialRepoUri(string repoName, IProgress progress)
		{
			return URI.Replace(RepositoryNameVariable, repoName);
		}

		public override bool CanConnect(string repoName, IProgress progress)
		{
			return Directory.Exists(PotentialRepoUri(repoName, progress));
		}

		public override List<string> GetPossibleCloneUris(string repoName, IProgress progress)
		{
			return new List<string>(new string[]{PotentialRepoUri(repoName, progress)});
		}
	}


	public class UsbKeyRepositorySource : RepositoryPath
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

		public UsbKeyRepositorySource(string uri, string sourceLabel, bool readOnly)
			: base(uri, sourceLabel, readOnly)
		{

		}


		/// <summary>
		/// Get a path to use with the version control
		/// </summary>
		/// <returns>null if can't find a usb key</returns>
		public override string PotentialRepoUri(string repoName, IProgress progress)
		{
			if (RootDirForUsbSourceDuringUnitTest != null)
			{
				return Path.Combine(RootDirForUsbSourceDuringUnitTest, repoName);
			}

			List<DriveInfo> drives = Chorus.Utilities.UsbUtilities.GetLogicalUsbDisks();
			if (drives.Count == 0)
				return null;

			//first try to find this repository on one of the usb keys
			foreach (DriveInfo drive in drives)
			{
				string pathOnUsb = Path.Combine(drive.RootDirectory.FullName, repoName);
				if (Directory.Exists(pathOnUsb))
				{
					return pathOnUsb;
				}
			}
			return null;
		}

		public override List<string> GetPossibleCloneUris(string repoName, IProgress progress)
		{
			progress.WriteStatus("Looking for usb keys to recieve clone...");
			List<string>  urisToTryCreationAt = new List<string>();

			if (RootDirForUsbSourceDuringUnitTest != null)
			{
				string path = Path.Combine(RootDirForUsbSourceDuringUnitTest, repoName);
		   //     Debug.Assert(Directory.Exists(path));

				urisToTryCreationAt.Add(path);
				return urisToTryCreationAt;
			}

			List<DriveInfo> drives = Chorus.Utilities.UsbUtilities.GetLogicalUsbDisks();

			if (drives.Count == 0)
				return null;

			// didn't find an existing one, so just create on on the first usb key we can
			foreach (DriveInfo drive in drives)
			{
				urisToTryCreationAt.Add(Path.Combine(drive.RootDirectory.FullName, repoName));
			}
			return urisToTryCreationAt;
		}



		public override bool CanConnect(string repoName, IProgress progress)
		{
			progress.WriteStatus("Looking for usb keys with existing repositories...");
			string path= PotentialRepoUri(repoName, progress);
			return (path != null) && Directory.Exists(path);
		}


	}
}