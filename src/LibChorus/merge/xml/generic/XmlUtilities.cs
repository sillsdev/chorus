using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge.xml.generic.xmldiff;
using SIL.Extensions;
using SIL.Providers;


namespace Chorus.merge.xml.generic
{
	public class NullXMlNodeList : XmlNodeList
	{
		public override XmlNode Item(int index)
		{
			throw new ArgumentOutOfRangeException();
		}

		public override IEnumerator GetEnumerator()
		{
			yield return null;
		}

		public override int Count
		{
			get { return 0; }
		}
	}

	public static class XmlUtilities
	{
		public static bool AreXmlElementsEqual(string ours, string theirs)
		{
			return (ours == theirs) || AreXmlElementsEqual(CreateNode(ours), CreateNode(theirs));
		}

		private static XmlNode CreateNode(string data)
		{
			using (var stringReader = new StringReader(data))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				var xmlDocument = new XmlDocument();
				var xmlNode = xmlDocument.ReadNode(xmlReader);
				xmlNode.Normalize();
				return xmlNode;
			}
		}

		private static XmlNode CreateNode(byte[] data)
		{
			using (var memoryStream = new MemoryStream(data))
			{
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(memoryStream); // This loads the MemoryStream as Utf8 xml. (I checked.)
				XmlNode xmlNode = xmlDocument.DocumentElement;
				xmlNode.Normalize();
				return xmlNode;
			}
		}

		public static bool AreXmlElementsEqual(byte[] ours, byte[] theirs)
		{
			// Painfully slow.
			//IStructuralEquatable equate = ours;
			//if (equate.Equals(theirs, EqualityComparer<byte>.Default))
			//    return true;
			return ours.AreByteArraysEqual(theirs) || AreXmlElementsEqual(CreateNode(ours), CreateNode(theirs));
		}

		/// <summary>
		/// This version of AreXmlElementsEqual is used to compare two xml strings
		/// and have it ignore certain specified attributes (if the corresponding string
		/// in astrAttributeToIgnore is non-null) or elements (if the corresponding string
		/// in astrAttributeToIgnore is null)
		/// </summary>
		/// <param name="ours">xml string for left-hand side of the comparison</param>
		/// <param name="theirs">xml string for right-hand side of the comparison</param>
		/// <param name="astrElementXPath">an array of XPath strings to the element in which the attribute is to be ignored. For example, if we're ignoring /StoryProject/stories/story/@stageDateTimeStamp, then one of the items of this list would be "/StoryProject/stories/story"</param>
		/// <param name="astrAttributeToIgnore">an array of attribute names to be ignored. For example, if we're ignoring /StoryProject/stories/story/@stageDateTimeStamp, then the items in this list would be "stageDateTimeStamp"</param>
		/// <returns></returns>
		public static bool AreXmlElementsEqual(string ours, string theirs,
			string[] astrElementXPath, string[] astrAttributeToIgnore)
		{
			if (ours == theirs)
				return true;

			var ourNode = CreateNode(ours);
			var theirNode = CreateNode(theirs);

			Debug.Assert(astrElementXPath.Length == astrAttributeToIgnore.Length);
			for (int i = 0; i < astrElementXPath.Length; i++)
			{
				RemoveItem(ourNode, astrElementXPath[i], astrAttributeToIgnore[i]);
				RemoveItem(theirNode, astrElementXPath[i], astrAttributeToIgnore[i]);
			}

			return AreXmlElementsEqual(ourNode, theirNode);
		}

		private static void RemoveItem(XmlNode node, string strXPath, string strAttribute)
		{
			XmlNodeList list = node.SelectNodes(strXPath);
			if (list != null)
				foreach (XmlNode nodeToRemove in list)
					RemoveItem(nodeToRemove, strAttribute);
		}

		private static void RemoveItem(XmlNode node, string strAttribute)
		{
			if (String.IsNullOrEmpty(strAttribute))
				// remove the whole element
				node.ParentNode.RemoveChild(node);
			else
				// just remove the attribute
				node.Attributes.RemoveNamedItem(strAttribute);
		}

		public static bool AreXmlElementsEqual(XmlNode ours, XmlNode theirs)
		{
			if (ours.NodeType == XmlNodeType.Text)
			{
				if (theirs.NodeType != XmlNodeType.Text)
				{
					return false;
				}
				var oursInnerTrimmed = ours.InnerText.Trim();
				var theirsInnerTrimmed = theirs.InnerText.Trim();
				var oursIsEmpty = String.IsNullOrEmpty(oursInnerTrimmed);
				var theirsIsEmpty = String.IsNullOrEmpty(theirsInnerTrimmed);
				if (oursIsEmpty != theirsIsEmpty)
				{
					return false;
				}
				return oursInnerTrimmed == theirsInnerTrimmed;
			}
			if (theirs.NodeType == XmlNodeType.Text)
				return false; // Theirs is text, but ours is not.

			var ourOuterXml = ours.OuterXml;
			var theirOuterXml = theirs.OuterXml;
			if (ourOuterXml == theirOuterXml)
				return true;
			return AreXmlElementsEqual(new XmlInput(ourOuterXml), new XmlInput(theirOuterXml));
		}


