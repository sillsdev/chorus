using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Chorus.VcsDrivers.Mercurial;
using Palaso.CommandLineProcessing;
using Palaso.Progress.LogBox;

namespace ChorusHub
{
	public class ChorusHubStarter
	{
		string kPort = "8000";
		private Process _hgServeProcess;
		private ConsoleProgress _consoleProgress = new ConsoleProgress();
		const string kChorusrepositories = "C:\\ChorusHub";
		public ChorusHubStarter()
		{

		}

		public void Start()
		{
			foreach (var hg  in Process.GetProcessesByName("hg")  )
			{
				Console.WriteLine("Killing old hg...");
				hg.Kill();
				if(!hg.WaitForExit(10000))
				{
					Console.WriteLine("ChorusHub was unable to stop an old hg from running. It will now give up and exit.\r\nPlease press a key.");
					Console.ReadKey();
					Process.GetCurrentProcess().Kill();
				}
			}


			//we make directories based on what we see in there, so start it afresh lest we re-create folder names
			//that the user long ago stopped using

			if (File.Exists(AccessLogPath))
				File.Delete(AccessLogPath);

			if (!Directory.Exists(kChorusrepositories))
				Directory.CreateDirectory(kChorusrepositories);

			Console.WriteLine("Starting hg");

			WriteConfigFile(kChorusrepositories);
			_hgServeProcess = new Process();
			_hgServeProcess.StartInfo.WorkingDirectory = kChorusrepositories;
			_hgServeProcess.StartInfo.FileName = Chorus.MercurialLocation.PathToHgExecutable;

			_hgServeProcess.StartInfo.Arguments = "serve -A accessLog.txt -E log.txt -p "+kPort+" ";

			const float kHgVersion = (float) 1.5;
			if(kHgVersion < 1.9)
			{
				_hgServeProcess.StartInfo.Arguments += "--webdir-conf hgweb.config";
			}
			else
			{
				_hgServeProcess.StartInfo.Arguments += "--web-conf hgweb.config";
			}
			_hgServeProcess.StartInfo.ErrorDialog = true;

//            var result = CommandLineRunner.Run(Chorus.MercurialLocation.PathToHgExecutable,
//                                  "serve  -A accessLog.txt -E log.txt --web-conf hgweb.config",
//                                  Encoding.UTF8, kChorusrepositories, -1,
//                                  _consoleProgress, (s)=>_consoleProgress.WriteMessage(s));
//


			_hgServeProcess.StartInfo.UseShellExecute = false;
			_hgServeProcess.StartInfo.RedirectStandardOutput = true;
			 _hgServeProcess.Start();

			 Console.WriteLine("Serving at http://" + GetLocalIpAddress()+ ":"+kPort);
		}


		private static void WriteConfigFile(string kChorusrepositories)
		{
			var sb = new StringBuilder();
			sb.AppendLine("[web]");
			sb.AppendLine("allow_push = *");
			sb.AppendLine("push_ssl = No");
			sb.AppendLine();

			sb.AppendLine("[paths]");
			sb.AppendLine("/ = " + kChorusrepositories + "/*");

//            foreach (var directory in Directory.GetDirectories(kChorusrepositories))
//            {
//                string directoryName = Path.GetDirectoryName(directory);
//                sb.AppendLine(directoryName + "/ = " + directoryName+"/");
//            }

			var path = Path.Combine(kChorusrepositories, "hgweb.config");
			if (File.Exists(path))
				File.Delete(path);

			File.WriteAllText(path,sb.ToString());
		}

		protected void OnStop()
		{
			if(_hgServeProcess!=null && !_hgServeProcess.HasExited)
			{
				_hgServeProcess.Kill();
				_hgServeProcess.WaitForExit(10*1000);
			}
		}
		private string AccessLogPath { get { return Path.Combine(kChorusrepositories, "accessLog.txt"); } }

		public void CheckForFailedPushes()
		{
			try
			{
				if (!File.Exists(AccessLogPath))
					return;

				using (var stream = File.Open(AccessLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (TextReader reader = new StreamReader(stream))
				{
					while (true)
					{
						try
						{
							var line = reader.ReadLine();
							if (line == null)
								return;

							var start = line.IndexOf("GET /") + 5;
							var end = line.IndexOf("?");
							if (line.Contains("404") && start > 9 & end > 0)
							{
								var name = line.Substring(start, end - start);
								string directory = Path.Combine(kChorusrepositories, name);
								directory = HttpUtility.UrlDecode(directory);//convert %20 --> space
								if (!Directory.Exists(directory))
								{
									Console.WriteLine("Creating new folder '" + name+"'");
									Directory.CreateDirectory(directory);
								}
								if (!Directory.Exists(Path.Combine(directory, ".hg")))
								{
									Console.WriteLine("Initializing blank repository: " + name +
													  ". Try Sending again in a few minutes, when hg notices the new directory.");
									HgRepository.CreateRepositoryInExistingDir(directory, _consoleProgress);
								}
							}
						}
						catch (Exception)
						{
							throw;
						}
					}
				}
			}
			catch (Exception)
			{
				//swallow
			}
		}

		public void Stop()
		{

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
					if(localIP!=null)
					{
						if (host.AddressList.Length>1)
							Console.WriteLine("Warning: this machine has more than one IP address");
					}
					localIP = ip.ToString();
				}
			}
			return localIP ?? "Could not determine IP Address!";
		}
	}
}
