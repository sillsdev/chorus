using System.Windows.Forms;
using Chorus.sync;

namespace Chorus.UI.Sync
{
	public partial class SyncDialog : Form
	{
		public SyncDialog(ProjectFolderConfiguration projectFolderConfiguration)
		{
			InitializeComponent();
			_syncControl.Model=new SyncControlModel(projectFolderConfiguration);
			AcceptButton = _syncControl._cancelOrCloseButton;
		   // CancelButton =  _syncControl._cancelOrCloseButton;
		}

		public SyncControlModel Model
		{
			get { return _syncControl.Model; }
		}

		private void SyncDialog_Shown(object sender, System.EventArgs e)
		{
			_syncControl.Synchronize();
		}

		private void _syncControl_CloseButtonClicked(object sender, System.EventArgs e)
		{
			Close();
		}


	}
}