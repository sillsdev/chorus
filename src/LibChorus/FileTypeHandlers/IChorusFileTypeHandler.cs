using System.Collections.Generic;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using SIL.Progress;

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// Interface that Chorus can use for diffing and merging file(s) with one or more file extensions.
	/// </summary>
	public interface IChorusFileTypeHandler
	{
		/// <summary>
		/// Checks to see if the implementation can do a diff on the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can do the diff on the file, otherwise false.</returns>
		bool CanDiffFile(string pathToFile);

		/// <summary>
		/// Checks to see if the implementation can do a merge on the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can do the merge on the file, otherwise false.</returns>
		bool CanMergeFile(string pathToFile);

		/// <summary>
		/// Checks to see if the implementation can present the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can present the file, otherwise false.</returns>
		bool CanPresentFile(string pathToFile);

		/// <summary>
		/// Checks to see if the implementation can validate the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can validate the file, otherwise false.</returns>
		bool CanValidateFile(string pathToFile);

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		void Do3WayMerge(MergeOrder mergeOrder);

		/// <summary>
		/// Do a 2-way diff.
		/// </summary>
		/// <param name="parent">The parent revision</param>
		/// <param name="child">The child revision of the parent</param>
		/// <param name="repository">The Mercurial repository</param>
		/// <returns>A collection of zero or more reports of changes between parent and child versions</returns>
		IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository);

		/// <summary>
		/// Get a presenter for the given change report.
		/// </summary>
		/// <param name="report">The change report to use to produce the presewntation</param>
		/// <param name="repository">mercurial repository</param>
		/// <returns>A presentation of the given change report</returns>
		IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository);

		/// <summary>
		/// return null if valid, otherwise nice verbose description of what went wrong
		/// </summary>
		string ValidateFile(string pathToFile, IProgress progress);

		/// <summary>
		/// This is like a diff, but for when the file is first checked in. So, for example, a dictionary
		/// handler might list any the words that were already in the dictionary when it was first checked in.
		/// </summary>
		IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file);

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		IEnumerable<string> GetExtensionsOfKnownTextFileTypes();

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		uint MaximumFileSize { get; }
	}
}
