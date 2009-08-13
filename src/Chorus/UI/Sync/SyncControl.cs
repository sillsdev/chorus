﻿using System;
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
		private int _desiredHeight;
		private TextBoxProgress _textBoxProgress;
		private bool _didSync=false;
		public event EventHandler CloseButtonClicked;


		public SyncControl()
		{
			this.Font = SystemFonts.MessageBoxFont;
			InitializeComponent();
			_tabControl.TabPages.Remove(_tasksTab);
			DesiredHeight = 320;
			_successIcon.Left = _warningIcon.Left;
			 _cancelButton.Top = _sendReceiveButton.Top;
		   _closeButton.Bounds = _cancelButton.Bounds;
			progressBar1.Visible = false;
			_statusText.Visible = false;

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
			set
			{
				_model = value;
				if(_model ==null)
					return;
				_model.SynchronizeOver += new EventHandler(_model_SynchronizeOver);
				UpdateDisplay();
			}
		}

		void _model_SynchronizeOver(object sender, EventArgs e)
		{
				Cursor.Current = Cursors.Default;
				progressBar1.MarqueeAnimationSpeed = 0;
				progressBar1.Style = ProgressBarStyle.Continuous;
				progressBar1.Maximum = 100;
				progressBar1.Value = progressBar1.Maximum;
			_didSync = true;
		}


		private void UpdateDisplay()
		{
			if (_model == null)
				return;
			_sendReceiveButton.Visible =  Model.EnableSendReceive;
			_cancelButton.Visible =  Model.EnableCancel;
			_successIcon.Visible = _didSync  && !(Model.StatusProgress.WarningEncountered || Model.StatusProgress.ErrorEncountered);
			_warningIcon.Visible = (Model.StatusProgress.WarningEncountered || Model.StatusProgress.ErrorEncountered);
			_closeButton.Visible = Model.EnableClose;
			progressBar1.Visible = Model.SynchronizingNow;// || _didSync;
			_statusText.Visible = progressBar1.Visible || _didSync;
			_statusText.Text = Model.StatusProgress.LastStatus;

			_syncTargets.Enabled = Model != null;
//
//            if (_sendReceiveButton.Enabled)
//            {
//                timer1.Enabled = false;
//            }
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

			if (!_model.ShowTabs)
			{
				_tabControl.Visible = false;
				DesiredHeight = _tabControl.Top;
			}
			else
			{
				if (!_model.HasFeature(SyncUIFeatures.RepositoryChooser))
				{
					_tabControl.TabPages.Remove(_chooseTargetsTab);
				}
				if (!_model.HasFeature(SyncUIFeatures.TaskList))
				{
					_tabControl.TabPages.Remove(_tasksTab);
				}
				if (!_model.HasFeature(SyncUIFeatures.Log))
				{
					_tabControl.TabPages.Remove(_logTab);
				}
			}

			if (!_model.ShowSyncButton)
			{
				_sendReceiveButton.Visible = false;
			}

			string message = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(message))
			{
				_logBox.ForeColor = System.Drawing.Color.Red;
				_logBox.Text = message;
				return;
			}

			_textBoxProgress = new TextBoxProgress(_logBox);
			Model.AddProgressDisplay(_textBoxProgress);

			LoadChoices();
		}

		/// <summary>
		/// this is a hack because of my problems with doing anything fancy with winforms sizing
		/// </summary>
		public int DesiredHeight
		{
			get { return _desiredHeight; }
			set { _desiredHeight = value;}
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
		   // _logBox.Height = 30;// _logTab.Height - 60;
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
			_didSync = false;

			progressBar1.Style = ProgressBarStyle.Marquee;
			progressBar1.MarqueeAnimationSpeed = 50;
			_logBox.Text = "";
			_logBox.Text = "Syncing..." + Environment.NewLine;
			Cursor.Current = Cursors.WaitCursor;
			timer1.Enabled = true;
			_textBoxProgress.ShowVerbose = _showVerboseLog.Checked;
			Model.Sync();
		}

		private void OnCancelButton_Click(object sender, EventArgs e)
		{
			if (_model.EnableCancel)
			{
				_model.Cancel();
			}
		 }

		private void OnCloseButton_Click(object sender, EventArgs e)
		{
		   if (CloseButtonClicked != null)
			{
				CloseButtonClicked.Invoke(this, null);
			}

		}

		private void SyncControl_Resize(object sender, EventArgs e)
		{
			_statusText.MaximumSize = new Size(_sendReceiveButton.Left - 20, 0);
		}
	}
}