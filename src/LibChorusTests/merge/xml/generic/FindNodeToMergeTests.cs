using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Tests for IFindNodeToMerge implementations.
	/// </summary>
	[TestFixture]
	public class FindNodeToMergeTests
	{
		[Test]
		public void CanFindMultipleAttributeKeyedElement()
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
			var result = nodeMatcher.GetNodeToMerge(nodeToMatch, otherDoc.DocumentElement);
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
				nodeMatcher.GetNodeToMerge(nodeToMatch, otherDoc.DocumentElement));
		}
	}
}