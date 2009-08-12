using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Sync;
using NUnit.Framework;

namespace LibChorus.Tests
{
	[TestFixture]
	public class SyncDialogTests
	{
		[Test, Ignore("Run by hand only")]
		public void LaunchDialog()
		{
			var setup = new RepositorySetup("pedro");
			{
				Application.EnableVisualStyles();
				var dlg = new SyncDialog(setup.ProjectFolderConfig);
				dlg.ShowDialog();

			}
		}
	}
}
