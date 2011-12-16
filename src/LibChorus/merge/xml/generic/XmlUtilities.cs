using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Chorus.merge.xml.generic.xmldiff;


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

	public static class XmlNodeExtensions
	{
		/// <summary>
		/// this is safe to use with foreach, unlike SelectNodes
		/// </summary>
		public static XmlNodeList SafeSelectNodes(this XmlNode node, string path, params object[] args)
		{
			var x = node.SelectNodes(string.Format(path,args));
			if (x == null)
				return new NullXMlNodeList();
			return x;
		}

		public static string SelectTextPortion(this XmlNode node, string path, params object[] args)
		{
			var x = node.SelectNodes(string.Format(path, args));
			if (x == null || x.Count ==0)
				return string.Empty;
			return x[0].InnerText;
		}

		public static string GetStringAttribute(this XmlNode node, string attr)
		{
			try
			{
				return node.Attributes[attr].Value;
			}
			catch (NullReferenceException)
			{
				throw new XmlFormatException(string.Format("Expected a '{0}' attribute on {1}.", attr, node.OuterXml));
			}
		}
		public static string GetOptionalStringAttribute(this XmlNode node, string attributeName, string defaultValue)
		{
			XmlAttribute attr = node.Attributes[attributeName];
			if (attr == null)
				return defaultValue;
			return attr.Value;
		}
	}

	public class XmlUtilities
	{


		public static bool AreXmlElementsEqual(string ours, string theirs)
		{
			if (ours == theirs)
				return true;

			StringReader osr = new StringReader(ours);
			XmlReader or = XmlReader.Create(osr);
			XmlDocument od = new XmlDocument();
			XmlNode on = od.ReadNode(or);
			on.Normalize();

			StringReader tsr = new StringReader(theirs);
			XmlReader tr = XmlReader.Create(tsr);
			XmlDocument td = new XmlDocument();
			XmlNode tn = td.ReadNode(tr);
			tn.Normalize();//doesn't do much

			return AreXmlElementsEqual(on, tn);
		}

		/// <summary>
		/// this version of AreXmlElementsEqual is used to compare two xml strings
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
			StringReader osr = new StringReader(ours);
			XmlReader or = XmlReader.Create(osr);
			XmlDocument od = new XmlDocument();
			XmlNode on = od.ReadNode(or);
			on.Normalize();

			StringReader tsr = new StringReader(theirs);
			XmlReader tr = XmlReader.Create(tsr);
			XmlDocument td = new XmlDocument();
			XmlNode tn = td.ReadNode(tr);
			tn.Normalize();//doesn't do much

			System.Diagnostics.Debug.Assert(astrElementXPath.Length == astrAttributeToIgnore.Length);
			for (int i = 0; i < astrElementXPath.Length; i++)
			{
				RemoveItem(on, astrElementXPath[i], astrAttributeToIgnore[i]);
				RemoveItem(tn, astrElementXPath[i], astrAttributeToIgnore[i]);
			}

			return AreXmlElementsEqual(on, tn);
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
				if (ours.NodeType != XmlNodeType.Text)
				{
					return false;
				}
				bool oursIsEmpty = (ours.InnerText == null || ours.InnerText.Trim() == String.Empty);
				bool theirsIsEmpty = (theirs.InnerText == null || theirs.InnerText.Trim() == String.Empty);
				if(oursIsEmpty != theirsIsEmpty)
				{
					return false;
				}
				return ours.InnerText.Trim() == theirs.InnerText.Trim();
			}

			return AreXmlElementsEqual(new XmlInput(ours.OuterXml), new XmlInput(theirs.OuterXml));
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