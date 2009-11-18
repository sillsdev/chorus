using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{

	[TestFixture]
	public class ConflictIOTests
	{
		[Test]
		public void BothEdittedTextConflict_RoundtripThroughXml()
		{
			MergeSituation mergesituation = new MergeSituation("path", "x", "1", "y", "2", MergeOrder.ConflictHandlingModeChoices.TheyWin);
			var c = new BothEdittedTextConflict(
				GetNodeFromString("<a>ours</a>"),
				GetNodeFromString("<a>theirs</a>"),
				GetNodeFromString("<a>ancestor</a>"),
				mergesituation);
			c.Context = new ContextDescriptor("testLabel", "testPath");
			string desc = c.GetFullHumanReadableDescription();

			var xml = WriteConflictXml(c);
			var conflictNode = GetNodeFromString(xml).SelectSingleNode("message/data");
			var regurgitated = Conflict.CreateFromXml(conflictNode);
			Assert.AreEqual("path", regurgitated.RelativeFilePath);
			Assert.AreEqual(desc, regurgitated.GetFullHumanReadableDescription());
		   Assert.AreEqual(c.Context.PathToUserUnderstandableElement, regurgitated.Context.PathToUserUnderstandableElement);
		   Assert.AreEqual(c.Context.DataLabel, regurgitated.Context.DataLabel);
		}
		[Test]
		public void RemovedVsEditedElementConflict_RoundtripThroughXml()
		{
			MergeSituation mergesituation = new MergeSituation("path", "x", "1", "y", "2", MergeOrder.ConflictHandlingModeChoices.TheyWin);
			var c = new RemovedVsEditedElementConflict("testElement",
				GetNodeFromString("<a>ours</a>"),
				GetNodeFromString("<a>theirs</a>"),
				GetNodeFromString("<a>ancestor</a>"),
				mergesituation, new ElementStrategy(false));
			c.Context = new ContextDescriptor("testLabel", "testPath");
			string desc = c.GetFullHumanReadableDescription();

			var xml = WriteConflictXml(c);
			var conflictNode = GetNodeFromString(xml).SelectSingleNode("message/data");
			var regurgitated = Conflict.CreateFromXml(conflictNode);
			Assert.AreEqual("path", regurgitated.RelativeFilePath);
			Assert.AreEqual(desc, regurgitated.GetFullHumanReadableDescription());
			Assert.AreEqual(c.Context.PathToUserUnderstandableElement, regurgitated.Context.PathToUserUnderstandableElement);
			Assert.AreEqual(c.Context.DataLabel, regurgitated.Context.DataLabel);
		}
		private string WriteConflictXml(IConflict c)
		{
			var b = new StringBuilder();
			using (StringWriter sw = new StringWriter(b))
			{
				using (var w = new XmlTextWriter(sw))
				{
					c.WriteAsXml(w);
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
	}
}