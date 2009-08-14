using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Misc
{
	public partial class ReadinessPanel : UserControl
	{
		public ReadinessPanel()
		{
			InitializeComponent();
			BorderStyle = System.Windows.Forms.BorderStyle.None;//having some trouble with this

			var msg = HgRepository.GetEnvironmentReadinessMessage("en");
			if (string.IsNullOrEmpty(msg))
			{
				_warningImage.Visible = false;
				_chorusGetTortoiseLink.Visible = false;
				_chorusGetHgLink.Visible = false;
				_chorusReadinessMessage.Visible = false;
				_chorusReadinessMessage.Text = "This computer is ready for Chorus.";
			}
			else
			{
				_chorusReadinessMessage.Text = msg;
			}
		}



		private void OnGetTortoiseHgClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(@"http://sourceforge.net/project/showfiles.php?group_id=199155");
		}

		private void OnGetMercurialClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(@"http://mercurial.selenic.com/wiki/BinaryPackages");
		}

		private void ReadinessPanel_Resize(object sender, EventArgs e)
		{
			_chorusReadinessMessage.MaximumSize = new Size(this.Width -(10+ _chorusReadinessMessage.Left), 0);
		}
	}
}
