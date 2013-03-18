using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class FindByKeyAttributeInListTests
	{
		[Test]
		public void FindsNothingInEmptyList()
		{
			var doc = new XmlDocument();
			var parent = XmlUtilities.GetDocumentNodeFromRawXml(@"<a/>", doc);
			var target = XmlUtilities.GetDocumentNodeFromRawXml(@"<b/>", doc);
			var finder = new FindByKeyAttributeInList("key");
			Assert.That(finder.GetNodeToMerge(target, parent, SetFromChildren.Get(parent)), Is.Null);
		}

		[Test]
		public void FindsNothingInNullList()
		{
			var doc = new XmlDocument();
			var target = XmlUtilities.GetDocumentNodeFromRawXml(@"<b/>", doc);
			var finder = new FindByKeyAttributeInList("key");
			Assert.That(finder.GetNodeToMerge(target, null, new HashSet<XmlNode>()), Is.Null);
		}

		[Test]
		public void FindsNothingWithNoKey()
		{
			var doc = new XmlDocument();
			var parent = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/></a>", doc);
			var target = XmlUtilities.GetDocumentNodeFromRawXml(@"<b/>", doc);
			var finder = new FindByKeyAttributeInList("key");
			Assert.That(finder.GetNodeToMerge(target, parent, SetFromChildren.Get(parent)), Is.Null);
		}

		[Test]
		public void FindsUniqueMatch()
		{
			var doc = new XmlDocument();
			var parent = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/><b key='two'/></a>", doc);
			var target = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='two'/></a>", doc).FirstChild;
			var finder = new FindByKeyAttributeInList("key");
			Assert.That(finder.GetNodeToMerge(target, parent, SetFromChildren.Get(parent)), Is.EqualTo(parent.ChildNodes[1]));
		}

		[Test]
		public void FindsCorrespondingMatchInSecondPlace()
		{
			var doc = new XmlDocument();
			var parent = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/><b key='two'/><b key='one'/></a>", doc);
			var target = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/><b key='one'/></a>", doc).ChildNodes[1];
			var finder = new FindByKeyAttributeInList("key");
			Assert.That(finder.GetNodeToMerge(target, parent, SetFromChildren.Get(parent)), Is.EqualTo(parent.ChildNodes[2]));
		}

		[Test]
		public void FindsNoMatchWhereTooFewOccurrences()
		{
			var doc = new XmlDocument();
			var parent = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/><b key='two'/></a>", doc);
			var target = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/><b key='one'/></a>", doc).ChildNodes[1];
			var finder = new FindByKeyAttributeInList("key");
			Assert.That(finder.GetNodeToMerge(target, parent, SetFromChildren.Get(parent)), Is.Null);
		}

		[Test]
		public void FindsSecondDuplicateWhereFirstNotAcceptable()
		{
			var doc = new XmlDocument();
			var parent = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/><b key='two'/><b key='two'/></a>", doc);
			var target = XmlUtilities.GetDocumentNodeFromRawXml(@"<a><b key='one'/><b key='two'/></a>", doc).ChildNodes[1];
			var desiredResult = parent.ChildNodes[2];
			var otherOption = parent.ChildNodes[1]; // acceptable but not matching
			var acceptableTargets = new HashSet<XmlNode>(new [] {otherOption, desiredResult});
			var finder = new FindByKeyAttributeInList("key");
			Assert.That(finder.GetNodeToMerge(target, parent, acceptableTargets), Is.EqualTo(parent.ChildNodes[2]));
		}
	}
}
