using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Chorus.ChorusHub;

namespace ChorusHub
{
	public class Advertiser : IDisposable
	{
		private Thread _thread;
		private UdpClient _client;
		private IPEndPoint _endPoint;
		private byte[] _sendBytes;
		private string _currentIpAddress;
		public int Port;

		public Advertiser(int port)
		{
			Port = port;
		}
		public void Start()
		{
			// The doc seems to indicate that EnableBroadcast is required for doing broadcasts.
			// In practice it seems to be required on Mono but not on Windows.
			// This may be fixed in a later version of one platform or the other, but please
			// test both if tempted to remove it.
			_client = new UdpClient
			{
				EnableBroadcast = true
			};
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
					_client.BeginSend(_sendBytes, _sendBytes.Length, _endPoint, SendCallback, _client);
					Thread.Sleep(1000);
				}
			}
			catch(ThreadAbortException)
			{
				//Progress.WriteVerbose("Advertiser Thread Aborting (that's normal)");
				_client.Close();
			}
			catch(Exception)
			{
				//EventLog.WriteEntry("Application", string.Format("Error in Advertiser: {0}", error.Message), EventLogEntryType.Error);
			}
		}

		public static void SendCallback(IAsyncResult args)
		{
		}

		/// <summary>
		/// Since this might not be a real "server", its ip address could be assigned dynamically,
		/// and could change each time someone "wakes up the server laptop" each morning
		/// </summary>
		private void UpdateAdvertisementBasedOnCurrentIpAddress()
		{
			if (_currentIpAddress != GetLocalIpAddress())
			{
				_currentIpAddress = GetLocalIpAddress();
				var serverInfo = new ChorusHubServerInfo(_currentIpAddress, ChorusHubOptions.MercurialPort.ToString(CultureInfo.InvariantCulture),
													   Environment.MachineName, ChorusHubServerInfo.VersionOfThisCode);
				_sendBytes = Encoding.ASCII.GetBytes(serverInfo.ToString());
				//EventLog.WriteEntry("Application", "Serving at http://" + _currentIpAddress + ":" + ChorusHubOptions.MercurialPort, EventLogEntryType.Information);
			}
		}

		private string GetLocalIpAddress()
		{
			string localIp = null;
			var host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (var ipAddress in host.AddressList.Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork))
			{
				if (localIp != null)
				{
					if (host.AddressList.Length > 1)
					{
						//EventLog.WriteEntry("Application", "Warning: this machine has more than one IP address", EventLogEntryType.Warning);
					}
				}
				localIp = ipAddress.ToString();
			}
			return localIp ?? "Could not determine IP Address!";
		}

		public void Stop()
		{
			if (_thread == null)
				return;

			//EventLog.WriteEntry("Application", "Advertiser Stopping...", EventLogEntryType.Information);
			_thread.Abort();
			_thread.Join(2 * 1000);
			_thread = null;
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
