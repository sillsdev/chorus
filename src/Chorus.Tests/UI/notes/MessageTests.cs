// Copyright (c) 2015-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
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
			string text = m.GetHtmlText(new EmbeddedMessageContentHandlerRepository());
			Assert.That(text, Does.Contain("<a"));
		}

		[Test]
		public void HtmlText_HasNoCData_ResultContainsNoHyperlink()
		{
			var element = XElement.Parse(
@"<message guid='abc' author='merger' status='open' date='2009-07-18T23:53:04Z'>
	 hello
</message>");

			var m = new Message(element);
			Assert.That(m.GetHtmlText(new EmbeddedMessageContentHandlerRepository()), Does.Not.Contain("<a"));
		}
	}
}
