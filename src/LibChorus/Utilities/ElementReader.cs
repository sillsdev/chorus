using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chorus.Utilities
{
	/// <summary>
	/// Responsible to read a file which has a sequence of similar elements
	/// and pass a byte array of each element to a delgate for processing.
	/// Use low-level byte array methods (eventually asynchronously, I [JohnT] hope).
	/// </summary>
	public class ElementReader : IDisposable
	{
		private byte[] _inputData;
		private readonly byte[] _openingMarker;
		private readonly byte[] _finalClosingTag;
		private Action<byte[]> _outputHandler;
		private int _startOfRecordsOffset;
		private int _endOfRecordsOffset;
		private readonly List<Byte> _endingWhitespace;

		public ElementReader(string openingMarker, string finalClosingTag, byte[] inputData, Action<byte[]> outputHandler)
		{
			_inputData = inputData;
			var enc = Encoding.UTF8;
			_openingMarker = enc.GetBytes(openingMarker);
			_finalClosingTag = enc.GetBytes(finalClosingTag);
			_outputHandler = outputHandler;
			_startOfRecordsOffset = 0;
			_endOfRecordsOffset = _inputData.Length;
			_endingWhitespace = new List<byte>();
			_endingWhitespace.AddRange(enc.GetBytes(" "));
			_endingWhitespace.AddRange(enc.GetBytes("\t"));
			_endingWhitespace.AddRange(enc.GetBytes("\r"));
			_endingWhitespace.AddRange(enc.GetBytes("\n"));
		}

		public void Run()
		{
			TrimInput();

			if (_startOfRecordsOffset == _endOfRecordsOffset)
				return; // Nothing to do.

			var openingAngleBracket = _openingMarker[0];
			for (var i = _startOfRecordsOffset; i < _endOfRecordsOffset; ++i)
			{
				var endOffset = FindStartOfElement(i + 1, openingAngleBracket);
				// We should have the complete <foo> element in the param.
				_outputHandler(_inputData.SubArray(i, endOffset - i));
				i = endOffset - 1;
			}
		}

		/// <summary>
		/// This method adjusts _startOfRecordsOffset to the offset to the start of the records,
		/// and adjusts _endOfRecordsOffset to the end of the last record.
		/// </summary>
		private void TrimInput()
		{
			// Trim off junk at the start.
			_startOfRecordsOffset = FindStartOfElement(0, _openingMarker[0]);
			// Trim off end tag. It really better be the last bunch of bytes!
			_endOfRecordsOffset = _inputData.Length - _finalClosingTag.Length;
		}

		private int FindStartOfElement(int currentOffset, byte openingAngleBracket)
		{
			// Need to get the next starting marker, or the main closing tag
			// When the end point is found, call _outputHandler with the current array
			// from 'offset' to 'i' (more or less).
			// Skip quickly over anything that doesn't match even one character.
			for (var i = currentOffset; i < _endOfRecordsOffset; ++i)
			{
				var currentByte = _inputData[i];
				// Need to get the next starting marker, or the main closing tag
				// When the end point is found, call _outputHandler with the current array
				// from 'offset' to 'i' (more or less).
				// Skip quickly over anything that doesn't match even one character.
				if (currentByte != openingAngleBracket)
					continue;

				// Try to match the rest of the marker.
				for (var j = 1; ; j++)
				{
					var current = _inputData[i + j];
					if (_endingWhitespace.Contains(current))
					{
						// Got it!
						return i;
					}
					if (_openingMarker[j] != current)
						break; // no match, resume searching for opening character.
					if (j != _openingMarker.Length - 1)
						continue;
				}
			}

			return _endOfRecordsOffset; // Found the end.
		}

		~ElementReader()
		{
			Debug.WriteLine("**** ElementReader.Finalizer called ****");
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
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			// Main data members.
			_inputData = null;
			_outputHandler = null;

			m_isDisposed = true;
		}
	}
}