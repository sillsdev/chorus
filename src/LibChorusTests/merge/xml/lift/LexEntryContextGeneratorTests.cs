using System;
using Chorus.FileTypeHandlers.lift;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.lift
{
	[TestFixture]
	public class LexEntryContextGeneratorTests
	{
		[Test]
		public void GenerateContextDescriptor_EntryHasGuid_XPathUsesGuid()
		{
			var x = new LexEntryContextGenerator();
			var descriptor = x.GenerateContextDescriptor("<entry id='fooid' guid='pretendGuid'/>", "blah.lift");
			Assert.AreEqual(@"lift://blah.lift?type=entry&id=pretendGuid", descriptor.PathToUserUnderstandableElement);
			Assert.AreEqual(string.Empty, descriptor.DataLabel);
		}
		[Test]
		public void GenerateContextDescriptor_EntryHasNoGuid_XPathUsesId()
		{
			var x = new LexEntryContextGenerator();
			var descriptor = x.GenerateContextDescriptor("<entry id='fooid'/>", "blah.lift");
			Assert.AreEqual(@"lift://blah.lift?type=entry&id=fooid", descriptor.PathToUserUnderstandableElement);
		}
	}
}
