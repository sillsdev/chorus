using System;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.lift
{
	public class LiftChangePresenter : IChangePresenter
	{
		private readonly IXmlChangeReport _report;

		public LiftChangePresenter(IXmlChangeReport report)
		{
			_report = report;
		}

		public string GetActionLabel()
		{
			return ((IChangeReport) _report).ActionLabel;
		}

		public string GetDataLabel()
		{
			//Enhance: this is just a lexeme form, not the headword, and not any other part of the lift file
			var nodes = FirstNonNullNode.SelectNodes("lexical-unit/form/text");
			if (nodes == null || nodes.Count == 0)
				return "??";
			return nodes[0].InnerText;

		}

		public string GetTypeLabel()
		{
			if(FirstNonNullNode.Name == "entry")
				return "lift entry";
			else
			{
				return "?";
			}
		}

		public string GetHtml()
		{
			var builder = new StringBuilder();
			builder.Append("<html>");
			if (_report is XmlAdditionChangeReport)
			{
				var r = _report as XmlAdditionChangeReport;
				builder.AppendFormat("<html><p>Added the entry: {0}</p>", GetDataLabel());

				builder.AppendFormat("<p><pre>{0}</pre></p>", XmlUtilities.GetXmlForShowingInHtml(r.ChildNode.OuterXml));
			}
			else if (_report is XmlDeletionChangeReport)
			{
				var r = _report as XmlDeletionChangeReport;
				builder.AppendFormat("<html><p>Deleted the entry: {0}</p>", GetDataLabel());

				builder.Append("<h3>Deleted Entry</h3>");
				builder.AppendFormat("<p><pre>{0}</pre></p>", XmlUtilities.GetXmlForShowingInHtml(r.ParentNode.OuterXml));
			}
			else if (_report is XmlChangedRecordReport)
			{
				var r = _report as XmlChangedRecordReport;
				builder.AppendFormat("<html><p>Changed the entry: {0}</p>", GetDataLabel());

//                var m = new Rainbow.MergeEngine.Merger(r.ParentNode.InnerXml.Replace("<", "&lt;"), r.ChildNode.InnerXml.Replace("<", "&lt;"));
				var original = XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ParentNode.InnerXml + "</entry>");
				var modified = XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ChildNode.InnerXml + "</entry>");
				var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
				var html = m.merge().Replace("&lt;entry>", "&lt;entry ...&gt;");
			   builder.Append(html);

				builder.Append("<h3>From</h3>");
				builder.AppendFormat("<p><pre>{0}</pre></p>", XmlUtilities.GetXmlForShowingInHtml(r.ParentNode.OuterXml));
				builder.Append("<h3>To</h3>");
				builder.AppendFormat("<p><pre>{0}</pre></p>", XmlUtilities.GetXmlForShowingInHtml(r.ChildNode.OuterXml));
			}
			builder.Append("</html>");
			return builder.ToString();
		}


		private XmlNode FirstNonNullNode
		{
			get
			{
				if (_report.ChildNode == null)
					return _report.ParentNode;
				return _report.ChildNode;
			}
		}
	}
}