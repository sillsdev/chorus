using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ThreadState=System.Threading.ThreadState;

namespace Chorus.Utilities
{
	public class ProcessOutputReader
	{
		private Thread _outputReader;
		private Thread _errorReader;

		private string _standardOutput = "";
		public string StandardOutput
		{
			get { return _standardOutput; }
		}
		private string _standardError = "";
		public const int kCancelled = 98;
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
		/// <returns>true if the process completed before the timeout or cancellation</returns>
		public bool Read(ref Process process, int secondsBeforeTimeOut, IProgress progress)
		{
			var outputReaderArgs = new ReaderArgs() {Proc = process, Reader = process.StandardOutput};
			if (process.StartInfo.RedirectStandardOutput)
			{
				_outputReader = new Thread(new ParameterizedThreadStart(ReadStream));
				_outputReader.Start(outputReaderArgs);
			}
		   var errorReaderArgs = new ReaderArgs() { Proc = process, Reader = process.StandardError };
		   if (process.StartInfo.RedirectStandardError)
			{
				_errorReader = new Thread(new ParameterizedThreadStart(ReadStream));
				_errorReader.Start(errorReaderArgs);
			}

			var end = DateTime.Now.AddSeconds(secondsBeforeTimeOut);
			while (_outputReader.ThreadState == ThreadState.Running || (_errorReader != null && _errorReader.ThreadState == ThreadState.Running))
			{
				if(progress.CancelRequested)
					return false;

				Thread.Sleep(100);
				if (DateTime.Now > end)
				{
					if (_outputReader != null)
						_outputReader.Abort();
					if (_errorReader != null)
						_errorReader.Abort();
					return false;
				}
			}
			// See http://www.wesay.org/issues/browse/WS-14948
			// The output reader threads may exit slightly prior to the application closing.
			// So we wait for the exit to be confirmed.
			process.WaitForExit(1000);
			_standardOutput = outputReaderArgs.Results;
			_standardError = errorReaderArgs.Results;

			return true;

		}

		//can't use this: that only gets called reliably if you have an application doevents going
//        void _errorReader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//        {
//            if(!e.Cancelled)
//             _standardError = e.Result as string;
//        }

		//can't use this: that only gets called reliably if you have an application doevents going
//        void _outputReader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//        {
//            if(!e.Cancelled)
//                _standardOutput = e.Result as string;
//        }

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

		private void ReadStream(object args)
		{
			StringBuilder result= new StringBuilder();
			//BackgroundWorker worker = (BackgroundWorker)sender;
			var readerArgs = args as ReaderArgs;

			var reader = readerArgs.Reader;
			do
			{
			   var s = reader.ReadLine();
			   if (s != null)
			   {
				   result.AppendLine(s.Trim());
//                   worker.ReportProgress(0, s);
			  }
			} while (!reader.EndOfStream);// && !readerArgs.Proc.HasExited);


//            if (worker.CancellationPending)
//            {
//                e.Cancel = true;
//            }
//            else
			{
				//this system doesn't work reliably if you have no UI pump: e.Result = result.ToString().Replace("\r\n", "\n");
				readerArgs.Results = result.ToString().Replace("\r\n", "\n");

			}
		}
	}

	class ReaderArgs
	{
		public StreamReader Reader;
		public Process Proc;
		public string Results;
	}
}