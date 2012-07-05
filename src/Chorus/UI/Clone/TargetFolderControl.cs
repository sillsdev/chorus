using System;
using System.IO;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	public partial class TargetFolderControl : UserControl
	{
		private readonly GetCloneFromInternetModel _model;

		[Obsolete("for forms designer only")]
		public TargetFolderControl()
		{
			InitializeComponent();
		}

		public TargetFolderControl(GetCloneFromInternetModel model)
		{
			_model = model;
			InitializeComponent();
		}

		private void UpdateDisplay()
		{
			_localFolderName.Text = _model.LocalFolderName;
			_downloadButton.Enabled = _model.ReadyToDownload;
			_localFolderName.Enabled = true;
			_targetInfoLabel.Visible = true;

			if (string.IsNullOrEmpty(_model.LocalFolderName))
			{
				_targetInfoLabel.Text = "For example, 'Swahili Project'";
			}

			if (_model.HaveGoodUrl)
			{
				_targetWarningImage.Visible = _model.TargetHasProblem;
				if (!Directory.Exists(_model.ParentDirectoryToPutCloneIn))
				{
					_targetInfoLabel.Text = string.Format("The directory {0} doesn't exist, but should have been created by the application.",
															_model.ParentDirectoryToPutCloneIn);
				}
				else if (!_model.HaveWellFormedTargetLocation)
				{
					if (_localFolderName.Text.Trim().Length == 0)
						_targetInfoLabel.Text = "Please enter a name";
					else
						_targetInfoLabel.Text = string.Format("That name contains characters which are not allowed.");
				}
				else if (!_model.TargetLocationIsUnused)
				{
					_targetInfoLabel.Text = string.Format("There is a already a project with that name at {0}",
															_model.TargetDestination);
				}
				else
				{
					_targetWarningImage.Visible = false;
					_targetInfoLabel.Text = string.Format("Project will be downloaded to {0}", _model.TargetDestination);
				}
			}

			toolTip1.SetToolTip(_downloadButton, _model.URL);
		}

		private void _localName_TextChanged(object sender, EventArgs e)
		{
			_model.LocalFolderName = _localFolderName.Text;
		   UpdateDisplay();
		}

		private void InternetCloneInstructionsControl_Load(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
