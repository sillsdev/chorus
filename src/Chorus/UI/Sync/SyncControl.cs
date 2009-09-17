using System;
using System.Drawing;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Sync
{
	public partial class SyncControl : UserControl
	{
		private SyncControlModel _model;
		private String _userName="anonymous";
		private int _desiredHeight;
		private bool _didSync=false;
		public event EventHandler CloseButtonClicked;


		public SyncControl()
		{
			this.Font = SystemFonts.MessageBoxFont;
			InitializeComponent();
			_cancelButton.Visible = false;
			_tabControl.TabPages.Remove(_tasksTab);
			DesiredHeight = 320;
			_successIcon.Left = _warningIcon.Left;
			// _cancelButton.Top = _sendReceiveButton.Top;
		   _closeButton.Bounds = _cancelButton.Bounds;
			progressBar1.Visible = false;
			_statusText.Visible = false;
			_updateDisplayTimer.Enabled = true;

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
			_cancelButton.Visible =  Model.EnableCancel && !_showCancelButtonTimer.Enabled;
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
				this._logBox.WriteError(message);
				return;
			}

			Model.AddProgressDisplay(_logBox);

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
			_didSync = false;

			//show something useful during the sync (ok to leave it on the log tab, but not any config ones)
			if (_tabControl.Visible && _tabControl.SelectedTab != _logTab && _tabControl.TabPages.Contains(_tasksTab))
			{
				_tabControl.SelectedTab = _tasksTab;
			}
			else if (_tabControl.Visible && _tabControl.TabPages.Contains(_logTab))
			{
				_tabControl.SelectedTab = _logTab;
			}

			progressBar1.Style = ProgressBarStyle.Marquee;
#if MONO
			progressBar1.MarqueeAnimationSpeed = 3000;
#else
			progressBar1.MarqueeAnimationSpeed = 50;
#endif
			_logBox.Clear();
			_logBox.WriteStatus("Syncing...");
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
			_statusText.MaximumSize = new Size(_sendReceiveButton.Left - 20, 0);
		}

		private void _showCancelButtonTimer_Tick(object sender, EventArgs e)
		{
			_showCancelButtonTimer.Enabled = false;
		}
	}
}