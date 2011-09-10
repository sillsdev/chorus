//#define UseWrapShellCall

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Chorus.Utilities;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgRunner
	{
		public static int TimeoutSecondsOverrideForUnitTests = 10000;



#if UseWrapShellCall
		public static ExecutionResult Run(string commandLine, string fromDirectory)
		{
			ExecutionResult result = new ExecutionResult();
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.WorkingDirectory = fromDirectory;
			p.StartInfo.FileName = "WrapShellCall";
			using (TempFile tempOut = new TempFile())
			{
				using (TempFile tempErr = new TempFile())
				{
					p.StartInfo.Arguments = "\""+tempOut.Path +"\" " + "\""+tempErr.Path + "\" " + commandLine;
					p.StartInfo.CreateNoWindow = true;
					p.Start();
					p.WaitForExit();

					result.StandardOutput = File.ReadAllText(tempOut.Path);
					result.StandardError = File.ReadAllText(tempErr.Path);
					result.ExitCode = p.ExitCode;
				}
			}
			return result;
		}
#else

		public static ExecutionResult Run(string commandLine, string fromDirectory, int secondsBeforeTimeOut, IProgress progress)
		{
			ExecutionResult result = new ExecutionResult();
			Process process = new Process();
			process.StartInfo.EnvironmentVariables["PYTHONPATH"] = Path.Combine(MercurialLocation.PathToMercurialFolder, "library.zip");
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WorkingDirectory = fromDirectory;
			process.StartInfo.FileName = MercurialLocation.PathToHgExecutable;
			process.StartInfo.Arguments = commandLine.Replace("hg ", ""); //we don't want the whole command line, just the args portion

			try
			{
				process.Start();
			}
			catch(Win32Exception error)
			{
				const int ERROR_FILE_NOT_FOUND = 2;

				if (error.NativeErrorCode == ERROR_FILE_NOT_FOUND &&
					!commandLine.Contains("version"))//don't recurse if the readinessMessage itself is what failed
				{
					string msg = HgRepository.GetEnvironmentReadinessMessage("en");
					if(!string.IsNullOrEmpty(msg))
						throw new ApplicationException(msg);
					else
					{
						throw error;
					}
				}
				else
				{
					throw error;
				}
			}
			var processReader = new ProcessOutputReader();
			if(secondsBeforeTimeOut > TimeoutSecondsOverrideForUnitTests)
				secondsBeforeTimeOut = TimeoutSecondsOverrideForUnitTests;

			bool timedOut = false;
			if (!processReader.Read(ref process, secondsBeforeTimeOut, progress))
			{
				timedOut = !progress.CancelRequested;
				try
				{
					if (process.HasExited)
					{
						progress.WriteWarning("Process exited, cancelRequested was {0}", progress.CancelRequested);
					}
					else
					{
						progress.WriteWarning("Killing Process...");
						process.Kill();
					}
				}
				catch(Exception e)
				{
					progress.WriteWarning("Exception while killing process, as though the process reader failed to notice that the process was over: {0}", e.Message);
					progress.WriteWarning("Process.HasExited={0}", process.HasExited.ToString());
				}
			}
			result.StandardOutput = processReader.StandardOutput;
			result.StandardError = processReader.StandardError;

			if (timedOut)
			{
				result.StandardError += Environment.NewLine + "Timed Out after waiting " + secondsBeforeTimeOut + " seconds.";
				result.ExitCode = ProcessStream.kTimedOut;
			}

			else if (progress.CancelRequested)
			{
				result.StandardError += Environment.NewLine + "User Cancelled.";
				result.ExitCode = ProcessStream.kCancelled;
			}
			else
			{
				result.ExitCode = process.ExitCode;
			}
			return result;
		}
#endif

	}
}