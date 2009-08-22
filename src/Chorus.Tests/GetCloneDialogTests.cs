using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Clone;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests
{
	[TestFixture]
	public class GetCLoneDialogTests
	{
		[Test, Ignore("Run by hand only")]
		public void LaunchDialog()
		{
			using (var f = new TempFolder("clonetest"))
			{
				using (var dlg = new GetCloneDialog(f.Path))
				{
					if(DialogResult.OK != dlg.ShowDialog())
						return;
					var repo = new HgRepository(dlg.PathToNewProject, new NullProgress());
					Assert.IsNotNull(repo.GetTip().Number);
				}
			}
		}



	}
}
