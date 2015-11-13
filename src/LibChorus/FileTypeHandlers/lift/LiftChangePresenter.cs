using System;
using System.Text;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.lift
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


		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">"+styleSheet+"</head>");

			if (_report is XmlAdditionChangeReport)
			{
				var r = _report as XmlAdditionChangeReport;
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

		private static XmlNode GetFormNodeForReferencedEntry(XmlDocument dom, string entryId)
		{
			// TODO XPath Review for &apos; and &quot;
			return dom.SelectSingleNode(String.Format("//entry[@id={0}]/lexical-unit", XmlUtilities.GetSafeXPathLiteral(entryId)));
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

			b.AppendLine("<div class='entry'><table>");

			var lexicalUnitNode = entry.SelectSingleNode("lexical-unit");
			if (lexicalUnitNode != null)
			{
				AddMultiTextHtml(b,  "lexeme form", lexicalUnitNode);
			}
			foreach (XmlNode node in entry.SafeSelectNodes("citation"))
			{
				AddMultiTextHtml(b,  "citation form", node);
			}
			foreach (XmlNode field in entry.SafeSelectNodes("relation"))
			{
				var type = field.GetStringAttribute("type");
				var id = field.GetStringAttribute("ref");

				var formNode = GetFormNodeForReferencedEntry(entry.OwnerDocument, id);
				if (null==formNode)
				{
					b.AppendFormat("Could not locate {0}", id);
					continue;
				}
				AddMultiTextHtml(b, type, formNode);
			}
			foreach (XmlNode field in entry.SafeSelectNodes("field"))
			{
				var label = field.GetStringAttribute("type");
				AddMultiTextHtml(b,  label, field);
			}
			foreach (XmlNode note in entry.SafeSelectNodes("note"))
			{
				AddMultiTextHtml(b, "note", note);
			}
			foreach (XmlNode node in entry.SafeSelectNodes("sense"))
			{
				AddSense(b, 0, node);
			}
			b.AppendLine("</table></div>");
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
			builder.Append("<tr><td><span class='fieldLabel'>Sense</span></td>");
			var pos = senseNode.SelectSingleNode("grammatical-info");
			if (pos != null)
			{
				builder.AppendFormat("<td><span id='pos'>&nbsp;{0}</span></td>" + Environment.NewLine, pos.GetStringAttribute("value"));
			}
			builder.Append("</tr>");

			foreach (XmlNode def in senseNode.SafeSelectNodes("definition"))
			{
				AddMultiTextHtml(builder,  "definition", def);
			}
			foreach (XmlNode gloss in senseNode.SafeSelectNodes("gloss"))
			{
				AddSingleFormHtml(gloss, builder, "gloss");
			}
			foreach (XmlNode example in senseNode.SafeSelectNodes("example"))
			{
				AddMultiTextHtml(builder, "example", example);
				foreach (XmlNode trans in example.SafeSelectNodes("translation"))
				{
					AddMultiTextHtml(builder,  "translation", trans);
				}
			}
			foreach (XmlNode field in senseNode.SafeSelectNodes("field"))
			{
				var label = field.GetStringAttribute("type");
				AddMultiTextHtml(builder, label, field);
			}
			foreach (XmlNode node in senseNode.SafeSelectNodes("illustration"))
			{
				builder.AppendFormat("<tr><td><span class='fieldLabel'>illustration</span></td><td>(an image)</td>");
			}
			foreach (XmlNode trait in senseNode.SafeSelectNodes("trait"))
			{
				var label = trait.GetStringAttribute("name");
				var traitValue = trait.GetStringAttribute("value");
				builder.AppendFormat("<tr><td><span class='fieldLabel'>{0}</span></td><td>{1}</td>", label, traitValue);
			}
			foreach (XmlNode note in senseNode.SafeSelectNodes("note"))
			{
				AddMultiTextHtml(builder,  "note", note);
			}
		}

		private static void AddMultiTextHtml(StringBuilder b,  string label, XmlNode node)
		{
			foreach (XmlNode formNode in node.SafeSelectNodes("form"))
			{
				AddSingleFormHtml(formNode, b, label);
			}

		}

		private static void AddSingleFormHtml(XmlNode node, StringBuilder builder, string label)
		{
			var lang = node.GetStringAttribute("lang");
			builder.AppendFormat("<tr><td><span class='fieldLabel'>{0}</span><span class='langid'>{1}</span></td>", label, lang);
			if (node.InnerText.Trim().EndsWith(".wav"))
			{
				builder.AppendFormat("<td>(a sound file)</td></tr>");
			}
			else
			{
				builder.AppendFormat("<td><span class='{0}'>{1}</span><br/></td></tr>", lang, node.InnerText);
			}
		}
	}
}