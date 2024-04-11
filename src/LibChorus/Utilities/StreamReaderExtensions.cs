// // Copyright (c) 2024-2024 SIL International
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Chorus.Utilities
{
	public static class StreamReaderExtensions
	{
		/// <summary>
		/// read the stream to the end, but return null if it takes too long
		/// if the read from the stream were to return null then an empty string is returned
		/// </summary>
		public static async Task<string> ReadToEndAsync(this StreamReader reader, int secondsBeforeTimeOut)
		{
			var readTask = reader.ReadToEndAsync();
			var timeoutTask = Task.Delay(TimeSpan.FromSeconds(secondsBeforeTimeOut));
			var result = await Task.WhenAny(readTask, timeoutTask);
			if (result == timeoutTask)
			{
				return null;
			}

			return await readTask ?? string.Empty;
		}
		/// <summary>
		/// read a line of text, but return null if the cancellation token is cancelled
		/// if the read from the stream were to return null then an empty string is returned
		/// </summary>
		public static async Task<string> ReadLineAsync(this StreamReader reader, CancellationToken cancellationToken)
		{
			try
			{
				var readTask = reader.ReadLineAsync();
				var timeoutTask = Task.Delay(-1, cancellationToken);
				var result = await Task.WhenAny(readTask, timeoutTask);
				if (result == timeoutTask) // should never happen since the delay is infinite
				{
					return null;
				}

				return await readTask ?? string.Empty;
			}
			catch (TaskCanceledException)
			{
				return null;
			}
		}
	}
}