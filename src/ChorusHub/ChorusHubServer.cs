using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Chorus.ChorusHub;

namespace ChorusHub
{
	public class ChorusHubServer : IDisposable
	{
		private ServiceHost _serviceHost;
		public int ServicePort;// = 8002;

		private static HgServeRunner _hgServer;
		private static Advertiser _advertiser;

		public ChorusHubServer()
		{
			ServicePort = ChorusHubOptions.ServicePort;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="includeMercurialServer">During tests that don't actually involve send/receive/clone, you can speed things
		/// up by setting this to false</param>
		public bool Start(bool includeMercurialServer)
		{
			try
			{
				if (includeMercurialServer)
				{
					_hgServer = new HgServeRunner(ChorusHubOptions.RootDirectory, ChorusHubOptions.MercurialPort);
					if (!_hgServer.Start())
						return false;
				}
				_advertiser = new Advertiser(ChorusHubOptions.AdvertisingPort);
				_advertiser.Start();

				//gave security error _serviceHost = new ServiceHost(this);
				_serviceHost = new ServiceHost(typeof(ChorusHubService));

			   EnableSendingExceptionsToClient();

				var address = "net.tcp://localhost:" + ServicePort;
				var binding = new NetTcpBinding
				{
					Security = {Mode = SecurityMode.None}
				};
				_serviceHost.AddServiceEndpoint(typeof(IChorusHubService), binding, address);
				//EventLog.WriteEntry("Application", string.Format("Starting extra chorus hub services on {0}", address), EventLogEntryType.Information);
				_serviceHost.Open();
				return true;
			}
			catch (Exception error)
			{
				//EventLog.WriteEntry("Application", error.Message, EventLogEntryType.Error);
				return false;
			}
		}

		private void EnableSendingExceptionsToClient()
		{
			var debug = _serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();

			// if not found - add behavior with setting turned on
			if (debug == null)
			{
				_serviceHost.Description.Behaviors.Add(new ServiceDebugBehavior{IncludeExceptionDetailInFaults = true});
			}
			else
			{
				// make sure setting is turned ON
				if (!debug.IncludeExceptionDetailInFaults)
				{
					debug.IncludeExceptionDetailInFaults = true;
				}
			}
		}


		public void Stop()
		{
			if (_advertiser != null)
			{
				_advertiser.Stop();
				_advertiser = null;
			}
			if (_hgServer != null)
			{
				_hgServer.Stop();
				_hgServer = null;
			}

			if (_serviceHost == null)
				return;

			_serviceHost.Close();
			_serviceHost = null;
		}

		public void Dispose()
		{
			Stop();
		}

		public void DoOccasionalBackgroundTasks()
		{
			if(_hgServer != null)
				_hgServer.CheckForFailedPushes();
		}
	}
}
