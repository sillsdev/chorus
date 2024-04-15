using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Chorus.Utilities
{
	/// <summary>
	/// This is class originally from  SeemabK (seemabk@yahoo.com).  It has been enhanced for chorus.
	/// </summary>
	public class ProcessStream
	{
		/*
		* Class to get process stdout/stderr streams
		* Author: SeemabK (seemabk@yahoo.com)
		* Usage:
		//create ProcessStream
		ProcessStream myProcessStream = new ProcessStream();
		//create and populate Process as needed
		Process myProcess = new Process();
		myProcess.StartInfo.FileName = "myexec.exe";
		myProcess.StartInfo.Arguments = "-myargs";

		//redirect stdout and/or stderr
		myProcess.StartInfo.UseShellExecute = false;
		myProcess.StartInfo.RedirectStandardOutput = true;
		myProcess.StartInfo.RedirectStandardError = true;

		//start Process
		myProcess.Start();
		//connect to ProcessStream
		myProcessStream.Read(ref myProcess);
		//wait for Process to end
		myProcess.WaitForExit();

		//get the captured output :)
		string output = myProcessStream.StandardOutput;
		string error = myProcessStream.StandardError;
		*/

		private string _standardOutput = "";
		public string StandardOutput
		{
			get { return _standardOutput; }
		}
		private string _standardError = "";
		public const int kTimedOut = 99;
		public const int kCancelled = 98;

		public string StandardError
		{
			get { return _standardError; }
		}

		public int Read(ref Process process, int secondsBeforeTimeOut)
		{
			Task<string> stdOutTask, stdErrTask;
			stdOutTask = stdErrTask = Task.FromResult(string.Empty);

			if (process.StartInfo.RedirectStandardOutput)
				stdOutTask = process.StandardOutput.ReadToEndAsync(secondsBeforeTimeOut);
			if (process.StartInfo.RedirectStandardError)
				stdErrTask = process.StandardError.ReadToEndAsync(secondsBeforeTimeOut);
			var stdOut = stdOutTask.Result;
			var stdErr = stdErrTask.Result;
			_standardOutput = stdOut ?? string.Empty;
			_standardError = stdErr ?? string.Empty;
			//null indicates the read timed out
			if (stdOut == null || stdErr == null)
			{
				return kTimedOut;
			}

			return 1;
		}
	}
}