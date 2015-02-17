using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using SIL.Progress;
using ThreadState=System.Threading.ThreadState;

namespace Chorus.Utilities
{
	public class ProcessOutputReader
	{
		private Thread _outputReader;
		private Thread _errorReader;

		private static DateTime _heartbeat;

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

			lock(this)
			{
				_heartbeat = DateTime.Now;
			}

			//nb: at one point I (jh) tried adding !process.HasExited, but that made things less stable.
			while (/*!process.HasExited &&*/ (_outputReader.ThreadState == ThreadState.Running || (_errorReader != null && _errorReader.ThreadState == ThreadState.Running)))
			{
				DateTime end;
				lock (this)
				{
					end = _heartbeat.AddSeconds(secondsBeforeTimeOut);
				}
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

		private void ReadStream(object args)
		{
			var result = new StringBuilder();
			var readerArgs = args as ReaderArgs;

			var reader = readerArgs.Reader;
			do
			{
			   var s = reader.ReadLine();
			   if (s != null)
			   {
				   // Eat up any heartbeat lines from the stream
				   if (s != Properties.Resources.MergeHeartbeat)
				   {
					   result.AppendLine(s.Trim());
				   }
				   lock (this)
				   {
					   // set the last heartbeat if data was read from the stream
					   _heartbeat = DateTime.Now;
				   }
			  }
			} while (!reader.EndOfStream);// && !readerArgs.Proc.HasExited);

			readerArgs.Results = result.ToString().Replace("\r\n", "\n");
		}
	}

	class ReaderArgs
	{
		public StreamReader Reader;
		public Process Proc;
		public string Results;
	}
}