using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using Palaso.Progress;

namespace ChorusHub
{
	[ServiceContract]
	public interface IChorusHubService
	{
		[OperationContract]
		IEnumerable<string> GetRepositoryNames(Func<string, bool> filter);

		[OperationContract]
		bool PrepareToReceiveRepository(string name);
	}

	public class ChorusHubService:IDisposable
	{
		public static ChorusHubParameters Parameters;
		private ServiceHost _serviceHost;
		public int ServicePort;// = 8002;

		private static HgServeRunner _hgServer;
		private static Advertiser _advertiser;
		public IProgress Progress = new ConsoleProgress();

		public ChorusHubService(ChorusHubParameters parameters)
		{
			Parameters = parameters;
			ServicePort = ChorusHubParameters.kServicePort;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="includeMercurialServer">During tests that don't actually involve send/receive/clone, you can speed things
		/// up by setting this to false</param>
		public bool Start(bool includeMercurialServer)
		{
			ChorusHubServiceImplementation.Progress = Progress;

			try
			{
				if (includeMercurialServer)
				{
					_hgServer = new HgServeRunner(Parameters.RootDirectory, ChorusHubParameters.kMercurialPort) { Progress = Progress };
					if (!_hgServer.Start())
						return false;
				}
				_advertiser = new Advertiser(ChorusHubParameters.kAdvertisingPort) { Progress = Progress };
				_advertiser.Start();

				//gave security error _serviceHost = new ServiceHost(this);
				_serviceHost = new ServiceHost(typeof(ChorusHubServiceImplementation));

			   EnableSendingExceptionsToClient();

				string address = "net.tcp://localhost:" + ServicePort.ToString();
				var binding = new NetTcpBinding();
				binding.Security.Mode = SecurityMode.None;
				_serviceHost.AddServiceEndpoint(typeof(IChorusHubService), binding, address);
				Progress.WriteVerbose("Starting extra chorus hub services on {0}", address);
				_serviceHost.Open();
				return true;
			}
			catch (Exception error)
			{
				Progress.WriteException(error);
				return false;
			}
		}

		private void EnableSendingExceptionsToClient()
		{
			ServiceDebugBehavior debug = _serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();

			// if not found - add behavior with setting turned on
			if (debug == null)
			{
				_serviceHost.Description.Behaviors.Add(
					new ServiceDebugBehavior() {IncludeExceptionDetailInFaults = true});
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

			if(_serviceHost!=null)
			{
				_serviceHost.Close();
				_serviceHost = null;
			}
		}

		public void Dispose()
		{
			Stop();
		}

		public void DoOccasionalBackgroundTasks()
		{
			if(_hgServer!=null)
				_hgServer.CheckForFailedPushes();
		}
	}
}
