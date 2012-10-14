using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace ChorusHub
{
	public class Advertiser :IDisposable
	{
		private Thread _thread;
		private UdpClient _client;
		private IPEndPoint _endPoint;
		private byte[] _sendBytes;
		private string _currentIpAddress;
		public const int Port=8883;

		public static void SendCallback(IAsyncResult args)
		{
//            UdpClient client = (UdpClient)args.AsyncState;
//
//            Console.WriteLine("number of bytes sent: {0}", client.EndSend(args));
			//messageSent = true;
		}

		public void Start()
		{
			_client = new UdpClient();
			_endPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), Port);
			 _thread = new Thread(Work);
			_thread.Start();
		}

		private void GetStringToSend()
		{

		}

		private void Work()
		{
			try
			{
				while (true)
				{
					UpdateAdvertisementBasedOnCurrentIpAddress();
					Console.Write(".");
					_client.BeginSend(_sendBytes, _sendBytes.Length, _endPoint, SendCallback, _client);
					Thread.Sleep(1000);
				}
			}
			catch(ThreadAbortException)
			{
				Debug.WriteLine("Advertiser Stopped");
				_client.Close();
				return;
			}
			catch(Exception error)
			{
				Console.WriteLine("Error in Advertiser: "+error.Message);
				Debug.WriteLine("Error in Advertiser: " + error.Message);
			}
		}


		/// <summary>
		/// Since this migt not be a real "server", its ipaddress could be assigned dynamically,
		/// and could change each time someone "wakes up the server laptop" each morning
		/// </summary>
		private void UpdateAdvertisementBasedOnCurrentIpAddress()
		{
			if (_currentIpAddress != GetLocalIpAddress())
			{
				_currentIpAddress = GetLocalIpAddress();
				ChorusHubInfo info = new ChorusHubInfo(_currentIpAddress, HgServeRunner.Port.ToString(),
													   System.Environment.MachineName);
				_sendBytes = Encoding.ASCII.GetBytes(info.ToString());
				Console.WriteLine("Serving at http://" + _currentIpAddress + ":" + HgServeRunner.Port);
			}
		}

		private string GetLocalIpAddress()
		{
			IPHostEntry host;
			string localIP = null;
			host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					if (localIP != null)
					{
						if (host.AddressList.Length > 1)
							Console.WriteLine("Warning: this machine has more than one IP address");
					}
					localIP = ip.ToString();
				}
			}
			return localIP ?? "Could not determine IP Address!";
		}

		public void Stop()
		{
			if (_thread != null)
			{
				Debug.WriteLine("Advertiser Stopping...");
				_thread.Abort();
				_thread.Join();
				//Debug.WriteLine("Advertiser Stopped");
				_thread = null;
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
