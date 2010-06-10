using System;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Class that is a FieldWorks Presenter, which is part of Chorus' MVP system.
	/// </summary>
	public class FieldWorksChangePresenter : IChangePresenter
	{
		private readonly IXmlChangeReport m_report;

		public FieldWorksChangePresenter(IXmlChangeReport report)
		{
			m_report = report;
		}

		private XmlNode FirstNonNullNode
		{
			get
			{
				return m_report.ChildNode ?? m_report.ParentNode;
			}
		}

		private static void GetRawXmlRendering(StringBuilder builder, XmlNode node)
		{
			builder.AppendFormat("<p><pre>{0}</pre></p>",
								 XmlUtilities.GetXmlForShowingInHtml(node.OuterXml));
		}

		#region Implementation of IChangePresenter

		public string GetDataLabel()
		{
			var firstNode = FirstNonNullNode;
			return firstNode.Name == "rt" ? firstNode.Attributes["class"].Value : "Custom property";
		}

		public string GetActionLabel()
		{
			return ((IChangeReport)m_report).ActionLabel;
		}

		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head>");

			switch (m_report.GetType().Name)
			{
				default:
					builder.Append("Don't know how to display a report of type: " + m_report.GetType().Name + "<p/>");
					break;
				case "XmlAdditionChangeReport":
					var additionChangeReport = (XmlAdditionChangeReport)m_report;
					builder.Append("Added the following object:<p/>");
					switch (style)
					{
						case "normal": // Fall through for now.
						//builder.Append(GetHtmlForEntry(additionChangeReport.ChildNode));
						//break;
						case "raw":
							GetRawXmlRendering(builder, additionChangeReport.ChildNode);
							break;
						default:
							return string.Empty;
					}
					break;
				case "XmlDeletionChangeReport":
					var deletionChangeReport = (XmlDeletionChangeReport)m_report;
					builder.Append("Deleted the following object:<p/>");
					switch (style)
					{
						case "normal": // Fall through for now.
						//builder.Append(GetHtmlForEntry(deletionChangeReport.ParentNode));
						//break;
						case "raw":
							GetRawXmlRendering(builder, deletionChangeReport.ParentNode);
							break;
						default:
							return string.Empty;
					}
					break;
				case "XmlChangedRecordReport":
					var changedRecordReport = (XmlChangedRecordReport)m_report;
					switch (style.ToLower())
					{
						case "normal": // Fall through for now.
						//var original = GetHtmlForEntry(changedRecordReport.ParentNode);
						//// XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ParentNode.InnerXml + "</entry>");
						//var modified = GetHtmlForEntry(changedRecordReport.ChildNode);
						//// XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ChildNode.InnerXml + "</entry>");
						//var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
						//builder.Append(m.merge());
						//break;
						case "raw":

							builder.Append("<h3>From</h3>");
							GetRawXmlRendering(builder, changedRecordReport.ParentNode);
							builder.Append("<h3>To</h3>");
							GetRawXmlRendering(builder, changedRecordReport.ChildNode);
							break;
						default:
							break;
					}
					break;
			}
			builder.Append("</html>");
			return builder.ToString();
		}

		public string GetTypeLabel()
		{
			var firstNode = FirstNonNullNode;
			return firstNode.Name == "rt" ? "FieldWorks data object" : "Custom property";
		}

		public string GetIconName()
		{
			return "file";
		}

		#endregion
	}
}