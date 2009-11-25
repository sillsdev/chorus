using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.annotations;
using NUnit.Framework;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class MessageTests
	{
		[Test]
		public void Guid_HasGuid_ReturnsGuid()
		{
			var guid = Guid.NewGuid();
			var a = new Message(XElement.Parse("<message guid='" + guid + "'/>"));
			Assert.AreEqual(guid.ToString(), a.Guid);
		}

		[Test]
		public void Author_HasAuthor_ReturnsAuthor()
		{
			var msg = new Message(XElement.Parse("<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'/>"));
			Assert.AreEqual("john", msg.Author);
		}

		[Test]
		public void Status_HasStatus_ReturnsStatus()
		{
			var msg = new Message(XElement.Parse("<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'/>"));
			Assert.AreEqual("open", msg.Status);
		}

		[Test]
		public void Date_HasDate_ReturnsDate()
		{
			var msg = new Message(XElement.Parse("<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'/>"));
			Assert.AreEqual(DateTime.Parse("2009-07-18T23:53:04Z"), msg.Date);
		}

		[Test]
		public void HtmlText_HasHtmlTags_TagsPreserved()
		{
			var content = "This has a <p> inside";
			var escapedContent = new XElement("test", content).FirstNode.ToString();
			var msg = new Message(XElement.Parse(@"<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>"
			+escapedContent+"</message>"));
			Assert.AreEqual(content, msg.HtmlText);
		}

		[Test]
		public void HtmlText_HasHtmlTagsAndData_GetJustHtml()
		{
			var content = "This has a <p> inside";
			// this would work, too: HttpUtility.HtmlEncode(content);
			var escapedContent = new XElement("test", content).FirstNode.ToString();
			var msg = new Message(XElement.Parse(@"<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>"
			+ escapedContent + "<data>blah</data></message>"));
			Assert.AreEqual(content, msg.HtmlText);
		}

		[Test]
		public void HtmlText_HasOnlyData_GetEmptyString()
		{
			var msg = new Message(XElement.Parse(@"<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>"
			+ "<data>blah</data></message>"));
			Assert.AreEqual(string.Empty, msg.HtmlText);
		}
	}


}