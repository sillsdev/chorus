using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;
using SIL.PlatformUtilities;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Tests for IFindNodeToMerge implementations.
	/// </summary>
	[TestFixture]
	public class FindNodeToMergeTests
	{
		[Test]
		public void MultipleAttributeKeyedElement_IsFound()
		{
			const string sourceXml = @"<root>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</root>";
			const string otherXml = @"<root>
<CustomField name='IsComplete' class='WfiWordform' type='Boolean' />
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
<CustomField name='Checkpoint' class='WfiWordform' type='String' />
</root>";

			var sourceDoc = new XmlDocument();
			sourceDoc.LoadXml(sourceXml);
			var nodeToMatch = sourceDoc.DocumentElement.FirstChild;

			var otherDoc = new XmlDocument();
			otherDoc.LoadXml(otherXml);

			var nodeMatcher = new FindByMultipleKeyAttributes(new List<string> {"name", "class"});
			var result = nodeMatcher.GetNodeToMerge(nodeToMatch, otherDoc.DocumentElement, SetFromChildren.Get(otherDoc.DocumentElement));
			Assert.AreSame(otherDoc.DocumentElement.ChildNodes[1], result);
		}

		[Test, Ignore("Resolve logical merge conflict")]
		public void MultipleAttributeKeyedElement_WithDoubleAndSingleQuoteInAttribute_IsFound()
		{
			const string sourceXml = @"<root>
<CustomField name='First quoted &quot;Apostrophe&apos;s&quot;' class='Second quoted &quot;Apostrophe&apos;s&quot;' type='Boolean' />
</root>";
			const string otherXml = @"<root>
<CustomField name='IsComplete' class='WfiWordform' type='Boolean' />
<CustomField name='First quoted &quot;Apostrophe&apos;s&quot;' class='Second quoted &quot;Apostrophe&apos;s&quot;' type='Boolean' />
<CustomField name='Checkpoint' class='WfiWordform' type='String' />
</root>";

			var sourceDoc = new XmlDocument();
			sourceDoc.LoadXml(sourceXml);
			var nodeToMatch = sourceDoc.DocumentElement.FirstChild;

			var otherDoc = new XmlDocument();
			otherDoc.LoadXml(otherXml);

			var nodeMatcher = new FindByMultipleKeyAttributes(new List<string> { "name", "class" });
			if (Platform.IsMono || otherDoc.DocumentElement == null)
				return;

			var acceptableTargets = new HashSet<XmlNode>();
			foreach (XmlNode node in otherDoc.DocumentElement.ChildNodes)
				acceptableTargets.Add(node);
			var result = nodeMatcher.GetNodeToMerge(nodeToMatch, otherDoc.DocumentElement, acceptableTargets);
			Assert.AreSame(otherDoc.DocumentElement.ChildNodes[1], result);
		}

		[Test]
		public void CanFindByMatchingAttributeNames()
		{
			const string sourceXml =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
</ldml>";
			const string otherXml =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1' />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
</ldml>";

			var sourceDoc = new XmlDocument();
			sourceDoc.LoadXml(sourceXml);
			var nodeToMatch = sourceDoc.DocumentElement.FirstChild;

			var otherDoc = new XmlDocument();
			otherDoc.LoadXml(otherXml);

			var nodeMatcher = new FindByMatchingAttributeNames(new HashSet<string> { "xmlns:palaso" });
			Assert.AreSame(otherDoc.DocumentElement.ChildNodes[1],
				nodeMatcher.GetNodeToMerge(nodeToMatch, otherDoc.DocumentElement,
					SetFromChildren.Get(otherDoc.DocumentElement)));
		}

		[Test]
		public void CanFindByMatchingAttributeNames_SkipsUnacceptable()
		{
			const string sourceXml =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
</ldml>";
			const string otherXml =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1' />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v2' />
</ldml>";

			var sourceDoc = new XmlDocument();
			sourceDoc.LoadXml(sourceXml);
			var nodeToMatch = sourceDoc.DocumentElement.FirstChild;

			var otherDoc = new XmlDocument();
			otherDoc.LoadXml(otherXml);

			var nodeMatcher = new FindByMatchingAttributeNames(new HashSet<string> { "xmlns:palaso" });

			// We don't want the first xmlns:palaso child (someone else deleted it, maybe)
			var acceptableTargets =
				new HashSet<XmlNode>(new[] {otherDoc.DocumentElement.FirstChild, otherDoc.DocumentElement.ChildNodes[2]});
			Assert.AreSame(otherDoc.DocumentElement.ChildNodes[2],
				nodeMatcher.GetNodeToMerge(nodeToMatch, otherDoc.DocumentElement, acceptableTargets));
		}
	}
}