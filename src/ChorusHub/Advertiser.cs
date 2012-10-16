using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Palaso.Progress;

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
		public IProgress Progress = new ConsoleProgress();


		public void Start()
		{
			_client = new UdpClient();
			_endPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), Port);
			 _thread = new Thread(Work);
			_thread.Start();
		}

		private void Work()
		{
			try
			{
				while (true)
				{
					UpdateAdvertisementBasedOnCurrentIpAddress();
					//Progress.Write(".");
					_client.BeginSend(_sendBytes, _sendBytes.Length, _endPoint, SendCallback, _client);
					Thread.Sleep(1000);
				}
			}
			catch(ThreadAbortException)
			{
				Progress.WriteVerbose("Advertiser Thread Aborting (that's normal)");
				_client.Close();
				return;
			}
			catch(Exception error)
			{
				Progress.WriteError("Error in Advertiser");
				Progress.WriteException(error);
			}
		}

		public static void SendCallback(IAsyncResult args)
		{
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
				Progress.WriteMessage("Serving at http://" + _currentIpAddress + ":" + HgServeRunner.Port);
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
							Progress.WriteWarning("Warning: this machine has more than one IP address");
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
				Progress.WriteVerbose("Advertiser Stopping...");
				_thread.Abort();
				_thread.Join(2*1000);
				_thread = null;
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
