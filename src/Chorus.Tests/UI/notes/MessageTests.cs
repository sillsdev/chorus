using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Chorus.notes;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class MessageTests
	{
		[Test]
		public void HtmlText_HadCData_ResultContainsHyperlink()
		{
			var element = XElement.Parse(
@"<message guid='abc' author='merger' status='open' date='2009-07-18T23:53:04Z'>
	  <![CDATA[some something]]>
</message>");

			var m = new Message(element);
			string text = m.GetHtmlText(new EmbeddedMessageContentHandlerFactory());
			Assert.IsTrue(text.Contains("<a"));
		}

		[Test]
		public void HtmlText_HasNoCData_ResultContainsNoHyperlink()
		{
			var element = XElement.Parse(
@"<message guid='abc' author='merger' status='open' date='2009-07-18T23:53:04Z'>
	 hello
</message>");

			var m = new Message(element);
			Assert.IsFalse(m.GetHtmlText(new EmbeddedMessageContentHandlerFactory()).Contains("<a"));
		}
	}
}
