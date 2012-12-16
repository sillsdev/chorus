using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.sync
{
	///<summary>
	/// Class that filters out large, binary, files from being put into the repo,
	/// or if a binary file already is in the repo, then don't commit it if it exceeds the maximum allowablew size.
	///
	/// JohnH comments:
	///		"we could use a “large file” filter in Chorus.
	///		Not one based in the “CommitCop class” (which would be easy but all-or-nothing),
	///		but rather something more complicated. We need something which
	///			1)	Decides whether to include the file based on its size
	///			2)	Decides to “forget” a file when it was previously small but is now too big.
	///
	///		And of course we’d have to have really good reporting,
	///		including perhaps a place-holder file or annotation or something,
	///		so that what happened is communicated throughout the team.
	///
	///		I don’t know if that’s worth the effort to you, but I think those would be the requirements
	///		before we allow video or add audio which is likely to be story-length."
	///
	///		"As it stands, we cannot allow code in Chorus which allows video files,
	///		nor software which uses audio beyond utterance length.
	///		If a project wants to allow either of those, Chorus will need a max-file size limit...
	///
	///		Ideally, it would also look at the whole changeset, and be able to break a too-large one into smaller sets.
	///		Lacking that, perhaps Chorus clients need to do a checkin after each media addition of any large size.
	///		For that matter, all clients should be doing their own filtering, in order to give a good user experience.
	///		If a too-large file finds its way all the way to chorus and gets rejected, that will be confusing.
	///		Chorus should still have the check, though, in case the media file grows without the client noticing."
	///
	/// Cambell comments:
	///		"Well I'd like to suggest absolutely not (well more than suggest) allowing the user to add large files to the repo.
	///		I'd go so far as to say we shouldn't allow video file extensions out right.
	///		And it would be better if we went as far as restricting the max file size of a binary.
	///		At this stage we don't, and it hasn't been a problem.
	///
	///		The key point is, if even one large file is committed to the repo,
	///		that repo is completely dead for all users of that project.
	///		I'm sure that is unacceptable to everyone.
	///
	///		I'd err on the side of precision, and our policies rather than the users discretion and what they could really try."
	///
	/// How to set the limits:
	///		1. hard-coded
	///		2. project manager determined
	///		3. Registry setting
	///		4. ??
	///</summary>
	public static class LargeFileFilter
	{
		///<summary>
		/// Return a standard Megabyte size.
		///</summary>
		public static uint Megabyte
		{
			get { return (uint)Math.Pow(2, 20); }
		}

		///<summary>
		/// Filter the files, before the commit. Files that are too large are added to the exclude section of the configuration.
		///</summary>
		///<returns>An empty string or a string with a listing of files that were not added/modified.</returns>
		public static string FilterFiles(HgRepository repository, ProjectFolderConfiguration configuration, ChorusFileTypeHandlerCollection handlerCollection)
		{
			var messageBuilder = new StringBuilder();

			foreach (var fileInRevision in repository.GetFilesInRevisionFromQuery(null, "status"))
			{
				var fir = fileInRevision;
				if (fir.ActionThatHappened == FileInRevision.Action.Deleted || fir.ActionThatHappened == FileInRevision.Action.NoChanges || fir.ActionThatHappened == FileInRevision.Action.Parent)
					continue; // Don't fret about the size of any these types of files.

				// Remaining options are: Added, Modified, and Unknown.
				// It will likely be Unknown when called by Synchronizer's Commit method,
				// as it will deal with the includes/excludes stuff and mark new stuff as 'add'.
				FilterFiles(repository, configuration, messageBuilder, fir, handlerCollection.Handlers.ToList());
			}

			var result = messageBuilder.ToString();
			return string.IsNullOrEmpty(result) ? null : result;
		}

		private static void FilterFiles(HgRepository repository, ProjectFolderConfiguration configuration,
											StringBuilder messageBuilder, FileInRevision fir, List<IChorusFileTypeHandler> handlers)
		{
			var filename = Path.GetFileName(fir.FullPath);
			var fileExtension = Path.GetExtension(filename);
			if (!string.IsNullOrEmpty(fileExtension))
			{
				fileExtension = fileExtension.Replace(".", null);
				if (fileExtension == "wav")
					return; // Nasty hack, but "wav" is put into repo no matter its size. TODO: FIX THIS, if Hg ever works right for "wav" files.
			}

			var pathToRepository = PathToRepository(repository);
			var fileInfo = new FileInfo(fir.FullPath);
			var fileSize = fileInfo.Length;
			handlers.Add(new DefaultFileTypeHandler());
			var allKnownExtensions = new HashSet<string>();
			foreach (var handler in handlers)
				allKnownExtensions.UnionWith(handler.GetExtensionsOfKnownTextFileTypes());

			HashSet<string> allNormallyExcludedPathNames = null;
			foreach (var handler in handlers)
			{
				// NB: we don't care if the handler can do anything with the file, or not.
				// We only care if it claims to handle the given extension.
				var knownExtensions = handler.GetExtensionsOfKnownTextFileTypes().ToList();
				if (handler.GetType() == typeof(DefaultFileTypeHandler) && !allKnownExtensions.Contains(fileExtension))
				{
					if (fileSize <= handler.MaximumFileSize)
						continue;

					if (allNormallyExcludedPathNames == null)
						allNormallyExcludedPathNames = CollectAllNormallyExcludedPathnamesOnce(configuration, pathToRepository);
					RegisterLargeFile(repository, configuration, messageBuilder, fir, filename, fileSize, handler.MaximumFileSize, allNormallyExcludedPathNames);
				}
				else
				{
					foreach (var knownExtension in knownExtensions)
					{
						if ((knownExtension.ToLowerInvariant() != fileExtension.ToLowerInvariant()
							|| fileSize <= handler.MaximumFileSize))
						{
							continue;
						}

						if (allNormallyExcludedPathNames == null)
							allNormallyExcludedPathNames = CollectAllNormallyExcludedPathnamesOnce(configuration, pathToRepository);
						RegisterLargeFile(repository, configuration, messageBuilder, fir, filename, fileSize, handler.MaximumFileSize, allNormallyExcludedPathNames);
					}
				}
			}
		}

		/// <summary>
		/// Warning: This is a REALLY REALLY REALLY expensive function. It should only be called once, and only if needed.
		///
		/// Before we say "that's too big", we need to make sure we wouldn't have used it anyhow, that is, that hg would
		/// have filtered it out.
		///
		/// NB: this could be rewritten to be fast; but at the moment a GetFiles is called for every filter. We could
		/// instead do a single GetFiles, and do our own filtering.
		/// </summary>
		/// <returns></returns>
		private static HashSet<string> CollectAllNormallyExcludedPathnamesOnce(ProjectFolderConfiguration configuration,
																string pathToRepository)
		{
			var allNormallyExcludedPathnames = new HashSet<string>();
			foreach (var currentExclusion in configuration.ExcludePatterns)
			{
				// Gather up all normally excluded files, so they are not reported as being filtered out for being large.
				// That extra/un-needed warning message will only serve to confuse users.
				if (currentExclusion.StartsWith("**" + Path.DirectorySeparatorChar))
				{
					// Something like the lift exclusion of **\Cache. (Or worse: **\foo\**\Cache. Not currently supported)
					var nestedFolderName = currentExclusion.Replace("**" + Path.DirectorySeparatorChar, null);
					var adjustedBaseDir = pathToRepository;
					CollectExcludedFilesFromDirectory(pathToRepository, adjustedBaseDir, nestedFolderName, allNormallyExcludedPathnames);
				}
				else if (currentExclusion.Contains(Path.DirectorySeparatorChar + "**" + Path.DirectorySeparatorChar))
				{
					// Some other filter like: foo\**\Cache.
					var idx = currentExclusion.IndexOf(Path.DirectorySeparatorChar + "**" + Path.DirectorySeparatorChar, StringComparison.Ordinal);
					var nestedFolderName = currentExclusion.Substring(idx + 4);
					var adjustedBaseDir = Path.Combine(pathToRepository, currentExclusion.Substring(0, idx));
					CollectExcludedFilesFromDirectory(pathToRepository, adjustedBaseDir, nestedFolderName, allNormallyExcludedPathnames);
				}
				else if (currentExclusion.Contains("*"))
				{
					// May be "*.*" or "**.*", but not "**".
					var adjustedBasePath = currentExclusion.Contains(Path.DirectorySeparatorChar)
											? Path.GetDirectoryName(Path.Combine(pathToRepository, currentExclusion))
											: pathToRepository;
					if (!Directory.Exists(adjustedBasePath))
						continue;
					var useNestingFilter = currentExclusion.Contains("**");
					foreach (var excludedPathname in Directory.GetFiles(pathToRepository,
																		useNestingFilter
																			? currentExclusion.Replace("**", "*")
																			: currentExclusion,
																		useNestingFilter
																			? SearchOption.AllDirectories
																			: SearchOption.TopDirectoryOnly))
					{
						allNormallyExcludedPathnames.Add(excludedPathname.Replace(pathToRepository, null));
					}
				}
				else
				{
					// An explicitly specified file with no wildcards, such as foo\some.txt
					var singletonPathname = Path.Combine(pathToRepository, currentExclusion);
					if (File.Exists(singletonPathname))
						allNormallyExcludedPathnames.Add(singletonPathname.Replace(pathToRepository, null));
				}
			}
			return allNormallyExcludedPathnames;
		}

		private static void CollectExcludedFilesFromDirectory(string pathToRepository, string adjustedBaseDir,
															  string nestedFolderName, HashSet<string> allNormallyExcludedPathnames	)
		{
			foreach (var directory in Directory.GetDirectories(adjustedBaseDir, nestedFolderName, SearchOption.AllDirectories))
			{
				foreach (var excludedPathname in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
					allNormallyExcludedPathnames.Add(excludedPathname.Replace(pathToRepository, null));
			}
		}

		private static void RegisterLargeFile(HgRepository repository, ProjectFolderConfiguration configuration,
											StringBuilder builder,
											FileInRevision fileInRevision, string filename,
											long fileSize, uint maxSize,
											ICollection<string> allExtantExcludedPathnames)
		{
			var longPathname = RemoveBasePath(repository, fileInRevision);
			if (allExtantExcludedPathnames.Contains(longPathname))
				return; // Standard "exclude" covers it, so skip the warning.

			var fileSizeString = (fileSize / (float)(1024*1024)).ToString("0.00") + " Megabytes";
			var maxSizeString = (maxSize / (float)(1024*1024)).ToString("0.0") + " Megabytes";

			configuration.ExcludePatterns.Add(longPathname);
			switch (fileInRevision.ActionThatHappened)
			{
				case FileInRevision.Action.Added:
				case FileInRevision.Action.Unknown:
					builder.AppendLine(String.Format("File '{0}' is too large to include in the Send/Receive system. It is {1}, but the maximum allowed is {2}. The file is at {3}.", filename, fileSizeString, maxSizeString, fileInRevision.FullPath));
					break;
				case FileInRevision.Action.Modified:
					builder.AppendLine(String.Format("File '{0}' has grown too large to include in the Send/Receive system.  It is {1}, but the maximum allowed is {2}. The file is at {3}.", filename, fileSizeString, maxSizeString, fileInRevision.FullPath));
					// TODO: What to do, if the file is "Modified" but now too big?
					// "remove" actually deletes the file in repo and in working folder, which seems a bit rude.
					// "forget" removes it from repo (history remains) and leaves it in working folder.
					// Tell Hg to 'forget' it, for now.
					repository.ForgetFile(longPathname);
					break;
			}
		}

		private static string RemoveBasePath(HgRepository repository, FileInRevision fir)
		{
			var pathToRepository = PathToRepository(repository);
			return fir.FullPath.Replace(pathToRepository, null);
		}

		private static string PathToRepository(HgRepository repository)
		{
			return repository.PathToRepo + Path.DirectorySeparatorChar;// This part of the full path must *not* be included in the exclude list. (Other code adds it, right before the commit.)
		}
	}
}