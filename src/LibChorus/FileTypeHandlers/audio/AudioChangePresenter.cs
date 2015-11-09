using System.IO;
using System.Text;
using Chorus.merge;

namespace Chorus.FileTypeHandlers.audio
{
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

		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head>");
			if (style == "normal")
			{
				builder.AppendFormat("<a href=\"playaudio:file:///{0}\">Play Sound</a>", _report.PathToFile);
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
			return "Sound";
		}

		public string GetIconName()
		{
			return "sound";
		}
	}
}