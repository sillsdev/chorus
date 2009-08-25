using System;
using System.IO;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	public partial class InternetRepositoryInfoControl : UserControl
	{
		private readonly string _parentDirectoryToPutCloneIn;

		public InternetRepositoryInfoControl(string parentDirectoryToPutCloneIn)
		{
			_parentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
			InitializeComponent();
		}

		private void UpdateDisplay()
		{
			_downloadButton.Enabled = HaveGoodUrl && HaveGoodTargetLocation;

			_warningImage.Visible =_targetInfoLabel.Visible = _localFolderName.Enabled = HaveGoodUrl;

			if (!HaveGoodTargetLocation)
			{
				_warningImage.Visible = true;
				_targetInfoLabel.Text = string.Format("There is a already a project with that name at {0}", TargetDestination);
			}
			else
			{
				_warningImage.Visible = false;
				_targetInfoLabel.Text = string.Format("Project will be downloaded to {0}", TargetDestination);
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
				var last = _urlBox.Text.LastIndexOf('/');
				if (last > 0)
				{
					return _urlBox.Text.Substring(last + 1);
				}
				return string.Empty;
			}
		}
		public bool ReadForDownload
		{
			get { return HaveGoodUrl && HaveGoodTargetLocation; }
		}

		protected bool HaveGoodTargetLocation
		{
			get { return Directory.Exists(_parentDirectoryToPutCloneIn) && !Directory.Exists(TargetDestination); }
		}

		public string TargetDestination
		{
			get { return Path.Combine(_parentDirectoryToPutCloneIn, _localFolderName.Text); }
		}

		protected bool HaveGoodUrl
		{
			get { return !string.IsNullOrEmpty(_urlBox.Text); }
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
	}
}
