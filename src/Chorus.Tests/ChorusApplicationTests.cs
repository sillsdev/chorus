using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests
{
	[TestFixture]
	public class ChorusApplicationTests
	{
		[Test]
		public void Launch_CloseAfterAFewSeconds_DoesntCrash()
		{
			using (var folder = new TempFolder("ChorusApplicationTests"))
			{
				Application.Idle += new EventHandler(Application_Idle);
				new Program.Runner().Run(folder.Path);
			}
		}

		void Application_Idle(object sender, EventArgs e)
		{
			Thread.Sleep(100);
			Application.Exit();
		}
	}

}
