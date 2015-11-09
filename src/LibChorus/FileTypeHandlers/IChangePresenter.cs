using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// Given a change report, extract info for UI display purposes, which is Data-type specific
	/// </summary>
	public interface IChangePresenter
	{
		string GetDataLabel();
		string GetActionLabel();
		string GetHtml(string style, string styleSheet);
		string GetTypeLabel();
		string GetIconName();
	}

	public class DefaultChangePresenter : IChangePresenter
	{
		private readonly IChangeReport _report;
		private readonly HgRepository _repository;

		public DefaultChangePresenter(IChangeReport report, HgRepository repository)
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

			if(style=="normal")
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

			var r = _report as DefaultChangeReport;
			if (r != null)
			{
				try
				{
					AppendDiffOfXmlFile(builder, r);
				}
				catch (Exception)
				{
					try
					{
						AppendDiffOfUnknownFile(builder, r);
					}
					catch (Exception error)
					{
						builder.Append("Could not retrieve or diff the file: " + error.Message);
					}
				}
			}
		}

		private void AppendDiffOfUnknownFile(StringBuilder builder, DefaultChangeReport r)
		{
			var modified =r.ChildFileInRevision.GetFileContents(_repository);

			if (r.ParentFileInRevision != null) // will be null when this file was just added
			{
				var original =r.ParentFileInRevision.GetFileContents(_repository);
				var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
				builder.Append(m.merge());
			}
			else
			{
				builder.Append(modified);
			}
		}

		private void AppendDiffOfXmlFile(StringBuilder builder, DefaultChangeReport r)
		{
			var modified =
				XmlUtilities.GetXmlForShowingInHtml(r.ChildFileInRevision.GetFileContents(_repository));

			if (r.ParentFileInRevision != null) // will be null when this file was just added
			{
				var original =
					XmlUtilities.GetXmlForShowingInHtml(r.ParentFileInRevision.GetFileContents(_repository));
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