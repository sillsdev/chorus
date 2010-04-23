using System;
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
		/* Sample handler/delegate Action:
		private static readonly byte[] s_guid = Encoding.UTF8.GetBytes("guid=\"");
		private static readonly byte[] s_class = Encoding.UTF8.GetBytes("class=\"");
		private static readonly byte s_close = Encoding.UTF8.GetBytes("\"")[0];

		void MakeSurrogate(byte[] xmlBytes)
		{
			var surrogate = m_surrogateFactory.Create(
				CmObjectId.Create(new Guid(GetAttribute(s_guid, xmlBytes))),
				GetAttribute(s_class, xmlBytes),
				xmlBytes);
			if (m_needConversion)
				RegisterSurrogateForConversion(surrogate);
			else
				RegisterInactiveSurrogate(surrogate);
		}

		string GetAttribute(byte[] name, byte[] input)
		{
			int start = input.IndexOfSubArray(name);
			if (start == -1)
				return null;
			start += name.Length;
			int end = Array.IndexOf(input, s_close, start);
			if (end == -1)
				return null; // error
			return Encoding.UTF8.GetString(input.SubArray(start, end - start));
		}
		*/
		private byte[] m_inputData;
		private int m_startingOffset;
		private int m_position; // index of next useable character in m_inputData.
		private readonly int m_limit; // one more than the last useable character in m_inputData.
		private readonly byte[] m_openingMarker;
		private readonly byte[] m_finalClosingTag;
		private Action<byte[]> m_outputHandler;
		private static readonly byte CloseBracket = Encoding.UTF8.GetBytes(">")[0];

		public ElementReader(string openingMarker, string finalClosingTag, byte[] inputData, Action<byte[]> outputHandler)
		{
			if (!openingMarker.EndsWith(" "))
				openingMarker += " ";

			m_inputData = inputData;
			var enc = Encoding.UTF8;
			m_openingMarker = enc.GetBytes(openingMarker);
			m_finalClosingTag = enc.GetBytes(finalClosingTag);
			m_outputHandler = outputHandler;
			m_position = 0;
			m_limit = m_inputData.Length;
		}

		public void Run()
		{
			int offset;
			if (!AdvanceToMarkerElement(out offset))
				return; // no elements!

			m_startingOffset = offset;
			while (ProcessElement())
			{
			}
		}

		/// <summary>
		/// Advance input, copying characters read to m_currentOutput if it is non-null, until we
		/// have successfully read the target marker, or reached EOF. Return true if we found it.
		/// Assumes m_marker is at least two characters. Also expects it to be an XML element marker,
		/// or at least that it's first character does not recur in the marker.
		/// </summary>
		/// <returns></returns>
		private bool AdvanceToMarkerElement(out int startOffset)
		{
			startOffset = m_limit;
			var openingAngleBracket = m_openingMarker[0];
			while (true)
			{
				// Skip quickly over anything that doesn't match even one character.
				while (More() && Next() != openingAngleBracket)
				{
				}
				if (!More())
					return false;
				// Try to match the rest of the marker.
				for (var i = 1; ; i++)
				{
					if (!More())
						return false;
					if (m_openingMarker[i] != Next())
						break; // no match, resume searching for opening character.
					if (i != m_openingMarker.Length - 1)
						continue;

					startOffset = m_position - m_openingMarker.Length;
					m_startingOffset = m_position - m_openingMarker.Length;
					return true; // got it!
				}
			}
		}
		/// <summary>
		/// Called when we have just read the input marker. Advances to the
		/// start of closing tag "languageproject" or just after the next element.
		/// </summary>
		/// <returns>false, if we have found the final closing tag, otherwise true</returns>
		private bool ProcessElement()
		{
			var currentStartOffset = m_startingOffset;
			int endOffset;
			var result = AdvanceToMarkerElement(out endOffset);
			// See to the character before the marker for the next element (if we got one)
			if (result)
			{
				//endOffset -= m_openingMarker.Length; // read a marker into the stream, skip it.
			}
			else
			{
				endOffset -= m_finalClosingTag.Length; // read the closing tag into the stream, skip it.
			}
			var xmlBytes = new byte[endOffset - currentStartOffset];
			Array.Copy(m_inputData, currentStartOffset, xmlBytes, 0, xmlBytes.Length);
			// We should have the complete <foo> element in xmlBytes.
			m_outputHandler(xmlBytes);
			return result;
		}

		// Return true if there are more bytes to read. Refill the buffer if need be.
		private bool More()
		{
			return m_position < m_limit;
		}

		private byte Next()
		{
			return m_inputData[m_position++];
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
			m_inputData = null;
			m_outputHandler = null;

			m_isDisposed = true;
		}
	}
}