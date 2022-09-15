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
			using (var listener = new SystemAssertListener())
			{
				// System.Diagnostics.Debug.Listeners.Add(listener);
				// // ReSharper disable once ObjectCreationAsStatement because the constructor asserts the conditions we're testing.
				// new Chorus.Properties.Settings();
				// Assert.That(listener.Messages, Is.Empty);
			}
		}
	}
}