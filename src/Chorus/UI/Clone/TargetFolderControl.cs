using System;
using System.IO;
using System.Windows.Forms;
using L10NSharp;

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

		internal void UpdateDisplay()
		{
			if (_localFolderName.Text != _model.LocalFolderName)
				_localFolderName.Text = _model.LocalFolderName;
			_downloadButton.Enabled = _model.ReadyToDownload;
			_localFolderName.Enabled = true;
			_targetInfoLabel.Visible = true;

			if (string.IsNullOrEmpty(_model.LocalFolderName))
			{
				_targetInfoLabel.Text = LocalizationManager.GetString("Messages.ExampleSwahili", "For example, 'Swahili Project'");
			}

			if (_model.HaveGoodUrl)
			{
				_targetWarningImage.Visible = _model.TargetHasProblem;
				if (!Directory.Exists(_model.ParentDirectoryToPutCloneIn))
				{
					_targetInfoLabel.Text = string.Format(LocalizationManager.GetString("Messages.DirectoryShouldExist", "The directory {0} doesn't exist, but should have been created by the application."),
															_model.ParentDirectoryToPutCloneIn);
				}
				else if (!_model.HaveWellFormedTargetLocation)
				{
					_targetInfoLabel.Text = _localFolderName.Text.Length == 0 ? LocalizationManager.GetString("Messages.PleaseEnterName", "Please enter a name") : LocalizationManager.GetString("Messages.IllegalCharacters", "That name contains characters which are not allowed.");
				}
				else if (!_model.TargetLocationIsUnused)
				{
					_targetInfoLabel.Text = string.Format(LocalizationManager.GetString("Messages.ProjectExists", "There is a already a project with that name at {0}"),
															_model.TargetDestination);
				}
				else
				{
					_targetWarningImage.Visible = false;
					_targetInfoLabel.Text = string.Format(LocalizationManager.GetString("Messages.ProjectWillGoTo", "Project will be downloaded to {0}"), _model.TargetDestination);
				}
			}

			toolTip1.SetToolTip(_downloadButton, _model.URL);
		}

		private void _localName_TextChanged(object sender, EventArgs e)
		{
			// LT-19858 don't allow whitespace before or after name of destination folder; this handles before
			// trimming the end is trickier because we'd like to allow spaces inside the name.
			var trimmedVersion = _localFolderName.Text.TrimStart();
			if (_model.LocalFolderName != trimmedVersion)
				_model.LocalFolderName = trimmedVersion;
		   UpdateDisplay();
		}

		private void InternetCloneInstructionsControl_Load(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
