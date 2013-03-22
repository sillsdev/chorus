using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ChorusHub;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;


namespace Chorus.UI.Clone
{
	///<summary>
	/// Dialog to allow user to find and select an Hg repository via a ChorusHub service on the LAN.
	///</summary>
	public partial class GetCloneFromChorusHubDialog : Form, ICloneSourceDialog
	{
		private GetCloneFromChorusHubModel _model;
		private BackgroundWorker _backgroundCloner;
		private MultiProgress _clonerMultiProgess;
		private TextBox _clonerStatusLabel;

		public GetCloneFromChorusHubDialog(GetCloneFromChorusHubModel model)
		{
			RepositoryKindLabel = "Project";

			_model = model;
			InitializeComponent();
		}

		private void OnGetButtonClick(object sender, EventArgs e)
		{
			getButton.Enabled = false;
			cancelButton.Enabled = false;

			SwitchControlsForCloning();

			_backgroundCloner = new BackgroundWorker();
			_backgroundCloner.RunWorkerCompleted += OnClonerCompleted;
			_backgroundCloner.DoWork += OnClonerDoWork;

			lock (this)
			{
				if (_backgroundCloner.IsBusy)
					throw new Exception("Background repository-cloning thread already busy.");
				_backgroundCloner.RunWorkerAsync();
			}
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			// Just ignore the user's request to cancel if the MakeClone thread is still running.
			if (_backgroundCloner!=null&&_backgroundCloner.IsBusy)
				e.Cancel = true;
		}

		private void OnRepositoryListViewDoubleClick(object sender, EventArgs e)
		{
			if (getButton.Enabled)
				OnGetButtonClick(sender, e);
		}

		private void OnRepositoryListViewSelectionChange(object sender, EventArgs e)
		{
			// Deal with case when user didn't really select anything they can clone:
			if (_projectRepositoryListView.SelectedItems.Count == 0 || _projectRepositoryListView.SelectedItems[0].ForeColor == CloneFromUsb.DisabledItemForeColor)
			{
				getButton.Enabled = false;
				return;
			}

			// Record selected repository in the model:
			var selectedItem = _projectRepositoryListView.SelectedItems[0];
			_model.RepositoryName = selectedItem.Text;

			// Allow user to commit selection:
			getButton.Enabled = true;
		}


		#region Background Hg repository cloning

		/// <summary>
		/// Make folder-browsing controls invisible, and add in some progress controls for the MakeClone procedure:
		/// </summary>
		private void SwitchControlsForCloning()
		{
			panel.Visible = false;
			progressBar.Visible = false;

			var logBox = new LogBox();
			logBox.Location = new Point(panel.Location.X, panel.Location.Y + 50);
			logBox.Width = panel.Width;
			logBox.Height = panel.Height - 50;
			logBox.Anchor = panel.Anchor;
			logBox.ShowCopyToClipboardMenuItem = true;
			logBox.ShowDetailsMenuItem = true;
			logBox.ShowDiagnosticsMenuItem = true;
			logBox.ShowFontMenuItem = true;

			var progressIndicator = new SimpleProgressIndicator();
			progressIndicator.Location = new Point(panel.Location.X, panel.Location.Y + 35);
			progressIndicator.Width = panel.Width;
			progressIndicator.Height = 10;
			progressIndicator.Style = ProgressBarStyle.Marquee;
			progressIndicator.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
#if MONO
			progressIndicator.MarqueeAnimationSpeed = 3000;
#else
			progressIndicator.MarqueeAnimationSpeed = 50;
#endif
			progressIndicator.IndicateUnknownProgress();

			_clonerStatusLabel = new TextBox();
			_clonerStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_clonerStatusLabel.BackColor = SystemColors.Control;
			_clonerStatusLabel.BorderStyle = BorderStyle.None;
			_clonerStatusLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
			_clonerStatusLabel.Location = panel.Location;
			_clonerStatusLabel.Multiline = true;
			_clonerStatusLabel.Name = "_clonerStatusLabel";
			_clonerStatusLabel.ReadOnly = true;
			_clonerStatusLabel.Size = new Size(panel.Width, 25);

			Controls.Add(logBox);
			Controls.Add(progressIndicator);
			Controls.Add(_clonerStatusLabel);

			_clonerMultiProgess = new MultiProgress();
			_clonerMultiProgess.AddMessageProgress(logBox);
			logBox.ProgressIndicator = progressIndicator;
			_clonerMultiProgess.ProgressIndicator = progressIndicator;

			_clonerStatusLabel.Text = string.Format("Getting {0}...",RepositoryKindLabel);
		}

