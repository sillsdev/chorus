using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.ChorusHub;
using Chorus.VcsDrivers;
using L10NSharp;
using SIL.PlatformUtilities;
using SIL.Progress;
using SIL.Windows.Forms.Progress;


namespace Chorus.UI.Clone
{
	///<summary>
	/// Dialog to allow user to find and select an Hg repository via a ChorusHub service on the LAN.
	///</summary>
	public partial class GetCloneFromChorusHubDialog : Form, ICloneSourceDialog
	{
		private readonly GetCloneFromChorusHubModel _model;
		private BackgroundWorker _backgroundCloner;
		private MultiProgress _clonerMultiProgess;
		private TextBox _clonerStatusLabel;

		public GetCloneFromChorusHubDialog(GetCloneFromChorusHubModel model)
		{
			RepositoryKindLabel = LocalizationManager.GetString("Messages.Project","Project");

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

			var logBox = new LogBox
			{
				Location = new Point(panel.Location.X, panel.Location.Y + 50),
				Width = panel.Width,
				Height = panel.Height - 50,
				Anchor = panel.Anchor,
				ShowCopyToClipboardMenuItem = true,
				ShowDetailsMenuItem = true,
				ShowDiagnosticsMenuItem = true,
				ShowFontMenuItem = true
			};

			var progressIndicator = new SimpleProgressIndicator
			{
				Location = new Point(panel.Location.X, panel.Location.Y + 35),
				Width = panel.Width,
				Height = 10,
				Style = ProgressBarStyle.Marquee,
				Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
				MarqueeAnimationSpeed = 50
			};

			if (Platform.IsMono)
				progressIndicator.MarqueeAnimationSpeed = 3000;

			progressIndicator.IndicateUnknownProgress();

			_clonerStatusLabel = new TextBox
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SystemColors.Control,
				BorderStyle = BorderStyle.None,
				Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
				Location = panel.Location,
				Multiline = true,
				Name = "_clonerStatusLabel",
				ReadOnly = true,
				Size = new Size(panel.Width, 25)
			};

			Controls.Add(logBox);
			Controls.Add(progressIndicator);
			Controls.Add(_clonerStatusLabel);

			_clonerMultiProgess = new MultiProgress();
			_clonerMultiProgess.AddMessageProgress(logBox);
			logBox.ProgressIndicator = progressIndicator;
			_clonerMultiProgess.ProgressIndicator = progressIndicator;

			_clonerStatusLabel.Text = string.Format(LocalizationManager.GetString("Messages.Getting","Getting {0}..."),RepositoryKindLabel);
		}

		private void OnClonerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (_model.CloneSucceeded)
			{
				_clonerStatusLabel.Text = LocalizationManager.GetString("Messages.Success","Success.");
				_clonerMultiProgess.ProgressIndicator.Initialize();
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				cancelButton.Enabled = true;
				_clonerStatusLabel.Text = LocalizationManager.GetString("Messages.Failed","Failed.");
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
			Text = string.Format(LocalizationManager.GetString("Messages.LookingForChorusHub","Looking for Chorus Hub..."));

			_getChorusHubInfoBackgroundWorker.DoWork += OnChorusHubInfo_DoWork;
			_getChorusHubInfoBackgroundWorker.RunWorkerCompleted += OnGetChorusHubInfo_Completed;
			_getChorusHubInfoBackgroundWorker.RunWorkerAsync();
		}

		/// <summary>
		/// Change this if your program doesn't refer to repositories as "Projects". E.g., Bloom calls them "Collections"
		/// </summary>
		protected string RepositoryKindLabel { get; set; }

		void OnGetChorusHubInfo_Completed(object sender, RunWorkerCompletedEventArgs e)
		{
			var results = e.Result as object[];
			if (results == null)
			{
				Text = LocalizationManager.GetString("Messages.NoChorusHub","Sorry, no Chorus Hub was found.");
			}
			else
			{
				var client = results[0] as ChorusHubClient;
				if (client == null)
				{
					Text = LocalizationManager.GetString("Messages.NoChorusHub", "Sorry, no Chorus Hub was found.");
				}
				else if (!client.ServerIsCompatibleWithThisClient)
				{
					Text = string.Format(LocalizationManager.GetString("Messages.ChorusHubIncompatible", "Found Chorus Hub but it is not compatible with this version of {0}"), Application.ProductName);
				}
				else
				{
					Text = string.Format(LocalizationManager.GetString("Messages.GetFromChorusHub", "Get {0} from Chorus Hub on {1}"), RepositoryKindLabel, client.HostName);
					_model.HubRepositoryInformation = (IEnumerable<RepositoryInformation>) results[1];
					foreach (var repoInfo in _model.HubRepositoryInformation)
					{
						if (repoInfo.RepoID == @"newRepo")
							continue; // Empty repo exists. It can receive any real repo, but cannot return a useful clone, however, so don't list it.
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
		}

		void OnChorusHubInfo_DoWork(object sender, DoWorkEventArgs e)
		{
			if (Platform.IsWindows)
			{
				// See https://bugzilla.xamarin.com/show_bug.cgi?id=4269. Remove if when using mono that fixes this.
				Thread.CurrentThread.Name = @"GetRepositoryInformation";
			}

			var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();

			if (chorusHubServerInfo == null || !chorusHubServerInfo.ServerIsCompatibleWithThisClient)
			{
				e.Result = null;
			}
			else
			{
				var results = new object[2];
				var client = new ChorusHubClient(chorusHubServerInfo);
				results[0] = client;
				results[1] = client.GetRepositoryInformation(_model.ProjectFilter);
				e.Result = results;
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
