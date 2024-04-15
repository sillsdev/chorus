using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Chorus.merge.xml.generic;
using SIL.Progress;
using ThreadState=System.Threading.ThreadState;

namespace Chorus.Utilities
{
	/// <summary>
	/// This class interacts with the output (and possibly input) when we launch an hg process
	/// </summary>
	public class HgProcessOutputReader
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
		private string _rootDirectory;

		public HgProcessOutputReader(string fromDirectory)
		{
			_rootDirectory = fromDirectory;
		}

		public const int kCancelled = 98;
		public const int kTimedOut = 99;
		/// <summary>
		/// For backward compatibility we need to enable the dotencode extension even though it no longer exists in Mercurial 3.
		/// We will swallow all warnings about having it enabled in the current version to avoid test failures and user alarm.
		/// </summary>
		private const string DotEncodeWarning = @"*** failed to import extension dotencode: No module named dotencode";

		public string StandardError
		{
			get { return _standardError; }
		}

		/// <summary>
		/// Safely read the streams of the process, must redirect stdErr, out and in before calling
		/// </summary>
		/// <returns>true if the process completed before the timeout or cancellation</returns>
		public bool Read(ref Process process, int secondsBeforeTimeOut, IProgress progress)
		{
			var cts = new CancellationTokenSource();
			var outputReaderArgs = new ReaderArgs(process, process.StandardOutput, cts.Token);
			_outputReader = new Thread(ReadStream);
			_outputReader.Start(outputReaderArgs);


			var errorReaderArgs = new ReaderArgs(process, process.StandardError, cts.Token);
			_errorReader = new Thread(ReadStream);
			_errorReader.Start(errorReaderArgs);


			lock(this)
			{
				_heartbeat = DateTime.Now;
			}

			//nb: at one point I (jh) tried adding !process.HasExited, but that made things less stable.
			while ( _outputReader.ThreadState != ThreadState.Stopped && _errorReader.ThreadState != ThreadState.Stopped)
			{
				DateTime end;
				lock (this)
				{
					end = _heartbeat.AddSeconds(secondsBeforeTimeOut);
				}

				if (progress.CancelRequested)
				{
					cts.Cancel();
					return false;
				}

				Thread.Sleep(100);
				if (DateTime.Now > end)
				{
					cts.Cancel();
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

		/// <summary>
		/// This method will detect changed vs deleted file conflicts in the output coming from mercurial.
		/// It produces a conflict for that situation and tells mercurial to keep the changed file.
		/// <note>The default response for mercurial is to keep the changed, but on some user systems (Windows 8?)
		/// mercurial will pause waiting for input instead of using the default answer.</note>
		/// </summary>
		private bool HandleChangedVsDeletedFiles(string line, StreamWriter standardInput)
		{
			// The two situations we are dealing with are:
			//
			// local changed [filename] which remote deleted
			// use (c)hanged version or (d)elete?
			//		and
			// remote changed [filename] which local deleted
			// use (c)hanged version or leave (d)eleted?

			string changedVsDeletedFile = null;
			var match = Regex.Match(line, @"file '(.*)' was deleted in other \[merge rev\] but was modified in local \[working copy\]");
			if(match.Captures.Count > 0)
			{
				changedVsDeletedFile = match.Groups[1].Value;
			}
			else
			{
				match = Regex.Match(line, @"file '(.*)' was deleted in local \[working copy\] but was modified in other \[merge rev\]");
				if(match.Captures.Count > 0)
				{
					changedVsDeletedFile = match.Groups[1].Value;
				}
			}
			if(changedVsDeletedFile != null)
			{
				var conflictPath = Path.Combine(_rootDirectory, changedVsDeletedFile);
				var conflictReport = new FileChangedVsFileDeletedConflict(conflictPath);
				using(var chorusNoteCreator = new ChorusNotesMergeEventListener(ChorusNotesMergeEventListener.GetChorusNotesFilePath(conflictPath)))
				{
					chorusNoteCreator.ConflictOccurred(conflictReport);
				}
				return true;
			}
			// Send hg the response it might be waiting for:
			if(line.Contains(@"(c)hanged") && line.Contains(@"(d)elete"))
			{
				standardInput.WriteLine('c');
				return true;
			}
			// This input line was not related to a Changed vs Deleted File situation
			return false;
		}

		private void ReadStream(object args)
		{
			var result = new StringBuilder();
			var readerArgs = (ReaderArgs)args;
			try
			{
				var reader = readerArgs.Reader;
				do
				{
					var s = reader.ReadLineAsync(readerArgs.Token).Result;
					if (s == null) return; //cancelled
					// Eat up any heartbeat lines from the stream, also remove warnings about dotencode
					if (s != Properties.Resources.MergeHeartbeat
						&& s != DotEncodeWarning
						&& !HandleChangedVsDeletedFiles(s, readerArgs.Proc.StandardInput))
					{
						result.AppendLine(s.Trim());
					}

					lock (this)
					{
						// set the last heartbeat if data was read from the stream
						_heartbeat = DateTime.Now;
					}
				} while (!reader.EndOfStream && !readerArgs.Token.IsCancellationRequested); // && !readerArgs.Proc.HasExited);
			}
			finally
			{
				readerArgs.Results = result.ToString().Replace("\r\n", "\n");
			}
		}
	}

	internal class ReaderArgs
	{
		public ReaderArgs(Process proc, StreamReader reader, CancellationToken token)
		{
			Token = token;
			Reader = reader;
			Proc = proc;
		}

		public CancellationToken Token;
		public StreamReader Reader;
		public Process Proc;
		public string Results;
	}
}