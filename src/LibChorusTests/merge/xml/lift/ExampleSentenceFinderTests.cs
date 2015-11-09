using System.Xml;
using Chorus.FileTypeHandlers.lift;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.lift
{
#if MaybeSomeday
	public class ExampleSentenceFinderTests
	{
		[Test]
		public void ExampleSentenceFinder_SingleIdenticalForm_FindsIt()
		{
			string ancestor =
				@"<?xml version='1.0' encoding='utf-8'?>
<sense id='123'>
	 <example>
		<form lang='chorus'>
		  <text>This is my example sentence.</text>
		</form>
	  </example>
</sense>";

			var dom = new XmlDocument();
			dom.LoadXml(ancestor);
			var parent = dom.SelectSingleNode("//sense");
			var guyToMatch = dom.SelectSingleNode("//example");
			var finder = new ExampleSentenceFinder();
			var match = finder.GetNodeToMerge(guyToMatch, parent, SetFromChildren.Get(parent));
			Assert.AreEqual(guyToMatch.OuterXml, match.OuterXml);
		}
	}
#endif
}