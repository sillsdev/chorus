//#define UseWrapShellCall

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;
using SIL.Progress;
using SIL.Reporting;

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
			if (String.IsNullOrEmpty(MercurialLocation.PathToMercurialFolder))
			{
				throw new ApplicationException("Mercurial location has not been configured.");
			}
			process.StartInfo.EnvironmentVariables["PYTHONPATH"] = Path.Combine(MercurialLocation.PathToMercurialFolder, "library.zip");
			process.StartInfo.EnvironmentVariables["HGENCODING"] = "UTF-8"; // See mercurial/encoding.py
			process.StartInfo.EnvironmentVariables["HGENCODINGMODE"] = "strict";
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WorkingDirectory = fromDirectory;
			process.StartInfo.FileName = MercurialLocation.PathToHgExecutable;

			var debug = Environment.GetEnvironmentVariable(@"CHORUSDEBUGGING") == null ? String.Empty : @"--debug ";
			process.StartInfo.Arguments = commandLine.Replace("hg ", debug); //we don't want the whole command line, just the args portion

			//The fixutf8 extension's job is to get hg to talk in this encoding
			process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
			process.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
			if(!String.IsNullOrEmpty(debug))
			{
				Logger.WriteEvent("Running hg command: hg --debug {0}", commandLine);
			}
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
			var processReader = new HgProcessOutputReader(fromDirectory);
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
						progress.WriteWarning("Killing Hg Process...");
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