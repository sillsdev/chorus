using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Chorus.Utilities;

namespace Baton.Settings
{
	public partial class SettingsView : UserControl
	{
		private readonly SettingsModel _model;
		private IProgress _progress = new NullProgress();

		public SettingsView(SettingsModel model)
		{
			_model = model;
			InitializeComponent();
		}

		protected override void OnLoad(System.EventArgs e)
		{
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
				MessageBox.Show("Could not change the name to that.");
				e.Cancel = true;
				throw;
			}
		}

		private void OnRepositoryAliases_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				_model.SetAddresses(_repositoryAliases.Text, _progress);
			}
			catch (Exception)
			{
				MessageBox.Show("There was a problem with the format of the repositories list.");
				e.Cancel = true;
			}
		}

		private void _repositoryAliases_Leave(object sender, EventArgs e)
		{
			try
			{
				_model.SetAddresses(_repositoryAliases.Text, _progress);
			}
			catch (Exception)
			{
				MessageBox.Show("There was a problem with the format of the repositories list.");
			}
		}
	}
}