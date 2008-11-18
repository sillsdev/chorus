//#define UseWrapShellCall

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgRunner
	{
		public static ExecutionResult Run(string commandLine)
		{
			return Run(commandLine, null);
		}

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
		 public static ExecutionResult Run(string commandLine, string fromDirectory)
		{
			ExecutionResult result = new ExecutionResult();
			Process p = new Process();
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			 p.StartInfo.WorkingDirectory = fromDirectory;
			p.StartInfo.FileName = "hg";
			p.StartInfo.Arguments = commandLine.Replace("hg ", ""); //we don't want the whole command line for this test

			try
			{p.Start();}
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
			 ProcessStream processStream = new ProcessStream();
			processStream.Read(ref p);
			p.WaitForExit();
			 result.StandardOutput = processStream.StandardOutput;
			 result.StandardError = processStream.StandardError;
			 result.ExitCode = p.ExitCode;
			return result;
		}
#endif

	}
}