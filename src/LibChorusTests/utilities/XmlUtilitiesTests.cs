using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.utilities
{
	[TestFixture]
	public class XmlUtilitiesTests
	{
		[Test]
		public void ClosedNodeAndEmptyNodeAreEqual()
		{
			const string ours =
@"<foo />";
			const string theirs =
@"<foo></foo>";
			Assert.True(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.True(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void ClosedNodeAndNewTextAreNotEqual()
		{
			const string ours =
@"<foo />";
			const string theirs =
@"<foo>New foo text.</foo>";
			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void ClosedNodeAndEmptyNodeWithAttrsAreEqual()
		{
			const string ours =
@"<foo attr='val' />";
			const string theirs =
@"<foo attr='val'></foo>";
			Assert.True(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.True(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void ClosedNodeAndNewTextWithAttributesAreNotEqual()
		{
			const string ours =
@"<foo attr='val' />";
			const string theirs =
@"<foo attr='val' >New foo text.</foo>";
			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void NestedClosedNodeAndEmptyWithAttributesAreEqual()
		{
			const string ours =
@"<foo attr='val'>
<bar attr='val'/>
</foo>";
			const string theirs =
@"<foo attr='val'>
<bar attr='val'></bar>
</foo>";
			Assert.True(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.True(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void NestedClosedNodeAndTextWithAttributesAreNotEqual()
		{
			const string ours =
@"<foo attr='val'>
<bar attr='val'/>
</foo>";
			const string theirs =
@"<foo attr='val'>
<bar attr='val'>new stuff.</bar>
</foo>";
			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void MoveToFirstAttributeFix_HasElementsEqual()
		{
			const string ours =
@"<rt class='ScrTxtPara' guid='0030a77d-63cd-4d51-b26a-27bac7d64f17' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5'>
<Contents>
<Str>
<Run ws='tuz'></Run>
</Str>
</Contents>
<ParseIsCurrent val='False' />
<StyleRules>
<Prop namedStyle='Section Head' />
</StyleRules>
<Translations>
<objsur guid='fe6f0999-ecb9-403f-abab-e934318542bc' t='o' />
</Translations>
</rt>";
			const string theirs =
@"<rt class='ScrTxtPara' guid='0030a77d-63cd-4d51-b26a-27bac7d64f17' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5'>
<Contents>
<Str>
<Run ws='tuz' />
</Str>
</Contents>
<ParseIsCurrent val='False' />
<StyleRules>
<Prop namedStyle='Section Head' />
</StyleRules>
<Translations>
<objsur guid='fe6f0999-ecb9-403f-abab-e934318542bc' t='o' />
</Translations>
</rt>";
			Assert.True(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.True(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void MoreMinimal_MoveToFirstAttributeFix_HasElementsEqual()
		{
			const string ours =
@"<rt class='ScrTxtPara' guid='0030a77d-63cd-4d51-b26a-27bac7d64f17' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5'>
<Contents>
<Str>
<Run ws='tuz'></Run>
</Str>
</Contents>
<ParseIsCurrent val='False' />
</rt>";
			const string theirs =
@"<rt class='ScrTxtPara' guid='0030a77d-63cd-4d51-b26a-27bac7d64f17' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5'>
<Contents>
<Str>
<Run ws='tuz' />
</Str>
</Contents>
<ParseIsCurrent val='False' />
</rt>";
			Assert.True(XmlUtilities.AreXmlElementsEqual(ours, theirs));
			Assert.True(XmlUtilities.AreXmlElementsEqual(theirs, ours));
		}

		[Test]
		public void EquivalentByteArraysAreEqual()
		{
			var ours = Encoding.UTF8.GetBytes(@"<rt class='ScrTxtPara' guid='0030a77d-63cd-4d51-b26a-27bac7d64f17' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5'>
<Contents>
<Str>
<Run ws='tuz' />
</Str>
</Contents>
<ParseIsCurrent val='False' />
</rt>");
			var theirs = Encoding.UTF8.GetBytes(@"<rt class='ScrTxtPara' guid='0030a77d-63cd-4d51-b26a-27bac7d64f17' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5'>
<Contents>
<Str>
<Run ws='tuz' />
</Str>
</Contents>
<ParseIsCurrent val='False' />
</rt>");

			Assert.IsTrue(XmlUtilities.AreXmlElementsEqual(ours, theirs), "ours != theirs");
			Assert.IsTrue(XmlUtilities.AreXmlElementsEqual(theirs, ours), "theirs != ours");
		}

		[Test]
		public void NonEquivalentByteArraysAreNotEqual()
		{
			var ours = Encoding.UTF8.GetBytes(@"<rt class='ScrTxtPara' guid='0030a77d-63cd-4d51-b26a-27bac7d64f17' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5' />");
			var theirs = Encoding.UTF8.GetBytes(@"<rt class='LexEntry' guid='0030a77d-63cd-4d51-b26a-27bac7d64f18' ownerguid='046d6079-2337-425f-a8bd-b0af047fb5e5' />");

			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(ours, theirs), "ours == theirs");
			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(theirs, ours), "theirs == ours");
		}

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

#region ReplaceOursWithTheirsTests

		[Test]
		public void ReplaceOursWithTheirs_OursNullTheirsNot()
		{
			var ourDoc = CreateTestNode(@"<parent></parent>");
			var theirDoc = CreateTestNode(@"<parent><child>theirs</child></parent>");
			XmlNode ours = null;
			XmlNode theirs = theirDoc.FirstChild;
			XmlUtilities.ReplaceOursWithTheirs(ourDoc, ref ours, theirs);
			Assert.AreSame(ourDoc.OwnerDocument, ours.OwnerDocument);
			Assert.IsTrue(XmlUtilities.AreXmlElementsEqual(theirs, ours), "theirs != ours");
		}

		[Test]
		public void ReplaceOursWithTheirs_OursNotNullTheirsNotNull()
		{
			var ourDoc = CreateTestNode(@"<parent><child>mine</child></parent>");
			var theirDoc = CreateTestNode(@"<parent><child>theirs</child></parent>");
			XmlNode ours = ourDoc.FirstChild;
			XmlNode theirs = theirDoc.FirstChild;
			XmlUtilities.ReplaceOursWithTheirs(ourDoc, ref ours, theirs);
			Assert.AreSame(ourDoc.OwnerDocument, ours.OwnerDocument, "Returned node not in inserted into our parent document");
			Assert.IsTrue(XmlUtilities.AreXmlElementsEqual(theirs, ours), "theirs != ours");
		}

		[Test]
		public void ReplaceOursWithTheirs_OursNotNullTheirsNull()
		{
			var ourDoc = CreateTestNode(@"<parent><child>mine</child></parent>");
			XmlNode ours = ourDoc.FirstChild;
			XmlUtilities.ReplaceOursWithTheirs(ourDoc, ref ours, null);
			Assert.Null(ours, "Our node not replaced with null");
			Assert.Null(ourDoc.FirstChild, "Our node was left in the document");
		}

		[Test]
		public void ReplaceOursWithTheirs_NullParentNodeThrows()
		{
			XmlNode ours = null;
			Assert.Throws<ArgumentNullException>(() => XmlUtilities.ReplaceOursWithTheirs(null, ref ours, null));
		}

		[Test]
		public void ReplaceOursWithTheirs_DocumentNodeAsParentThrows()
		{
			var ourDoc = CreateTestNode(@"<parent><child>mine</child></parent>");
			var theirDoc = CreateTestNode(@"<parent><child>theirs</child></parent>");

			Assert.Throws<ArgumentException>(() => XmlUtilities.ReplaceOursWithTheirs(ourDoc.OwnerDocument, ref ourDoc, theirDoc));
		}

		private XmlNode CreateTestNode(string xml)
		{
			using (var stringReader = new StringReader(xml))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				var xmlDocument = new XmlDocument();
				var xmlNode = xmlDocument.ReadNode(xmlReader);
				return xmlNode;
			}
		}
#endregion
	}
}
