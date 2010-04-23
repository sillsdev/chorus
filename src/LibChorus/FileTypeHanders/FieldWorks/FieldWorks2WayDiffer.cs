using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.generic.xmldiff;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Class that handles diffing of two versions for FieldWorks 7.0 xml data.
	/// </summary>
	public class FieldWorks2WayDiffer
	{
		private readonly IMergeEventListener m_eventListener;
		private readonly FileInRevision m_parentFileInRevision;
		private readonly FileInRevision m_childFileInRevision;
		private byte[] m_parentBytes;
		private byte[] m_childBytes;

		public static FieldWorks2WayDiffer CreateFromFileInRevision(FileInRevision parent, FileInRevision child, IMergeEventListener changeAndConflictAccumulator, HgRepository repository)
		{
			return new FieldWorks2WayDiffer(parent.GetFileContentsAsBytes(repository), child.GetFileContentsAsBytes(repository), changeAndConflictAccumulator, parent, child);
		}
		/// <summary>Used by unit tests only.</summary>
		public static FieldWorks2WayDiffer CreateFromStrings(string parentXml, string childXml, IMergeEventListener eventListener)
		{
			var enc = Encoding.UTF8;
			return new FieldWorks2WayDiffer(enc.GetBytes(parentXml), enc.GetBytes(childXml), eventListener, null, null);
		}

		private FieldWorks2WayDiffer(byte[] parentBytes, byte[] childBytes, IMergeEventListener eventListener, FileInRevision parent, FileInRevision child)
		{
			m_parentFileInRevision = parent;
			m_childFileInRevision = child;
			m_parentBytes = parentBytes;
			m_childBytes = childBytes;
			m_eventListener = eventListener;
		}

		public void ReportDifferencesToListener()
		{
			// This arbitrary length (400) is based on two large databases,
			// one 360M with 474 bytes/object, and one 180M with 541.
			// It's probably not perfect, but we're mainly trying to prevent
			// fragmenting the large object heap by growing it MANY times.
			const int estimatedObjectCount = 400;
			var parentIndex = new Dictionary<string, byte[]>(m_parentBytes.Length / estimatedObjectCount);
			using (var prepper = new FieldWorksDictionaryPrepper(parentIndex, m_parentBytes))
			{
				prepper.Run();
			}
			m_parentBytes = null;
			var childIndex = new Dictionary<string, byte[]>(m_childBytes.Length / estimatedObjectCount);
			using (var prepper = new FieldWorksDictionaryPrepper(childIndex, m_childBytes))
			{
				prepper.Run();
			}
			m_childBytes = null;

			var enc = Encoding.UTF8;
			var parentDoc = new XmlDocument();
			var childDoc = new XmlDocument();
			foreach (var kvpParent in parentIndex)
			{
				var parentKey = kvpParent.Key;
				var parentValue = kvpParent.Value;
				byte[] childValue;
				if (childIndex.TryGetValue(parentKey, out childValue))
				{
					childIndex.Remove(parentKey);
					if (parentValue.Length == childValue.Length)
					{
						if (!parentValue.Where((t, i) => t != childValue[i]).Any())
							continue;
					}

					var parentStr = enc.GetString(parentValue);
					var childStr = enc.GetString(childValue);
					var parentInput = new XmlInput(parentStr);
					var childInput = new XmlInput(childStr);
					if (XmlUtilities.AreXmlElementsEqual(childInput, parentInput))
						continue;

					m_eventListener.ChangeOccurred(new XmlChangedRecordReport(
													m_parentFileInRevision,
													m_childFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
													XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));
				}
				else
				{
					m_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
													m_parentFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(kvpParent.Value), parentDoc),
													null)); // Child Node? How can we put it in, if it was deleted?
				}
			}
			foreach (var child in childIndex.Values)
			{
				m_eventListener.ChangeOccurred(new XmlAdditionChangeReport(
												m_childFileInRevision,
												XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(child), childDoc)));
			}
		}

		private class FieldWorksDictionaryPrepper : IDisposable
		{
			private static readonly byte[] s_guidDoubleQuote = Encoding.UTF8.GetBytes("guid=\"");
			private static readonly byte[] s_guidSingleQuote = Encoding.UTF8.GetBytes("guid='");
			private static readonly byte s_closeDoubleQuote = Encoding.UTF8.GetBytes("\"")[0];
			private static readonly byte s_closeSingleQuote = Encoding.UTF8.GetBytes("'")[0];
			private ElementReader m_reader;
			private IDictionary<string, byte[]> m_dictionary;

			internal FieldWorksDictionaryPrepper(IDictionary<string, byte[]> dictionary, byte[] data)
			{
				m_dictionary = dictionary;
				m_reader = new ElementReader("<rt ", "</languageproject>", data, PrepareIndex);
			}

			internal void Run()
			{
				m_reader.Run();
			}

			private void PrepareIndex(byte[] fwData)
			{
				var guid = GetAttribute(s_guidDoubleQuote, s_closeDoubleQuote, fwData) ??
						   GetAttribute(s_guidSingleQuote, s_closeSingleQuote, fwData);
				m_dictionary.Add(guid, fwData);
			}

			static string GetAttribute(byte[] name, byte closeQuote, byte[] input)
			{
				var start = input.IndexOfSubArray(name);
				if (start == -1)
					return null;

				start += name.Length;
				var end = Array.IndexOf(input, closeQuote, start);
				return end == -1
					? null
					: Encoding.UTF8.GetString(input.SubArray(start, end - start));
			}

			#region Implementation of IDisposable

			~FieldWorksDictionaryPrepper()
			{
				Debug.WriteLine("**** FieldWorksDictionaryPrepper.Finalizer called ****");
				Dispose(false);
				// The base class finalizer is called automatically.
			}

			public void Dispose()
			{
				Dispose(true);
				// This object will be cleaned up by the Dispose method.
				// Therefore, you should call GC.SupressFinalize to
				// take this object off the finalization queue
				// and prevent finalization code for this object
				// from executing a second time.
				GC.SuppressFinalize(this);
			}

			private bool m_isDisposed;
			private void Dispose(bool disposing)
			{
				if (m_isDisposed)
					return; // Done already, so nothing left to do.

				if (disposing)
				{
					if (m_reader != null)
						m_reader.Dispose();
				}

				// Dispose unmanaged resources here, whether disposing is true or false.

				// Main data members.
				m_reader = null;
				m_dictionary = null;

				m_isDisposed = true;
			}

			#endregion
		}
	}
}
