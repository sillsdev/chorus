using System;
using System.Drawing;
using System.Windows.Forms;

namespace Chorus.UI.Sync
{
	public partial class SyncPanel : UserControl
	{
		private SyncControl _syncControl;

		public SyncPanel(SyncControlModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_syncControl = new SyncControl(model);
			_syncControl.Location= new Point(25, 100);
			_syncControl.Anchor = AnchorStyles.Top | AnchorStyles.Left;


			this.Controls.Add(_syncControl);
			InitializeComponent();
			// UpdateDisplay();
		}


		private void timer1_Tick(object sender, EventArgs e)
		{
			// _syncControl.UpdateDisplay();
		}

		private void SyncPanel_Resize(object sender, EventArgs e)
		{
			_syncControl.Width = this.Width - _syncControl.Left;
			_syncControl.Height = this.Height - (20+_syncControl.Top);

		}
	}
}