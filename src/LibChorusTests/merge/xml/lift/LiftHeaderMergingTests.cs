using System.IO;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

namespace LibChorus.Tests.merge.xml.lift
{
	class LiftHeaderMergingTests
	{
		private string _ours;
		private string _theirs;
		private string _ancestor;

		[SetUp]
		public void Setup()
		{
			/*
			 * Bad idea to put this in both ours and theirs, since it causes a crash in a Dictionary.
			 * The likelihood of two apps adding the same entry with the same guid is precisely 0, after all.
						<entry id='sameInBoth'>
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
			*/
			_ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='SIL.FLEx 7.2.2.40940'>
						<header>
							<ranges>
								<range
									id='etymology'
									href='file://C:/Documents and Settings/JBharani/Local Settings/Application Data/LiftBridge/NewTest/Sena 3.lift.lift-ranges' />
								<range
									id='dialect'
									href='file://C:/Documents and Settings/JBharani/Local Settings/Application Data/LiftBridge/NewTest/Sena 3.lift.lift-ranges' />
								<range
									id='newrange1'
									href='file://C:/Documents and Settings/JBharani/Local Settings/Application Data/LiftBridge/NewTest/Sena 3.lift.lift-ranges' />
							</ranges>
							<fields>
								<field
									tag='comment'>
									<form
										lang='en'>
										<text>This records a comment (note) in a LexEtymology in FieldWorks.</text>
									</form>
								</field>
								<field
									tag='cv-pattern'>
									<form
										lang='en'>
										<text>This records the syllable pattern for a LexPronunciation in FieldWorks.</text>
									</form>
								</field>
								<field
									tag='tone'>
									<form
										lang='en'>
										<text>This records the tone information for a LexPronunciation in FieldWorks.</text>
									</form>
								</field>
								<field tag='ournew'><text>Other new field</text></field>
							</fields>
						</header>
						<entry id='usOnly' guid='c1ecf892-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByOther' guid='c1ecf893-e382-11de-8a39-0800200c9a66' />
						<entry id='brewingConflict' guid='c1ecf894-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>us</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			_theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift producer='SIL.FLEx 7.2.2.40940' version='0.13'>
						<header>
							<ranges>
								<range
									id='dialect'
									href='file://C:/Documents and Settings/User/Local Settings/Application Data/LiftBridge/Test/Sena 3.lift.lift-ranges' />
								<range
									id='etymology'
									href='file://C:/Documents and Settings/User/Local Settings/Application Data/LiftBridge/Test/Sena 3.lift.lift-ranges' />
								<range
									id='newrange2'
									href='file://C:/Documents and Settings/User/Local Settings/Application Data/LiftBridge/Test/Sena 3.lift.lift-ranges' />
							</ranges>
							<fields>
								<field
									tag='tone'>
									<form
										lang='en'>
										<text>This records the tone information for a LexPronunciation in FieldWorks.</text>
									</form>
								</field>
								<field
									tag='cv-pattern'>
									<form
										lang='en'>
										<text>This records the syllable pattern for a LexPronunciation in FieldWorks.</text>
									</form>
								</field>
								<field
									tag='comment'>
									<form
										lang='en'>
										<text>This records a comment (note) in a LexEtymology in FieldWorks.</text>
									</form>
								</field>
								<field tag='theirnewfield'>
									<form lang='en'><text>This records a potential problem</text></form>
								</field>
							</fields>
						</header>
						<entry id='themOnly' guid='c1ecf895-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUs' guid='c1ecf896-e382-11de-8a39-0800200c9a66' />

						<entry id='brewingConflict' guid='c1ecf894-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>them</text>
								 </gloss>
							 </sense>
						</entry>

					</lift>";
			_ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='SIL.FLEx 7.2.2.40940'>
						<header>
							<ranges>
								<range
									id='dialect'
									href='file://C:/Documents and Settings/JBharani/Local Settings/Application Data/LiftBridge/NewTest/Sena 3.lift.lift-ranges' />
								<range
									id='etymology'
									href='file://C:/Documents and Settings/JBharani/Local Settings/Application Data/LiftBridge/NewTest/Sena 3.lift.lift-ranges' />
							</ranges>
							<fields>
								<field
									tag='cv-pattern'>
									<form
										lang='en'>
										<text>This records the syllable pattern for a LexPronunciation in FieldWorks.</text>
									</form>
								</field>
								<field
									tag='tone'>
									<form
										lang='en'>
										<text>This records the tone information for a LexPronunciation in FieldWorks.</text>
									</form>
								</field>
								<field
									tag='comment'>
									<form
										lang='en'>
										<text>This records a comment (note) in a LexEtymology in FieldWorks.</text>
									</form>
								</field>
							</fields>
						</header>
						<entry id='doomedByOther' guid='c1ecf893-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByUs' guid='c1ecf896-e382-11de-8a39-0800200c9a66' />
						<entry id='brewingConflict' guid='c1ecf894-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
		}

		[Test]
		public void RangeSectionMergedCorrectly()
		{
			using (var oursTemp = new TempFile(_ours))
			using (var theirsTemp = new TempFile(_theirs))
			using (var ancestorTemp = new TempFile(_ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='usOnly']");
				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"lift/header/ranges/range", 4);
			}
		}

		[Test]
		public void FieldSectionMergedCorrectly()
		{
			using (var oursTemp = new TempFile(_ours))
			using (var theirsTemp = new TempFile(_theirs))
			using (var ancestorTemp = new TempFile(_ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/header/fields/field[@tag='ournew']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/header/fields/field[@tag='theirnewfield']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/header/fields/field[@tag='tone']");
				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"lift/header/fields/field", 5);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='usOnly']");
			}
		}
	}
}
