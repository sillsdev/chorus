using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chorus.ChorusHub
{
	/// <summary>
	/// Just serializes/deserializes location of the server
	/// </summary>
	public class ChorusHubServerInfo
	{
		private static ChorusHubServerInfo _chorusHubServerInfo;
		private readonly string _ipAddress;
		private readonly string _port;
		public string HostName;
		public int VersionOfServerChorusHub;
		/// <remarks>
		/// Updated JohnT September 2013 for change making GetRepositoryInformation return a single string instead of enumeration.
		/// Note: a few implementations with the code change but still having the old version may have escaped.
		/// </remarks>
		public const int VersionOfThisCode = 3;

		public ChorusHubServerInfo(string ipAddress, string port, string hostName, int version)
		{
			_ipAddress = ipAddress;
			_port = port;
			HostName = hostName;
			VersionOfServerChorusHub = version;
		}

		public static void ClearServerInfoForTests()
		{
			_chorusHubServerInfo = null;
		}

		public static ChorusHubServerInfo Parse(string parameters)
		{
			var start = parameters.IndexOf('?');
			parameters = parameters.Substring(start + 1, (parameters.Length - start) - 1);
			var host = GetValue(parameters, "hostname");
			var address = GetValue(parameters, "address");
			var port = GetValue(parameters, "port");
			var version = int.Parse(GetValue(parameters, "version"));

			return new ChorusHubServerInfo(address, port, host,version);
		}

		private static string GetValue(string parameters, string name)
		{
			var queryParameters = new NameValueCollection();
			var querySegments = parameters.Split('&');
			foreach (var segment in querySegments)
			{
				var parts = segment.Split('=');
				if (parts.Length < 1)
					continue;

				var key = parts[0].Trim(new[] { '?', ' ' });
				var val = parts[1].Trim();

				queryParameters.Add(key, val);
			}

			var r = queryParameters.GetValues(name);
			return r == null ? "?" : r.First();
		}


		public static bool IsChorusHubInfo(string parameters)
		{
			return parameters.StartsWith("ChorusHubInfo");
		}

		public bool ServerIsCompatibleWithThisClient => VersionOfServerChorusHub == VersionOfThisCode;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"ChorusHubInfo?version={VersionOfThisCode}&address={_ipAddress}&port={_port}&hostname={HostName}";
		}

		public string ServiceUri => $"net.tcp://{_ipAddress}:{ChorusHubOptions.ServicePort}";

		public string GetHgHttpUri(string directoryName)
		{
			//the "chorushub" pretend user name here is to help build helpful error reports if something
			//goes wrong. The error-explainer can look at the url and know that we were trying to reach
			//a chorus hub, and give more helpful advise.
			return string.Format("http://chorushub@{0}:{1}/{2}", _ipAddress, _port, directoryName);
		}

		public static ChorusHubServerInfo FindServerInformation()
		{
			var ipEndPoint = StartFinding();
			for (var i = 0; i < 20; i++)
			{
				if (_chorusHubServerInfo != null)
					break;
				Thread.Sleep(200);
			}
			StopFinding(ipEndPoint);
			return _chorusHubServerInfo; //will be null if none found
		}

		public static UdpClient StartFinding()
		{
			var ipEndPoint = new IPEndPoint(IPAddress.Any, ChorusHubOptions.AdvertisingPort);
			var udpClient = new UdpClient();

			//This reuse business is in hopes of avoiding the dreaded "Only one usage of each socket address is normally permitted"
			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			udpClient.Client.Bind(ipEndPoint);

			udpClient.BeginReceive(ReceiveFindingCallback, new object[]
			{
				udpClient, ipEndPoint
			});

			return udpClient;
		}

		public static void StopFinding(UdpClient udpClient)
		{
			try
			{
				udpClient.Close();
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

		private static void ReceiveFindingCallback(IAsyncResult args)
		{
			Byte[] receiveBytes;
			try
			{
				var udpClient = (UdpClient)((object[])args.AsyncState)[0];
				if (udpClient.Client == null)
					return;

				var ipEndPoint = (IPEndPoint)((object[])args.AsyncState)[1];
				receiveBytes = udpClient.EndReceive(args, ref ipEndPoint);
			}
			catch (ObjectDisposedException)
			{
				//this is actually the expected behavior, if there is no chorus hub out there!
				//http://stackoverflow.com/questions/4662553/how-to-abort-sockets-beginreceive
				//note the check for Client == null above seems to help some...
				return;
			}

			try
			{
				var s = Encoding.ASCII.GetString(receiveBytes);
				if (IsChorusHubInfo(s))
				{
					_chorusHubServerInfo = Parse(s);
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
	}
}
