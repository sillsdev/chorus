using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.FileTypeHanders.text;
using Chorus.merge;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// Handler for files with extension of ".lift-ranges".
	/// </summary>
	public class LiftRangesFileTypeHandler : IChorusFileTypeHandler
	{
		internal LiftRangesFileTypeHandler()
		{}

		private const string kExtension = "lift-ranges";

		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, kExtension);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return true;
		}
		public bool CanValidateFile(string pathToFile)
		{
			return false;
		}
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			throw new NotImplementedException();
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			Guard.AgainstNull(mergeOrder, "mergeOrder");

			var commonAncestor = File.ReadAllText(mergeOrder.pathToCommonAncestor);
			var ours = File.ReadAllText(mergeOrder.pathToOurs);
			var theirs = File.ReadAllText(mergeOrder.pathToTheirs);

			if (commonAncestor == ours && commonAncestor == theirs)
				return; // Nothing to do.

			if (ours == theirs)
			{
				// Both made same change(s).
				mergeOrder.EventListener.ChangeOccurred(new TextEditChangeReport(mergeOrder.pathToOurs, commonAncestor, ours));
				return;
			}

			if (ours != commonAncestor & theirs == commonAncestor)
			{
				// We changed, they did nothing.
				mergeOrder.EventListener.ChangeOccurred(new TextEditChangeReport(mergeOrder.pathToOurs, commonAncestor, ours));
				return;
			}

			if (ours == commonAncestor && theirs != commonAncestor)
			{
				// They changed, we did nothing.
				mergeOrder.EventListener.ChangeOccurred(new TextEditChangeReport(mergeOrder.pathToTheirs, commonAncestor, theirs));
				File.Copy(mergeOrder.pathToTheirs, mergeOrder.pathToOurs, true);
				return;
			}

			mergeOrder.EventListener.ConflictOccurred(new UnmergableFileTypeConflict(mergeOrder.MergeSituation));
			if (mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.TheyWin)
				File.Copy(mergeOrder.pathToTheirs, mergeOrder.pathToOurs, true);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			throw new ApplicationException(string.Format("Chorus could not find a handler to diff files like '{0}'", child.FullPath));
		}


		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return new DefaultChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return kExtension;
		}

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		public uint MaximumFileSize
		{
			get { return UInt32.MaxValue; }
		}
	}
}