using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	public partial class GetCloneFromNetworkFolderDlg : Form
	{
		private GetCloneFromNetworkFolderModel _model;
		private BackgroundWorker _backgroundWorker;

		public GetCloneFromNetworkFolderDlg()
		{
			InitializeComponent();

			InitializeBackgroundWorker();
		}

		private void InitializeBackgroundWorker()
		{
			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.DoWork += _backgroundWorker_DoWork;
			// _backgroundWorker.RunWorkerCompleted
			_backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;

		}

		private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			// Initiate recursive searching for Hg repositories:
			FillRepositoryList(_model.FolderPath, _backgroundWorker);
		}

		public void LoadFromModel(GetCloneFromNetworkFolderModel model)
		{
			_model = model;
			folderBrowserControl.SelectedPath = _model.FolderPath;
		}

		private void folderBrowserControl_PathChanged(object sender, EventArgs e)
		{
			if (_model == null)
				throw new InvalidDataException(@"_model not initialized. Call LoadFromModel() in GetCloneFromNetworkFolderDlg object before displaying dialog.");

			_model.FolderPath = folderBrowserControl.SelectedPath;

			_backgroundWorker.RunWorkerAsync();
		}

		private void FillRepositoryList(string folderPath, BackgroundWorker worker)
		{
			statusLabel.Text = folderPath;//silly!

			if (worker.CancellationPending)
			{
				// Bail-out base case:
				return;
			}

			if (_model.IsValidRepository(folderPath))
			{
				// Add folderPath details to projectRepositoryListView:
				var folderItem = new ListViewItem();
				folderItem.SubItems.Add(Path.GetFileName(folderPath));
				folderItem.SubItems.Add(File.GetLastWriteTime(folderPath).ToString());
				projectRepositoryListView.Items.Add(folderItem);
				// Main base case:
				return;
			}

			if (!_model.IsFolderWorthSearching(folderPath))
			{
				// Bail-out base case:
				return;
			}

			var subFolders = Directory.GetDirectories(folderPath);
			foreach (var subFolder in subFolders)
				FillRepositoryList(subFolder, worker);
		}

		private void panel_Resize(object sender, EventArgs e)
		{
			const int panelMidGapHalf = 3;
			var newWidth = panel.Width;

			folderBrowserControl.Width = newWidth / 2 - panelMidGapHalf;
			lookInLabel.Width = folderBrowserControl.Width;

			projectRepositoryListView.Location = new Point(newWidth / 2 + panelMidGapHalf, projectRepositoryListView.Location.Y);
			projectRepositoryListView.Width = newWidth / 2 - panelMidGapHalf;
			chooseRepositoryLabel.Location = new Point(projectRepositoryListView.Location.X, chooseRepositoryLabel.Location.Y);
			chooseRepositoryLabel.Width = projectRepositoryListView.Width;
		}
	}
}
