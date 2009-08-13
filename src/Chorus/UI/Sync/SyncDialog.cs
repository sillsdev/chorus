using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.sync;

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
		}

		void _syncControl_SynchronizeOver(object sender, EventArgs e)
		{
			if (Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseIfSuccessful)
			{
				_closeWhenDoneTimer.Enabled = true;
			}
		}

		public SyncUIDialogBehaviors Behavior{ get;private set;}

		public SyncControlModel Model
		{
			get { return _syncControl.Model; }
		}


		private void SyncDialog_Shown(object sender, System.EventArgs e)
		{
			if (Behavior == SyncUIDialogBehaviors.StartImmediately || Behavior == SyncUIDialogBehaviors.StartImmediatelyAndCloseIfSuccessful)
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
			this.ClientSize = new Size( 490, _syncControl.DesiredHeight +40);

		}


	}

	public enum SyncUIDialogBehaviors
	{
		Lazy=0,
		StartImmediately,
		StartImmediatelyAndCloseIfSuccessful
	} ;
}