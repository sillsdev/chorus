using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.TestUtilities;

namespace Chorus.Tests
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class ChorusApplicationTests
	{
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
