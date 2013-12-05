using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class ConflictTests
	{
		[Test]
		public void EnsureConflictClassHasContext()
		{
			var randomConflict = new MergeWarning(string.Empty);
			Assert.IsNotNull(randomConflict.Context);
			Assert.IsInstanceOf<NullContextDescriptor>(randomConflict.Context);

			// Try to set it to null.
			randomConflict.Context = null;
			Assert.IsNotNull(randomConflict.Context);
			Assert.IsInstanceOf<NullContextDescriptor>(randomConflict.Context);
		}
	}
}