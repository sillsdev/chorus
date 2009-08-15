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
		public SyncDialog(ProjectFolderConfiguration projectFolderConfiguration, SyncUIDialogBehaviors behavior, SyncUIFeatures uiFeatureFlags)
		{
			InitializeComponent();
			Behavior = behavior;
			_syncControl.Model=new SyncControlModel(projectFolderConfiguration, uiFeatureFlags);
			AcceptButton = _syncControl._cancelButton;
		   // CancelButton =  _syncControl._cancelOrCloseButton;

			_syncControl.Model.SynchronizeOver += new EventHandler(_syncControl_SynchronizeOver);

			//we don't want clients digging down this deeply, so we present it as one of our properties
			FinalStatus = _syncControl.Model.StatusProgress;
		}

		public SyncOptions SyncOptions
		{ get { return _syncControl.Model.SyncOptions; }
		}

		void _syncControl_SynchronizeOver(object sender, EventArgs e)
		{
			if (Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseWhenFinished)
			{
				_closeWhenDoneTimer.Enabled = true;
			}
		}

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
				_syncControl.Synchronize();
			}
		}

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