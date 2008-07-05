using System;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI
{
	public partial class SyncPanel : UserControl
	{
		private SyncPanelModel _model;
		private TextBoxProgress _progress;
		private ProjectFolderConfiguration _project;
		private String _userName="anonymous";

		public SyncPanel()
		{
			InitializeComponent();
			UpdateDisplay();
		}

		public ProjectFolderConfiguration ProjectFolderConfig
		{
			get { return _project; }
			set { _project = value; }
		}

		/// <summary>
		/// most client apps won't have anything to put in here, that's ok
		/// </summary>
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}


		private void UpdateDisplay()
		{
			_syncButton.Enabled = _model != null && _model.EnableSync;
			_syncTargets.Enabled = _model != null;
		}

		private void syncButton_Click(object sender, EventArgs e)
		{
			_logBox.Text = "";
			_logBox.Text = "Syncing..."+Environment.NewLine;
			Cursor.Current = Cursors.WaitCursor;
			_model.Sync();
			Cursor.Current = Cursors.Default;
		}

		private void _syncTargets_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			//this is awkward because at the time of this event, the change hasn't yet been reflected in the CheckItems
//             _model.RepositorySourcesToTry.Clear();
//            foreach (RepositorySource descriptor in _syncTargets.CheckedItems)
//            {
//                _model.RepositorySourcesToTry.Add(descriptor);
//            }

			RepositorySource repositorySource = (RepositorySource) _syncTargets.Items[e.Index];
			if(e.NewValue == CheckState.Unchecked)
			{
				_model.RepositoriesToTry.Remove(repositorySource);
			}
			else
			{
				if (!_model.RepositoriesToTry.Contains(repositorySource))
				{
					_model.RepositoriesToTry.Add(repositorySource);
				}
			}
			UpdateDisplay();
		}

		private void SyncPanel_Load(object sender, EventArgs e)
		{
			if (DesignMode)
				return;

			string message = RepositoryManager.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(message))
			{
				_logBox.ForeColor = System.Drawing.Color.Red;
				_logBox.Text = message;
				return;
			}

			_progress = new TextBoxProgress(_logBox);

			_model = new SyncPanelModel(_project, UserName, _progress);

			_syncTargets.Items.Clear();
			foreach (RepositorySource descriptor in _model.RepositoriesToList)
			{
				_syncTargets.Items.Add(descriptor, _model.RepositoriesToTry.Contains(descriptor));
			}
			UpdateDisplay();
		}


	}
}
