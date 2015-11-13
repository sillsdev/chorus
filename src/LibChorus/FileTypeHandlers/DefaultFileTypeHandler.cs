using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using SIL.Code;
using SIL.IO;
using SIL.Progress;

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// This is the handler of last resort.
	/// </summary>
	public class DefaultFileTypeHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			return false;
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
		  //  Debug.Fail("john");
			Guard.AgainstNull(mergeOrder, "mergeOrder");

			XmlMergeService.AddConflictToListener(mergeOrder.EventListener, new UnmergableFileTypeConflict(mergeOrder.MergeSituation));
			switch (mergeOrder.MergeSituation.ConflictHandlingMode)
			{
				default: // just leave our file there
					break;
				case MergeOrder.ConflictHandlingModeChoices.WeWin:
					break;// just leave our file there
				case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					File.Copy(mergeOrder.pathToTheirs, mergeOrder.pathToOurs, true);
					break;

			}
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
			yield break;
		}

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		public uint MaximumFileSize
		{
			get { return LargeFileFilter.Megabyte; }
		}
	}
}