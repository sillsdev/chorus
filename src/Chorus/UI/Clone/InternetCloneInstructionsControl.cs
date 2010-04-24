using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	public partial class InternetCloneInstructionsControl : UserControl
	{
		private readonly GetCloneFromInternetModel _model;

		public InternetCloneInstructionsControl(GetCloneFromInternetModel model)
		{
			_model = model;

			InitializeComponent();

			foreach (KeyValuePair<string, string> pair in _model.Servers)
			{
				_serverCombo.Items.Add(pair.Key);
			}
			_serverCombo.SelectedIndexChanged += OnSelectedIndexChanged;
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_model.SelectedServerLabel != (string)_serverCombo.SelectedItem)
			{
				_model.SelectedServerLabel = (string) _serverCombo.SelectedItem;

				UpdateDisplay();
			}
		}

		private void UpdateDisplay()
		{
			_serverCombo.SelectedItem = _model.SelectedServerLabel;

			_customUrl.Text = _model.URL;
			_customUrl.Visible = _model.CustomUrlSelected;
			_customUrlLabel.Visible = _model.CustomUrlSelected;

			_localFolderName.Text = _model.LocalFolderName;

			_downloadButton.Enabled = _model.ReadyToDownload;

		   _localFolderName.Enabled =  _model.HaveNeededAccountInfo;

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
				else if (!_model.TargetLocationIsUnused)
				{
					_targetInfoLabel.Text = string.Format("There is a already a project with that name at {0}",
														  _model.TargetDestination);
				}
				else if (!_model.HaveWellFormedTargetLocation)
				{
					if (_localFolderName.Text.Trim().Length == 0)
						_targetInfoLabel.Text = "Please enter a name";
					else
						_targetInfoLabel.Text = string.Format("That name contains characters which are not allowed.");
				}
				else
				{
					_targetWarningImage.Visible = false;
					_targetInfoLabel.Text = string.Format("Project will be downloaded to {0}", _model.TargetDestination);
				}
			}


			_accountName.Visible = _model.NeedProjectDetails;
			_projectId.Visible = _model.NeedProjectDetails;
			_password.Visible = _model.NeedProjectDetails;
			_accountLabel.Visible = _model.NeedProjectDetails;
			_projectIdLabel.Visible = _model.NeedProjectDetails;
			_passwordLabel.Visible = _model.NeedProjectDetails;
			_localFolderName.Enabled = true;

			toolTip1.SetToolTip(_downloadButton, _model.URL);
		}

		private void _localName_TextChanged(object sender, EventArgs e)
		{
			_model.LocalFolderName = _localFolderName.Text.Trim();
		   UpdateDisplay();
		}

		private void _customUrl_TextChanged(object sender, EventArgs e)
		{
			_model.CustomUrl = _customUrl.Text.Trim();
			UpdateDisplay();
		}

		private void _projectId_TextChanged(object sender, EventArgs e)
		{
			_model.ProjectId = _projectId.Text.Trim();
			UpdateDisplay();
		}

		private void _accountName_TextChanged(object sender, EventArgs e)
		{
			_model.AccountName = _accountName.Text.Trim();
			UpdateDisplay();
		}

		private void _password_TextChanged(object sender, EventArgs e)
		{
			_model.Password = _password.Text.Trim();
		}

		private void InternetCloneInstructionsControl_Load(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
