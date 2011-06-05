using System;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

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
		public static string FilterFiles(HgRepository repository, ProjectFolderConfiguration configuration, ChorusFileTypeHandlerCollection handlers, IProgress progress)
		{
			var builder = new StringBuilder();

			foreach (var fileInRevision in repository.GetFilesInRevisionFromQuery(null, "status"))
			{
				var fir = fileInRevision;
				if (fir.ActionThatHappened == FileInRevision.Action.Deleted || fir.ActionThatHappened == FileInRevision.Action.NoChanges || fir.ActionThatHappened == FileInRevision.Action.Parent)
					continue; // Don't fret about the size of any these types of files.

				// Remaining options are: Added, Modified, and Unknown.
				// It will likely be Unknown when called by Synchronizer's Commit method,
				// as it will deal with the includes/excludes stuff and mark new stuff as 'add'.

				// This part of the full path must *not* be included in the exclude list,
				// as it is prepended by HgRepository.
				var pathToRepository = repository.PathToRepo + Path.PathSeparator;// This part of the full path must *not* be included in the exclude list,
				var filename = Path.GetFileName(fir.FullPath);
				var fileInfo = new FileInfo(fir.FullPath);
				var fileSize = fileInfo.Length;
				foreach (var msg in from handler in handlers.Handlers
									where (handler.CanValidateFile(fir.FullPath) && handler.MaximumFileSize != UInt32.MaxValue) && fileSize >= handler.MaximumFileSize
									select (fir.ActionThatHappened == FileInRevision.Action.Added || fir.ActionThatHappened == FileInRevision.Action.Unknown) ? String.Format("File '{0}' is too large to add to Chorus.", filename) : String.Format("File '{0}' is too large to be updated in Chorus.", filename))
				{
					progress.WriteVerbose(msg);
					builder.AppendLine(msg);
					configuration.ExcludePatterns.Add(fir.FullPath.Replace(pathToRepository, ""));

					// TODO: What to do, if the file is "Modified" but now too big?
					// "remove" actually deletes the file in repo and in working folder, which seems a bit rude.
					// "forget" removes it from repo (history remains) and leaves it in working folder.
					if (fir.ActionThatHappened == FileInRevision.Action.Modified)
					{
						// Tell Hg to 'forget' it.
						repository.ForgetFile(fir.FullPath.Replace(pathToRepository, ""));
					}
				}
			}

			return builder.ToString();
		}
	}
}