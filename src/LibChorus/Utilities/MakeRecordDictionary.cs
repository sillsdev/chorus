using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Palaso.Extensions;
using Palaso.Xml;

namespace Chorus.Utilities
{
	/// <summary>
	/// This Method Class processes source xml data (a whole file) and places 'records' into
	/// the given Dictionary. The client is then assumed to use the results in the
	/// dictionary.
	///
	/// A 'record' in this class means one, or more, main xml elements, each of which has the same element name.
	/// Lift data would have 'entry' to mark 'records', and FieldWorks 7.0 will use 'rt'
	/// to mark its 'records'.
	///
	/// The given ElementReader tokenizes the input xml data and uses this class' "AddKeyToIndex" delegate
	/// to add 'records' to the dictionary
	/// </summary>
	public class MakeRecordDictionary : IDisposable
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

		/// <summary>
		/// This function should return true if the Run method should continue on
		/// If this function is not provide by the client an exception will be thrown if a duplicate is encountered.
		/// </summary>
		public Func<string, bool> ShouldContinueAfterDuplicateKey;

		public MakeRecordDictionary(IDictionary<string, byte[]> dictionary, string pathname,
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

		/// <summary>
		/// Will throw if a duplicate is encountered, unless ShouldContinueAfterDuplicateKey is declared and returns false.
		/// </summary>
		public void Run()
		{
			bool foundOptionalFirstElement;
			foreach (var record in _elementSplitter.GetSecondLevelElementBytes(_firstElementTag, _recordStartingTag, out foundOptionalFirstElement))
			{
				if (foundOptionalFirstElement)
				{
					_dictionary.Add(_firstElementTag.ToLowerInvariant(), record);
					foundOptionalFirstElement = false;
				}
				else
				{
					try
					{
						AddKeyToIndex(record);
					}
					catch (Exception error)
					{
						if (ShouldContinueAfterDuplicateKey != null)
						{
							if (!ShouldContinueAfterDuplicateKey(error.Message))
							{
								throw;
							}
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		/// <summary>
		/// The ElementReader calls back to this delegate to add items to the dictionary.
		/// </summary>
		/// <param name="data"></param>
		private void AddKeyToIndex(byte[] data)
		{
			var guid = GetAttribute(_identifierWithDoubleQuote, _closeDoubleQuote, data)
				   ?? GetAttribute(_identifierWithSingleQuote, _closeSingleQuote, data);
			try
			{
				_dictionary.Add(guid, data);
			}
			catch (ArgumentException error)
			{
				throw new ArgumentException("There is more than one element with the guid " + guid, error);
			}
		}

		private string GetAttribute(byte[] name, byte closeQuote, byte[] input)
		{
			var start = input.IndexOfSubArray(name);
			if (start == -1)
				return null;

			var isWhiteSpace = IsWhitespace(input[start - 1]);
			start += name.Length;
			var end = Array.IndexOf(input, closeQuote, start);

			// Check to make sure start -1 is not another letter in a similarly named attr (e.g., id vs guid).
			if (isWhiteSpace)
			{
				return (end == -1)
						? null
						: ReplaceBasicSetOfEntitites(_utf8.GetString(input.SubArray(start, end - start)).ToLowerInvariant());
			}

			return GetAttribute(name, closeQuote, input.SubArray(end + 1, input.Length - end));
		}

		private static string ReplaceBasicSetOfEntitites(string input)
		{
			return input
				.Replace("&amp;", "&")
				.Replace("&lt;", "<")
				.Replace("&gt;", ">")
				.Replace("&quot;", "\"")
				.Replace("&apos;", "'");
		}

		private static bool IsWhitespace(byte input)
		{
			return (input == ' ' || input == '\t' || input == '\r' || input == '\n');
		}

		#region Implementation of IDisposable

		~MakeRecordDictionary()
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