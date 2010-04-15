using Chorus.Utilities;
using NUnit.Framework;

namespace LibChorus.Tests.utilities
{
	[TestFixture]
	public class UrlHelperTests
	{
		[Test]
		public void EscapeUrlForUseInXmlAttribute_Empty_ReturnEmpty()
		{
			Assert.AreEqual(string.Empty, UrlHelper.GetEscapedUrl(string.Empty));
		}

		[Test]
		public void EscapeUrlForUseInXmlAttribute_HasQueryPortionWithAmpersand_ProperlyEscaped()
		{
			var x = UrlHelper.GetEscapedUrl("lift://somefile.lift?label=blah&somethingelse=3");
			Assert.AreEqual("lift://somefile.lift?label=blah&amp;somethingelse=3", x);
		}

		[Test]
		public void EscapeUrlForUseInXmlAttribute_HasQueryPortionWithSingleQuote_ProperlyEscaped()
		{
			var x = UrlHelper.GetEscapedUrl("lift://somefile.lift?label=it's");
			Assert.AreEqual("lift://somefile.lift?label=it&apos;s", x);
		}

		[Test]
		public void GetPathOnly_HasPathAndQuery_ReturnsPathOnly()
		{
			var x = UrlHelper.GetPathOnly("lift://somefile.lift?label=it's");
			Assert.AreEqual("lift://somefile.lift", x);
		}

	}
}