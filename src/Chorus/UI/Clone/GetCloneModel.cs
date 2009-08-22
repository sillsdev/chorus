using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chorus.Utilities;
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
		}

		public IEnumerable<string> GetDirectoriesWithMecurialRepos()
		{
			foreach (var drive in Chorus.Utilities.UsbDrive.UsbDriveInfo.GetDrives())
			{
				foreach (var dir in Directory.GetDirectories(drive.RootDirectory.FullName))
				{
					if (Directory.Exists(Path.Combine(dir, ".hg")))
					{
						yield return dir;
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
