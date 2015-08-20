using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using Chorus.ChorusHub;

namespace ChorusHub
{
	public partial class ChorusHub : ServiceBase
	{
		private ChorusHubServer _chorusHubServer;
		private bool _running;
		private Timer _serviceTimer;

		public ChorusHub()
		{
			InitializeComponent();

			CanPauseAndContinue = false;
		}

		private void ServiceTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			_chorusHubServer.DoOccasionalBackgroundTasks();
		}

		protected override void OnStart(string[] args)
		{
			if (!_running)
			{
				var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
				if(chorusHubServerInfo != null)
				{
					EventLog.WriteEntry(string.Format("Only one ChorusHub can be run on a network but there is already one running on {0}", chorusHubServerInfo.HostName), EventLogEntryType.Error);
					Stop();
					return;
				}
				_chorusHubServer = new ChorusHubServer();
				EventLog.WriteEntry("Chorus Hub Service is starting....", EventLogEntryType.Information);
				_running = _chorusHubServer.Start(true);
				_serviceTimer = new Timer
				{
					Interval = 500, Enabled = true
				};
				_serviceTimer.Elapsed += ServiceTimerOnElapsed;
				_serviceTimer.Start();
			}
			EventLog.WriteEntry("Chorus Hub Service started.", EventLogEntryType.Information);
		}

		protected override void OnStop()
		{
			if (_running)
			{
				_serviceTimer.Stop();
				_serviceTimer.Enabled = false;
				_serviceTimer.Elapsed -= ServiceTimerOnElapsed;
				_serviceTimer.Dispose();
				_serviceTimer = null;

				EventLog.WriteEntry("Chorus Hub Service stopping....");
				_chorusHubServer.Stop();
				_chorusHubServer.Dispose();
				_chorusHubServer = null;
				_running = false;
			}
			EventLog.WriteEntry("Chorus Hub Service stopped.");
		}
	}
}
