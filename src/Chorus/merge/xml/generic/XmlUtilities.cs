using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
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
		public static XmlNodeList SafeSelectNodes(this XmlNode node, string path)
		{
			var x = node.SelectNodes(path);
			if (x == null)
				return new NullXMlNodeList();
			return x;
		}

	}

	public class XmlUtilities
	{


		public static bool AreXmlElementsEqual(string ours, string theirs)
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

//            StringBuilder builder = new StringBuilder();
//            XmlWriter w = XmlWriter.Create(builder);


			return AreXmlElementsEqual(on, tn);
		}

		public static bool AreXmlElementsEqual(XmlNode ours, XmlNode theirs)
		{
			if (ours.NodeType == XmlNodeType.Text)
			{
				if (ours.NodeType != XmlNodeType.Text)
				{
					return false;
				}
				bool oursIsEmpty = (ours.InnerText == null || ours.InnerText.Trim() == string.Empty);
				bool theirsIsEmpty = (theirs.InnerText == null || theirs.InnerText.Trim() == string.Empty);
				if(oursIsEmpty != theirsIsEmpty)
				{
					return false;
				}
				return ours.InnerText.Trim() == theirs.InnerText.Trim();
			}
			// DiffConfiguration config = new DiffConfiguration(WhitespaceHandling.None);
			XmlDiff diff = new XmlDiff(new XmlInput(ours.OuterXml), new XmlInput(theirs.OuterXml));//, config);
			DiffResult d = diff.Compare();
			return (d == null || d.Difference == null || !d.Difference.MajorDifference);
		}

		public static string GetStringAttribute(XmlNode form, string attr)
		{
			try
			{
				return form.Attributes[attr].Value;
			}
			catch(NullReferenceException)
			{
				throw new XmlFormatException(string.Format("Expected a {0} attribute on {1}.", attr, form.OuterXml));
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
			if(string.IsNullOrEmpty(outerXml))
			{
				throw new ArgumentException();
			}
			XmlDocument doc = nodeMaker as XmlDocument;
			if(doc == null)
			{
				doc = nodeMaker.OwnerDocument;
			}
			using (StringReader sr = new StringReader(outerXml))
			{
				using (XmlReader r = XmlReader.Create(sr))
				{
					r.Read();
					return doc.ReadNode(r);
				}
			}
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