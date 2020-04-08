using System.IO;
using System.Text;
using System.Xml;
using Chorus.VcsDrivers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using System.Linq;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{

	[TestFixture]
	public class ConflictIOTests
	{
		[Test]
		public void BothEditedTextConflict_RoundtripThroughXml()
		{
			MergeSituation mergesituation = new MergeSituation("path", "x", "1", "y", "2", MergeOrder.ConflictHandlingModeChoices.TheyWin);
			var c = new BothEditedTextConflict(
				GetNodeFromString("<a>y</a>"),      //NB: since "y" is the "alpha-dog" under "TheyWin" policy, it is the 1st parameter
				GetNodeFromString("<a>x</a>"),
				GetNodeFromString("<a>ancestor</a>"),
				mergesituation, "theWinner");
			c.Context = new ContextDescriptor("testLabel", "testPath");
			c.HtmlDetails = "<body>this is a conflict</body>";
			string desc = c.GetFullHumanReadableDescription();

			var annotationXml = XmlTestHelper.WriteConflictAnnotation(c);
			var regurgitated = Conflict.CreateFromChorusNotesAnnotation(annotationXml);
			Assert.AreEqual("path", regurgitated.RelativeFilePath);
			Assert.AreEqual(desc, regurgitated.GetFullHumanReadableDescription());
		   Assert.AreEqual(c.Context.PathToUserUnderstandableElement, regurgitated.Context.PathToUserUnderstandableElement);
		   Assert.AreEqual(c.Context.DataLabel, regurgitated.Context.DataLabel);
			Assert.That(regurgitated.HtmlDetails, Is.EqualTo(c.HtmlDetails));
		}
		[Test]
		public void RemovedVsEditedElementConflict_RoundtripThroughXml()
		{
			MergeSituation mergesituation = new MergeSituation("path", "x", "1", "y", "2", MergeOrder.ConflictHandlingModeChoices.TheyWin);
			var c = new RemovedVsEditedElementConflict("testElement",
				GetNodeFromString("<a>ours</a>"),
				GetNodeFromString("<a>theirs</a>"),
				GetNodeFromString("<a>ancestor</a>"),
				mergesituation, new ElementStrategy(false), "theWinner");
			c.Context = new ContextDescriptor("testLabel", "testPath");
			string desc = c.GetFullHumanReadableDescription();

			var annotationXml = XmlTestHelper.WriteConflictAnnotation(c);
			var regurgitated = Conflict.CreateFromChorusNotesAnnotation(annotationXml);
			Assert.AreEqual("path", regurgitated.RelativeFilePath);
			Assert.AreEqual(desc, regurgitated.GetFullHumanReadableDescription());
			Assert.AreEqual(c.Context.PathToUserUnderstandableElement, regurgitated.Context.PathToUserUnderstandableElement);
			Assert.AreEqual(c.Context.DataLabel, regurgitated.Context.DataLabel);
		}

		private XmlNode GetNodeFromString(string xml)
		{
		   var dom = new XmlDocument();
			dom.LoadXml(xml);
			return dom.FirstChild;
		}

		[Test]
		public void CanCreateNonStandardConflictType()
		{
			var conflict = new DemoConflict(new NullMergeSituation());
			conflict.Context = new ContextDescriptor("testLabel", "testPath");
			var annotationXml = XmlTestHelper.WriteConflictAnnotation(conflict);
			Conflict.RegisterContextClass(typeof (DemoConflict));
			var regurgitated = Conflict.CreateFromChorusNotesAnnotation(annotationXml);
			Assert.That(regurgitated, Is.InstanceOf<DemoConflict>());
		}

		[Test]
		public void ConflictWithInvalidUtf8DetailsWorks()
		{
			var conflict = new DemoConflict(new NullMergeSituation());
			conflict.Context = new ContextDescriptor("testLabel", "testPath");
			conflict.HtmlDetails = "Bad\uDBFFegg"; // Unmatched low surrogate
			var annotationXml = XmlTestHelper.WriteConflictAnnotation(conflict);
			Conflict.RegisterContextClass(typeof(DemoConflict));
			var regurgitated = Conflict.CreateFromChorusNotesAnnotation(annotationXml);
			Assert.That(regurgitated.HtmlDetails, Is.StringContaining("Badegg"));// the /uDB80 should have dropped
		}

		[Test]
		public void CreateFromConflictElement_ProducesDifferentConflictReports()
		{
			//Setup
			const string conflictNode1 = @"<conflict
				typeGuid='3d9ba4ae-4a25-11df-9879-0800200c9a66'
				class='Chorus.merge.xml.generic.BothEditedTheSameAtomicElement'
				relativeFilePath='Linguistics\TextCorpus\Text_74506f5d-3c43-4a4b-92ec-385aa1b1bf36.textincorpus'
				type='Both Edited the Same Atomic Element'
				guid='f63a2116-5917-4c67-845f-7b9ea456e96b'
				date='2012-07-06T20:13:49Z'
				whoWon='Gordon'
				htmlDetails='&lt;head&gt;&lt;/head&gt;&lt;body&gt;&lt;div&gt;Text &quot;My text&quot;&lt;/div&gt;&lt;/body&gt;'
				contextPath='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=d36e3143-ec72-4187-8059-a1647d04a39c&amp;tag=&amp;label=Text &quot;My text&quot; Contents Paragraphs'
				contextDataLabel='Text &quot;My text&quot; Contents Paragraphs'>
				<MergeSituation
					alphaUserId='Gordon'
					betaUserId='Fred'
					alphaUserRevision='9fa7329596ff'
					betaUserRevision='7754ffdecf94'
					path='Linguistics\TextCorpus\Text_74506f5d-3c43-4a4b-92ec-385aa1b1bf36.textincorpus'
					conflictHandlingMode='WeWin' />
				</conflict>";

			const string conflictNode2 = @"<conflict
					typeGuid='3d9ba4ae-4a25-11df-9879-0800200c9a66'
					class='Chorus.merge.xml.generic.BothEditedTheSameAtomicElement'
					relativeFilePath='Linguistics\Lexicon\Lexicon.lexdb'
					type='Both Edited the Same Atomic Element'
					guid='5186426d-f200-4d32-af17-9df145cdd80a'
					date='2012-07-03T19:17:04Z'
					whoWon='Gordon'
					htmlDetails='&lt;head&gt;&lt;/head&gt;&lt;body&gt;&lt;div&gt;Entry &quot;text&quot; Participle:&lt;/div&gt;&lt;/body&gt;'
					contextPath='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=774efee3-0f2e-4dec-a548-c3b90fe0961a&amp;tag=&amp;label=Entry &quot;text&quot; Participle'
					contextDataLabel='Entry &quot;text&quot; Participle'>
					<MergeSituation
						alphaUserId='Gordon'
						betaUserId='Fred'
						alphaUserRevision='27de9e012ffc'
						betaUserRevision='ba8826e5fbf9'
						path='Linguistics\Lexicon\Lexicon.lexdb'
						conflictHandlingMode='WeWin' />
				</conflict>";
			var doc = new XmlDocument();
			doc.LoadXml(conflictNode1);
			var node1 = doc.DocumentElement as XmlNode;
			var doc2 = new XmlDocument();
			doc2.LoadXml(conflictNode2);
			var node2 = doc2.DocumentElement as XmlNode;

			//SUT
			var conflict1 = Conflict.CreateFromConflictElement(node1);
			var conflict2 = Conflict.CreateFromConflictElement(node2);

			//verify
			Assert.IsFalse(ReferenceEquals(conflict1, conflict2), "Two different conflicts of the same type but different istances.");
			Assert.AreNotEqual(conflict1.Guid, conflict2.Guid);
		}
	}

	[TypeGuid("F76A3182-A405-4685-8881-8C369CB8A506")]
	class DemoConflict : Conflict
	{
		public DemoConflict(XmlNode xmlRepresentation) : base(xmlRepresentation)
		{
		}

		public DemoConflict(MergeSituation situation) : base(situation)
		{
		}

		public DemoConflict(MergeSituation situation, string whoWon) : base(situation, whoWon)
		{
		}

		public override string GetFullHumanReadableDescription()
		{
			return "a human-readable description";
		}

		public override string Description
		{
			get { return "a description"; }
		}

		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			return "a record";
		}
	}
}