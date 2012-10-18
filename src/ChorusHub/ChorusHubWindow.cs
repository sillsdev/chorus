using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ChorusHub
{
	/// <summary>
	/// Gives a simple logbox windows with a control to stop the server
	/// </summary>
	public partial class ChorusHubWindow : Form
	{
		private ChorusHubService _service;
		private bool _running;

		public ChorusHubWindow(string path)
		{
			InitializeComponent();
			_service = new ChorusHubService(path);
			_service.Progress = _logBox;
			_logBox.ShowDetailsMenuItem = true;
			_logBox.ShowCopyToClipboardMenuItem = true;
		}

		private void ChorusHubWindow_Load(object sender, EventArgs e)
		{
		   _running = _service.Start(true);
			if(!_logBox.ErrorEncountered)
			{
				_logBox.WriteMessageWithColor(Color.Blue, "Chorus Hub Started");
			}
			_serviceTimer.Enabled = true;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(_running)
			{
				_logBox.WriteMessage("Please stop the server before closing.");
				e.Cancel = true;
				return;
			}
			base.OnClosing(e);
		}

		private void _stopChorusHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (_running)
				_service.Stop();
			_serviceTimer.Enabled = false;
			_running = false;
			_stopChorusHub.Enabled = false;
			_stopChorusHub.Text = "Stopped";
			_logBox.WriteMessageWithColor(Color.Blue,"Chorus Hub Stopped");
			_logBox.WriteMessage("Quit and run Chorus Hub to start again.");
			//Close();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			_service.DoOccasionalBackgroundTasks();
		}
	}
}
