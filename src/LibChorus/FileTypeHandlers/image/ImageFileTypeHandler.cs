using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
using SIL.IO;
using SIL.Progress;

namespace Chorus.FileTypeHandlers.image
{
	public class ImageFileTypeHandler : IChorusFileTypeHandler
	{
		internal ImageFileTypeHandler()
		{}

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
			var ext = Path.GetExtension(pathToFile); // NB: has the '.'
			return !string.IsNullOrEmpty(ext) && GetExtensionsOfKnownTextFileTypes().Contains(ext.Replace(".", null));
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
			throw new ApplicationException(string.Format("Chorus could not find a handler to merge files like '{0}'", mergeOrder.pathToOurs));
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			throw new ApplicationException(string.Format("Chorus could not find a handler to diff files like '{0}'", child.FullPath));
		}



		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return new ImageChangePresenter(report);
		}



		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			return new List<string> { "bmp", "jpg", "jpeg", "gif", "png", "tif", "tiff", "ico", "wmf", "pcx", "cgm" };
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