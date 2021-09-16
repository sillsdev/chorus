using System;
using System.Threading;
using System.Windows.Forms;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.TestUtilities;

namespace Chorus.Tests
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class ChorusApplicationTests
	{
		[Test]
		public void AllSettingsUseCrossPlatformProvider()
		{
			using (var listener = new SystemAssertListener())
			{
				System.Diagnostics.Debug.Listeners.Add(listener);
				// ReSharper disable once ObjectCreationAsStatement because the constructor asserts the conditions we're testing.
				new Properties.Settings();
				Assert.That(listener.Messages, Is.Empty);
			}
		}

		[Test]
		[Category("SkipOnBuildServer")]
		public void Launch_CloseAfterAFewSeconds_DoesntCrash()
		{
			using (var folder = new TemporaryFolder("ChorusApplicationTests"))
			{
				Application.Idle += new EventHandler(Application_Idle);
				new Program.Runner().Run(folder.Path, new Arguments(new object[]{}));
			}
		}

		void Application_Idle(object sender, EventArgs e)
		{
			Thread.Sleep(100);
			Application.Exit();
		}
	}

}
