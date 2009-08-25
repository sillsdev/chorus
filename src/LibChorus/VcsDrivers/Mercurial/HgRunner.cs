//#define UseWrapShellCall

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Chorus.Utilities;

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
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WorkingDirectory = fromDirectory;
			process.StartInfo.FileName = "hg";
			process.StartInfo.Arguments = commandLine.Replace("hg ", ""); //we don't want the whole command line for this test

			try
			{process.Start();}
			catch(Win32Exception error)
			{
				const int ERROR_FILE_NOT_FOUND = 2;

				if (error.NativeErrorCode == ERROR_FILE_NOT_FOUND)
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
				process.Kill();
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