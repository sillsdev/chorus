using System;
using System.IO;
using Chorus.VcsDrivers.Git;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers
{
	/// <summary>
	/// Factory for creating implementations of the IDVCSRepository interface.
	/// </summary>
	public static class DVCSFactory
	{
		/// <summary>
		/// Create an instance of the IDVCSRepository interface, based on the repository type found in <paramref name="path"/>.
		///
		/// If there is no repository, then create the default one (currently HgRepository).
		/// </summary>
		public static IDVCSRepository Create(string path, IProgress progressIndicator)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");
			if (!Directory.Exists(path))
				throw new ArgumentException("Folder does not exist for: " + path);

			if (Directory.Exists(Path.Combine(path, ".hg")))
				return new HgRepository(path, progressIndicator);

			if (Directory.Exists(Path.Combine(path, ".git")))
				return new GitRepository(path, progressIndicator);

			return new HgRepository(path, progressIndicator);
		}
	}
}
