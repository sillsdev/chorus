using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Chorus.UI
{
	public partial class SyncPanel : UserControl
	{
		public SyncPanel()
		{
			InitializeComponent();
			_syncTargets.SetItemChecked(0, true);
		}

		private void SyncPanel_Load(object sender, EventArgs e)
		{

		}
	}
}
