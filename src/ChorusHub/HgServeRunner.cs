using System;
using System.Diagnostics;
using System.IO;
using System.Text;
//using System.Web;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace ChorusHub
{
	public class HgServeRunner :IDisposable
	{
		public static string Port = "8000";
		private Process _hgServeProcess;
		private ConsoleProgress _consoleProgress = new ConsoleProgress();
		private string _rootFolder;

		public HgServeRunner(string rootFolder)
		{
			_rootFolder = rootFolder;
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

			if (!Directory.Exists(_rootFolder))
				Directory.CreateDirectory(_rootFolder);

			Console.WriteLine("Starting hg");

			WriteConfigFile(_rootFolder);
			_hgServeProcess = new Process();
			_hgServeProcess.StartInfo.WorkingDirectory = _rootFolder;
			_hgServeProcess.StartInfo.FileName = Chorus.MercurialLocation.PathToHgExecutable;

			_hgServeProcess.StartInfo.Arguments = "serve -A accessLog.txt -E log.txt -p "+Port+" ";

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
//                                  Encoding.UTF8, _rootFolder, -1,
//                                  _consoleProgress, (s)=>_consoleProgress.WriteMessage(s));
//


			_hgServeProcess.StartInfo.UseShellExecute = false;
			_hgServeProcess.StartInfo.RedirectStandardOutput = true;
			 _hgServeProcess.Start();
		}


		private static void WriteConfigFile(string _rootFolder)
		{
			var sb = new StringBuilder();
			sb.AppendLine("[web]");
			sb.AppendLine("allow_push = *");
			sb.AppendLine("push_ssl = No");
			sb.AppendLine();

			sb.AppendLine("[paths]");
			sb.AppendLine("/ = " + _rootFolder + "/*");

//            foreach (var directory in Directory.GetDirectories(_rootFolder))
//            {
//                string directoryName = Path.GetDirectoryName(directory);
//                sb.AppendLine(directoryName + "/ = " + directoryName+"/");
//            }

			var path = Path.Combine(_rootFolder, "hgweb.config");
			if (File.Exists(path))
				File.Delete(path);

			File.WriteAllText(path,sb.ToString());
		}

		private string AccessLogPath { get { return Path.Combine(_rootFolder, "accessLog.txt"); } }

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
								string directory = Path.Combine(_rootFolder, name);

								//requires full .net framework: directory = HttpUtility.UrlDecode(directory);//convert %20 --> space
								directory = Palaso.Network.HttpUtilityFromMono.UrlDecode(directory);
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
			if (_hgServeProcess != null && !_hgServeProcess.HasExited)
			{
				Debug.WriteLine("Hg Server Stopping...");
				_hgServeProcess.Kill();
				if(_hgServeProcess.WaitForExit(1 * 1000))
				{
					Debug.WriteLine("Hg Server Stopped");
				}
				else
				{
					Debug.WriteLine("***Gave up on hg server stopping");
				}
				_hgServeProcess = null;
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
