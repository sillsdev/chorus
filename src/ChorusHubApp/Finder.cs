using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChorusHub
{
	public class Finder
	{
		private State _state;
		private IAsyncResult _asyncResult;
		private ChorusHubInfo _foundHub;

		private class State
		{
			public IPEndPoint Endpoint;
			public UdpClient udpClient;
		}

		public void Start()
		{
			_state = new State();
			_state.Endpoint = new IPEndPoint(IPAddress.Any, Advertiser.Port);
			_state.udpClient = new UdpClient(_state.Endpoint);
			_asyncResult = _state.udpClient.BeginReceive(ReceiveCallback, _state);
		}

		public void Stop()
		{
		 //just kills us _state.udpClient.EndReceive(_asyncResult, ref _state.Endpoint);
	   //     Debug.WriteLine("Finder Stopped");
		}

		public void ReceiveCallback(IAsyncResult args)
		{
			UdpClient u = ((State)(args.AsyncState)).udpClient;
			IPEndPoint e = ((State)(args.AsyncState)).Endpoint;

			Byte[] receiveBytes = u.EndReceive(args, ref e);
			string s = Encoding.ASCII.GetString(receiveBytes);
			if(ChorusHubInfo.IsChorusHubInfo(s))
			{
				_foundHub = ChorusHubInfo.Parse(s);
			}
		}

		public ChorusHubInfo Find()
		{
			_foundHub = null;
			Start();
			for (int i = 0; i < 20; i++)
			{
				if (_foundHub != null)
					break;
				Thread.Sleep(200);
			}
			Stop();
			return _foundHub;//will be null if none found
		}
	}
}
