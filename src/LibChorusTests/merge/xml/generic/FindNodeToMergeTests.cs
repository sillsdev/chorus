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
	}
}