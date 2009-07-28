using System;
using Chorus.FileTypeHanders.lift;
using NUnit.Framework;

namespace Chorus.Tests.merge.xml.lift
{
	[TestFixture]
	public class LexEntryContextGeneratorTests
	{
		[Test]
		public void GenerateContextDescriptor_EntryHasGuid_XPathUsesGuid()
		{
			var x = new LexEntryContextGenerator();
			var descriptor = x.GenerateContextDescriptor("<entry id='fooid' guid='pretendGuid'/>");
			Assert.AreEqual(@"lift/entry[@guid='pretendGuid']", descriptor.PathToUserUnderstandableElement);
			Assert.AreEqual(string.Empty, descriptor.DataLabel);
		}
		[Test]
		public void GenerateContextDescriptor_EntryHasNoGuid_XPathUsesId()
		{
			var x = new LexEntryContextGenerator();
			var descriptor = x.GenerateContextDescriptor("<entry id='fooid'/>");
			Assert.AreEqual(@"lift/entry[@id='fooid']", descriptor.PathToUserUnderstandableElement);
		}
		[Test, ExpectedException(typeof(ApplicationException))]
		public void GenerateContextDescriptor_EntryHasNoGuidOrId_Throws()
		{
			var x = new LexEntryContextGenerator();
			var descriptor = x.GenerateContextDescriptor("<entry/>");
		}
	}
}
