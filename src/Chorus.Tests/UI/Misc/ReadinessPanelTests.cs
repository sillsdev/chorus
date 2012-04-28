using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Sync;
using Chorus.VcsDrivers;
using LibChorus.TestUtilities;
using LibChorus.Tests;
using NUnit.Framework;

namespace Chorus.Tests.UI.Misc
{
	[TestFixture]
	public class ReadinessPanelTests
	{
		[Test, Ignore("Run by hand only")]
		public void ShowIt()
		{
			var setup = new RepositorySetup("pedro");
			{
				var c = new Chorus.UI.Misc.ReadinessPanel();
				c.ProjectFolderPath = setup.ProjectFolder.Path;
				var f = new Form();
				f.Width = c.Width + 20;
				f.Height = c.Height + 20;
				c.Dock = DockStyle.Fill;
				f.Controls.Add(c);
				Application.Run(f);
			}
		}
	}
}