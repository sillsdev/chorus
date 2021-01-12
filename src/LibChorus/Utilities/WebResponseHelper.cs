using System;
using System.Net;

namespace Chorus.Utilities
{
	internal class WebResponseHelper
	{
		internal static byte[] ReadResponseContent(WebResponse response)
		{
			var stream = response.GetResponseStream();

			if (stream == null || string.IsNullOrEmpty(response.Headers["Content-Length"]))
			{
				return new byte[0];
			}

			var length =  Convert.ToInt32(response.Headers["Content-Length"]);
			var buffer = new byte[length];
			var offset = 0;
			int bytesRead;
			do
			{
				bytesRead = stream.Read(buffer, offset, length - offset);
				offset += bytesRead;
			} while (bytesRead > 0 && offset < length);
			return buffer;
		}
	}
}
