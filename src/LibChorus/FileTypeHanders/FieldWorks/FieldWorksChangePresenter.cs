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
		private readonly IXmlChangeReport _report;

		public FieldWorksChangePresenter(IXmlChangeReport report)
		{
			_report = report;
		}

		private XmlNode FirstNonNullNode
		{
			get
			{
				return _report.ChildNode ?? _report.ParentNode;
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
			return ((IChangeReport)_report).ActionLabel;
		}

		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head>");

			switch (_report.GetType().Name)
			{
				default:
					builder.Append("Don't know how to display a report of type: " + _report.GetType().Name + "<p/>");
					break;
				case "XmlAdditionChangeReport":
					var additionChangeReport = (XmlAdditionChangeReport)_report;
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
					var deletionChangeReport = (XmlDeletionChangeReport)_report;
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
					var changedRecordReport = (XmlChangedRecordReport)_report;
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
			return "FieldWorks data object";
		}

		public string GetIconName()
		{
			return "file";
		}

		#endregion
	}
}