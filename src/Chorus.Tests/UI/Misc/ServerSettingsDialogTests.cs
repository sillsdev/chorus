using System.Threading;
using System.Windows.Forms;
using Chorus.Model;
using Chorus.UI.Misc;
using NUnit.Framework;

namespace Chorus.Tests.UI.Misc
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class ServerSettingsDialogTests
	{
		[SetUp]
		public void Setup()
		{
			Application.EnableVisualStyles();
		}

		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_FullAddress()
		{
			LaunchCustomUrl("https://joe:pass@hg-public.languageforge.org/tpi");
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