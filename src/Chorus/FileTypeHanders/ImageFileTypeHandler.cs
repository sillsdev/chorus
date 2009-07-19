using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.FileTypeHanders
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
			return new ImageChangePresenter(report);
		}




		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision.FullPath, "Added") };
		}

	}

	public class ImageChangePresenter : IChangePresenter
	{
		private readonly IChangeReport _report;

		public ImageChangePresenter(IChangeReport report)
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

		public string GetHtml()
		{
			string path = _report.PathToFile;
			if (Path.GetExtension(path) == ".tif") // IE can't show tifs
			{
				var image = Image.FromFile(_report.PathToFile);
				path = Path.GetTempFileName() + ".bmp";//enhance... this leaks disk space, albeit in the temp folder
				image.Save(path, ImageFormat.Bmp);
			}
			return string.Format("<html><img src=\"file:///{0}\" width=100/></html>", path);
		}

		public string GetTypeLabel()
		{
			return "Image";
		}

		public string GetIconName()
		{
			return "image";
		}
	}
}