using System;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Sync
{
	/// <summary>
	/// Control that allows the "Bridge" applications to use the
	/// SyncStartControl and the Log tab of the SyncControl.
	/// </summary>
	public partial class BridgeSyncControl : UserControl
	{
		/// <summary></summary>
		public BridgeSyncControl()
		{
			InitializeComponent();
		}

		/// <summary></summary>
		public BridgeSyncControl(HgRepository repository, SyncControlModel model)
			: this()
		{
			try
			{
				_syncControl.Model = model;
				_syncStartControl1.Visible = true;
				//_syncStartControl1.Dock = DockStyle.Fill;
				_syncControl.Visible = false;

				_syncStartControl1.Init(repository);
			}
			catch (Exception)
			{
				_syncStartControl1.Dispose(); // without this, the usbdetector just goes on and on
				throw;
			}
		}

		private void SelectedRepository(object sender, SyncStartArgs e)
		{
			_syncStartControl1.Visible = false;
			_syncControl.Visible = true;
#if MONO
			_syncControl.Refresh();
#endif
			_syncControl.Model.SyncOptions.RepositorySourcesToTry.Clear();
			_syncControl.Model.SyncOptions.RepositorySourcesToTry.Add(e.Address);
			if (!string.IsNullOrEmpty(e.CommitMessage))
			{
				_syncControl.Model.SyncOptions.CheckinDescription += ": " + e.CommitMessage;
			}
			_syncControl.Synchronize(true);
		}
	}
}
