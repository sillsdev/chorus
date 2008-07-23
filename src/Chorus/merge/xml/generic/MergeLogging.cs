using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge.xml.generic
{
	public interface IMergeEventListener
	{
		void ConflictOccurred(IConflict conflict);
	}

	public class DispatchingMergeEventListener : IMergeEventListener
	{
		private List<IMergeEventListener> _listeners = new List<IMergeEventListener>();

		public void AddEventListener(IMergeEventListener listener)
		{
			_listeners.Add(listener);
		}

		public void ConflictOccurred(IConflict conflict)
		{
			foreach (IMergeEventListener listener in _listeners)
			{
				listener.ConflictOccurred(conflict);
			}
		}
	}

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

		public void Dispose()
		{
			_stream.Close();
		}
	}


	public class XmlLogMergeEventListener : IMergeEventListener, IDisposable
	{
		private XmlWriter _writer;

		public XmlLogMergeEventListener(string path)
		{
			_writer = XmlWriter.Create(path);
			_writer.WriteStartDocument();
			_writer.WriteStartElement("conflicts");
		}
		public void ConflictOccurred(IConflict conflict)
		{
			_writer.WriteStartElement("conflict");
			_writer.WriteAttributeString("type", string.Empty, conflict.ConflictTypeHumanName);
			_writer.WriteString(conflict.GetFullHumanReadableDescription());
			_writer.WriteEndElement();
		}

		public void Dispose()
		{
			_writer.WriteEndDocument();
			_writer.Close();
		}
	}


//    public class MergeReport : IMergeEventListener
//    {
//        private List<IConflict> _conflicts=new List<IConflict>();
//        //private string _result;
//        public void ConflictOccurred(IConflict conflict)
//        {
//            _conflicts.Add(conflict);
//        }
//    }

//    public interface IMergeReportMaker
//    {
//        MergeReport GetReport();
//    }

//    public class DefaultMergeReportMaker : IMergeReportMaker
//    {
//
//        public MergeReport GetReport()
//        {
//            return new MergeReport();
//        }
//
//    }
}