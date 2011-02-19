using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml
{
	public class XmlTestHelper
	{
		public static void AssertXPathMatchesExactlyOne(string xml, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			AssertXPathMatchesExactlyOneInner(doc, xpath);
		}
		public static void AssertXPathMatchesExactlyOne(XmlNode node, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(node.OuterXml);
			AssertXPathMatchesExactlyOneInner(doc, xpath);
		}

		private static void AssertXPathMatchesExactlyOneInner(XmlDocument doc, string xpath)
		{
			XmlNodeList nodes = doc.SelectNodes(xpath);
			if (nodes == null || nodes.Count != 1)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
				if (nodes != null && nodes.Count > 1)
				{
					Assert.Fail("Too Many matches for XPath: {0}", xpath);
				}
				else
				{
					Assert.Fail("No Match: XPath failed: {0}", xpath);
				}
			}
		}

		public static void AssertXPathNotNull(string documentPath, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node == null)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNotNull(node);
		}

		public static void AssertXPathIsNull(string xml, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node != null)
			{
				XmlWriterSettings settings = new XmlWriterSettings
												{
													Indent = true,
													ConformanceLevel = ConformanceLevel.Fragment
												};
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNull(node);
		}
	}
}