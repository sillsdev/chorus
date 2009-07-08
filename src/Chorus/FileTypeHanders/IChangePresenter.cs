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
		string GetHtml();
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
			return "N/A";
		}

		public string GetActionLabel()
		{
			return _report.ActionLabel;
		}

		public string GetHtml()
		{
			return string.Format("<p>File: {0}</p><p>Action: {1}</p><p>Data: {2}</p>", Path.GetFileName(_report.PathToFile), GetActionLabel(), GetDataLabel());
		}
	}
}