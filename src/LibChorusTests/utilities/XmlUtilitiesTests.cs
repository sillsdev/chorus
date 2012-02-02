using System.Xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.utilities
{
	[TestFixture]
	public class XmlUtilitiesTests
	{
		[Test]
		public void NullNodeIsTextlevel()
		{
			Assert.IsTrue(XmlUtilities.IsTextLevel(null));
		}

		[Test]
		public void XmlDeclarationNodeIsNotTextlevel()
		{
			var doc = new XmlDocument();
			var decNode = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
			Assert.IsFalse(XmlUtilities.IsTextLevel(decNode));
		}

		[Test]
		public void EmptyElementNodeIsTextlevel()
		{
			var doc = new XmlDocument();
			var node = doc.CreateElement("myelement");
			Assert.IsTrue(XmlUtilities.IsTextLevel(node));
		}

		[Test]
		public void TextNodeIsNotTextlevel()
		{
			var doc = new XmlDocument();
			var node = doc.CreateTextNode("mytext");
			Assert.IsFalse(XmlUtilities.IsTextLevel(node));
		}

		[Test]
		public void ElementNodeWithTextNodeChildIsTextlevel()
		{
			var doc = new XmlDocument();
			var node = doc.CreateElement("myelement");
			var textNode = doc.CreateTextNode("mytext");
			node.AppendChild(textNode);
			Assert.IsTrue(XmlUtilities.IsTextLevel(node));
		}

		[Test]
		public void ElementNodeWithElementNodeChildIsNotTextlevel()
		{
			var doc = new XmlDocument();
			var node = doc.CreateElement("myelement");
			var childNode = doc.CreateElement("mychild");
			node.AppendChild(childNode);
			Assert.IsFalse(XmlUtilities.IsTextLevel(node));
		}

		[Test]
		public void NonNullElementsWithNoChildNodesIsNotTextlevel()
		{
			var doc = new XmlDocument();
			var ours = doc.CreateElement("myelement");
			var theirs = doc.CreateElement("myelement");
			var ancestor = doc.CreateElement("myelement");
			Assert.IsFalse(XmlUtilities.IsTextLevel(ours, theirs, ancestor));
		}

		[Test]
		public void AllNullsIsNotTextlevel()
		{
			Assert.IsFalse(XmlUtilities.IsTextLevel(null, null, null));
		}
	}
}
