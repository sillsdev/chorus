using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ChorusHub;

namespace ChorusHub
{
	 [ServiceContract]
	public interface IChorusHubService
	{
		[OperationContract]
		 IEnumerable<string> GetRepositoryNames();
	}

	 [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	 internal class ServiceImplementation : IChorusHubService
	 {
		 public IEnumerable<string> GetRepositoryNames()
		 {
			 Console.WriteLine("Client requested repository names.");

			 foreach (var directory in Directory.GetDirectories(ChorusHubService._rootPath))
			 {
				 yield return Path.GetFileName(directory);
			 }
		 }
	 }

	 //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class ChorusHubService:IDisposable//, IChorusHubService
	{
		internal static string _rootPath;
		private ServiceHost _serviceHost;
		public const int ServicePort = 8002;
		private static HgServeRunner _hgServer;
		private static Advertiser _advertiser;

		public ChorusHubService(string rootPath)
		{
			_rootPath = rootPath;
		}
//         public IEnumerable<string> GetRepositoryNames()
//         {
//             Console.WriteLine("Client requested repository names.");
//
//             foreach (var directory in Directory.GetDirectories(_rootPath))
//             {
//                 yield return directory;
//             }
//         }

		/// <summary>
		///
		/// </summary>
		/// <param name="includeMercurialServer">During tests that don't actually involve send/receive/clone, you can speed things
		/// up by setting this to false</param>
		public void Start(bool includeMercurialServer)
		{
			if(includeMercurialServer)
			{
				_hgServer = new HgServeRunner(_rootPath);
				_hgServer.Start();
			}
			_advertiser = new Advertiser();
			_advertiser.Start();

			//gave security error _serviceHost = new ServiceHost(this);
			_serviceHost = new ServiceHost(typeof(ServiceImplementation));
			//_serviceHost.AddServiceEndpointtypeof(IChorusHubService), new NetTcpBinding(), "net.tcp://localhost:" + ServicePort.ToString());
			_serviceHost.AddServiceEndpoint(typeof(IChorusHubService), new NetTcpBinding(), "net.tcp://localhost:" + ServicePort.ToString());
			_serviceHost.Open();
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