		public static bool AreXmlElementsEqual(XmlInput ours, XmlInput theirs)
		{
			// Must use 'config', or whitespace only differences will make the elements different.
			// cf. diffing changeset 240 and 241 in the Tok Pisin project for such whitespace differences.
			var config = new DiffConfiguration(WhitespaceHandling.None);
			var diff = new XmlDiff(ours, theirs, config);
			var diffResult = diff.Compare();
			return (diffResult == null || diffResult.Difference == null || !diffResult.Difference.MajorDifference);
		}

		public static string GetStringAttribute(XmlNode form, string attr)
		{
			try
			{
				return form.Attributes[attr].Value;
			}
			catch(NullReferenceException)
			{
				throw new XmlFormatException(String.Format("Expected a {0} attribute on {1}.", attr, form.OuterXml));
			}
		}

		public static string GetOptionalAttributeString(XmlNode xmlNode, string attributeName)
		{
			XmlAttribute attr = xmlNode.Attributes[attributeName];
			if (attr == null)
				return null;
			return attr.Value;
		}

		public static XmlNode GetDocumentNodeFromRawXml(string outerXml, XmlNode nodeMaker)
		{
			if(String.IsNullOrEmpty(outerXml))
			{
				throw new ArgumentException();
			}
			XmlDocument doc = nodeMaker as XmlDocument ?? nodeMaker.OwnerDocument;
			using (StringReader sr = new StringReader(outerXml))
			{
				using (XmlReader r = XmlReader.Create(sr))
				{
					r.Read();
					return doc.ReadNode(r);
				}
			}
		}

		public static string GetXmlForShowingInHtml(string xml)
		{
			var s = GetIndendentedXml(xml).Replace("<", "&lt;");
			s = s.Replace("\r\n", "<br/>");
			s = s.Replace("  ", "&nbsp;&nbsp;");
			return s;
		}

		public static string GetIndendentedXml(string xml)
		{
			return XElement.Parse(xml).ToString();
		}

		/// <summary>
		/// From http://stackoverflow.com/a/1352556/723299
		/// Produce an XPath literal equal to the value if possible; if not, produce
		/// an XPath expression that will match the value.
		///
		/// Note that this function will produce very long XPath expressions if a value
		/// contains a long run of double quotes.
		/// </summary>
		/// <param name="value">The value to match.</param>
		/// <returns>If the value contains only single or double quotes, an XPath
		/// literal equal to the value.  If it contains both, an XPath expression,
		/// using concat(), that evaluates to the value.</returns>
		public static string GetSafeXPathLiteral(string value)
		{
			// if the value contains only single or double quotes, construct
			// an XPath literal
			if (!value.Contains("\""))
			{
				return "\"" + value + "\"";
			}
			if (!value.Contains("'"))
			{
				return "'" + value + "'";
			}

			// if the value contains both single and double quotes, construct an
			// expression that concatenates all non-double-quote substrings with
			// the quotes, e.g.:
			//
			//    concat("foo", '"', "bar")
			StringBuilder sb = new StringBuilder();
			sb.Append("concat(");
			string[] substrings = value.Split('\"');
			for (int i = 0; i < substrings.Length; i++)
			{
				bool needComma = (i > 0);
				if (substrings[i] != "")
				{
					if (i > 0)
					{
						sb.Append(", ");
					}
					sb.Append("\"");
					sb.Append(substrings[i]);
					sb.Append("\"");
					needComma = true;
				}
				if (i < substrings.Length - 1)
				{
					if (needComma)
					{
						sb.Append(", ");
					}
					sb.Append("'\"'");
				}

			}
			sb.Append(")");
			return sb.ToString();
		}
		public static string SafelyGetStringTextNode(XmlNode node)
		{
			return (node == null || node.InnerText == String.Empty) ? String.Empty : node.InnerText.Trim();
		}

		public static bool IsTextLevel(XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (ours == null && theirs == null && ancestor == null)
				return false;

			// At least one of them has to be a text container.
			var ourStatus = IsTextNodeContainer(ours);
			var theirStatus = IsTextNodeContainer(theirs);
			var ancestorStatus = IsTextNodeContainer(ancestor);

			// If any of them is not a text container, the three are not, even if one or more of the others is.
			if (ourStatus == TextNodeStatus.IsNotTextNodeContainer || theirStatus == TextNodeStatus.IsNotTextNodeContainer || ancestorStatus == TextNodeStatus.IsNotTextNodeContainer)
				return false;
			// Unable to determine, so guess no.
			if (ourStatus == TextNodeStatus.IsAmbiguous && theirStatus == TextNodeStatus.IsAmbiguous && ancestorStatus == TextNodeStatus.IsAmbiguous)
				return false;

			/******************* WARNING *****************/
			// Don't let R# 'help' with the return layout, or it will continue 'helping',
			// until there is only one gianormous return that is understandable only by the compiler.
			/******************* WARNING *****************/
			if (ourStatus == TextNodeStatus.IsTextNodeContainer || theirStatus == TextNodeStatus.IsTextNodeContainer || ancestorStatus == TextNodeStatus.IsTextNodeContainer)
				return true; // One node is a text container, even if the other two aren't sure.

			return false;
		}

