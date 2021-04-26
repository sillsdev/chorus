using System;
using System.Diagnostics;
using System.Threading;

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

		private Thread StandardOutputReader;
		private Thread StandardErrorReader;
		private static Process _srunningProcess;

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

		public ProcessStream()
		{
			Init();
		}

		public int Read(ref Process process, int secondsBeforeTimeOut)
		{
//            try
//            {
				Init();
				_srunningProcess = process;

				if (_srunningProcess.StartInfo.RedirectStandardOutput)
				{
					StandardOutputReader = new Thread(new ThreadStart(ReadStandardOutput));
					StandardOutputReader.Start();
				}
				if (_srunningProcess.StartInfo.RedirectStandardError)
				{
					StandardErrorReader = new Thread(new ThreadStart(ReadStandardError));
					StandardErrorReader.Start();
				}

				//_srunningProcess.WaitForExit();
				if (StandardOutputReader != null)
				{
					if (!StandardOutputReader.Join(new TimeSpan(0, 0, 0, secondsBeforeTimeOut)))
					{
						return kTimedOut;
					}
				}
				if (StandardErrorReader != null)
				{
					if (!StandardErrorReader.Join(new TimeSpan(0, 0, 0, secondsBeforeTimeOut)))
					{
						return kTimedOut;
					}
				}
//            }
//            catch
//            { }

			return 1;
		}

		private void ReadStandardOutput()
		{
			if (_srunningProcess != null)
				_standardOutput = _srunningProcess.StandardOutput.ReadToEnd();
		}

		private void ReadStandardError()
		{
			if (_srunningProcess != null)
				_standardError = _srunningProcess.StandardError.ReadToEnd();
		}

		private int Init()
		{
			_standardError = "";
			_standardOutput = "";
			_srunningProcess = null;
			Stop();
			return 1;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public int Stop()
		{
			try { StandardOutputReader.Abort(); }
			catch { }
			try { StandardErrorReader.Abort(); }
			catch { }
			StandardOutputReader = null;
			StandardErrorReader = null;
			return 1;
		}
	}
}