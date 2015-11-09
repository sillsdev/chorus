using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Chorus.merge;

namespace Chorus.FileTypeHandlers.image
{
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

		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head>");
			if (style == "normal")
			{
				string path = _report.PathToFile;
				if (Path.GetExtension(path) == ".tif") // IE can't show tifs
				{
					var image = Image.FromFile(_report.PathToFile);
					path = Path.GetTempFileName() + ".bmp";
					//enhance... this leaks disk space, albeit in the temp folder
					image.Save(path, ImageFormat.Bmp);
				}
				builder.AppendFormat("<img src=\"file:///{0}\" width=100/>", path);
			}
			else
			{
				return string.Empty;
			}
			builder.Append("</html>");
			return builder.ToString();

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