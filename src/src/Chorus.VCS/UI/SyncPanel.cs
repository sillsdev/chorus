using System;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI
{
	public partial class SyncPanel : UserControl
	{
		private SyncPanelModel _model;
		private StringBuilderProgress _progress;

		public SyncPanel()
		{
			InitializeComponent();
			UpdateDisplay();
		}


		public void Init(ApplicationSyncContext syncContext)
		{
			_progress = new StringBuilderProgress();

			_model = new SyncPanelModel(syncContext, _progress);

			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			_syncButton.Enabled = _model !=null;
			_syncTargets.Enabled = _model != null;
		}

		private void syncButton_Click(object sender, EventArgs e)
		{
			_progress.Clear();
			_logBox.Text = "Syncing...";
			_model.Sync();
			_logBox.Text = _progress.Text;
		}
	}
}
