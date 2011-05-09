using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Chorus.FileTypeHanders.image
{
	public class ImageFileTypeHandler : IChorusFileTypeHandler
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
			var ext = Path.GetExtension(pathToFile);
			return ((new string[] { ".tif", ".jpg", ".png", ".bmp" }.Contains(ext)));
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

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield break;
		}
	}
}