using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;


namespace LibChorus.Tests.merge.xml.lift
{
	[TestFixture]
	public class MultiTextMergingTests
	{
		[Test]
		public void ConvertBogusElementToTextElementInLiftFile()
		{
			// Hack conversion, because Flex exported some lift-ranges stuff that wasn't legal.
			const string data = @"<entry
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
					</entry>";
			var originalValue = XmlMergeService.RemoveAmbiguousChildNodes;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			var result = XmlMergeService.RemoveAmbiguousChildren(new ListenerForUnitTests(), new MergeStrategies(), data, "some.lift");
			XmlMergeService.RemoveAmbiguousChildNodes = originalValue;
			Assert.That(result, Does.Not.Contain("<element name=\"text\">first</element>"), "Still has bogus <element> element.");
			Assert.That(result, Does.Contain("<text>myStuff</text>"), "Converted <text> element is not present.");
		}

		[Test]
		public void ConvertBogusElementToTextElementInLiftRangesFile()
		{
			// Hack conversion, because Flex exported some lift-ranges stuff that wasn't legal.
			const string data =
@"<range
		id='theone'
		attr='data' >
							<form
								lang='ldb-fonipa-x-emic'>
								<element name='text'>myStuff</element>
							</form>
	</range>";
			var originalValue = XmlMergeService.RemoveAmbiguousChildNodes;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			var doc = new XmlDocument();
			doc.LoadXml(data);
			var results = XmlMergeService.RemoveAmbiguousChildren(new ListenerForUnitTests(), new MergeStrategies(), data, "some.lift-ranges");
			XmlMergeService.RemoveAmbiguousChildNodes = originalValue;
			Assert.That(results, Does.Not.Contain("<element name=\"text\">first</element>"), "Still has bogus <element> element.");
			Assert.That(results, Does.Contain("<text>myStuff</text>"), "Converted <text> element is not present.");
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
			Assert.That(doc.DocumentElement.OuterXml, Does.Not.Contain("<text>myStuff</text>"), "Converted <element> element to <text>, but should not have.");
			Assert.That(doc.DocumentElement.OuterXml, Does.Contain("<element name=\"text\">myStuff</element>"), "Element <element> went away, but should have been present.");
		}
	}
}
