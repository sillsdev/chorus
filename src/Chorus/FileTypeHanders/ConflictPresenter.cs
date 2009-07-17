using System.Text;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// The key thing to understand here is that the conflict file only *grows*... it doesn't get editted
	/// normally.  So the only changes we're interested in are *additions* of conflict reports to the file.
	/// </summary>
	public class ConflictPresenter : IChangePresenter
	{
		private readonly XmlAdditionChangeReport _report;
		private IConflict _conflict;

		public ConflictPresenter(IXmlChangeReport report)
		{
			_report = report as XmlAdditionChangeReport;
			if (_report == null)
			{
				_conflict = new UnreadableConflict(_report.ChildNode);
			}
			else
			{
				_conflict = Conflict.CreateFromXml(_report.ChildNode);
			}
		}

		public string GetDataLabel()
		{
			return "data";
		}

		public string GetActionLabel()
		{
			return XmlUtilities.GetStringAttribute(_report.ChildNode, "type");
		}

		public string GetHtml()
		{
			var builder = new StringBuilder();
			builder.Append("<html>");
			if (_report is XmlAdditionChangeReport)
			{
				builder.AppendFormat("<p>{0}</p>", XmlUtilities.GetXmlForShowingInHtml(_report.ChildNode.OuterXml));
			}

			builder.Append("</html>");
			return builder.ToString();
		}

		public string GetTypeLabel()
		{
			return "conflict";
		}
	}
}