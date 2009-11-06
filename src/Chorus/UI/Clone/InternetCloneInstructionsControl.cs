using System;
using System.IO;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	public partial class InternetCloneInstructionsControl : UserControl
	{
		private readonly string _parentDirectoryToPutCloneIn;

		public InternetCloneInstructionsControl(string parentDirectoryToPutCloneIn)
		{
			_parentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
			InitializeComponent();
		}

		private void UpdateDisplay()
		{
			var haveGoodUrl = HaveGoodUrl;
			_downloadButton.Enabled = haveGoodUrl && TargetLocationIsUnused && HaveWellFormedTargetLocation;

			_targetInfoLabel.Visible = _localFolderName.Enabled = HaveGoodUrl;
			_sourcetWarningImage.Visible = !haveGoodUrl;

			if (haveGoodUrl)
			{
				_targetWarningImage.Visible = !TargetLocationIsUnused || !HaveWellFormedTargetLocation;

				if (!Directory.Exists(_parentDirectoryToPutCloneIn))
				{
					_targetInfoLabel.Text = string.Format("The directory {0} doesn't exist, but should have been created by the application.",
														  _parentDirectoryToPutCloneIn);
				}
				else if (!TargetLocationIsUnused)
				{
					_targetInfoLabel.Text = string.Format("There is a already a project with that name at {0}",
														  TargetDestination);
				}
				else if (!HaveWellFormedTargetLocation)
				{
					_targetInfoLabel.Text = string.Format("That name contains characters which are not allowed.");
				}
				else
				{
					_targetWarningImage.Visible = false;
					_targetInfoLabel.Text = string.Format("Project will be downloaded to {0}", TargetDestination);
				}
			}
		}

		private void OnLocalNameChanged(object sender, EventArgs e)
		{
		   UpdateDisplay();
		}
		public string URL
		{
			get { return _urlBox.Text; }
			set { _urlBox.Text = value; }
		}
		public string NameOfProjectOnRepository
		{
			get
			{
				if (!HaveGoodUrl)
					return string.Empty;
				var uri = new Uri(_urlBox.Text);
				return uri.PathAndQuery.Trim('/').Replace('/', '-').Replace('?', '-').Replace('*', '-').Replace('\\', '-');
			}
		}
		public bool ReadForDownload
		{
			get { return HaveGoodUrl && TargetLocationIsUnused && HaveWellFormedTargetLocation; }
		}

		protected bool TargetLocationIsUnused
		{
			get
			{
				try
				{
					// the target location is "unused" if either the Target Destination doesn't exist OR
					//  if it has nothing in it (I tried Clone once and it failed because the repo spec
					//  was wrong, but since it had created the Target Destination folder, it wouldn't
					//  try again-rde)
					return Directory.Exists(_parentDirectoryToPutCloneIn) &&
						   (!Directory.Exists(TargetDestination)
						   || (Directory.GetFiles(TargetDestination, "*.*", SearchOption.AllDirectories).Length == 0));
				}
				catch(Exception)
				{
					return false;
				}
			}
		}

		protected bool HaveWellFormedTargetLocation
		{
			get
			{
				return (_localFolderName.Text.LastIndexOfAny(Path.GetInvalidFileNameChars()) == -1);
			}
		}

		public string TargetDestination
		{
			get { return Path.Combine(_parentDirectoryToPutCloneIn, _localFolderName.Text); }
		}

		protected bool HaveGoodUrl
		{
			get
			{
				try
				{
					var uri = new Uri(_urlBox.Text);
					return uri.Scheme =="http" &&
						   Uri.IsWellFormedUriString(_urlBox.Text, UriKind.Absolute) &&
						   !string.IsNullOrEmpty(uri.PathAndQuery.Trim('/'));
				}
				catch(Exception)
				{
					return false;
				}
			}
		}

		private void AccountInfo_Load(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void _urlBox_TextChanged(object sender, EventArgs e)
		{
		   _localFolderName.Text = NameOfProjectOnRepository;
			UpdateDisplay();
		}

		private void _downloadButton_Click(object sender, EventArgs e)
		{

		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://public.languagedepot.org");
		}
	}
}
