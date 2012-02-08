using System.IO;
using System.Text;
using System.Xml;
using Chorus.VcsDrivers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using System.Linq;
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

			var annotationXml = WriteConflictAnnotation(c);
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

			var annotationXml = WriteConflictAnnotation(c);
			var regurgitated = Conflict.CreateFromChorusNotesAnnotation(annotationXml);
			Assert.AreEqual("path", regurgitated.RelativeFilePath);
			Assert.AreEqual(desc, regurgitated.GetFullHumanReadableDescription());
			Assert.AreEqual(c.Context.PathToUserUnderstandableElement, regurgitated.Context.PathToUserUnderstandableElement);
			Assert.AreEqual(c.Context.DataLabel, regurgitated.Context.DataLabel);
		}
		private string WriteConflictAnnotation(IConflict c)
		{
			var b = new StringBuilder();
			using (StringWriter sw = new StringWriter(b))
			{
				using (var w = new XmlTextWriter(sw))
				{
					c.WriteAsChorusNotesAnnotation(w);
				}
			}
			return b.ToString();
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
			var annotationXml = WriteConflictAnnotation(conflict);
			Conflict.RegisterContextClass(typeof (DemoConflict));
			var regurgitated = Conflict.CreateFromChorusNotesAnnotation(annotationXml);
			Assert.That(regurgitated, Is.InstanceOf<DemoConflict>());
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