using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;


namespace LiftIO.Tests.Merging
{
	[TestFixture]
	public class MultiTextMergingTests
	{
		[Test]
		public void ConvertBogusElementToTextElementInLiftFile()
		{
			// Hack conversion, because Flex exported some lift-ranges stuff that wasn't legal.
			const string data = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						dateCreated='2011-03-09T17:08:44Z'
						dateModified='2012-05-18T08:31:54Z'
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='ldb-fonipa-x-emic'>
								<element name='text'>myStuff</element>
							</form>
						</lexical-unit>
					</entry>
				</lift>";
			var originalValue = XmlMergeService.RemoveAmbiguousChildNodes;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			var doc = new XmlDocument();
			doc.LoadXml(data);
			XmlMergeService.RemoveAmbiguousChildren(new ListenerForUnitTests(), new MergeStrategies(), doc.DocumentElement);
			XmlMergeService.RemoveAmbiguousChildNodes = originalValue;
			Assert.IsFalse(doc.DocumentElement.OuterXml.Contains("<element name=\"text\">first</element>"), "Still has bogus element");
			Assert.IsTrue(doc.DocumentElement.OuterXml.Contains("<text>myStuff</text>"), "Missing converted element");
		}

		[Test]
		public void ConvertBogusElementToTextElementInLiftRangesFile()
		{
			// Hack conversion, because Flex exported some lift-ranges stuff that wasn't legal.
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone'
		attr='data' >
							<form
								lang='ldb-fonipa-x-emic'>
								<element name='text'>myStuff</element>
							</form>
	</range>
</lift-ranges>";
			var originalValue = XmlMergeService.RemoveAmbiguousChildNodes;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			var doc = new XmlDocument();
			doc.LoadXml(data);
			XmlMergeService.RemoveAmbiguousChildren(new ListenerForUnitTests(), new MergeStrategies(), doc.DocumentElement);
			XmlMergeService.RemoveAmbiguousChildNodes = originalValue;
			Assert.IsFalse(doc.DocumentElement.OuterXml.Contains("<element name=\"text\">first</element>"), "Still has bogus element");
			Assert.IsTrue(doc.DocumentElement.OuterXml.Contains("<text>myStuff</text>"), "Missing converted element");
		}

		[Test]
		public void SkipConvertingElementToTextElementInAnotherFile()
		{
			// Hack conversion skip, because Flex exported some lift-ranges stuff that wasn't legal.
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<foo>
	<range
		id='theone'
		attr='data' >
							<form
								lang='ldb-fonipa-x-emic'>
								<element name='text'>myStuff</element>
							</form>
	</range>
</foo>";
			var originalValue = XmlMergeService.RemoveAmbiguousChildNodes;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			var doc = new XmlDocument();
			doc.LoadXml(data);
			XmlMergeService.RemoveAmbiguousChildren(new ListenerForUnitTests(), new MergeStrategies(), doc.DocumentElement);
			XmlMergeService.RemoveAmbiguousChildNodes = originalValue;
			Assert.IsTrue(doc.DocumentElement.OuterXml.Contains("<element name=\"text\">myStuff</element>"), "Still has bogus element");
			Assert.IsFalse(doc.DocumentElement.OuterXml.Contains("<text>myStuff</text>"), "Missing converted element");
		}
/*
		[Test]
		public void MergeMultiTextNodes_OneAddedNewMultiTextElement()
		{
			string red = @"<lexical-unit/>";
			string ancestor = red;

			string blue = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
							</lexical-unit>";

			CheckBothWays(red, blue, ancestor, "lexical-unit/form[@lang='one']/text[text()='first']");
		}

		//        private void CheckBothWays(string red, string blue, string ancestor, string xpath)
		//        {
		//            XmlNode result= LiftSavvyMergeStrategy.MergeMultiTextPieces(red, blue, ancestor);
		//            XmlTestHelper.AssertXPathMatchesExactlyOne(result.OuterXml, xpath);
		//            result= LiftSavvyMergeStrategy.MergeMultiTextPieces(blue, red, ancestor);
		//            XmlTestHelper.AssertXPathMatchesExactlyOne(result.OuterXml, xpath);
		//        }

		private void CheckBothWays(string red, string blue, string ancestor, params string[] xpaths)
		{
			CheckOneWay(red, blue, ancestor, xpaths);
			CheckOneWay(blue, red, ancestor, xpaths);
		}

		private void CheckOneWay(string ours, string theirs, string ancestor, params string[] xpaths)
		{
			XmlNode result = MultiTextMerger.MergeMultiTextPieces(ours, theirs, ancestor);
			foreach (string xpath in xpaths)
			{
				XmlTestHelper.AssertXPathMatchesExactlyOne(result.OuterXml, xpath);
			}
		}

		[Test]
		public void MergeMultiTextNodes_EachAddedDifferentAlternatives_GetBoth()
		{

			string ancestor = @"<lexical-unit>
							</lexical-unit>";


			string red = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
							</lexical-unit>";

			string blue = @"<lexical-unit>
								<form lang='two'>
									<text>second</text>
								</form>
							</lexical-unit>";

			CheckBothWays(red, blue, ancestor,
				"lexical-unit/form[@lang='one']/text[text()='first']",
				"lexical-unit/form[@lang='two']/text[text()='second']");
		}

		[Test]
		public void MergeMultiTextNodes_OneAddedAnAlternatives_GetBoth()
		{
			string red = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
							</lexical-unit>";

			string ancestor = red;

			string blue = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
								<form lang='two'>
									<text>second</text>
								</form>
							</lexical-unit>";

			CheckBothWays(red, blue, ancestor,
				"lexical-unit/form[@lang='one']/text[text()='first']",
				"lexical-unit/form[@lang='two']/text[text()='second']");
		}

		[Test]
		public void MergeMultiTextNodes_OnePutSomethingInPreviouslyEmptyForm()
		{
			string red = @"<lexical-unit>
								<form lang='one'/>
							</lexical-unit>";

			string ancestor = red;

			string blue = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
							</lexical-unit>";

			Assert.IsFalse(Utilities.AreXmlElementsEqual(red, blue));

			CheckBothWays(red, blue, ancestor,
				"lexical-unit/form[@lang='one']/text[text()='first']");
		}

		[Test]
		public void MergeMultiTextNodes_OnePutSomethingInPreviouslyEmptyFormText()
		{
			string red = @"<lexical-unit>
								<form lang='one'><text/></form>
							</lexical-unit>";

			string ancestor = red;


			string blue = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
							</lexical-unit>";

			CheckBothWays(red, blue, ancestor,
				"lexical-unit/form[@lang='one']/text[text()='first']");
		}

		[Test]
		public void WeDeletedAForm_FormRemoved()
		{
			string red = @"<lexical-unit></lexical-unit>";
			string blue = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
							</lexical-unit>";
			string ancestor = blue;

			CheckOneWay(blue, red, ancestor, "lexical-unit[ not(form)]");
		}

		[Test]
		public void TheyDeleteAForm_FormRemoved()
		{
			string red = @"<lexical-unit></lexical-unit>";
			string blue = @"<lexical-unit>
								<form lang='one'>
									<text>first</text>
								</form>
							</lexical-unit>";
			string ancestor = blue;

			CheckOneWay(blue, red, ancestor, "lexical-unit[ not(form)]");
		}
*/
	}
}
