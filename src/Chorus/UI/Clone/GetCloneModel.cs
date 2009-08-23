using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chorus.Utilities;
using Chorus.Utilities.UsbDrive;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Clone
{
	public class GetCloneModel
	{
		private readonly string _parentDirectoryToPutCloneIn;
		public event EventHandler LoadList;

		public GetCloneModel(string parentDirectoryToPutCloneIn)
		{
			_parentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
			DriveInfoRetriever = new RetrieveUsbDriveInfo();
		}

		/// <summary>
		/// Use this to insert an artificial drive info system for unit tests
		/// </summary>
		public IRetrieveUsbDriveInfo DriveInfoRetriever { get; set; }

		public IEnumerable<string> GetDirectoriesWithMecurialRepos()
		{
			foreach (var drive in DriveInfoRetriever.GetDrives())
			{
				foreach (var dir in Directory.GetDirectories(drive.RootDirectory.FullName))
				{
					if (Directory.Exists(Path.Combine(dir, ".hg")))
					{
						yield return dir;
					}
					else //we'll look just at the next level down
					{
						foreach (var subdir in Directory.GetDirectories(dir))
						{
							if (Directory.Exists(Path.Combine(subdir, ".hg")))
							{
								yield return subdir;
							}
						}
					}
				}
			}

		}

		public string GetClone(string sourcePath, IProgress progress)
		{
			var repo = new HgRepository(sourcePath, progress);

			var path = Path.Combine(_parentDirectoryToPutCloneIn, Path.GetFileName(sourcePath));
			repo.Clone(path);
			return path;
		}

	}
}
