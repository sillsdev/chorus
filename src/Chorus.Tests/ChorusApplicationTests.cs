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
				var t = new Thread(() =>
									   {
										   Thread.Sleep(2000);
										   Application.Exit();
									   });
				t.Start();
				new Program.Runner().Run(folder.Path);
			}
		}
	}

}
