using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace Chorus.Tests.merge.xml.generic
{

	[TestFixture]
	public class ConflictIOTests
	{
		[Test]
		public void BothEdittedTextConflict_RoundtripThroughXml()
		{
			MergeSituation mergesituation = new MergeSituation("path", "x", "1", "y", "2");
			var c = new BothEdittedTextConflict(
				GetNodeFromString("<a>ours</a>"),
				GetNodeFromString("<a>theirs</a>"),
				GetNodeFromString("<a>ancestor</a>"),
				mergesituation);
			string desc = c.GetFullHumanReadableDescription();
			string context = c.Context;

			var xml = WriteConflictXml(c);
			var regurgitated = Conflict.CreateFromXml(GetNodeFromString(xml));
			Assert.AreEqual("path", regurgitated.RelativeFilePath);
			Assert.AreEqual(desc, regurgitated.GetFullHumanReadableDescription());
			Assert.AreEqual(context, regurgitated.Context);
		}
		[Test]
		public void RemovedVsEditedElementConflict_RoundtripThroughXml()
		{
			MergeSituation mergesituation = new MergeSituation("path", "x", "1", "y", "2");
			var c = new RemovedVsEditedElementConflict("testElement",
				GetNodeFromString("<a>ours</a>"),
				GetNodeFromString("<a>theirs</a>"),
				GetNodeFromString("<a>ancestor</a>"),
				mergesituation, new ElementStrategy(false));
			string desc = c.GetFullHumanReadableDescription();
			string context = c.Context;

			var xml = WriteConflictXml(c);
			var regurgitated = Conflict.CreateFromXml(GetNodeFromString(xml));
			Assert.AreEqual("path", regurgitated.RelativeFilePath);
			Assert.AreEqual(desc, regurgitated.GetFullHumanReadableDescription());
			Assert.AreEqual(context, regurgitated.Context);
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