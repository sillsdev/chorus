using System;
using System.Collections.Generic;
using System.ServiceModel;
using Palaso.Progress;

namespace ChorusHub
{
	 [ServiceContract]
	public interface IChorusHubService
	{
		  [OperationContract]
		 IEnumerable<string> GetRepositoryNames();

		[OperationContract]
		bool PrepareToReceiveRepository(string name);
	}

	public class ChorusHubService:IDisposable
	{
		internal static string _rootPath;
		private ServiceHost _serviceHost;
		public const int ServicePort = 8002;
		private static HgServeRunner _hgServer;
		private static Advertiser _advertiser;
		public IProgress Progress = new ConsoleProgress();

		public ChorusHubService(string rootPath)
		{
			_rootPath = rootPath;
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
					_hgServer = new HgServeRunner(_rootPath) { Progress = Progress };
					if (!_hgServer.Start())
						return false;
				}
				_advertiser = new Advertiser() { Progress = Progress };
				_advertiser.Start();

				//gave security error _serviceHost = new ServiceHost(this);
				_serviceHost = new ServiceHost(typeof(ServiceImplementation));
				string address = "net.tcp://localhost:" + ServicePort.ToString();
				_serviceHost.AddServiceEndpoint(typeof(IChorusHubService), new NetTcpBinding(), address);
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

		public void Tick()
		{
			_hgServer.CheckForFailedPushes();
		}
	}
}
