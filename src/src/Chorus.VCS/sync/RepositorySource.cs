using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;

namespace Chorus.sync
{
	public abstract class RepositorySource
	{
		protected string _uri;
		private string _sourceLabel;

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
			if (uri == "UsbKey")
			{
				return new UsbKeyRepositorySource(uri, sourceName, readOnly);
			}
			if (Directory.Exists(uri))
			{
				return new FilePathRepositorySource(uri, sourceName, readOnly);
			}
			else
				throw new ArgumentException("RepositorySource recognize this kind of uri (" + uri + ")");
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
		public string SourceLabel
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

		public abstract bool CanConnect(string repoName, IProgress progress);

		public abstract string PotentialRepoUri(string repoName, IProgress progress);

		/// <summary>
		/// used with usb source
		/// </summary>
		/// <returns></returns>
		public virtual List<string> GetPossibleCloneUris(string name, IProgress progress)
		{
			return null;
		}


		public override string ToString()
		{
			return SourceLabel;
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
			return Path.Combine(_uri, repoName);
		}

		public override bool CanConnect(string repoName, IProgress progress)
		{
			return Directory.Exists(PotentialRepoUri(repoName, progress));
		}
	}

	public class UsbKeyRepositorySource : RepositorySource
	{
		internal string PathToPretendUsbKeyForTesting;

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
			if (PathToPretendUsbKeyForTesting != null)
			{
				return Path.Combine(PathToPretendUsbKeyForTesting, repoName);
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

			if (PathToPretendUsbKeyForTesting != null)
			{
				string path = Path.Combine(PathToPretendUsbKeyForTesting, repoName);
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