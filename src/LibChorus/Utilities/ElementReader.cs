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
		private int m_limit; // one more than the last useable character in m_inputData.
		private readonly byte[] m_openingMarker;
		private readonly byte[] m_finalClosingTag;
		private Action<byte[]> m_outputHandler;

		public ElementReader(string openingMarker, string finalClosingTag, byte[] inputData, Action<byte[]> outputHandler)
		{
			if (!openingMarker.EndsWith(" "))
				openingMarker += " ";

			m_inputData = inputData;
			var enc = Encoding.UTF8;
			m_openingMarker = enc.GetBytes(openingMarker);
			m_finalClosingTag = enc.GetBytes(finalClosingTag);
			m_outputHandler = outputHandler;
			m_limit = m_inputData.Length;
		}

		public void Run()
		{
			TrimInput();

			var openingAngleBracket = m_openingMarker[0];
			for (var i = 0; i < m_limit; ++i)
			{
				var endOffset = FindStartOfElement(i + 1, openingAngleBracket);
				// We should have the complete <foo> element in the param.
				m_outputHandler(m_inputData.SubArray(i, endOffset - i));
				i = endOffset - 1;
			}
		}

		private void TrimInput()
		{
			// Trim off junk at the start.
			var openingAngleBracket = m_openingMarker[0];
			var canStop = false;
			for (var i = 0; i < m_limit; ++i)
			{
				var currentByte = m_inputData[i];
				// Need to get the next starting marker, or the main closing tag
				// When the end point is found, call m_outputHandler with the current array
				// from 'offset' to 'i' (more or less).
				// Skip quickly over anything that doesn't match even one character.
				if (currentByte != openingAngleBracket)
					continue; // Useless check for any xml input that starts with the normal <?xml version='1.0' encoding='utf-8'?>

				// Try to match the rest of the marker.
				for (var j = 1; ; j++)
				{
					if (m_openingMarker[j] != m_inputData[i + j])
						break; // no match, resume searching for opening character.
					if (j != m_openingMarker.Length - 1)
						continue;

					// Got it!
					m_inputData = m_inputData.SubArray(i, m_limit - i);
					m_limit = m_inputData.Length;
					canStop = true;
					break;
				}
				if (canStop)
					break;
			}

			// Trim off end tag. It really better be the last bunch of bytes!
			m_inputData = m_inputData.SubArray(0, m_inputData.Length - m_finalClosingTag.Length);
			m_limit = m_inputData.Length;
		}

		private int FindStartOfElement(int currentOffset, byte openingAngleBracket)
		{
			// Need to get the next starting marker, or the main closing tag
			// When the end point is found, call m_outputHandler with the current array
			// from 'offset' to 'i' (more or less).
			// Skip quickly over anything that doesn't match even one character.
			for (var i = currentOffset; i < m_limit; ++i)
			{
				var currentByte = m_inputData[i];
				// Need to get the next starting marker, or the main closing tag
				// When the end point is found, call m_outputHandler with the current array
				// from 'offset' to 'i' (more or less).
				// Skip quickly over anything that doesn't match even one character.
				if (currentByte != openingAngleBracket)
					continue;

				// Try to match the rest of the marker.
				for (var j = 1; ; j++)
				{
					if (m_openingMarker[j] != m_inputData[i + j])
						break; // no match, resume searching for opening character.
					if (j != m_openingMarker.Length - 1)
						continue;

					// Got it!
					return i;
				}
			}

			return m_limit; // Found the end.
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