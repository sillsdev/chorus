using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Palaso.Progress.LogBox;

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
		private const int MaxProgressValue = 10000;
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

			var langProjName = Path.GetFileNameWithoutExtension(_model.UserSelectedRepositoryPath);
			var target = Path.Combine(_model._baseFolder, langProjName);
			if (Directory.Exists(target))
			{
				MessageBox.Show(this, "You can not obtain a project that you already have.", "Project folder already exists");
				return;
			}

			_model.MakeClone(_model.UserSelectedRepositoryPath, target, new LogBox());
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

			// Don't initiate a search if the selected path is empty (for example at model initiation):
			if (folderBrowserControl.SelectedPath == "")
				return;

			// Get list of subfolders to search (which we will do one thread per folder)
			// or information that the selected folder is an actual repository:
			List<string> subFolders;
			var repositoryParentPaths = _model.GetRepositoriesAndNextLevelSearchFolders(new List<string> { _model.FolderPath }, out subFolders, 0);

			// Sanity check: should never get into this state:
			if (repositoryParentPaths.Count > 1)
				throw new DataException("Single folder '" + _model.FolderPath + "' represents " + repositoryParentPaths.Count + " repositories: " + repositoryParentPaths.Aggregate("", (list, repo) => list + (", " + repo)));

			// Deal with possibility that the user selected an actual Hg Repository,
			// before we go to all that trouble of creating new threads, making the computer
			// do extra work, blah, blah...))
			if (repositoryParentPaths.Count == 1)
			{
				_currentProgress = new ProgressData
									{
										Progress = MaxProgressValue,
										Status = "Selected folder is a repository."
									};
				_currentProgress.CurrentRepositories.Add(MakeRepositoryListViewItem(repositoryParentPaths.First()));
			}
			else if (subFolders.Count == 0)
			{
				_currentProgress = new ProgressData
									{
										Progress = 0,
										Status = "Selected folder could not be read or contains no repositories."
									};
			}
			else
			{
				// Start a background search for all Hg repositories from selected folder:
				InitializeBackgroundWorkers(subFolders);
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

			if (_model == null)
				throw new InvalidDataException(@"_model not initialized. Call LoadFromModel() in GetCloneFromNetworkFolderDlg object before displaying dialog.");

			// Record selected repository in the model:
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
		private void InitializeBackgroundWorkers(List<string> initialFolders)
		{
			if (_backgroundWorkers.Count != 0)
				throw new DataException(@"_backgroundWorkers collection not empty at start of InitializeBackgroundWorkers call.");

			var initialFoldersCount = initialFolders.Count;

			if (initialFoldersCount == 0)
				throw new DataException(@"InitializeBackgroundWorkers called with empty initialFolders List.");

			// Reset progress data:
			_currentProgress = new ProgressData { Progress = 0, Status = "Searching for Project Repositories..." };

			// Calculate how much of the progress bar each background worker may influence:
			var progressBarPortions = GetPortionSizes(MaxProgressValue, initialFoldersCount);

			// Create a bunch of background workers:
			for (int i = 0; i < initialFoldersCount; i++)
			{
				var folder = initialFolders[i];

				// Create a new background worker:
				var worker = new FolderSearchWorker(folder, progressBarPortions[i], _currentProgress, _model);
				_backgroundWorkers.Add(worker);

				// Set the background worker going in its own thread:
				new Thread(worker.DoWork).Start();
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
				progressBar.Value = _currentProgress.Progress;
				// This next line has no effect if Application.EnableVisualStyles() has been called:
				progressBar.ForeColor = progressBar.Value == MaxProgressValue ? Color.Gray : _progressBarColor;

				// Set status text:
				statusLabel.Text = _currentProgress.Status;

				// Get a simple IEnumerable collection of ListViewItems currently in the repsoitory ListView:
				var existingItems = projectRepositoryListView.Items.Cast<ListViewItem>().ToList();

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
			var folderItem = new ListViewItem(Path.GetFileName(folderPath))
								{
									Tag = folderPath,
									ToolTipText = folderPath
								};

			var fileTime = File.GetLastWriteTime(folderPath);
			folderItem.SubItems.Add(fileTime.ToShortDateString() + " " + fileTime.ToShortTimeString());

			return folderItem;
		}

		/// <summary>
		/// Typically used for dividing up progress bar into units of work.
		/// Given the size of the bar (or of a segment) and the number
		/// of subdivisions needed, it calculates the size of each subdivision.
		/// It takes care of rounding issues by dividing the initial remainder
		/// (after integer division) among the first few values, so the first
		/// few values will be one bigger than the last few.
		/// </summary>
		/// <param name="total">Overall size to be broken down, essentially the numerator</param>
		/// <param name="portions">Number of shares, essentially the divisor</param>
		/// <returns>An array of integers, each being either the next integer above or below the quotient</returns>
		private static int[] GetPortionSizes(int total, int portions)
		{
			var size = new int[portions];
			var intPortionSize = total / portions;
			var remainder = total % portions;

			for (int i = 0; i < portions; i++)
			{
				size[i] = intPortionSize;
				if (i % portions < remainder)
					size[i]++;
			}
			return size;
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
			// Data model passed in by parent:
			private readonly GetCloneFromNetworkFolderModel _model;


			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="folder">Folder to begin searching from</param>
			/// <param name="progressBarPortion">The portion of the progress bar we get to update (relative to MaxProgressValue)</param>
			/// <param name="progressData">Structure into which we record our progress, so dialog can update UI progress controls</param>
			/// <param name="model">From parent class. Provided here to overcome "Cannot access non-static field in static context" error</param>
			public FolderSearchWorker(string folder, int progressBarPortion, ProgressData progressData, GetCloneFromNetworkFolderModel model)
			{
				_initialFolder = folder;
				_progressBarPortion = progressBarPortion;
				_progressData = progressData;
				_model = model;
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
			/// Recursive method to search folders and add details of any suitable repositories discovered
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

				// Do a folder search with zero recursion here (i.e. child folders only) so that we
				// don't spend too long away from the ability to update the progress object:
				var searchFolders = new List<string> { folderPath };
				List<string> subFolders;
				var repositoryParentPaths = _model.GetRepositoriesAndNextLevelSearchFolders(searchFolders, out subFolders, 0);

				foreach (var repositoryParentPath in repositoryParentPaths)
				{
					// Add repository details to the ListView:
					AddListItem(MakeRepositoryListViewItem(repositoryParentPath));
				}

				// Test for recursion base case:
				if (subFolders.Count == 0)
				{
					UpdateProgress(myProgressBarPortion);
					return;
				}

				// Calculate how much of the progress bar each recursive call may influence:
				var progressBarPortions = GetPortionSizes(myProgressBarPortion, subFolders.Count);

				// Recurse for each subfolder:
				for (var i = 0; i < subFolders.Count; i++)
				{
					FillRepositoryList(subFolders[i], progressBarPortions[i]);
				}
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
