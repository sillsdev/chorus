using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
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

		// Object to handle updating the progress bar and status string. Background threads may make changes to
		// this object, and a Timer event will cause the dialog UI thread to read the object and update the
		// progress bar accordingly:
		private class ProgressData
		{
			public int progress;
			public string status;
		}
		private ProgressData _currentProgress;

		///<summary>
		/// Constructor
		///</summary>
		public GetCloneFromNetworkFolderDlg()
		{
			InitializeComponent();

			progressBar.Maximum = MaxProgressValue;
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

			// Deal with possibility that the user selected an actual Hg Repository,
			// before we go to all that trouble of creating new threads, making the computer
			// do extra work, blah, blah...
			if (GetCloneFromNetworkFolderModel.IsValidRepository(_model.FolderPath))
			{
				var repoItem = MakeRepositoryListViewItem(_model.FolderPath);
				AddRepositoryListViewItem(repoItem);
				_currentProgress = new ProgressData { progress = MaxProgressValue, status = "Selected folder is a repository." };
			}
			else
			{
				// Start a background search for all Hg repositories under _model.FolderPath:
				// TODO: handle crash if we are not allowed access to _model.FolderPath
				InitializeBackgroundWorkers(Directory.GetDirectories(_model.FolderPath));
			}
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
			// TODO: make sure background workers can't call upon UI controls after we return.
			TerminateBackgroundWorkers();
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
			{
				// This is crucial for avoiding race conditions: before we tell each backgroud worker
				// to quit, we remove its ability to add repository details to the listview by
				// unplugging the delegate that it calls to do the adding:
				existingBackgroundWorker.AddListItem -= InvokeAddRepositoryListViewItem;
				// Now we can tell it to quit:
				existingBackgroundWorker.Cancel();
			}
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
				_currentProgress = new ProgressData {progress = MaxProgressValue, status = "Selected folder has no subfolders."};
			}
			else
			{
				// Reset progress data:
				_currentProgress = new ProgressData { progress = 0, status = "Searching for Project Repositories..." };

				// Calculate regular and final (compensating for integer division truncation) progress bar portions:
				var standardPortion = MaxProgressValue / initialFoldersCount;
				var finalPortion = standardPortion + MaxProgressValue % initialFoldersCount;

				// Create a new bunch of background workers:
				for (int i = 0; i < initialFoldersCount; i++)
				{
					var folder = initialFolders[i];

					// If we're on the last iteration, use remaining unused progress portion:
					var progressBarPortion = (i < initialFoldersCount - 1) ? standardPortion : finalPortion;

					var worker = new FolderSearchWorker(folder, progressBarPortion, _currentProgress);
					// Plug in the delegate to allow new background worker to add repository details
					// to the listview:
					worker.AddListItem += InvokeAddRepositoryListViewItem;
					_backgroundWorkers.Add(worker);
					// Set the background worker going:
					new Thread(worker.DoWork).Start();
				}
			}
		}

		#region Deal with Progress updates

		/// <summary>
		/// Handler that gets fired every 100 milliseconds or so. This is our mechanism for
		/// updating the progress bar (and text) without causing deadlocks or race conditions
		/// that could involve disposed objects.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProgressTick(object sender, EventArgs e)
		{
			if (_currentProgress == null)
				return;

			lock (_currentProgress)
			{
				progressBar.Value = _currentProgress.progress;
				// Todo: make progress bar go grey if at MaxProgressValue
				statusLabel.Text = _currentProgress.status;
			}
		}

		#endregion

		#region Deal with ListView updates
		// Define a delegate which will allow cross-thread additions to the repository listview:
		private delegate void AddListItemDelegate(ListViewItem item);

		/// <summary>
		/// Adds given ListViewItem to the ListView of Hg repositories, using Invoke to cross over
		/// thread boundaries.
		/// </summary>
		/// <param name="item">Hg repository item to be added</param>
		private void InvokeAddRepositoryListViewItem(ListViewItem item)
		{
			// Use Invoke to call AddRepositoryListViewItem (see below) cross-threadedly:
			Invoke(new AddListItemDelegate(AddRepositoryListViewItem), item);
		}

		/// <summary>
		/// Adds givent ListViewItem to ListView of Hg repositories.
		/// </summary>
		/// <param name="item">ListViewItem to add</param>
		private void AddRepositoryListViewItem(ListViewItem item)
		{
			// Don't bother adding an item if it is already in the list:
			foreach (ListViewItem existingItem in projectRepositoryListView.Items)
			{
				// Compare the ListViewItem Tags, which contain the full path to the repository:
				if (existingItem.Tag.ToString().Equals(item.Tag.ToString()))
					return; // We already have this ListViewItem in the list
			}
			projectRepositoryListView.Items.Add(item);
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
			private ProgressData _progressData;

			// Define a delegate for adding a ListViewItem to a ListView:
			internal delegate void AddListItemEvent(ListViewItem listItem);
			internal event AddListItemEvent AddListItem = delegate { };

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
			/// Parent dialog will read object later and make updates to progress controls.
			/// </summary>
			/// <param name="increment">Amount to update progress value by</param>
			private void UpdateProgress(int increment)
			{
				lock (_progressData)
				{
					_progressData.progress += increment;
					if (_progressData.progress == MaxProgressValue)
						_progressData.status = "Search complete.";
				}
			}
		}
	}
}
