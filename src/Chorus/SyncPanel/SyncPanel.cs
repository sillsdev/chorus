using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI
{
	public partial class SyncPanel : UserControl
	{
		private SyncPanelModel _model;
		private String _userName="anonymous";

		public SyncPanel(SyncPanelModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_model = model;
			InitializeComponent();
			UpdateDisplay();
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

			if (_syncButton.Enabled)
			{
				timer1.Enabled = false;
			}
		}

		private void syncButton_Click(object sender, EventArgs e)
		{
			_logBox.Text = "";
			_logBox.Text = "Syncing..."+Environment.NewLine;
			Cursor.Current = Cursors.WaitCursor;
			timer1.Enabled = true;
			_model.Sync();
			Cursor.Current = Cursors.Default;
		}

		private void _syncTargets_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			//this is awkward because at the time of this event, the change hasn't yet been reflected in the CheckItems
//             _model.RepositorySourcesToTry.Clear();
//            foreach (RepositoryAddress descriptor in _syncTargets.CheckedItems)
//            {
//                _model.RepositorySourcesToTry.Add(descriptor);
//            }

			_model.PathEnabledChanged(_syncTargets.Items[e.Index] as RepositoryAddress, e.NewValue);
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

			_model.ProgressDisplay = new TextBoxProgress(_logBox);

			LoadChoices();
		}

		private void SyncPanel_VisibleChanged(object sender, EventArgs e)
		{
			if(!this.Visible )
				return;

			LoadChoices();
		}

		private void LoadChoices()
		{
			_syncTargets.Items.Clear();
			foreach (var descriptor in _model.GetRepositoriesToList())
			{
				_syncTargets.Items.Add(descriptor, descriptor.Enabled);
			}
			UpdateDisplay();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
