using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace ChorusHub
{
	public class ChorusHubClient
	{
		private ChorusHubInfo _foundHubInfo;
		private IPEndPoint _ipEndPoint;
		private UdpClient _udpClient;
		private IAsyncResult _asyncResult;
		private IEnumerable<string> _repositoryNames;

		public string HostName
		{
			get { return _foundHubInfo!=null ? _foundHubInfo.HostName : ""; }
		}

		public void StartFinding()
		{

			_ipEndPoint = new IPEndPoint(IPAddress.Any, Advertiser.Port);
			_udpClient = new UdpClient();

			//This reuse business is in hopes of avoiding the dreaded "Only one usage of each socket address is normally permitted"
			_udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_udpClient.Client.Bind(_ipEndPoint);

			_asyncResult = _udpClient.BeginReceive(ReceiveFindingCallback, null);
		}

		public void StopFinding()
		{
			try
			{
				_udpClient.Close();
				Debug.WriteLine("Finder Stopped");
			}
			catch (Exception)
			{
				//not worth bothering the user
#if DEBUG
				throw;
#endif
			}
		}

		private void ReceiveFindingCallback(IAsyncResult args)
		{

			Byte[] receiveBytes;
			try
			{
				if (_udpClient.Client == null)
					return;
				receiveBytes  = _udpClient.EndReceive(args, ref _ipEndPoint);
			}
			catch(ObjectDisposedException)
			{
				//this is actually the expected behavior, if there is no chorus hub out there!
				//http://stackoverflow.com/questions/4662553/how-to-abort-sockets-beginreceive
				//note the check for Client == null above seems to help some...

				return;
			}

			try
			{
				string s = Encoding.ASCII.GetString(receiveBytes);
				if (ChorusHubInfo.IsChorusHubInfo(s))
				{
					_foundHubInfo = ChorusHubInfo.Parse(s);
				}
			}
			catch (Exception)
			{
#if DEBUG
				throw;
#endif
				//else, not worth doing any more than, well, not finding the hub.
			}
		}

		public ChorusHubInfo FindServer()
		{
			_foundHubInfo = null;
			StartFinding();
			for (int i = 0; i < 20; i++)
			{
				if (_foundHubInfo != null)
					break;
				Thread.Sleep(200);
			}
			StopFinding();
			return _foundHubInfo; //will be null if none found
		}

		public IEnumerable<string> GetRepositoryNames()
		{
			if(_repositoryNames!=null)
				return _repositoryNames; //for now, there's no way to get an updated list except by making a new client

			if(_foundHubInfo==null)
				throw new ApplicationException("Programmer, call Find() and get a non-null response before GetRepositoryNames");

			var factory = new ChannelFactory<IChorusHubService>(new NetTcpBinding(), _foundHubInfo.ServiceUri);

			var channel = factory.CreateChannel();
			try
			{
				_repositoryNames = channel.GetRepositoryNames();
			}
			finally
			{
				(channel as ICommunicationObject).Close();
			}
			return _repositoryNames;
		}

		public string GetUrl(string repositoryName)
		{
			return _foundHubInfo.GetHgHttpUri(repositoryName);
		}

		/// <summary>
		/// Since Hg Serve doesn't provide a way to make new repositories, this asks our ChorusHub wrapper
		/// to do create the repository. The complexity comes in the timing; hg serve will eventually
		/// notice the new server, but we don't really know when.
		/// </summary>
		/// <param name="directoryName"></param>
		public bool PrepareHubToSync(string directoryName)
		{
			//Enchance: after creating and init'ing the folder, it would be possible to keep asking
			//hg serve if it knows about the repository until finally it says "yes", instead of just
			//guessing at a single amount of time to wait
			var factory = new ChannelFactory<IChorusHubService>(new NetTcpBinding(), _foundHubInfo.ServiceUri);

			var channel = factory.CreateChannel();
			try
			{
				var doWait = channel.PrepareToReceiveRepository(directoryName);
				return doWait;
			}
			finally
			{
				(channel as ICommunicationObject).Close();
			}
		}
	}
}