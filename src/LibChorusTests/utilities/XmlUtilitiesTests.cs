using System.Xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.utilities
{
	[TestFixture]
	public class XmlUtilitiesTests
	{
		#region IsTextNodeContainer

		[Test]
		public void NullNodeIsAnAmbiguousTextNode()
		{
			Assert.AreEqual(TextNodeStatus.IsAmbiguous, XmlUtilities.IsTextNodeContainer(null));
		}

		[Test]
		public void EmptyElementNodeIsAnAmbiguousTextNode()
		{
			var doc = new XmlDocument();
			var node = doc.CreateElement("myelement");
			Assert.AreEqual(TextNodeStatus.IsAmbiguous, XmlUtilities.IsTextNodeContainer(node));
		}

		[Test]
		public void XmlDeclarationNodeIsNotTextNode()
		{
			var doc = new XmlDocument();
			var decNode = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
			Assert.AreEqual(TextNodeStatus.IsNotTextNodeContainer, XmlUtilities.IsTextNodeContainer(decNode));
		}

		[Test]
		public void TextNodeIsNotTextNode()
		{
			var doc = new XmlDocument();
			var node = doc.CreateTextNode("mytext");
			Assert.AreEqual(TextNodeStatus.IsNotTextNodeContainer, XmlUtilities.IsTextNodeContainer(node));
		}

		[Test]
		public void ElementNodeWithElementNodeChildIsNotTextlevel()
		{
			var doc = new XmlDocument();
			var node = doc.CreateElement("myelement");
			var childNode = doc.CreateElement("mychild");
			node.AppendChild(childNode);
			Assert.AreEqual(TextNodeStatus.IsNotTextNodeContainer, XmlUtilities.IsTextNodeContainer(node));
		}

		[Test]
		public void ElementNodeWithTextNodeChildIsTextlevel()
		{
			var doc = new XmlDocument();
			var node = doc.CreateElement("myelement");
			var textNode = doc.CreateTextNode("mytext");
			node.AppendChild(textNode);
			Assert.AreEqual(TextNodeStatus.IsTextNodeContainer, XmlUtilities.IsTextNodeContainer(node));
		}

		#endregion IsTextNodeContainer

		#region IsTextLevel

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

		#endregion IsTextLevel
	}
}
