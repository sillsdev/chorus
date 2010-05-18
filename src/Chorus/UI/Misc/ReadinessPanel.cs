using System;
using System.Drawing;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Misc
{
	/// <summary>
	/// Show this control somewhere in the setup UI of your application.
	/// It will help people know if the project is ready to use LanguageDepot, and
	/// gives them a button to edit the relevant settings.
	/// </summary>
	public partial class ReadinessPanel : UserControl
	{
		public ReadinessPanel()
		{
			InitializeComponent();
			BorderStyle = System.Windows.Forms.BorderStyle.None;//having some trouble with this
		}

		/// <summary>
		/// This must be set by the client before this control is displayed
		/// </summary>
		public string ProjectFolderPath { get; set; }

//
//        private void OnGetTortoiseHgClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
//        {
//            Process.Start(@"http://sourceforge.net/project/showfiles.php?group_id=199155");
//        }
//
//        private void OnGetMercurialClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
//        {
//            Process.Start(@"http://mercurial.selenic.com/wiki/BinaryPackages");
//        }

		private void ReadinessPanel_Resize(object sender, EventArgs e)
		{
			_chorusReadinessMessage.MaximumSize = new Size(this.Width -(10+ _chorusReadinessMessage.Left), 0);
		}

		private void ReadinessPanel_Load(object sender, EventArgs e)
		{
			var repo = new HgRepository(ProjectFolderPath, new NullProgress());
			string message;
			var ready = repo.GetIsReadyForInternetSendReceive(out message);
			_warningImage.Visible = !ready;
			_chorusReadinessMessage.Text = message;
		}

		private void _editServerInfoButton_Click(object sender, EventArgs e)
		{
			var model = new ServerSettingsModel();
			using (var dlg = new Chorus.UI.Misc.ServerSettingsDialog(model))
			{
				dlg.ShowDialog();
			}
		}
	}
}
