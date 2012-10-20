using System;
using System.Diagnostics;
using System.IO;
using System.Text;
//using System.Web;
using System.Threading;
using Chorus.VcsDrivers.Mercurial;
using Palaso.CommandLineProcessing;
using Palaso.Progress;

namespace ChorusHub
{
	public class HgServeRunner :IDisposable
	{
		public readonly int Port;
//        private Process _hgServeProcess;
		public IProgress Progress = new ConsoleProgress();
		private readonly string _rootFolder;
		private Thread _hgServeThread;

		public HgServeRunner(string rootFolder, int port)
		{
			_rootFolder = rootFolder;
			Port = port;
		}
		/// <summary>
		///
		/// </summary>
		/// <returns>false if it couldn't start</returns>
		public bool Start()
		{
			try
			{
				foreach (var hg in Process.GetProcessesByName("hg"))
				{
					Progress.WriteMessage("Killing old hg...");
					hg.Kill();
					if (!hg.WaitForExit(10000))
					{
						Progress.WriteError(
							"ChorusHub was unable to stop an old hg from running. It will now give up. You should stop the server and run it again after killing whatever 'hg.exe' process is running.");
						return false;
					}
				}


				//we make directories based on what we see in there, so start it afresh lest we re-create folder names
				//that the user long ago stopped using

				if (File.Exists(AccessLogPath))
					File.Delete(AccessLogPath);

				if (!Directory.Exists(_rootFolder))
					Directory.CreateDirectory(_rootFolder);

				Progress.WriteMessage("Starting Mercurial Server");

				WriteConfigFile(_rootFolder);

				var arguments = "serve -A accessLog.txt -E log.txt -p " + Port.ToString() + " ";

				const float kHgVersion = (float)1.5;
				if (kHgVersion < 1.9)
				{
					arguments += "--webdir-conf hgweb.config";
				}
				else
				{
					arguments += "--web-conf hgweb.config";
				}

#if CommandWindow
				_hgServeProcess = new Process();
				_hgServeProcess.StartInfo.WorkingDirectory = _rootFolder;
				_hgServeProcess.StartInfo.FileName = Chorus.MercurialLocation.PathToHgExecutable;
				_hgServeProcess.StartInfo.Arguments = arguments;
				_hgServeProcess.StartInfo.ErrorDialog = true;
				_hgServeProcess.StartInfo.UseShellExecute = false;
				_hgServeProcess.StartInfo.RedirectStandardOutput = true;
				_hgServeProcess.Start();
#else
				_hgServeThread = new Thread(() =>
												{
													var commandLineRunner = new CommandLineRunner();
													try
													{
														commandLineRunner.Start(
															Chorus.MercurialLocation.PathToHgExecutable,
															arguments,
															Encoding.UTF8, _rootFolder, -1,
															Progress, (s) => Progress.WriteMessage(s));
													}
													catch (ThreadAbortException)
													{
														Progress.WriteVerbose("Hg Serve command Thread Aborting (that's normal when stopping)");
														if(!commandLineRunner.Abort(1))
														{
															Progress.WriteWarning("Hg Serve might not have closed down.");
														}
													}
												});
				_hgServeThread.Start();
#endif

				return true;
			}
			catch (Exception error)
			{
				Progress.WriteException(error);
				return false;
			}
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
									Progress.WriteMessage("Creating new folder '" + name + "'");
									Directory.CreateDirectory(directory);
								}
								if (!Directory.Exists(Path.Combine(directory, ".hg")))
								{
									Progress.WriteMessage("Initializing blank repository: " + name +
													  ". Try Sending again in a few minutes, when hg notices the new directory.");
									HgRepository.CreateRepositoryInExistingDir(directory, Progress);
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
			catch (Exception error)
			{
				Progress.WriteException(error);
			}
		}

		public void Stop()
		{
			//if (_hgServeProcess != null && !_hgServeProcess.HasExited)
			if(_hgServeThread !=null && _hgServeThread.IsAlive)
			{
				Progress.WriteMessage("Hg Server Stopping...");
				//_hgServeProcess.Kill();
				_hgServeThread.Abort();

				if(_hgServeThread.Join(2*1000))
				{
					Progress.WriteMessage("Hg Server Stopped");
				}
				else
				{
					Progress.WriteError("***Gave up on hg server stopping");
				}
				//_hgServeProcess = null;
				_hgServeThread = null;
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
