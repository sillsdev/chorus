using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using Chorus.VcsDrivers.Mercurial;
using SIL.CommandLineProcessing;
using SIL.Progress;

namespace ChorusHub
{
	public class HgServeRunner : IDisposable
	{
		public readonly int Port;
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
					//EventLog.WriteEntry("Application", "Killing old hg...", EventLogEntryType.Information);
					hg.Kill();
					if (!hg.WaitForExit(10000))
					{
						//EventLog.WriteEntry("Application", "ChorusHub was unable to stop an old hg from running. It will now give up. You should stop the server and run it again after killing whatever 'hg.exe' process is running.", EventLogEntryType.Error);
						return false;
					}
				}


				//we make directories based on what we see in there, so start it afresh lest we re-create folder names
				//that the user long ago stopped using

				if (File.Exists(AccessLogPath))
					File.Delete(AccessLogPath);

				if (!Directory.Exists(_rootFolder))
					Directory.CreateDirectory(_rootFolder);

				//EventLog.WriteEntry("Application", "Starting Mercurial Server", EventLogEntryType.Information);

				WriteConfigFile(_rootFolder);

				var arguments = "serve -A accessLog.txt -E log.txt -p " + Port + " --verbose ";

				//const float kHgVersion = (float)1.5;
				//if (kHgVersion < 1.9)
				//{
					arguments += "--webdir-conf hgweb.config";
				//}
				//else
				//{
				//	arguments += "--web-conf hgweb.config";
				//}

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
														var progress = new ConsoleProgress();
														commandLineRunner.Start(
															Chorus.MercurialLocation.PathToHgExecutable,
															arguments,
															Encoding.UTF8, _rootFolder, -1,
															progress, s => progress.WriteMessage(s));
													}
													catch (ThreadAbortException)
													{
														//Progress.WriteVerbose("Hg Serve command Thread Aborting (that's normal when stopping)");
                                                        try
                                                        {
                                                            if (!commandLineRunner.Abort(1))
                                                            {
                                                                //EventLog.WriteEntry("Application", "Hg Serve might not have closed down.", EventLogEntryType.Information);
                                                            }
                                                        }
                                                        catch { }

													}
												});
				_hgServeThread.Start();
#endif

				return true;
			}
			catch
			{
				//EventLog.WriteEntry("Application", error.Message, EventLogEntryType.Error);
				return false;
			}
		}


		private static void WriteConfigFile(string rootFolder)
		{
			var sb = new StringBuilder();
			sb.AppendLine("[web]");
			sb.AppendLine("allow_push = *");
			sb.AppendLine("push_ssl = No");
			sb.AppendLine();

			sb.AppendLine("[paths]");
			sb.AppendLine("/ = " + rootFolder + "/*");

			var path = Path.Combine(rootFolder, "hgweb.config");
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
						var line = reader.ReadLine();
						if (line == null)
							return;

						var start = line.IndexOf("GET /") + 5;
						var end = line.IndexOf("?");
						if (line.Contains("404") && start > 9 & end > 0)
						{
							var name = line.Substring(start, end - start);
							string directory = Path.Combine(_rootFolder, name);

							directory = HttpUtility.UrlDecode(directory); // convert %20 --> space
							if (!Directory.Exists(directory))
							{
								//Progress.WriteMessage("Creating new folder '" + name + "'");
								Directory.CreateDirectory(directory);
							}
							if (!Directory.Exists(Path.Combine(directory, ".hg")))
							{
								//Progress.WriteMessage("Initializing blank repository: " + name +
								//				  ". Try Sending again in a few minutes, when hg notices the new directory.");
								HgRepository.CreateRepositoryInExistingDir(directory, new ConsoleProgress());
							}
						}
					}
				}
			}
			catch
			{
				//EventLog.WriteEntry("Application", error.Message, EventLogEntryType.Error);
			}
		}

		public void Stop()
		{
			if(_hgServeThread !=null && _hgServeThread.IsAlive)
			{
				//Progress.WriteMessage("Hg Server Stopping...");
				_hgServeThread.Abort();

				if(_hgServeThread.Join(2 * 1000))
				{
					//Progress.WriteMessage("Hg Server Stopped");
				}
				else
				{
					//Progress.WriteError("***Gave up on hg server stopping");
				}
				_hgServeThread = null;
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
