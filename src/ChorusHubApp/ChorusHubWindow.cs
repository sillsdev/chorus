using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ChorusHub;

namespace ChorusHubApp
{
	/// <summary>
	/// Gives a simple logbox windows with a control to stop the server
	/// </summary>
	public partial class ChorusHubWindow : Form
	{
		private readonly ChorusHubServer _chorusHubServer;
		private bool _running;

		public ChorusHubWindow()
		{
			InitializeComponent();
			_chorusHubServer = new ChorusHubServer();
			_logBox.ShowDetailsMenuItem = true;
			_logBox.ShowCopyToClipboardMenuItem = true;
		}

		private void ChorusHubWindow_Load(object sender, EventArgs e)
		{
		   _running = _chorusHubServer.Start(true);
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
				_chorusHubServer.Stop();
			_serviceTimer.Enabled = false;
			_running = false;
			_stopChorusHub.Enabled = false;
			_stopChorusHub.Text = @"Stopped";
			_logBox.WriteMessageWithColor(Color.Blue,"Chorus Hub Stopped");
			_logBox.WriteMessage("Quit and run Chorus Hub to start again.");
			//Close();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			_chorusHubServer.DoOccasionalBackgroundTasks();
		}
	}
}
