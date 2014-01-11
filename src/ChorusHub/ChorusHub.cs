using System.ServiceProcess;
using System.Timers;

namespace ChorusHub
{
	public partial class ChorusHub : ServiceBase
	{
		private ChorusHubService _chorusHubService;
		private bool _running;
		private Timer _serviceTimer;

		public ChorusHub()
		{
			InitializeComponent();

			CanPauseAndContinue = false;
		}

		private void ServiceTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			_chorusHubService.DoOccasionalBackgroundTasks();
		}

		protected override void OnStart(string[] args)
		{
			if (!_running)
			{
				_chorusHubService = new ChorusHubService();
				EventLog.WriteEntry("Chorus Hub Service is starting....");
				_running = _chorusHubService.Start(true);
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
				_chorusHubService.Stop();
				_chorusHubService.Dispose();
				_chorusHubService = null;
				_running = false;
			}
			EventLog.WriteEntry("Chorus Hub Service stopped.");
		}
	}
}
