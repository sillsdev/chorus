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

			_syncTargets.Items.Clear();
			foreach (RepositorySource descriptor in _model.RepositoriesToList)
			{
				_syncTargets.Items.Add(descriptor, _model.RepositoriesToTry.Contains(descriptor) );
			}
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			_syncButton.Enabled = _model != null && _model.EnableSync;
			_syncTargets.Enabled = _model != null;
		}

		private void syncButton_Click(object sender, EventArgs e)
		{
			_progress.Clear();
			_logBox.Text = "Syncing...";
			_model.Sync();
			_logBox.Text = _progress.Text;
		}

		private void _syncTargets_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			 _model.RepositoriesToTry.Clear();
			foreach (RepositorySource descriptor in _syncTargets.CheckedItems)
			{
				_model.RepositoriesToTry.Add(descriptor);
			}
			UpdateDisplay();
		}


	}
}
