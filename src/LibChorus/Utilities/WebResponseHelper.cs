using System;
using System.Net;

namespace Chorus.Utilities
{
	internal class WebResponseHelper
	{
		internal static byte[] ReadResponseContent(WebResponse response, int expectedLength = 0)
		{
			// Get the stream, if any
			var stream = response.GetResponseStream();
			if (stream == null)
			{
				return new byte[0];
			}

			// Read the length from the headers, if any
			var responseContentLength = response.Headers["Content-Length"];
			int capacity, maxLength;
			if (string.IsNullOrEmpty(responseContentLength))
			{
				capacity = expectedLength;
				// Any capacity < maxLength can be doubled; this maxLength prevents integer overflow
				// (not that we expect to download nearly that much data)
				maxLength = int.MaxValue / 2;
			}
			else
			{
				capacity = maxLength = Convert.ToInt32(response.Headers["Content-Length"]);
			}

			var buffer = new byte[capacity];
			if (capacity == 0)
			{
				return buffer;
			}

			var offset = 0;
			int bytesRead;
			do
			{
				bytesRead = stream.Read(buffer, offset, capacity - offset);
				offset += bytesRead;
				if (offset == capacity && offset < maxLength && bytesRead > 0)
				{
					var fullBuffer = buffer;
					buffer = new byte[capacity * 2];
					for (var i = 0; i < capacity; i++)
					{
						buffer[i] = fullBuffer[i];
					}
					capacity = buffer.Length;
				}
			} while (bytesRead > 0 && offset < capacity);
			return buffer;
		}
	}
}
