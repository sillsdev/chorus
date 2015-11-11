using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using SIL.Code;
using SIL.Progress;
using Chorus.Model;

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
			_showSettingsLink.Font = SystemFonts.DialogFont;
		}

		/// <summary>
		/// This must be set by the client before this control is displayed
		/// </summary>
		public string ProjectFolderPath { get; set; }

		private void ReadinessPanel_Resize(object sender, EventArgs e)
		{
			_chorusReadinessMessage.MaximumSize = new Size(this.Width -(10+ _chorusReadinessMessage.Left), _chorusReadinessMessage.Height);
		}

		private void ReadinessPanel_Load(object sender, EventArgs e)
		{
			BackColor = Parent.BackColor;
//kill designer in clients
			//RequireThat.Directory(ProjectFolderPath).Exists();
//instead
			if (!Directory.Exists(ProjectFolderPath))
			{
				_chorusReadinessMessage.Text = "ProjectFolderPath will need a valid path, at runtime.";
				return;
			}

			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			var repo = new HgRepository(ProjectFolderPath, new NullProgress());
			string message;
			var ready = repo.GetIsReadyForInternetSendReceive(out message);
			_warningImage.Visible = !ready;
			_chorusReadinessMessage.Text = message;
		}

		private void ReadinessPanel_FontChanged(object sender, EventArgs e)
		{
			_chorusReadinessMessage.Font = this.Font;
			_showSettingsLink.Font = this.Font;
			betterLabel1.Font = new Font(this.Font,FontStyle.Bold);
		}

		private void _showSettingsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var model = new ServerSettingsModel();
			model.InitFromProjectPath(ProjectFolderPath);
			using (var dlg = new ServerSettingsDialog(model))
			{
				dlg.ShowDialog();
				UpdateDisplay();
			}
		}
	}
}
