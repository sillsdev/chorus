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
		public void ConflictOccurred(IConflict conflict)
		{
			_stream.WriteLine(conflict.GetFullHumanReadableDescription());
		}

		public void ChangeOccurred(IChangeReport change)
		{
			_stream.WriteLine(change.ToString());
		}

		public void EnteringContext(string context)
		{

		}

		public void Dispose()
		{
			_stream.Close();
		}
	}
}