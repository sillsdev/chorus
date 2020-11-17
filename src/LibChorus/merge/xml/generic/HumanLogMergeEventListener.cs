using System;
using System.IO;

namespace Chorus.merge.xml.generic
{
	public class HumanLogMergeEventListener : IMergeEventListener, IDisposable
	{
		private StreamWriter _stream;

		public HumanLogMergeEventListener(string path)
		{
			_stream = File.CreateText(path);
		}
		/// <summary>
		/// Historically, this class's implementation of ConflictOccurred (before it was split into two
		/// interface members) did not push any context.
		/// To keep the behavior the same, RecordContextInConflict does nothing.
		/// </summary>
		/// <param name="conflict"></param>
		public void RecordContextInConflict(IConflict conflict)
		{
		}
		public void ConflictOccurred(IConflict conflict)
		{
			_stream.WriteLine(conflict.GetFullHumanReadableDescription());
		}

		public void WarningOccurred(IConflict warning)
		{
			_stream.WriteLine($"warning: {warning.GetFullHumanReadableDescription()}");
		}

		public void ChangeOccurred(IChangeReport change)
		{
			_stream.WriteLine(change.ToString());
		}

		public void EnteringContext(ContextDescriptor context)
		{

		}

		public void Dispose()
		{
			_stream.Close();
		}
	}
}