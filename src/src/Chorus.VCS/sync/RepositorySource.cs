using System;
using System.Collections.Generic;
using System.IO;
using Chorus.Utilities;

namespace Chorus.sync
{
	public abstract class RepositorySource
	{
		protected string _uri;
		private string _sourceName;

		/// <summary>
		/// THis will be false for, say, usb-keys or shared internet repos
		/// but true for other people on LANs (maybe?)
		/// </summary>
		private bool _readOnly;

		public static RepositorySource Create(string uri, string repoName, bool readOnly)
		{
			if(uri=="UsbKey")
			{
				return new UsbKeyRepositorySource(uri, repoName, readOnly);
			}
			else
				throw new ArgumentException("RepositorySource doesn't understand that kind of uri yet");
		}

		protected RepositorySource(string uri, string repoName, bool readOnly)
		{
			URI = uri;
			_sourceName = repoName;
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
		public string SourceName
		{
			get { return _sourceName; }
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

		public virtual string ResolveUri(string name, IProgress progress)
		{
			return _uri;// review: haven't decided yet if the uri contain the actual repo name. if not, needs to be added here
		}

		public virtual bool ShouldCreateClone(string name, IProgress progress, out string resolvedUriToCreateAt)
		{
			resolvedUriToCreateAt = null;
			return false;
		}
	}

	public class UsbKeyRepositorySource : RepositorySource
	{
		internal string PathToPretendUsbKeyForTesting;

		public UsbKeyRepositorySource(string uri, string repoName, bool readOnly)
			: base(uri, repoName, readOnly)
		{

		}

		/// <summary>
		/// Get a path to use with the version control
		/// </summary>
		/// <returns>null if can't find a usb key</returns>
		public override string ResolveUri(string repoName, IProgress progress)
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

		public override bool ShouldCreateClone(string repoName, IProgress progress, out string resolvedUriToCreateAt)
		{
			resolvedUriToCreateAt = null;

			if (PathToPretendUsbKeyForTesting != null)
			{
				resolvedUriToCreateAt = Path.Combine(PathToPretendUsbKeyForTesting, repoName);
				return !Directory.Exists(resolvedUriToCreateAt);
			}

			if (CanConnect(repoName, progress))
				return false;

			List<DriveInfo> drives = Chorus.Utilities.UsbUtilities.GetLogicalUsbDisks();

			if (drives.Count == 0)
				return false;

			// didn't find an existing one, so just create on on the first usb key we can
			foreach (DriveInfo drive in drives)
			{
				try
				{
					progress.WriteMessage("Creating repository on {0}", drive.Name);
					resolvedUriToCreateAt = Path.Combine(drive.RootDirectory.FullName, repoName);

				}
				catch (Exception error)
				{
					progress.WriteMessage("Failed to create repository there.  {0}", error.Message);
				}
			}
			return false;
		}



		public override bool CanConnect(string repoName, IProgress progress)
		{
			return ResolveUri(repoName, progress) != null;
		}


	}
}