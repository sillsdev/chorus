using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Chorus.Utilities
{
	public class ProcessOutputReader
	{
		private BackgroundWorker _outputReader;
		private BackgroundWorker _errorReader;

		private string _standardOutput = "";
		public string StandardOutput
		{
			get { return _standardOutput; }
		}
		private string _standardError = "";
		public const int kTimedOut = 99;

		public string StandardError
		{
			get { return _standardError; }
		}

		/// <summary>
		/// Safely read the streams of the process
		/// </summary>
		/// <param name="process"></param>
		/// <param name="secondsBeforeTimeOut"></param>
		/// <returns>true if the process completed before the timeout</returns>
		public bool Read(ref Process process, int secondsBeforeTimeOut)
		{
			if (process.StartInfo.RedirectStandardOutput)
			{
				_outputReader = new BackgroundWorker();
				_outputReader.WorkerReportsProgress = true;
				_outputReader.WorkerSupportsCancellation = true;
				_outputReader.DoWork += ReadStream;
				_outputReader.ProgressChanged += OnOutputReaderProgressChanged;
				var args = new ReaderArgs() {Proc = process, Reader = process.StandardOutput};
				_outputReader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_outputReader_RunWorkerCompleted);
				_outputReader.RunWorkerAsync(args);
			}
			if (process.StartInfo.RedirectStandardError)
			{
				_errorReader = new BackgroundWorker();
				_errorReader.WorkerReportsProgress = true;
				_errorReader.WorkerSupportsCancellation = true;
				_errorReader.DoWork += ReadStream;
				_errorReader.ProgressChanged += OnErrorReaderProgressChanged;
				var args = new ReaderArgs() { Proc = process, Reader = process.StandardError };
				_errorReader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_errorReader_RunWorkerCompleted);
				_errorReader.RunWorkerAsync(args);
			}

			var end = DateTime.Now.AddSeconds(secondsBeforeTimeOut);
			while (_outputReader.IsBusy || (_errorReader != null && _errorReader.IsBusy))
			{
				Thread.Sleep(100);
				if (DateTime.Now > end)
				{
					if(_outputReader!=null)         //review: do these matter?
						_outputReader.CancelAsync();
					if (_errorReader != null)
						_errorReader.CancelAsync();
					return false;
				}
			}


			return true;

		}

		void _errorReader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if(!e.Cancelled)
			 _standardError = e.Result as string;
		}

		void _outputReader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if(!e.Cancelled)
				_standardOutput = e.Result as string;
		}

		void OnOutputReaderProgressChanged(object sender, ProgressChangedEventArgs e)
		{
//            if(e.UserState ==null)
//                return;
//            if (!string.IsNullOrEmpty(_standardOutput))
			//                _standardOutput += "\n";
//            _standardOutput += ((string)(e.UserState)).Trim();
		}
		void OnErrorReaderProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			/*THESE TURNED OUT TO BE UNRELIABLE... now I'm just using the result
				when I get back to look at this again... maybe this class was returning
				before the last one of these was actually process?  Or something like that?
			 *
			 * Eventually, we want something giving progress real-time, rather than at the end,
			 * so we'll have to figure this out
			 */

//            if(e.UserState ==null)
//                return;
//            if (!string.IsNullOrEmpty(_standardError))
//                _standardError += "\n";
//            _standardError += ((string)(e.UserState)).Trim();
		}

		private void ReadStream(object sender, DoWorkEventArgs e)
		{
			StringBuilder result= new StringBuilder();
			BackgroundWorker worker = (BackgroundWorker)sender;
			var readerArgs = e.Argument as ReaderArgs;

			var reader = readerArgs.Reader;
			do
			{
			   var s = reader.ReadLine();
			   if (s != null)
			   {
				   result.AppendLine(s.Trim());
//                   worker.ReportProgress(0, s);
			  }
			} while (!worker.CancellationPending && !reader.EndOfStream);// && !readerArgs.Proc.HasExited);


			if (worker.CancellationPending)
			{
				e.Cancel = true;
			}
			else
			{
				e.Result = result.ToString().Replace("\r\n", "\n");
			}
		}
	}

	class ReaderArgs
	{
		public StreamReader Reader;
		public Process Proc;
	}
}