﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using SIL.PlatformUtilities;
using SIL.Progress;

namespace Chorus.UI.Sync
{
	public partial class SyncControl : UserControl
	{
		private SyncControlModel _model;
		private int _desiredHeight;
		private bool _didAttemptSync=false;
		public event EventHandler CloseButtonClicked;


		public SyncControl()
		{
			this.Font = SystemFonts.MessageBoxFont;
			InitializeComponent();
			_cancelButton.Visible = false;
			_tabControl.TabPages.Remove(_tasksTab);
			_successIcon.Left = _warningIcon.Left;
			// _cancelButton.Top = _sendReceiveButton.Top;
		   _closeButton.Bounds = _sendReceiveButton.Bounds;
			progressBar1.Visible = false;
			_statusText.Visible = false;
			_statusText.Text = "";  // clear the label
			_updateDisplayTimer.Enabled = true;
		}
		public SyncControl(SyncControlModel model)
			:this()
		{
			Model = model;
			_logBox.GetDiagnosticsMethod = model.GetDiagnostics;
			UpdateDisplay();
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
				_model.AddMessagesDisplay(_logBox);
				_model.AddStatusDisplay(_statusText);
				_model.ProgressIndicator = new MultiPhaseProgressIndicator(progressBar1, 2);  // for now we only specify 2 phases (pull, then push).
				_model.UIContext = SynchronizationContext.Current;
				UpdateDisplay();
			}
		}

		void _model_SynchronizeOver(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.Default;
			//Model.ProgressIndicator.Finish();
			_didAttemptSync = true;
			UpdateDisplay();
		}


		private void UpdateDisplay()
		{
			if (_model == null)
				return;
			if(_model.CancellationPending)
			{
				_cancelButton.Text = "Cancelling..";
			}
			_sendReceiveButton.Visible =  Model.EnableSendReceive;
			_cancelButton.Visible =  Model.EnableCancel && !_showCancelButtonTimer.Enabled;
			_successIcon.Visible = _didAttemptSync  && !(Model.ErrorsOrWarningsEncountered);
			_warningIcon.Visible = Model.ErrorsOrWarningsEncountered;
			_closeButton.Visible = Model.EnableClose;
			if (_closeButton.Visible && Parent!=null && (Parent is Form))
			{
				((Form) Parent).AcceptButton = _closeButton;
				((Form) Parent).CancelButton = _closeButton;
			}
			progressBar1.Visible = Model.SynchronizingNow;// || _didAttemptSync;
			_statusText.Visible = progressBar1.Visible;
			_logBox.ShowDetailsMenuItem = true;
			_logBox.ShowDiagnosticsMenuItem = true;
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
				if (_model.HasFeature(SyncUIFeatures.SimpleRepositoryChooserInsteadOfAdvanced)
					|| _model.Features == SyncUIFeatures.Minimal)
				{
					if (Platform.IsMono)
					{
						// in mono 2.0, the log tab is forever empty if we remove this tab
						_chooseTargetsTab.Enabled = false;
						_chooseTargetsTab.Visible = false;
					}
					else
					{
						_tabControl.TabPages.Remove(_chooseTargetsTab);
					}
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
				this._logBox.WriteError(message);
				return;
			}

			LoadChoices();

			if (Platform.IsMono)
			{
				Invalidate(true);
				Refresh();
				_tabControl.Refresh();
				_logTab.Refresh();
				_logBox.Refresh();
				//    MessageBox.Show("inval/refreshed");
			}
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
			if(Model==null)
				return;//design-time in another control

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
			Synchronize(false);
		}

		private void OnUpdateDisplayTimerTick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}


		/// <summary>
		/// Sync
		/// </summary>
		/// <param name="useTargetsAsSpecifiedInSyncOptions">This is a bit of a hack
		/// until I figure out something better... it will be true for cases where
		/// the app is just doing a backup..., false were we want to sync to whatever
		/// sites the user has indicated</param>
		public void Synchronize(bool useTargetsAsSpecifiedInSyncOptions)
		{
			_didAttemptSync = false;

			//show something useful during the sync (ok to leave it on the log tab, but not any config ones)
			if (_tabControl.Visible && _tabControl.SelectedTab != _logTab && _tabControl.TabPages.Contains(_tasksTab))
			{
				_tabControl.SelectedTab = _tasksTab;
			}
			else if (_tabControl.Visible && _tabControl.TabPages.Contains(_logTab))
			{
				_tabControl.SelectedTab = _logTab;
			}

			_logBox.Clear();
			Cursor.Current = Cursors.WaitCursor;
			Model.Sync(useTargetsAsSpecifiedInSyncOptions);
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
			// cjh feb-2012: I'm not sure what the purpose of resizing the statustext was, but I commented it
			// out because it was making the Label invisible!
			// after commenting the line below out, I can see the status text again.

			//_statusText.MaximumSize = new Size((_sendReceiveButton.Left-_statusText.Left) - 20, _statusText.Height);
		}

		private void _showCancelButtonTimer_Tick(object sender, EventArgs e)
		{
			_showCancelButtonTimer.Enabled = false;
		}

		private void _statusText_Click(object sender, EventArgs e)
		{

		}
	}
}