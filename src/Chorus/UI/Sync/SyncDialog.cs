using System;
using System.Drawing;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using SIL.PlatformUtilities;
using SIL.Progress;
using SIL.Windows.Forms.Progress;

namespace Chorus.UI.Sync
{
	public partial class SyncDialog : Form
	{
		public delegate SyncDialog Factory(SyncUIDialogBehaviors behavior, SyncUIFeatures uiFeatureFlags);//autofac uses this

		public SyncDialog(ProjectFolderConfiguration projectFolderConfiguration,
			SyncUIDialogBehaviors behavior, SyncUIFeatures uiFeatureFlags)
		{
			InitializeComponent();
			try
			{
				Behavior = behavior;
				_syncControl.Model = new SyncControlModel(projectFolderConfiguration, uiFeatureFlags, null/*to do*/);
				AcceptButton = _syncControl._cancelButton;
				// CancelButton =  _syncControl._cancelOrCloseButton;

				_syncControl.Model.SynchronizeOver += new EventHandler(_syncControl_SynchronizeOver);

				//we don't want clients digging down this deeply, so we present it as one of our properties
				FinalStatus = _syncControl.Model.StatusProgress;

				//set the default based on whether this looks like a backup or local commit operation
				UseTargetsAsSpecifiedInSyncOptions = (Behavior == SyncUIDialogBehaviors.StartImmediately ||
													  Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished);

				//in case the user cancels before the sync and the client doesn't check to see if the result is null
				if ((uiFeatureFlags & SyncUIFeatures.SimpleRepositoryChooserInsteadOfAdvanced) == SyncUIFeatures.SimpleRepositoryChooserInsteadOfAdvanced)
				{
					SyncResult = new SyncResults();
					SyncResult.Succeeded = false;

					_syncStartControl.Init(HgRepository.CreateOrUseExisting(projectFolderConfiguration.FolderPath, new NullProgress()));

					_syncControl.Dock = DockStyle.Fill;//in designer, we don't want it to cover up everything, but we do at runtime
					_syncStartControl.Visible = true;
					_syncControl.Visible = false;
					Height = _syncStartControl.DesiredHeight;
				}
				else
				{
					_syncStartControl.Visible = false;
					_syncControl.Visible = true;
					Height = _syncControl.DesiredHeight;
				}
				ResumeLayout(true);
				this.Text = string.Format("Send/Receive ({0})", _syncControl.Model.UserName);
			}
			catch (Exception)
			{
				_syncStartControl.Dispose();//without this, the usbdetector just goes on and on
				throw;
			}
		}

		public void SetSynchronizerAdjunct(ISychronizerAdjunct adjunct)
		{
			_syncControl.Model.SetSynchronizerAdjunct(adjunct);
		}

		public SyncOptions SyncOptions
		{
			get { return _syncControl.Model.SyncOptions; }
		}


		void _syncControl_SynchronizeOver(object syncResults, EventArgs e)
		{
			if (Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished)
			{
				_closeWhenDoneTimer.Enabled = true;
			}
	   //this makes it close right away!    this.DialogResult = System.Windows.Forms.DialogResult.OK;

			if ((Behavior & SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished) != SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished)
			{//don't show close if we're going to auto-close
				_syncControl.Model.EnableClose = true;
			}
			this.SyncResult = syncResults as SyncResults;
		}

		public SyncResults SyncResult{get;private set;}

		public SimpleStatusProgress FinalStatus
		{
			get;
			private set;
		}
		public SyncUIDialogBehaviors Behavior{ get;private set;}

		private void SyncDialog_Shown(object sender, System.EventArgs e)
		{
			if (Behavior == SyncUIDialogBehaviors.StartImmediately || Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished)
			{
				_syncControl.Synchronize(UseTargetsAsSpecifiedInSyncOptions);
			}
		}


		/// <summary>
		/// Set this to true when simpling doing a backup...,
		/// false were we want to sync to whatever sites the user has indicated
		/// </summary>
	   public bool UseTargetsAsSpecifiedInSyncOptions { get; set; }

		private void _syncControl_CloseButtonClicked(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void _closeWhenDoneTimer_Tick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void SyncDialog_Load(object sender, EventArgs e)
		{
			var height = _syncControl.Visible ? _syncControl.DesiredHeight + 10 : _syncStartControl.DesiredHeight + 10;
			ClientSize = new Size( 490, height);
		}

		private void _syncStartControl1_RepositoryChosen(object sender, SyncStartArgs args)
		{
			_syncStartControl.Visible = false;
			_syncControl.Visible = true;
			Height = _syncControl.DesiredHeight;
			ResumeLayout(true);
			if (Platform.IsMono)
				_syncControl.Refresh();

			_syncControl.Model.SyncOptions.RepositorySourcesToTry.Clear();
			_syncControl.Model.SyncOptions.RepositorySourcesToTry.Add(args.Address);
			if(!string.IsNullOrEmpty(args.CommitMessage))
			{
				_syncControl.Model.SyncOptions.CheckinDescription += " "+ args.CommitMessage;
			}
			_syncControl.Synchronize(true);
		}
	}

	public enum SyncUIDialogBehaviors
	{
		Lazy=0,
		StartImmediately,
		StartImmediatelyAndCloseWhenFinished
	} ;
}