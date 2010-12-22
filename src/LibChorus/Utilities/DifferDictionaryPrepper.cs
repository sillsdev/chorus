using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chorus.Utilities
{
	/// <summary>
	/// This class processes source xml data (a whole file) and places 'records' into
	/// the given Dictionary. The client is then assumed to use the results in the
	/// dictionary.
	///
	/// A 'record' in this class means one, or more, main xml elements, each of which has the same element name.
	/// Lift data would have 'entry' to mark 'records', and FieldWorks 7.0 will use 'rt'
	/// to mark its 'records'.
	///
	/// The given ElementReader tokenizes the input xml data and uses this class' "PrepareIndex" delegate
	/// to add 'records' to the dictionary
	/// </summary>
	public class DifferDictionaryPrepper : IDisposable
	{
		private readonly byte[] _identifierWithDoubleQuote;
		private readonly byte[] _identifierWithSingleQuote;
		private static readonly byte _closeDoubleQuote = Encoding.UTF8.GetBytes("\"")[0];
		private static readonly byte _closeSingleQuote = Encoding.UTF8.GetBytes("'")[0];
		private FastXmlElementSplitter _elementSplitter;
		private IDictionary<string, byte[]> _dictionary;
		private readonly string _firstElementTag;
		private readonly string _recordStartingTag;
		private readonly Encoding _utf8;

		internal DifferDictionaryPrepper(IDictionary<string, byte[]> dictionary, string pathname,
			string firstElementMarker,
			string recordStartingTag, string identifierAttribute)
		{
			_dictionary = dictionary;
			_firstElementTag = firstElementMarker; // May be null, which is fine.
			_recordStartingTag = recordStartingTag;
			_elementSplitter = new FastXmlElementSplitter(pathname);
			_utf8 = Encoding.UTF8;
			_identifierWithDoubleQuote = _utf8.GetBytes(identifierAttribute + "=\"");
			_identifierWithSingleQuote = _utf8.GetBytes(identifierAttribute + "='");
		}

		internal void Run()
		{
			bool foundOptionalFirstElement;
			foreach (var record in _elementSplitter.GetSecondLevelElementBytes(_firstElementTag, _recordStartingTag, out foundOptionalFirstElement))
			{
				if (foundOptionalFirstElement)
				{
					_dictionary.Add(_firstElementTag, record);
					foundOptionalFirstElement = false;
				}
				else
				{
					PrepareIndex(record);
				}
			}
		}

		/// <summary>
		/// The ElementReader calls back to this delegate to add items to the dictionary.
		/// </summary>
		/// <param name="data"></param>
		private void PrepareIndex(byte[] data)
		{
			var guid = GetAttribute(_identifierWithDoubleQuote, _closeDoubleQuote, data)
					   ?? GetAttribute(_identifierWithSingleQuote, _closeSingleQuote, data);
			_dictionary.Add(guid, data);
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
					: _utf8.GetString(input.SubArray(start, end - start)).ToLowerInvariant();
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

		private bool _isDisposed;
		private void Dispose(bool disposing)
		{
			if (_isDisposed)
				return; // Done already, so nothing left to do.

			if (disposing)
			{
				if (_elementSplitter != null)
					_elementSplitter.Dispose();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			// Main data members.
			_elementSplitter = null;
			_dictionary = null;

			_isDisposed = true;
		}

		#endregion
	}
}