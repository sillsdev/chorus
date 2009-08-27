using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI.Sync
{
	public partial class SyncDialog : Form
	{

		public SyncDialog(ProjectFolderConfiguration projectFolderConfiguration,
			SyncUIDialogBehaviors behavior, SyncUIFeatures uiFeatureFlags)
		{
			InitializeComponent();
			Behavior = behavior;
			_syncControl.Model=new SyncControlModel(projectFolderConfiguration, uiFeatureFlags);
			AcceptButton = _syncControl._cancelButton;
		   // CancelButton =  _syncControl._cancelOrCloseButton;

			_syncControl.Model.SynchronizeOver += new EventHandler(_syncControl_SynchronizeOver);

			//we don't want clients digging down this deeply, so we present it as one of our properties
			FinalStatus = _syncControl.Model.StatusProgress;

			//set the default based on whether this looks like a backup or local commit operation
			UseTargetsAsSpecifiedInSyncOptions = (Behavior == SyncUIDialogBehaviors.StartImmediately ||
												  Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished);

			//in case the user cancels before the sync and the client doesn't check to see if the result is null
			SyncResult = new SyncResults();
			SyncResult.Succeeded = false;
		}

		public SyncOptions SyncOptions
		{ get { return _syncControl.Model.SyncOptions; }
		}


		void _syncControl_SynchronizeOver(object syncResults, EventArgs e)
		{
			if (Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished)
			{
				_closeWhenDoneTimer.Enabled = true;
			}
	   //this makes it close right away!    this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SyncResult = syncResults as SyncResults;
		}

		public SyncResults SyncResult{get;private set;}

		public StatusProgress FinalStatus
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
		/// false were we want to sync to whatever sites the user has indicated</param>
		/// </summary>
	   public bool UseTargetsAsSpecifiedInSyncOptions { get; set; }

		private void _syncControl_CloseButtonClicked(object sender, System.EventArgs e)
		{
			Close();
		}

		private void _closeWhenDoneTimer_Tick(object sender, EventArgs e)
		{
			Close();
		}

		private void SyncDialog_Load(object sender, EventArgs e)
		{
			this.ClientSize = new Size( 490, _syncControl.DesiredHeight+10);

		}


	}

	public enum SyncUIDialogBehaviors
	{
		Lazy=0,
		StartImmediately,
		StartImmediatelyAndCloseWhenFinished
	} ;
}