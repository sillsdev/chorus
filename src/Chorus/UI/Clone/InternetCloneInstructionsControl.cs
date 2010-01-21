using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Linq;

namespace Chorus.UI.Clone
{
	public partial class InternetCloneInstructionsControl : UserControl
	{
		private readonly string _parentDirectoryToPutCloneIn;
		private readonly Dictionary<string, string> _servers = new Dictionary<string, string>();

		public InternetCloneInstructionsControl(string parentDirectoryToPutCloneIn)
		{
			_parentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;

			InitializeComponent();

			_servers.Add("languageDepot.org", "hg-public.languageDepot.org");
			_servers.Add("private.languageDepot.org", "hg-private.languageDepot.org");
			foreach (KeyValuePair<string, string> pair in _servers)
			{
				_serverCombo.Items.Add(pair.Key);
			}
			_serverCombo.SelectedIndex = 0;
		}

		private void UpdateDisplay()
		{
			var haveGoodUrl = HaveNeededAccountInfo;
			_downloadButton.Enabled = haveGoodUrl && TargetLocationIsUnused && HaveWellFormedTargetLocation;

			_targetInfoLabel.Visible = _localFolderName.Enabled = HaveNeededAccountInfo;
//            _sourcetWarningImage.Visible = !haveGoodUrl;

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
					if (_localFolderName.Text.Trim().Length == 0)
						_targetInfoLabel.Text = "Please enter a name";
					else
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
			get { return "http://"+
				HttpUtility.UrlEncode(_accountName.Text) + ":" +
				HttpUtility.UrlEncode(_password.Text) + "@" + ServerPath + "/" +
				HttpUtility.UrlEncode(_projectId.Text);
			}
		   // set { _urlBox.Text = value; }
		}

		protected string ServerPath
		{
			get
			{
				if(_serverCombo.SelectedIndex < 0)
						return string.Empty;
				return _servers[(string)_serverCombo.SelectedItem];
			}
		}

		public string NameOfProjectOnRepository
		{
			get
			{
				if (!HaveNeededAccountInfo)
					return string.Empty;
//                var uri = new Uri(URL);
//                return uri.PathAndQuery.Trim('/').Replace('/', '-').Replace('?', '-').Replace('*', '-').Replace('\\', '-');
				return _projectId.Text;
			}
		}
		public bool ReadyForDownload
		{
			get { return HaveNeededAccountInfo && HaveWellFormedTargetLocation && TargetLocationIsUnused ; }
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
				return (_localFolderName.Text.Trim().Length > 0 && _localFolderName.Text.LastIndexOfAny(Path.GetInvalidFileNameChars()) == -1);
			}
		}

		public string TargetDestination
		{
			get { return Path.Combine(_parentDirectoryToPutCloneIn, _localFolderName.Text); }
		}

		protected bool HaveNeededAccountInfo
		{
			get
			{
				try
				{
//                    var uri = new Uri(_urlBox.Text);
//                    return uri.Scheme =="http" &&
//                           Uri.IsWellFormedUriString(_urlBox.Text, UriKind.Absolute) &&
//                           !string.IsNullOrEmpty(uri.PathAndQuery.Trim('/'));
					return _projectId.Text.Trim().Length > 1 &&
							_accountName.Text.Trim().Length > 1 &&
							_password.Text.Trim().Length > 1;
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

		private void OnAccountInfoTextChanged(object sender, EventArgs e)
		{
		   _localFolderName.Text = NameOfProjectOnRepository;
			UpdateDisplay();
			toolTip1.SetToolTip(_downloadButton, URL);
		}


		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://public.languagedepot.org");
		}

	}
}
