using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Chorus.Utilities;

namespace Chorus.sync
{
	public abstract class RepositorySource
	{
		protected string _uri;
		private string _sourceLabel;
		public enum HardWiredSources{UsbKey};

		/// <summary>
		/// THis will be false for, say, usb-keys or shared internet repos
		/// but true for other people on LANs (maybe?)
		/// </summary>
		private bool _readOnly;

		/// <summary>
		///
		/// </summary>
		/// <param name="uri">examples: http://sil.org/chorus, c:\work, UsbKey, //GregSmith/public/language projects</param>
		/// <param name="sourceName">examples: SIL Language Depot, My Machine, USB Key, Greg</param>
		/// <param name="readOnly">normally false for local repositories (usb, hard drive) and true for other people's repositories</param>
		/// <returns></returns>
		public static RepositorySource Create(string uri, string sourceName, bool readOnly)
		{

			if (uri.Trim().StartsWith("http"))
			{
				return new HttpRepositorySource(uri, sourceName, readOnly);
			}
			else
			{
				return new FilePathRepositorySource(uri, sourceName, readOnly);
			}

		}

		public static RepositorySource Create(HardWiredSources hardWiredSource, string sourceName, bool readOnly)
		{
			switch (hardWiredSource)
			{
				case HardWiredSources.UsbKey:
					return new UsbKeyRepositorySource(HardWiredSources.UsbKey.ToString(), sourceName, readOnly);
				default:
					throw new ArgumentException("RepositorySource does not recognize this kind of source (" + HardWiredSources.UsbKey.ToString() + ")");
			}
		}

		protected RepositorySource(string uri, string sourceLabel, bool readOnly)
		{
			URI = uri;
			_sourceLabel = sourceLabel;
			ReadOnly = readOnly;
		}

		public string URI
		{
			get { return _uri; }
			set { _uri = value; }
		}

		/// <summary>
		/// In the case of a repo sitting on the user's machine, this will be a person's name.
		/// It might also be the name of the web-based repo
		/// </summary>
		public string Name
		{
			get { return _sourceLabel; }
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

	public class HttpRepositorySource : RepositorySource
	{

		public HttpRepositorySource(string uri, string sourceLabel, bool readOnly)
			: base(uri, sourceLabel, readOnly)
		{

		}

		public override string PotentialRepoUri(string repoName, IProgress progress)
		{
			return _uri.Replace("%repoName%", repoName);
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

	public class FilePathRepositorySource : RepositorySource
	{

		public FilePathRepositorySource(string uri, string sourceLabel, bool readOnly)
			: base(uri, sourceLabel, readOnly)
		{

		}

		public override string PotentialRepoUri(string repoName, IProgress progress)
		{
			return _uri.Replace("%repoName%", repoName);
//            return Path.Combine(_uri, repoName);
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

	public class UsbKeyRepositorySource : RepositorySource
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