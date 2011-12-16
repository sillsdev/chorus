using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.merge.xml.lift
{
	/// <summary>
	/// examples are tricky because they are complex, multiple, but have no ids!
	/// </summary>

	[TestFixture]
	public class ExampleSentenceMergingTests
	{
		[Test, Ignore("not implemented yet")]
		public void OneEditedExampleWhileOtherAddedTranslation_MergesButRaiseWarning()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
<lift version='0.10' producer='WeSay 1.0.0.0'>
	<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
		<sense id='123'>
	 <example>
		<form lang='chorus'>
		  <text>This is my example sentence.</text>
		</form>
	  </example>
	</sense>
	</entry>
</lift>";

			var ours = ancestor.Replace("This is my", "This is our");
			var theirs = ancestor.Replace("</example>","<translation><form lang='en'><text>hello</text></form></translation></example>");

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation)
					{ EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"header",
					"entry", "guid", LiftFileHandler.WritePreliminaryInformation);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.AreEqual(1, listener.Conflicts.Count);
				var warning = listener.Warnings[0];
				Assert.AreEqual(typeof(BothEditedDifferentPartsOfDependentPiecesOfDataWarning), warning.GetType(), warning.ToString());
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//example");
			}
		}

		[Test, Ignore("not implemented yet")]
		public void OneAddedOneTranslationWhileOtherAddedAnother_Merged()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
<lift version='0.10' producer='WeSay 1.0.0.0'>
	<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
		<sense id='123'>
	 <example>
		<form lang='chorus'>
		  <text>This is my example sentence.</text>
		</form>
	  </example>
	</sense>
	</entry>
</lift>";

			var ours = ancestor.Replace("</example>", "<translation><form lang='tp'><text>Dispela em i sentens bilong mi.</text></form></translation></example>");
			var theirs = ancestor.Replace("</example>", "<translation><form lang='en'><text>hello</text></form></translation></example>");

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation)
					{ EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"header",
					"entry", "guid", LiftFileHandler.WritePreliminaryInformation);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//example");
			}
		}
	}
}