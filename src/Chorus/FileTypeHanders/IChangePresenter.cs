using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chorus.merge;

namespace Chorus.FileTypeHanders
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

		public DefaultChangePresenter(IChangeReport report)
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
			if(style=="normal")
				return string.Format("<html><p>The file: '{0}' was {1}.</p></html>", Path.GetFileName(_report.PathToFile), GetActionLabel().ToLower());
			else
			{
				return string.Empty;
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