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
			{
				//if the child was marked as deleted, we actually need to look to the parent node
				if (_report.ParentNode != null)
				{
					nodes = _report.ParentNode.SelectNodes("lexical-unit/form/text");
				}
			}
			if (nodes == null || nodes.Count == 0)
			{
				return "??";
			}

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

		public string GetIconName()
		{
			return "wesay";
		}


		public string GetHtml(string style)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>");
			builder.Append(@"<style type='text/css'><!--

BODY { font-family: verdana,arial,helvetica,sans-serif; }

span.langid {color: 'gray'; font-size: xx-small;position: relative;
	top: 0.3em;
}

span.fieldLabel {color: 'gray'; font-size: x-small;}

div.entry {color: 'blue';font-size: x-small;}

span.en {
color: 'green';
}
span.es {
color: 'green';
}
span.fr {
color: 'green';
}
span.tpi {
color: 'purple';
}

--></style>");

			builder.Append("</head>");
			if (_report is XmlAdditionChangeReport)
			{
				var r = _report as XmlAdditionChangeReport;
 //               builder.AppendFormat("<html><p>Added the entry: {0}</p>", GetDataLabel());

//                builder.AppendFormat("<p><pre>{0}</pre></p>", XmlUtilities.GetXmlForShowingInHtml(r.ChildNode.OuterXml));
   //             builder.AppendFormat(GetHtmlForEntry(r.ChildNode));

				switch (style)
				{
					case "normal":
						builder.Append(GetHtmlForEntry(r.ChildNode));
						break;
					case "raw":
						builder.AppendFormat("<p><pre>{0}</pre></p>",
								XmlUtilities.GetXmlForShowingInHtml(r.ChildNode.OuterXml));
						break;
					default:
						return string.Empty;
				}

			}
			else if (_report is XmlDeletionChangeReport)
			{
				var r = _report as XmlDeletionChangeReport;
				 builder.Append("Deleted the following lexicon entry:<p/>");
			   switch (style)
				{
					case "normal":
						builder.Append(GetHtmlForEntry(r.ParentNode));
						break;
					case "raw":
						builder.AppendFormat("<p><pre>{0}</pre></p>",
								XmlUtilities.GetXmlForShowingInHtml(r.ParentNode.OuterXml));
						break;
					default:
						return string.Empty;
				}
			}
			else if (_report is XmlChangedRecordReport)
			{
				GetHtmlForChange(style, builder);
			}
			builder.Append("</html>");
			return builder.ToString();
		}

		private void GetHtmlForChange(string style, StringBuilder builder)
		{
			var r = _report as XmlChangedRecordReport;
			switch (style.ToLower())
			{
				case "normal":

					var original = GetHtmlForEntry(r.ParentNode);
						// XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ParentNode.InnerXml + "</entry>");
					var modified = GetHtmlForEntry(r.ChildNode);
						// XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ChildNode.InnerXml + "</entry>");
					var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
					builder.Append(m.merge());
					break;

				case "raw":

					builder.Append("<h3>From</h3>");
					builder.AppendFormat("<p><pre>{0}</pre></p>",
										 XmlUtilities.GetXmlForShowingInHtml(r.ParentNode.OuterXml));
					builder.Append("<h3>To</h3>");
					builder.AppendFormat("<p><pre>{0}</pre></p>",
										 XmlUtilities.GetXmlForShowingInHtml(r.ChildNode.OuterXml));
					break;
				default:
					break;
			}
		}

		public static string GetHtmlForEntry(XmlNode entry)
		{
			var b = new StringBuilder();

			b.AppendLine("<div class='entry'>");
			var lexicalUnitNode = entry.SelectSingleNode("lexical-unit");
			if (lexicalUnitNode != null)
			{
				AddMultiTextHtml(b, 0, "LexemeForm", lexicalUnitNode);
			}
				foreach (XmlNode node in entry.SafeSelectNodes("sense"))
				{
					AddSense(b, 0, node);
				}
			b.AppendLine("</div>");
			return b.ToString();
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

		private static void AddSense(StringBuilder builder, int indentLevel, XmlNode senseNode)
		{
			builder.Append("<span class='fieldLabel'>Sense</span>");
			var pos = senseNode.SelectSingleNode("grammatical-info");
			if (pos != null)
			{
				builder.AppendFormat("<span id='pos'>&nbsp;{0}</span>" + Environment.NewLine, pos.GetStringAttribute("value"));
			}
			builder.Append("<br/>");

			foreach (XmlNode def in senseNode.SafeSelectNodes("definition"))
			{
				AddMultiTextHtml(builder, 1 + indentLevel, "Definition", def);
			}
			foreach (XmlNode example in senseNode.SafeSelectNodes("example"))
			{
				AddMultiTextHtml(builder, 1 + indentLevel, "Example", example);
				foreach (XmlNode trans in example.SafeSelectNodes("translation"))
				{
					AddMultiTextHtml(builder, 2 + indentLevel, "Translation", trans);
				}
			}
		}

		private static void AddMultiTextHtml(StringBuilder b, int indentLevel, string label, XmlNode node)
		{
			string indent = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;".Substring(0, indentLevel * 6 * 4);
			foreach (XmlNode formNode in node.SafeSelectNodes("form"))
			{
				b.AppendFormat("{0}<span class='fieldLabel'>{1}</span><span class='langid'>{2}</span>: <span class='{2}'>{3}</span><br/>" + Environment.NewLine, indent, label, formNode.GetStringAttribute("lang"), formNode.InnerText);
			}

		}
	}
}