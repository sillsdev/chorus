using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.FileTypeHanders
{
	public class AudioFileTypeHandler : IChorusFileTypeHandler
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
			return ((new string[] {".wav",".mp3"}.Contains(ext)));
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			throw new ApplicationException(string.Format("Chorus could not find a handler to merge files like '{0}'", mergeOrder.pathToOurs));
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision fileInRevision, string pathToParent, string pathToChild)
		{
			throw new ApplicationException(string.Format("Chorus could not find a handler to diff files like '{0}'", pathToChild));
		}

		public IChangePresenter GetChangePresenter(IChangeReport report)
		{
			return new AudioChangePresenter(report);
		}




		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision.FullPath, "Added") };
		}

	}

	public class AudioChangePresenter : IChangePresenter
	{
		private readonly IChangeReport _report;

		public AudioChangePresenter(IChangeReport report)
		{
			_report = report;
		}

		public string GetDataLabel()
		{
			return Path.GetFileName(_report.PathToFile);
		}

		public string GetActionLabel()
		{
			return _report.ActionLabel;
		}

		public string GetHtml(string style)
		{
			if (style == "normal")
			{
				return string.Format("<html><a href=\"file:///{0}\">Play Sound</a></html>", _report.PathToFile);
			}
			else
			{
				return string.Empty;
			}

		}

		public string GetTypeLabel()
		{
			return "Sound";
		}

		public string GetIconName()
		{
			return "sound";
		}
	}
}