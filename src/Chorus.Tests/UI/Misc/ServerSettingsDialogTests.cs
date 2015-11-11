using System.Windows.Forms;
using Chorus.UI.Misc;
using NUnit.Framework;
using Chorus.Model;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class ServerSettingsDialogTests
	{
		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_FullAddress()
		{
			LaunchCustomUrl("http://joe:pass@hg-public.languagedepot.org/tpi");
		}

		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_CustomAddress()
		{
			LaunchCustomUrl(@"\\myserver/tpi");
		}

		private void LaunchCustomUrl(string url)
		{
			var model = new ServerSettingsModel();
			model.InitFromUri(url);
			using (var dlg = new ServerSettingsDialog(model))
			{
				if (DialogResult.OK != dlg.ShowDialog())
					return;
			}
		}
	}
}