		public static TextNodeStatus IsTextNodeContainer(XmlNode node)
		{
			if (node == null)
				return TextNodeStatus.IsAmbiguous;

			var badNodeTypes = new HashSet<XmlNodeType>
			{
									XmlNodeType.None,
									XmlNodeType.Element,
									XmlNodeType.Attribute,
									XmlNodeType.CDATA,
									XmlNodeType.EntityReference,
									XmlNodeType.Entity,
									XmlNodeType.ProcessingInstruction,
									XmlNodeType.Comment,
									XmlNodeType.Document,
									XmlNodeType.DocumentType,
									XmlNodeType.DocumentFragment,
									XmlNodeType.Notation,
									XmlNodeType.EndElement,
									XmlNodeType.EndEntity,
									XmlNodeType.XmlDeclaration
								};
			if (node.ChildNodes.Cast<XmlNode>().Any(childNode => badNodeTypes.Contains(childNode.NodeType)))
				return TextNodeStatus.IsNotTextNodeContainer;

			if (node.NodeType != XmlNodeType.Element)
				return TextNodeStatus.IsNotTextNodeContainer;

			if (!node.HasChildNodes)
				return TextNodeStatus.IsAmbiguous;

			var goodNodeTypes = new HashSet<XmlNodeType>
			{
										XmlNodeType.SignificantWhitespace,
										XmlNodeType.Whitespace,
										XmlNodeType.Text
									};

			// I (RBR) think the only ones we can cope with are:
			// Text, Whitespace, and SignificantWhitespace.
			// Alternatively, one might be able to use the XmlText, XmlWhitespace,
			// and XmlSignificantWhitespace subclasses of XmlCharacterData (leaving out XmlCDataSection and XmlComment), which match well with those legal enums
			//return node.ChildNodes.Cast<XmlNode>().Any(childNode => !goodNodeTypes.Contains(childNode.NodeType))
			//    ? TextNodeStatus.IsNotTextNodeContainer
			//    : TextNodeStatus.IsTextNodeContainer;

			// It should have at least one XmlNodeType.Text
			if (node.InnerXml.Trim() == String.Empty)
				return TextNodeStatus.IsAmbiguous;
			return node.ChildNodes.Cast<XmlNode>().Any(childNode => !goodNodeTypes.Contains(childNode.NodeType))
				? TextNodeStatus.IsNotTextNodeContainer
				: TextNodeStatus.IsTextNodeContainer;
		}

		internal static void AddDateCreatedAttribute(XmlNode elementNode)
		{
			AddAttribute(elementNode, "dateCreated", DateTimeProvider.Current.Now.ToString(LiftUtils.LiftTimeFormatWithTimeZone));
		}

		internal static void AddAttribute(XmlNode element, string name, string value)
		{
			var attr = element.OwnerDocument.CreateAttribute(name);
			attr.Value = value;
			element.Attributes.Append(attr);
		}

		internal static IEnumerable<XmlAttribute> GetAttrs(XmlNode node)
		{
			return (node is XmlCharacterData || node == null)
					? new List<XmlAttribute>()
					: new List<XmlAttribute>(node.Attributes.Cast<XmlAttribute>()); // Need to copy so we can iterate while changing.
		}

		internal static XmlAttribute GetAttributeOrNull(XmlNode node, string name)
		{
			return node == null ? null : node.Attributes.GetNamedItem(name) as XmlAttribute;
		}

		/// <summary>
		/// This should be used when we want to replace a child element in our document with the contents
		/// found in their equivalent child.
		/// </summary>
		internal static void ReplaceOursWithTheirs(XmlNode ourParent, ref XmlNode ours, XmlNode theirs)
		{
			if (ourParent == null)
				throw new ArgumentNullException("ourParent");
			var ourOwnerDocument = ourParent.OwnerDocument;
			if (ourOwnerDocument == null)
				throw new ArgumentException("This method can not be used to replace root nodes.");
			if (theirs == null)
			{
				if (ours != null) // If both are null there is nothing to do, but if theirs is null delete ours
				{
					ourParent.RemoveChild(ours);
					ours = null;
				}
			}
			else
			{
				var theirData = theirs.Clone();
				theirData = ourOwnerDocument.ImportNode(theirData, true);
				if (ours == null)
					ourParent.AppendChild(theirData);
				else
					ourParent.ReplaceChild(theirData, ours);
				ours = theirData;
			}
		}
	}

	public enum TextNodeStatus
	{
		IsTextNodeContainer,
		IsNotTextNodeContainer,
		IsAmbiguous
	}

	public class XmlFormatException : ApplicationException
	{
		private string _filePath;
		public XmlFormatException(string message)
			: base(message)
		{
		}

		public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}
	}
}