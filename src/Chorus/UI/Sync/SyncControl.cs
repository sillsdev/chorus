using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Sync
{
	public partial class SyncControl : UserControl
	{
		private SyncControlModel _model;
		private String _userName="anonymous";
		public event EventHandler CloseButtonClicked;

		public SyncControl()
		{
			this.Font = SystemFonts.MessageBoxFont;
			InitializeComponent();
			_tabControl.TabPages.Remove(_tasksTab);

		}
		public SyncControl(SyncControlModel model)
			:this()
		{
			Model = model;
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

		public SyncControlModel Model
		{
			get { return _model; }
			set { _model = value;
				UpdateDisplay();}
		}


		private void UpdateDisplay()
		{
			if (_model == null)
				return;
			_syncButton.Enabled = Model != null && Model.EnableSync;
			if (_model.EnableCancel)
			{
				_cancelOrCloseButton.Text = "&Cancel";
			}
			else if (CloseButtonClicked!=null)
			{
				_cancelOrCloseButton.Text = "&Close";
			}
			if (_model.EnableSync)
			{
				Cursor.Current = Cursors.Default;
				progressBar1.MarqueeAnimationSpeed = 0;
			}

			_syncTargets.Enabled = Model != null;

			if (_syncButton.Enabled)
			{
				timer1.Enabled = false;
			}
		}



		private void _syncTargets_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			//this is awkward because at the time of this event, the change hasn't yet been reflected in the CheckItems
//             _model.RepositorySourcesToTry.Clear();
//            foreach (RepositoryAddress descriptor in _syncTargets.CheckedItems)
//            {
//                _model.RepositorySourcesToTry.Add(descriptor);
//            }

			Model.PathEnabledChanged(_syncTargets.Items[e.Index] as RepositoryAddress, e.NewValue);
			UpdateDisplay();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			if (DesignMode)
				return;

			string message = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(message))
			{
				_logBox.ForeColor = System.Drawing.Color.Red;
				_logBox.Text = message;
				return;
			}

			Model.ProgressDisplay = new TextBoxProgress(_logBox);

			LoadChoices();
		}

		private void OnRepositoryChoicesVisibleChanged(object sender, EventArgs e)
		{
			if(!_syncTargets.Visible )
				return;

			LoadChoices();
		}

		private void LoadChoices()
		{
			_syncTargets.Items.Clear();
			foreach (var descriptor in Model.GetRepositoriesToList())
			{
				_syncTargets.Items.Add(descriptor, descriptor.Enabled);
			}
			UpdateDisplay();
		}

		private void _logTab_Resize(object sender, EventArgs e)
		{
			_logBox.Height = _logTab.Height - 30;
		}

		private void _syncButton_Click(object sender, EventArgs e)
		{
			Synchronize();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}


		public void Synchronize()
		{
			progressBar1.Style = ProgressBarStyle.Marquee;
			progressBar1.MarqueeAnimationSpeed = 50;
			_logBox.Text = "";
			_logBox.Text = "Syncing..." + Environment.NewLine;
			Cursor.Current = Cursors.WaitCursor;
			timer1.Enabled = true;
			Model.ProgressDisplay.ShowVerbose = _showVerboseLog.Checked;
			Model.Sync();
		}

		private void _cancelOrCloseButton_Click(object sender, EventArgs e)
		{
			if (_model.EnableCancel)
			{
				_model.Cancel();
				return;
			}
			if (CloseButtonClicked != null)
			{
				CloseButtonClicked.Invoke(this, null);
			}
		}
	}
}