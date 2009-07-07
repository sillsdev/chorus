using System;
using System.Collections.Generic;
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
	}

  /*  /// <summary>
	/// Find a change presenter which understands this kind of data, or get a default one.
	/// </summary>
	public class ChangePresenterFactory
	{
		public static IChangePresenter GetChangePresenter(IChangeReport report)
		{
			//nb: we will eventually need file name/extension info

			IXmlChangeReport r = report as IXmlChangeReport;
			if (r == null)
				return new DefaultChangePresenter(report);
			if ((r.ChildNode != null && r.ChildNode.Name == "entry")
				|| (r.ParentNode != null && r.ParentNode.Name == "entry"))
				return new LiftChangePresenter(r);
			return new DefaultChangePresenter(report);
		}
	}*/
}