		private void OnClonerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (_model.CloneSucceeded)
			{
				_clonerStatusLabel.Text = "Success.";
				_clonerMultiProgess.ProgressIndicator.Initialize();
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				cancelButton.Enabled = true;
				_clonerStatusLabel.Text = "Failed.";
				_clonerMultiProgess.ProgressIndicator.Initialize();
				var error = e.Result as Exception;
				if(error!=null)
					_clonerMultiProgess.WriteError(error.Message);
			}
		}

		private void OnClonerDoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				_model.MakeClone(_clonerMultiProgess);
			}
			catch (Exception error)
			{
				e.Result = error;
			}
		}

		#endregion

		private void OnLoad(object sender, EventArgs e)
		{
			Text = string.Format("Looking for Chorus Hub...");

			_getChorusHubInfoBackgroundWorker.DoWork += OnChorusHubInfo_DoWork;
			_getChorusHubInfoBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnGetChorusHubInfo_Completed);
			_getChorusHubInfoBackgroundWorker.RunWorkerAsync();
		}

		/// <summary>
		/// Change this if your program doesn't refer to repositories as "Projects". E.g., Bloom calls them "Collections"
		/// </summary>
		protected string RepositoryKindLabel { get; set; }

		void OnGetChorusHubInfo_Completed(object sender, RunWorkerCompletedEventArgs e)
		{
			var client = e.Result as ChorusHubClient;
			if (client == null)
			{
				Text = "Sorry, no Chorus Hub was found.";
			}
			else if(!client.ServerIsCompatibleWithThisClient)
			{
				Text = "Found Chorus Hub but it is not compatible with this version of "+Application.ProductName;;
			}
			else
			{
				Text = string.Format("Get {0} from Chorus Hub on {1}", RepositoryKindLabel, client.HostName);
				foreach (var repoInfo in client.GetRepositoryInformation(_model.ProjectFilter))
				{
					var item = new ListViewItem(repoInfo.RepoName);
					string dummy;
					if (_model.ExistingRepositoryIdentifiers != null &&
						_model.ExistingRepositoryIdentifiers.TryGetValue(repoInfo.RepoID, out dummy))
					{
						item.ForeColor = CloneFromUsb.DisabledItemForeColor;
						item.ToolTipText = CloneFromUsb.ProjectWithSameNameExists;
					}
					_projectRepositoryListView.Items.Add(item);
				}
			}
		}

		void OnChorusHubInfo_DoWork(object sender, DoWorkEventArgs e)
		{
			Thread.CurrentThread.Name = "GetRepositoryInformation";
			var client = new ChorusHubClient();
			if(client.FindServer()!=null)
			{
				// Why do we do this? The returned information isn't used.
				client.GetRepositoryInformation(_model.ProjectFilter);
				e.Result = client;
			}
			else
			{
				e.Result = null;
			}
		}

		public string PathToNewlyClonedFolder
		{
			get { return _model.NewlyClonedFolder; }
		}

		/// <summary>
		/// Used to check if the repository is the right kind for your program, so that the only projects that can be chosen are ones
		/// your application is prepared to open.
		///
		/// Note: the comparison is based on how hg stores the file name/extenion, not the original form!
		/// </summary>
		/// <example>Bloom uses "*.bloom_collection.i" to test if there is a ".BloomCollection" file</example>
		public void SetFilePatternWhichMustBeFoundInHgDataFolder(string pattern)
		{
			if (!string.IsNullOrEmpty(pattern))
			{
				_model.ProjectFilter = pattern;
			}
		}
	}
}
