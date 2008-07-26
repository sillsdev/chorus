using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge.xml.generic
{
	public interface IMergeEventListener
	{
		void ConflictOccurred(IConflict conflict);
	}

	public class NullMergeEventListener : IMergeEventListener
	{
		public void ConflictOccurred(IConflict conflict)
		{

		}
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
		private bool _modifyingExistingFile;
		private XmlDocument _xmlDoc;
		private string _path;

		static public string GetXmlConflictFilePath(string baseXmlFile)
		{
			return baseXmlFile + ".conflicts.xml";
		}

		public XmlLogMergeEventListener(string path)
		{
			_path = path;

			try
			{
				if (!File.Exists(path))
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml("<conflicts/>");
					doc.Save(path);
				 }
			}
			catch (Exception error)
			{
				Debug.Fail("Something went wrong trying to create a blank onflict file :"+error.Message);
				//todo log that the xml was the wrong format
			}

			_xmlDoc = new XmlDocument();
			_xmlDoc.Load(path);
			_writer = _xmlDoc.CreateNavigator().SelectSingleNode("conflicts").AppendChild();
					this._modifyingExistingFile = true;

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
			_writer.Close();
			_xmlDoc.Save(_path);
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