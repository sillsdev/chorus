using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests
{
	[TestFixture]
	public class MiscTests
	{
		[Test]
		public void AllSettingsUseCrossPlatformProvider()
		{
			CrossPlatformSettingsUtil.ValidateProperties(Chorus.Properties.Settings.Default.Properties);
		}
	}
}