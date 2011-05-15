using System;
using System.IO;
using Chorus.Properties;
using Chorus.VcsDrivers.Git;
using Chorus.VcsDrivers.Mercurial;
using Palaso.CommandLineProcessing;
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

			var newDefaultRepo = new HgRepository(path, progressIndicator);
			newDefaultRepo.Init(path, progressIndicator);
			return newDefaultRepo;
		}

		/// <summary>
		/// Given a file path or directory path, first try to find an existing repository at this
		/// location or in one of its parents.  If not found, create the current defautl one at this location.
		/// </summary>
		public static IDVCSRepository CreateOrLocate(string startingPointForPathSearch, IProgress progress)
		{
#if notyet
			if (!Directory.Exists(startingPointForPathSearch) && !File.Exists(startingPointForPathSearch))
				throw new ArgumentException(AnnotationImages.kFileOrFolderNotFound, startingPointForPathSearch);

			if (!Directory.Exists(startingPointForPathSearch)) // if it's a file... we need a directory
				startingPointForPathSearch = Path.GetDirectoryName(startingPointForPathSearch);

			var root = GetRepositoryRoot(ExecuteErrorsOk("root", startingPointForPathSearch, 100, progress));
			if (!string.IsNullOrEmpty(root))
				return Create(root, progress);

			/*
			 I'm leaning away from this intervention at the moment.
				string newRepositoryPath = AskUserForNewRepositoryPath(startingPath);

			 Let's see how far we can get by just silently creating it, and leave it to the future
			 or user documentation/training to know to set up a repository at the level they want.
			*/
			var newRepositoryPath = startingPointForPathSearch;

			if (!string.IsNullOrEmpty(startingPointForPathSearch) && Directory.Exists(newRepositoryPath))
			{
				//review: Machine name would be more accurate, but most people have, like "Compaq" as their machine name
				//but in any case, this is just a default until they set the name explicity
				var repo = Create(newRepositoryPath, progress);
				repo.Init(newRepositoryPath, progress);
				return repo;
			}
#endif

			return null;
		}

		private static string GetRepositoryRoot(ExecutionResult secondsBeforeTimeout)
		{
			return secondsBeforeTimeout.ExitCode == 0
				? secondsBeforeTimeout.StandardOutput.Trim()
				: null;
		}
	}
}
