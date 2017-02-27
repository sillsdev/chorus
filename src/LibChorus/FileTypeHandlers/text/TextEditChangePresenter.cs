using System;
using System.IO;
using System.Text;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHandlers.text
{
	public class TextEditChangePresenter : IChangePresenter
	{
		private readonly TextEditChangeReport _report;
		private readonly HgRepository _repository;

		public TextEditChangePresenter(TextEditChangeReport report, HgRepository repository)
		{
			_report = report;
			_repository = repository;
		}

		public string GetDataLabel()
		{
			return Path.GetFileName(_report.PathToFile);
		}

		public string GetActionLabel()
		{
			return _report.ActionLabel;
		}

		public virtual string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head><body>");

			if (style == "normal")
				builder.AppendFormat("<p>The file: '{0}' was {1}.</p>", Path.GetFileName(_report.PathToFile), GetActionLabel().ToLower());
			else
			{
				AppendRawDiffOfFiles(builder);
			}

			builder.Append("</body></html>");
			return builder.ToString();
		}

		protected void AppendRawDiffOfFiles(StringBuilder builder)
		{
			builder.AppendFormat("<p>The file: '{0}' was {1}.</p>", Path.GetFileName(_report.PathToFile), GetActionLabel().ToLower());

			try
			{
				AppendDiffOfTextFile(builder, _report);
			}
			catch (Exception error)
			{
				builder.Append("Could not retrieve or diff the file: " + error.Message);
			}

		}

		private void AppendDiffOfTextFile(StringBuilder builder, TextEditChangeReport r)
		{
			var modified = r.ChildFileInRevision.GetFileContents(_repository);

			if (r.ParentFileInRevision != null) // will be null when this file was just added
			{
				var original = r.ParentFileInRevision.GetFileContents(_repository);
				var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
				builder.Append(m.merge());
			}
			else
			{
				builder.Append(modified);
			}
		}



		public string GetTypeLabel()
		{
			return "--";
		}

		public virtual string GetIconName()
		{
			return "file";
		}
	}
}