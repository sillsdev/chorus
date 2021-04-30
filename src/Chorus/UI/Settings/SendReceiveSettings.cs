using System;
using System.IO;
using System.Windows.Forms;
using Chorus.Utilities.Help;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;
using SIL.Code;
using SIL.Extensions;
using SIL.PlatformUtilities;
using SIL.Progress;
using Chorus.Model;

namespace Chorus.UI.Settings
{
	public partial class SendReceiveSettings : Form
	{
		private SettingsModel _model;

		private ServerSettingsModel _internetModel;

		[Obsolete("for designer support only")]
		public SendReceiveSettings()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Make sure that the html content is displayed properly on both linux and windows by using Navigate on the chorusHubSetup control after the dialog is shown
		/// </summary>
		private void SendReceiveSettingsShown(object sender, EventArgs e)
		{
			var hubSetupText = LocalizationManager.GetString("SendReceiveSettings.ChorusHubSetupInstructionsHtml",
				// Hard-code the string instead of pulling from resx so that L10NSharp recognizes it as a localizable string.
				@"<!DOCTYPE html><html><body style='font-family:""Microsoft Sans Serif"",""sans-serif"";font-size:9pt'>
<p style='margin:0in;'>Chorus Hub is a simple way to Send/Receive on a local network, like in an office or home. These instructions are basic, but they should be enough to get you started.</p>
<ol>
<li style='margin-left:-10pt'>Designate one computer on your network as ""the server"".</li>
<li style='margin-left:-10pt'>Install and run Chorus Hub on ""the server"".
<ol><li style='list-style-type:lower-alpha;margin-left:-10pt'> On Windows, locate the file ChorusHubInstaller.msi (normally in C:\Program Files (x86)\SIL\FieldWorks 8\Installers). Double-click it to install Chorus Hub. This will install the program and start the Chorus Hub Sharing Service. By default it is set to restart whenever the machine is rebooted. More details are available at <a target='_blank' href='{0}'>http://fieldworks.sil.org/wp-content/TechnicalDocs/Technical%20Notes%20on%20FieldWorks%20Send-Receive.pdf</a>.</li>
<li style='list-style-type:lower-alpha;margin-left:-10pt'>On Linux, Chorus Hub is automatically installed when you install FLEx. Use the Dash or Main Menu to find and launch Chorus Hub.</li>
</ol></li>
<li style='margin-left:-10pt'>Above these instructions, there is a checkbox labeled ""Show Chorus Hub as a Send/Receive option"". Make sure this checkbox is selected.</li>
</ol>
<p style='margin:0in;'>If all goes well, there is nothing more to do! Now from any other computer on the network, when you do Send/Receive, there should be a button labeled ""Chorus Hub"", and under it should be a message telling you it has found Chorus Hub on the machine it is running on. If you click that button, your project will be Sent/Received.</p>
</body></html>",
				"Instructions shown before first Send/Receive. Please keep HTML tags as-is to preserve formatting, and use HTML escapes");
			// Inject the actual link.
			// Different for Linux because XULRunner crashes trying to download file. https://bugzilla.mozilla.org/show_bug.cgi?id=851217 says this
			// was fixed in Firefox 27 (but it is obviously broken in XULRunner 29). A quick google reveals that others have been able to get around
			// this problem by including a branding .dtd file (this may be a Linux packaging problem?), but I'm too lazy to try that approach.
			hubSetupText = hubSetupText.FormatWithErrorStringInsteadOfException(Platform.IsUnix
				? @"http://fieldworks.sil.org/fw-info/technical-documents/" // The PDF can be downloaded from this webpage
				: @"http://fieldworks.sil.org/wp-content/TechnicalDocs/Technical%20Notes%20on%20FieldWorks%20Send-Receive.pdf");
			var tempFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".html"));
			File.WriteAllText(tempFile, hubSetupText);
			var uri = new Uri(tempFile);
			chorusHubSetup.Navigate(uri.AbsoluteUri);
		}

		public SendReceiveSettings(string repositoryLocation)
		{
			InitializeComponent();

			Shown += SendReceiveSettingsShown;
			RequireThat.Directory(repositoryLocation).Exists();
			var repository = HgRepository.CreateOrUseExisting(repositoryLocation, new NullProgress());
			_model = new SettingsModel(repository);
			userNameTextBox.Text = _model.GetUserName(new NullProgress());

			_internetModel = new ServerSettingsModel();
			_internetModel.InitFromProjectPath(repositoryLocation);
			_serverSettingsControl.Model = _internetModel;

			_internetButtonEnabledCheckBox.CheckedChanged += internetCheckChanged;
			_internetButtonEnabledCheckBox.Checked = Properties.Settings.Default.InternetEnabled;
			_serverSettingsControl.Enabled = _internetButtonEnabledCheckBox.Checked;

			_showChorusHubInSendReceive.Checked = Properties.Settings.Default.ShowChorusHubInSendReceive;
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			if(_internetButtonEnabledCheckBox.Checked)
			{
				_internetModel.SaveSettings();
			}
			_model.SaveSettings();
			Properties.Settings.Default.InternetEnabled = _internetButtonEnabledCheckBox.Checked;
			Properties.Settings.Default.ShowChorusHubInSendReceive = _showChorusHubInSendReceive.Checked;
			Properties.Settings.Default.Save();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void userNameTextBox_TextChanged(object sender, EventArgs e)
		{
			var _userName = userNameTextBox;
			if (_model.GetUserName(new NullProgress()) != _userName.Text.Trim() && _userName.Text.Trim().Length > 0)
			{
				_model.SetUserName(_userName.Text.Trim(), new NullProgress());
			}
		}

		private void internetCheckChanged(object sender, EventArgs e)
		{
			_serverSettingsControl.Enabled = _internetButtonEnabledCheckBox.Checked;
		}

		private void _helpButton_Click(object sender, EventArgs e)
		{
			string helpFile = HelpUtils.GetHelpFile();

			var selectedTab = settingsTabs.SelectedTab;
			if (selectedTab == internetTab)
			{
				Help.ShowHelp(this, helpFile,
					"Tasks/Internet_tab.htm");
			}
			else if (selectedTab == chorusHubTab)
			{
				Help.ShowHelp(this, helpFile,
					"/Tasks/Chorus_Hub_tab.htm");
			}
		}
	}
}
