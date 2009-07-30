using System;
using System.Collections.Generic;
using System.Text;

namespace Chorus.VCS
{
	using System;
	using System.Diagnostics;
	using System.Threading;

	// from SeemabK in a comment here: http://www.hanselman.com/blog/CommentView.aspx?guid=362
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
		private static Process RunProcess;

		private string _StandardOutput = "";
		public string StandardOutput
		{
			get { return _StandardOutput; }
		}
		private string _StandardError = "";
		public string StandardError
		{
			get { return _StandardError; }
		}

		public ProcessStream()
		{
			Init();
		}

		public int Read(ref Process process)
		{
			try
			{
				Init();
				RunProcess = process;

				if (RunProcess.StartInfo.RedirectStandardOutput)
				{
					StandardOutputReader = new Thread(new ThreadStart(ReadStandardOutput));
					StandardOutputReader.Start();
				}
				if (RunProcess.StartInfo.RedirectStandardError)
				{
					StandardErrorReader = new Thread(new ThreadStart(ReadStandardError));
					StandardErrorReader.Start();
				}

				//RunProcess.WaitForExit();
				if (StandardOutputReader != null)
					StandardOutputReader.Join();
				if (StandardErrorReader != null)
					StandardErrorReader.Join();
			}
			catch
			{ }

			return 1;
		}

		private void ReadStandardOutput()
		{
			if (RunProcess != null)
				_StandardOutput = RunProcess.StandardOutput.ReadToEnd();
		}

		private void ReadStandardError()
		{
			if (RunProcess != null)
				_StandardError = RunProcess.StandardError.ReadToEnd();
		}

		private int Init()
		{
			_StandardError = "";
			_StandardOutput = "";
			RunProcess = null;
			Stop();
			return 1;
		}

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
