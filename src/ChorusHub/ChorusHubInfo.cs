using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Chorus.Utilities;

namespace ChorusHub
{
	/// <summary>
	/// Just serializes/deserializes location of the server
	/// </summary>
	public class ChorusHubInfo
	{
		private readonly string IpAddress;
		private readonly string Port;
		public string HostName;


		public static ChorusHubInfo Parse(string parameters)
		{
			int start = parameters.IndexOf('?');
			parameters = parameters.Substring(start + 1, (parameters.Length - start) - 1);
			var host = GetValue(parameters, "hostname");
			var address = GetValue(parameters, "address");
			var port = GetValue(parameters, "port");

			return new ChorusHubInfo(address, port, host);
		}

		private static string GetValue(string parameters, string name)
		{
			NameValueCollection queryParameters = new NameValueCollection();
			string[] querySegments = parameters.Split('&');
			foreach (string segment in querySegments)
			{
				string[] parts = segment.Split('=');
				if (parts.Length > 0)
				{
					string key = parts[0].Trim(new char[] { '?', ' ' });
					string val = parts[1].Trim();

					queryParameters.Add(key, val);
				}
			}

			var r = queryParameters.GetValues(name);
			return r == null ? "?" : r.First();
		}


		public static bool IsChorusHubInfo(string parameters)
		{
			return parameters.StartsWith("ChorusHubInfo");
		}

		public ChorusHubInfo(string ipAddress, string port, string hostName)
		{
			IpAddress = ipAddress;
			Port = port;
			HostName = hostName;
		}

		public override string ToString()
		{
			return string.Format("ChorusHubInfo?version=1&address={0}&port={1}&hostname={2}", IpAddress, Port, HostName);
		}

		public string ServiceUri
		{
			get { return string.Format("net.tcp://{0}:{1}", IpAddress, ChorusHubService.ServicePort); }
		}

		public string GetHgHttpUri(string directoryName)
		{

			//the "chorushub" pretend user name here is to help build helpful error reports if somethig
			//goes wrong. The error-explainer can look at the url and know that we were trying to reach
			//a chorus hub, and give more helpful advise.
			return string.Format("http://chorushub@{0}:{1}/{2}", IpAddress, Port, directoryName);
		}
	}
}
