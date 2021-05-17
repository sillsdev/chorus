using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml
{
	[TestFixture]
	public sealed class FindByKeyAttributeTests
	{
		// A regression test: http://jira.palaso.org/issues/browse/WS-33895
		// Some lift ids have single quotes in them, so the xpath needs to be escaped correctly.
		[Test]
		public void GetNodeToMerge_WithSingleQuoteInAttribute_NoThrow()
		{
			string xml =
				@"<lift>
					<entry id=""te'st"" />
				</lift>";

			var doc1 = new XmlDocument();
			doc1.LoadXml(xml);

			var finder = new FindByKeyAttribute("id");
			var node = doc1.SelectSingleNode("//entry");
			var result = finder.GetNodeToMerge(node, doc1.DocumentElement, SetFromChildren.Get(doc1.DocumentElement));
			Assert.NotNull(result);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "entry[@id=\"te'st\"]");
		}

		[Test]
		public void GetNodeToMerge_WithDoubleQuoteInAttribute_FindsIt()
		{
			string xml =
				@"<lift>
					<entry id='she said &quot;Hi!&quot;' />
				</lift>";

			var doc1 = new XmlDocument();
			doc1.LoadXml(xml);

			var finder = new FindByKeyAttribute("id");
			var node = doc1.SelectSingleNode("//entry");
			var result = finder.GetNodeToMerge(node, doc1.DocumentElement, SetFromChildren.Get(doc1.DocumentElement));
			Assert.AreEqual(node,result);
		}

		[Test]
		public void GetNodeToMerge_WithoutKeyAttr_ReturnsNull()
		{
			const string xml =
				@"<a>
					<b />
				</a>";

			var doc1 = new XmlDocument();
			doc1.LoadXml(xml);

			var finder = new FindByKeyAttribute("id");
			var node = doc1.SelectSingleNode("//b");
			Assert.That(finder.GetNodeToMerge(node, doc1.DocumentElement, SetFromChildren.Get(doc1.DocumentElement)), Is.Null);
		}

		[Test]
		public void GetNodeToMerge_WithDoubleAndSingleQuotesInAttribute_FindsIt()
		{
			string xml =
				@"<lift>
					<entry id='she said &quot;It&apos;s raining!&quot;' />
				</lift>";

			var doc1 = new XmlDocument();
			doc1.LoadXml(xml);

			var finder = new FindByKeyAttribute("id");
			var node = doc1.SelectSingleNode("//entry");
			var result = finder.GetNodeToMerge(node, doc1.DocumentElement, SetFromChildren.Get(doc1.DocumentElement));
			Assert.AreEqual(node, result);
		}

		[Test]
		public void GetNodeToMerge_ReturnsNode()
		{
			string xml =
				@"<lift>
					<entry id='test' />
				</lift>";

			var doc1 = new XmlDocument();
			doc1.LoadXml(xml);

			var finder = new FindByKeyAttribute("id");
			var node = doc1.SelectSingleNode("//entry");
			var result = finder.GetNodeToMerge(node, doc1.DocumentElement, SetFromChildren.Get(doc1.DocumentElement));
			Assert.NotNull(result);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "entry[@id=\"test\"]");
		}

		// This is what we would expect to happen if we enhance this strategy so that there really can be multiple items
		// with the same key, and acceptableTargets can help select the right one. Currently it fails because the duplicate
		// key just causes a crash.
//        [Test]
//        public void GetNodeToMerge_WithTwoOptionsFirstNotAcceptable_ReturnsSecond()
//        {
//            string xml =
//                @"<lift>
//                    <entry id='test' />
//                    <entry id='test' />
//		         </lift>";

//            var doc1 = new XmlDocument();
//            doc1.LoadXml(xml);

//            var finder = new FindByKeyAttribute("id");
//            var node1 = doc1.DocumentElement.ChildNodes[0];
//            var node2 = doc1.DocumentElement.ChildNodes[0];
//            var acceptableTargets = new HashSet<XmlNode>();
//            acceptableTargets.Add(node2);
//            var result = finder.GetNodeToMerge(node1, doc1.DocumentElement, acceptableTargets);
//            Assert.That(result, Is.EqualTo(node2));
//        }
	}

}
