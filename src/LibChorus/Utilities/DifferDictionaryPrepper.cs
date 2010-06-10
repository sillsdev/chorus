using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chorus.Utilities
{
	public class DifferDictionaryPrepper : IDisposable
	{
		private readonly byte[] m_identifierWithDoubleQuote;
		private readonly byte[] m_identifierWithSingleQuote;
		private static readonly byte s_closeDoubleQuote = Encoding.UTF8.GetBytes("\"")[0];
		private static readonly byte s_closeSingleQuote = Encoding.UTF8.GetBytes("'")[0];
		private ElementReader m_reader;
		private IDictionary<string, byte[]> m_dictionary;
		private readonly Encoding m_utf8;

		internal DifferDictionaryPrepper(IDictionary<string, byte[]> dictionary, byte[] data, string recordStartingTag, string fileClosingTag, string identifierAttribute)
		{
			m_dictionary = dictionary;
			m_reader = new ElementReader(recordStartingTag, fileClosingTag, data, PrepareIndex);
			m_utf8 = Encoding.UTF8;
			m_identifierWithDoubleQuote = m_utf8.GetBytes(identifierAttribute + "=\"");
			m_identifierWithSingleQuote = m_utf8.GetBytes(identifierAttribute + "='");
		}

		internal void Run()
		{
			m_reader.Run();
		}

		private void PrepareIndex(byte[] data)
		{
			var guid = GetAttribute(m_identifierWithDoubleQuote, s_closeDoubleQuote, data)
					   ?? GetAttribute(m_identifierWithSingleQuote, s_closeSingleQuote, data);
			m_dictionary.Add(guid, data);
		}

		private string GetAttribute(byte[] name, byte closeQuote, byte[] input)
		{
			var start = input.IndexOfSubArray(name);
			if (start == -1)
				return null;

			start += name.Length;
			var end = Array.IndexOf(input, closeQuote, start);
			return (end == -1)
					? null
					: m_utf8.GetString(input.SubArray(start, end - start));
		}

		#region Implementation of IDisposable

		~DifferDictionaryPrepper()
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