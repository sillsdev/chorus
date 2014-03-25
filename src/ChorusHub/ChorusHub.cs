using System.ServiceProcess;
using System.Timers;

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
				_chorusHubServer = new ChorusHubServer();
				EventLog.WriteEntry("Chorus Hub Service is starting....");
				_running = _chorusHubServer.Start(true);
				_serviceTimer = new Timer
				{
					Interval = 500, Enabled = true
				};
				_serviceTimer.Elapsed += ServiceTimerOnElapsed;
				_serviceTimer.Start();
			}
			EventLog.WriteEntry("Chorus Hub Service started.");
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
