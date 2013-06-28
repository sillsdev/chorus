using System;
using System.Windows.Forms;
using Chorus.Utilities;
using L10NSharp;
using Palaso.Progress;

namespace Chorus.UI.Settings
{
	public partial class SettingsView : UserControl
	{
		private readonly SettingsModel _model;
		private IProgress _progress = new NullProgress();
		private bool _didLoad;

		public SettingsView(SettingsModel model)
		{
			_model = model;
			InitializeComponent();
		}

		protected override void OnLoad(System.EventArgs e)
		{
			_didLoad = true;// trying to track down WS-14977
			base.OnLoad(e);
			if(_model==null)
				return;

			_userName.Text = _model.GetUserName(_progress);
			_repositoryAliases.Text = _model.GetAddresses();

		}

		private void OnUserName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				_model.SetUserName(_userName.Text, _progress);
			}
			catch (Exception)
			{
				MessageBox.Show(LocalizationManager.GetString("Messages.CouldNotChangeName", "Could not change the name to that."));
				e.Cancel = true;
				throw;
			}
		}

		private void OnRepositoryAliases_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				if (!_didLoad && _repositoryAliases.Text.IndexOf("=" ) <0)
				{
					//avoid mono bug where it calls validate on stuff it never loaded.
					return;
				}
				_model.SetAddresses(_repositoryAliases.Text, _progress);
			}
			catch (Exception error)
			{
				ReportProblemStoringAddresses(error);
				e.Cancel = true;
			}
		}

		private static void ReportProblemStoringAddresses(Exception error)
		{
			MessageBox.Show(
				LocalizationManager.GetString("Messages.ProblemStoringAddresses",
					"Chorus encounterd a problem while trying to store these addresses:") + "\r\n" + error.Message,
				LocalizationManager.GetString("Messages.SettingsProblem", "Chorus settings problem"), MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}

		private void _repositoryAliases_Leave(object sender, EventArgs e)
		{
			if (!_didLoad && _repositoryAliases.Text.IndexOf("=") < 0)
			{
				MessageBox.Show(LocalizationManager.GetString("Messages.MonoCallingLeave", "Please report to issues@wesay.org: mono is calling leave() on SettingsView which was never loaded", "never seen in normal operation"));
				return;
			}
			try
			{
				_model.SetAddresses(_repositoryAliases.Text, _progress);
			}
			catch (Exception error)
			{
				ReportProblemStoringAddresses(error);
			}
		}
	}
}