using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	///<summary>
	/// Dialog to allow user to find and select an Hg repository via a folder browser.
	///</summary>
	public partial class GetCloneFromNetworkFolderDlg : Form
	{
		// Data model for this view:
		private GetCloneFromNetworkFolderModel _model;
		// List of background workers that go looking for Hg repositories in the user's folders:
		private readonly List<FolderSearchWorker> _backgroundWorkers = new List<FolderSearchWorker>();
		// Define upper range limit of progess bar:
		private const int MaxProgressValue = 1000;
		// Place to store original color of progress bar:
		private readonly Color _progressBarColor;

		// Object to handle updating the progress bar, status string and repository ListView.
		// Background threads may make changes to this object, and a Timer event will cause the
		// dialog UI thread to read the object and update its controls accordingly:
		private class ProgressData
		{
			public int Progress;
			public string Status = "";
			public readonly List<ListViewItem> CurrentRepositories = new List<ListViewItem>();
		}
		private ProgressData _currentProgress;


		///<summary>
		/// Constructor
		///</summary>
		public GetCloneFromNetworkFolderDlg()
		{
			InitializeComponent();

			progressBar.Maximum = MaxProgressValue;
			_progressBarColor = progressBar.ForeColor;
		}

		///<summary>
		/// Plugs in the model to this view.
		///</summary>
		///<param name="model"></param>
		public void LoadFromModel(GetCloneFromNetworkFolderModel model)
		{
			_model = model;
			folderBrowserControl.SelectedPath = _model.FolderPath;
		}

		#region Dialog event handlers

		/// <summary>
		/// Handles event when user clicks on Get button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnGetButtonClick(object sender, EventArgs e)
		{
			TerminateBackgroundWorkers();
			DialogResult = DialogResult.OK;
			Close();
		}

		/// <summary>
		/// Handles event when user clicks dialog Cancel button.
		/// We need to terminate all the background worker threads so they don't try
		/// to access dialog controls after the controls are destroyed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCancelButtonClick(object sender, EventArgs e)
		{
			TerminateBackgroundWorkers();
		}

		/// <summary>
		/// Allows user to double-click on a listed repository instead of selecting it
		/// and pressing the Get button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRepositoryListViewDoubleClick(object sender, EventArgs e)
		{
			if (getButton.Enabled)
				OnGetButtonClick(sender, e);
		}

		/// <summary>
		/// Handles event when user resizes dialog. We need to maintain a 50-50 split
		/// between the folder browser control and the repository list control.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PanelResize(object sender, EventArgs e)
		{
			// There is to be a 6 pixel gap between the folder browser control and the repository list control.
			// Half way across that gap is the middle of the panel containing those two controls:
			const int panelMidGapHalf = 3;
			var newWidth = panel.Width;

			// Adjust controls on the left of the panel:
			folderBrowserControl.Width = newWidth / 2 - panelMidGapHalf;
			lookInLabel.Width = folderBrowserControl.Width;

			// Adjust controls on the right of the panel
			projectRepositoryListView.Location = new Point(newWidth / 2 + panelMidGapHalf, projectRepositoryListView.Location.Y);
			projectRepositoryListView.Width = newWidth / 2 - panelMidGapHalf;
			chooseRepositoryLabel.Location = new Point(projectRepositoryListView.Location.X, chooseRepositoryLabel.Location.Y);
			chooseRepositoryLabel.Width = projectRepositoryListView.Width;
		}

		/// <summary>
		/// Handles event when user selects a valid folder path in the folder browser control.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FolderBrowserControlPathChanged(object sender, EventArgs e)
		{
			if (_model == null)
				throw new InvalidDataException(@"_model not initialized. Call LoadFromModel() in GetCloneFromNetworkFolderDlg object before displaying dialog.");

			// Abandon any search that was already going on:
			TerminateBackgroundWorkers();

			// Update model with user's new selection:
			_model.FolderPath = folderBrowserControl.SelectedPath;

			// Remove all items from the repository listview, except for repositories under the newly-selected
			// folder and its descendents (e.g. user selects subfolder of previous selection, some repositories
			// may still be relevant):
			for (int i = projectRepositoryListView.Items.Count - 1; i >= 0; i--)
			{
				var existingItem = projectRepositoryListView.Items[i];
				if (!existingItem.Tag.ToString().StartsWith(_model.FolderPath))
					projectRepositoryListView.Items.RemoveAt(i);
			}
			// If there is no longer a selected repository, grey-out the Get button:
			if (projectRepositoryListView.SelectedItems.Count == 0)
				getButton.Enabled = false;


			// Deal with possibility that the user selected an actual Hg Repository,
			// before we go to all that trouble of creating new threads, making the computer
			// do extra work, blah, blah...
			if (GetCloneFromNetworkFolderModel.IsValidRepository(_model.FolderPath))
			{
				_currentProgress = new ProgressData { Progress = MaxProgressValue, Status = "Selected folder is a repository." };
				_currentProgress.CurrentRepositories.Add(MakeRepositoryListViewItem(_model.FolderPath));
			}
			else
			{
				// Try to get subfolders of _model.FolderPath. One reason this might throw an
				// exception is if user does not have access rights to _model.FolderPath:
				string[] subFolders = null;
				try
				{
					subFolders = (Directory.GetDirectories(_model.FolderPath));
				}
				catch
				{
				}

				// Start a background search for all Hg repositories under _model.FolderPath:
				if (subFolders == null)
				{
					// We don't care why it failed, we can't dig down into the subfolders, so forget it.
					_currentProgress = new ProgressData { Progress = 0, Status = "Selected folder cannot be read." };
				}
				else
				{
					InitializeBackgroundWorkers(subFolders);
				}
			}
		}

		/// <summary>
		/// Handles event when user selects an item in the repository ListView.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRepositoryListViewSelectionChange(object sender, EventArgs e)
		{
			// Deal with case when user didn't really select anything:
			if (projectRepositoryListView.SelectedItems.Count == 0)
			{
				getButton.Enabled = false;
				return;
			}

			// Recorde selected repository in the model:
			var selectedItem = projectRepositoryListView.SelectedItems[0];
			_model.UserSelectedRepositoryPath = selectedItem.Tag.ToString();

			// Allow user to commit selection:
			getButton.Enabled = true;
		}

		#endregion

		/// <summary>
		/// Terminates the background workers (that are doing folder searches for Hg
		/// repositories). The background threads terminate asynchronously so we will
		/// simply abandon them once we tell them to quit.
		/// </summary>
		private void TerminateBackgroundWorkers()
		{
			foreach (var existingBackgroundWorker in _backgroundWorkers)
				existingBackgroundWorker.Cancel();

			_backgroundWorkers.Clear();
		}

		/// <summary>
		/// Creates a bunch of background workers to carry out a new search through folders to find Hg repositories.
		/// </summary>
		/// <param name="initialFolders">Folder paths to search under</param>
		private void InitializeBackgroundWorkers(string[] initialFolders)
		{
			if (_backgroundWorkers.Count != 0)
				throw new DataException(@"_backgroundWorkers collection not empty at start of InitializeBackgroundWorkers call.");

			var initialFoldersCount = initialFolders.Length;

			if (initialFoldersCount == 0)
			{
				// Nothing to do, so create new progress data signifying as such:
				_currentProgress = new ProgressData {Progress = MaxProgressValue, Status = "Selected folder has no subfolders."};
			}
			else
			{
				// Reset progress data:
				_currentProgress = new ProgressData { Progress = 0, Status = "Searching for Project Repositories..." };

				// Calculate regular and final (compensating for integer division truncation) progress bar portions:
				var standardPortion = MaxProgressValue / initialFoldersCount;
				var finalPortion = standardPortion + MaxProgressValue % initialFoldersCount;

				// Create a new bunch of background workers:
				for (int i = 0; i < initialFoldersCount; i++)
				{
					var folder = initialFolders[i];

					// If we're on the last iteration, use remaining unused progress portion:
					var progressBarPortion = (i < initialFoldersCount - 1) ? standardPortion : finalPortion;

					// Create a new background worker:
					var worker = new FolderSearchWorker(folder, progressBarPortion, _currentProgress);
					_backgroundWorkers.Add(worker);

					// Set the background worker going in its own thread:
					new Thread(worker.DoWork).Start();
				}
			}
		}

		#region Deal with Progress updates

		/// <summary>
		/// Handler that gets fired every 100 milliseconds or so. This is our mechanism for
		/// updating the progress bar, status text and repository ListView without causing
		/// deadlocks or race conditions that could involve disposed objects.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProgressTick(object sender, EventArgs e)
		{
			if (_currentProgress == null)
				return;

			lock (_currentProgress)
			{
				// Set progress bar:
				progressBar.Value = _currentProgress.Progress;
				progressBar.ForeColor = progressBar.Value == MaxProgressValue ? Color.Gray : _progressBarColor;

				// Set status text:
				statusLabel.Text = _currentProgress.Status;

				// Get a simple IEnumerable collection of ListViewItems currently in the repsoitory ListView:
				var existingItems = projectRepositoryListView.Items.Cast<ListViewItem>();

				// Update repository ListView, removing ListViewItems from the _currentProgress object
				// as we add them to the UI control:
				for (int i = _currentProgress.CurrentRepositories.Count - 1; i >= 0; i--)
				{
					// Get current repository ListViewItem and also its path
					var currentRepository = _currentProgress.CurrentRepositories[i];
					var currentRepositoryPath = currentRepository.Tag.ToString();

					// Don't bother adding an item if it is already shown (which can ligitimately happen):
					if (!existingItems.Any(existing => currentRepositoryPath.Equals(existing.Tag)))
						projectRepositoryListView.Items.Add(currentRepository);

					_currentProgress.CurrentRepositories.RemoveAt(i);
				}
			}
		}

		#endregion

		/// <summary>
		/// Creates a ListViewItem containing known details of an Hg repository (specified as a folder path).
		/// </summary>
		/// <param name="folderPath">Folder path known to be an Hg repository</param>
		/// <returns>a ListViewItem containing repository details</returns>
		private static ListViewItem MakeRepositoryListViewItem(string folderPath)
		{
			var folderItem = new ListViewItem(Path.GetFileName(folderPath));
			folderItem.Tag = folderPath;
			folderItem.ToolTipText = folderPath;

			var fileTime = File.GetLastWriteTime(folderPath);
			folderItem.SubItems.Add(fileTime.ToShortDateString() + " " + fileTime.ToShortTimeString());

			return folderItem;
		}

		/// <summary>
		/// A class to hunt for Hg repositories in a given folder and its descendents.
		/// </summary>
		private class FolderSearchWorker
		{
			// Flag to indicate if we've been told to quit:
			private bool _condemned;
			// Highest level folder for our search:
			private readonly string _initialFolder;
			// Range of progress bar that we are responsible for:
			private readonly int _progressBarPortion;
			// The ProgressData object that the dialog created for our search:
			private readonly ProgressData _progressData;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="folder">Folder to begin searching from</param>
			/// <param name="progressBarPortion">The portion of the progress bar we get to update (relative to MaxProgressValue)</param>
			/// <param name="progressData">Structure into which we record our progress, so dialog can update UI progress controls</param>
			public FolderSearchWorker(string folder, int progressBarPortion, ProgressData progressData)
			{
				_initialFolder = folder;
				_progressBarPortion = progressBarPortion;
				_progressData = progressData;
			}

			/// <summary>
			/// Begins the termination of this background worker's thread.
			/// </summary>
			public void Cancel()
			{
				_condemned = true;
			}

			/// <summary>
			/// Entry point for background thread.
			/// </summary>
			public void DoWork()
			{
				// Recursively search folders and add Hg repository data to the repository ListView:
				FillRepositoryList(_initialFolder, _progressBarPortion);
			}

			/// <summary>
			/// Recursive method to search folders and add details of any Hg repositories discovered
			/// to the repository ListView.
			/// </summary>
			/// <param name="folderPath">Folder to begin searching from</param>
			/// <param name="myProgressBarPortion">Portion of progress bar (relative to MaxProgressValue) that we get to update</param>
			private void FillRepositoryList(string folderPath, int myProgressBarPortion)
			{
				if (_condemned)
				{
					// Bail-out base case:
					return;
				}

				if (GetCloneFromNetworkFolderModel.IsValidRepository(folderPath))
				{
					// Add folderPath details to projectRepositoryListView:
					var folderItem = MakeRepositoryListViewItem(folderPath);
					AddListItem(folderItem);

					UpdateProgress(myProgressBarPortion);

					// Main base case:
					return;
				}

				// Check if it is worth recursing into current folder:
				if (!GetCloneFromNetworkFolderModel.IsFolderWorthSearching(folderPath))
				{
					// Bail-out base case:
					UpdateProgress(myProgressBarPortion);
					return;
				}

				// Get an array of paths of subfolders of the folderPath:
				string[] subFolders;
				try
				{
					subFolders = Directory.GetDirectories(folderPath);
				}
				catch (Exception)
				{
					// Typically because we do not have permission to read the current folderPath's contents
					UpdateProgress(myProgressBarPortion);
					// Bail-out base case:
					return;
				}

				if (subFolders.Length == 0)
				{
					// No subfolders, so no real work to do:
					UpdateProgress(myProgressBarPortion);
					// Bail-out base case:
					return;
				}

				// Divide up the portion of the progress bar that we get to update among the subfolder recursion calls:
				var standardPortion = myProgressBarPortion/subFolders.Length;

				// Recurse for each subfolder:
				foreach (var subFolder in subFolders)
					FillRepositoryList(subFolder, standardPortion);

				// Update the progress data by any remainder left after integer division truncation.
				var remainingPortion = myProgressBarPortion % subFolders.Length;
				UpdateProgress(remainingPortion);
			}

			/// <summary>
			/// Updates the ProgressData object in a mutex zone.
			/// Adds the given increment to the overall progress value.
			/// Parent dialog will read object later and make updates to progress controls.
			/// </summary>
			/// <param name="increment">Amount to update progress value by</param>
			private void UpdateProgress(int increment)
			{
				lock (_progressData)
				{
					_progressData.Progress += increment;
					if (_progressData.Progress == MaxProgressValue)
						_progressData.Status = "Search complete.";
				}
			}

			/// <summary>
			/// Updates the ProgressData object in a mutex zone.
			/// Adds the given item to the list of current repositories.
			/// Parent dialog will read object later and make updates to ListView control.
			/// </summary>
			/// <param name="item">ListViewItem to add</param>
			private void AddListItem(ListViewItem item)
			{
				lock (_progressData)
				{
					_progressData.CurrentRepositories.Add(item);
				}
			}
		}
	}
}
