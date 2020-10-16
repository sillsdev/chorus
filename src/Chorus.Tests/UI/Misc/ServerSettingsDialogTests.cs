using Chorus.Model;
using Chorus.UI.Misc;
using NUnit.Framework;

namespace Chorus.Tests.UI.Misc
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

		private static void LaunchCustomUrl(string url)
		{
			var model = new ServerSettingsModel();
			model.InitFromUri(url);
			using (var dlg = new ServerSettingsDialog(model))
			{
				dlg.ShowDialog();
			}
		}
	}
}