using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chorus.FileTypeHandlers;
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
			Dictionary<string, Dictionary<string, List<string>>> fileStatusDictionary = GetStatusOfFilesOfInterest(repository, configuration);
			Dictionary<string, uint> extensionToMaximumSize = CacheMaxSizesOfExtension(handlerCollection);

			foreach (KeyValuePair<string, Dictionary<string, List<string>>> filesOfOneStatus in fileStatusDictionary)
			{
				var userNotificationMessageBase = "File '{0}' is too large to include in the Send/Receive system. It is {1}, but the maximum allowed is {2}. The file is at {3}.";
				var forgetItIfTooLarge = false;

				switch (filesOfOneStatus.Key)
				{
					case "M": // modified
						// May have grown too large.
						// Untrack it (forget), if is too large and keep out.
						forgetItIfTooLarge = true;
						userNotificationMessageBase = "File '{0}' has grown too large to include in the Send/Receive system.  It is {1}, but the maximum allowed is {2}. The file is at {3}.";
						//FilterModifiedFiles(repository, configuration, extensionToMaximumSize, messageBuilder, PathToRepository(repository), statusCheckResultKvp.Value);
						break;
					case "A": // marked for 'add' with; hg add
						// Untrack it (forget), if is too large and keep out.
						forgetItIfTooLarge = true;
						//FilterAddedFiles(repository, configuration, extensionToMaximumSize, messageBuilder, PathToRepository(repository), statusCheckResultKvp.Value);
						break;
					case "?": // untracked, but going to be added.
						// Keep out, if too large.
						//FilterUntrackedFiles(configuration, extensionToMaximumSize, messageBuilder, PathToRepository(repository), statusCheckResultKvp.Value);
						break;

					//case "!": // tracked but deleted. Fall through
					//case "R": // tracked, and marked for removal with: hg rm
						// No need to mess with these ones.
					//	break;
					// If there are other keys, we don't really care about them.
				}
				Dictionary<string, List<string>> extensionToFilesDictionary = filesOfOneStatus.Value;
				FilterFiles(repository, configuration, extensionToMaximumSize, messageBuilder, PathToRepository(repository),
							extensionToFilesDictionary, userNotificationMessageBase, forgetItIfTooLarge);
			}

			var result = messageBuilder.ToString();
			return string.IsNullOrEmpty(result) ? null : result;
		}

		private static void FilterFiles(HgRepository repository, ProjectFolderConfiguration configuration,
				IDictionary<string, uint> extensionToMaximumSize, StringBuilder messageBuilder, string repositoryBasePath,
				Dictionary<string, List<string>> extensionToFilesMap, string userNotificationMessageBase, bool forgetItIfTooLarge)
		{
			foreach (var filesOfOneExtension in extensionToFilesMap)
			{
				var maxForExtension = GetMaxSizeForExtension(extensionToMaximumSize, filesOfOneExtension.Key);
				foreach (var partialPathname in filesOfOneExtension.Value)
				{
					var fullPathname = Path.Combine(repositoryBasePath, partialPathname);
					var fileInfo = new FileInfo(fullPathname);
					var fileSize = fileInfo.Length;
					if (fileSize <= maxForExtension)
						continue;

					var fileSizeString = (fileSize / (float)Megabyte).ToString("0.00") + " Megabytes";
					var maxSizeString = (maxForExtension / (float)Megabyte).ToString("0.0") + " Megabytes";
					messageBuilder.AppendLine(String.Format(userNotificationMessageBase, Path.GetFileName(fullPathname), fileSizeString, maxSizeString, fullPathname));

					var shortPathname = fullPathname.Replace(repositoryBasePath, null);
					configuration.ExcludePatterns.Add(shortPathname);
					if (forgetItIfTooLarge)
						repository.ForgetFile(shortPathname);
				}
			}
		}

		private static uint GetMaxSizeForExtension(IDictionary<string, uint> extensionToMaximumSize, string extension)
		{
			uint maxForExtension;
			if (!extensionToMaximumSize.TryGetValue(extension, out maxForExtension))
			{
				maxForExtension = Megabyte;
			}
			return maxForExtension;
		}

		private static Dictionary<string, uint> CacheMaxSizesOfExtension(ChorusFileTypeHandlerCollection handlerCollection)
		{
			var cacheExtensionToMaxSize = new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);

			foreach (var handler in handlerCollection.Handlers)
			{
				var maxForHandler = handler.MaximumFileSize;
				foreach (var extension in handler.GetExtensionsOfKnownTextFileTypes())
				{
					uint extantMax;
					if (cacheExtensionToMaxSize.TryGetValue(extension, out extantMax))
					{
						// Use larger value, if two handlers are fighting for it.
						cacheExtensionToMaxSize[extension] = (maxForHandler >= extantMax) ? maxForHandler : extantMax;
					}
					else
					{
						cacheExtensionToMaxSize.Add(extension, maxForHandler);
					}
				}
			}

			return cacheExtensionToMaxSize;
		}

		private static string PathToRepository(HgRepository repository)
		{
			return repository.PathToRepo + Path.DirectorySeparatorChar;// This part of the full path must *not* be included in the exclude list. (Other code adds it, right before the commit.)
		}

		/// <summary>
		/// Gets the status for the files marked as 'modified', 'added', and 'unknown/untracked' (-mau option)
		/// </summary>
		/// <returns>A dictionary of hg status codes --> (a dictionary of file extensions --> a list of files)</returns>
		internal static Dictionary<string, Dictionary<string, List<string>>> GetStatusOfFilesOfInterest(HgRepository repository, ProjectFolderConfiguration configuration)
		{
			var statusOfFilesByExtension = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.InvariantCultureIgnoreCase);

			repository.CheckAndUpdateHgrc();
			var args = new StringBuilder();
			args.Append(" -mau "); // Only modified, added, and unknown (not tracked).

			// Don't use these, as they may mask some large files that are outside the included space, but that are too large, and already tracked.
			//foreach (var pattern in configuration.IncludePatterns) //.Select(pattern => Path.Combine(_pathToRepository, pattern)))
			//{
			//    args.Append(" -I " + SurroundWithQuotes(pattern));
			//}
			foreach (var pattern in configuration.ExcludePatterns) //.Select(pattern => Path.Combine(_pathToRepository, pattern)))
			{
				args.Append(" -X " + HgRepository.SurroundWithQuotes(pattern));
			}
			var result = repository.Execute(repository.SecondsBeforeTimeoutOnLocalOperation, "status", args.ToString());

			var lines = result.StandardOutput.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				if (line.Trim() == string.Empty)
					continue;

				var status = line.Substring(0, 1);
				Dictionary<string, List<string>> statusToFilesMap;
				if (!statusOfFilesByExtension.TryGetValue(status, out statusToFilesMap))
				{
					statusToFilesMap = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
					statusOfFilesByExtension.Add(status, statusToFilesMap);
				}
				var filename = line.Substring(2); // ! data.txt
				var extension = Path.GetExtension(filename);
				if (string.IsNullOrEmpty(extension))
					extension = "noextensionforfile";
				extension = extension.Replace(".", null).ToLowerInvariant();
				List<string> fileList;
				if (!statusToFilesMap.TryGetValue(extension, out fileList))
				{
					fileList = new List<string>();
					statusToFilesMap.Add(extension, fileList);
				}
				fileList.Add(filename);
			}

			return statusOfFilesByExtension;
		}
	}
}