using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LibChorus.IntegrationTests
{
	/// <summary>
	/// A TCP proxy to an HTTP server that drops each of the first <c>cuts</c> connections once the server
	/// has sent a set number of bytes over it, so a download that keeps its connection open is cut off
	/// part-way through. Connections after that are passed through untouched.
	/// </summary>
	internal sealed class ConnectionCuttingProxy : IDisposable
	{
		private readonly Uri _target;
		private readonly long _bytesBeforeCut;
		private readonly int _cutsWanted;
		private readonly TcpListener _listener;
		private readonly CancellationTokenSource _stopping = new CancellationTokenSource();
		private readonly Task _accepting;
		private int _cuts;

		/// <summary>Base URL of the proxy, with a trailing slash</summary>
		public string BaseUrl { get; }

		/// <summary>How many connections have been cut so far</summary>
		public int Cuts => _cuts;

		public ConnectionCuttingProxy(Uri target, long bytesBeforeCut, int cuts)
		{
			_target = target;
			_bytesBeforeCut = bytesBeforeCut;
			_cutsWanted = cuts;
			_listener = new TcpListener(IPAddress.Loopback, 0);
			_listener.Start();
			BaseUrl = $"http://127.0.0.1:{((IPEndPoint) _listener.LocalEndpoint).Port}/";
			_accepting = AcceptAsync();
		}

		private async Task AcceptAsync()
		{
			while (!_stopping.IsCancellationRequested)
			{
				TcpClient client;
				try
				{
					client = await _listener.AcceptTcpClientAsync();
				}
				catch (Exception) when (_stopping.IsCancellationRequested)
				{
					return;
				}
				_ = Task.Run(() => RelayAsync(client));
			}
		}

		private async Task RelayAsync(TcpClient client)
		{
			using (client)
			using (var server = new TcpClient())
			{
				await server.ConnectAsync(_target.Host, _target.Port);
				var cut = Interlocked.Increment(ref _cuts) <= _cutsWanted;
				if (!cut)
				{
					Interlocked.Decrement(ref _cuts);
				}
				var budget = cut ? _bytesBeforeCut : long.MaxValue;
				var clientStream = client.GetStream();
				var serverStream = server.GetStream();
				var upstream = CopyAsync(clientStream, serverStream, long.MaxValue);
				var downstream = CopyAsync(serverStream, clientStream, budget);
				await Task.WhenAny(upstream, downstream);
				// closing both sockets ends whichever copy is still running (and, for a cut, drops the
				// client's connection with the download unfinished)
			}
		}

		private async Task CopyAsync(Stream from, Stream to, long budget)
		{
			var buffer = new byte[16384];
			try
			{
				while (budget > 0)
				{
					int read = await from.ReadAsync(buffer, 0, (int) Math.Min(buffer.Length, budget), _stopping.Token);
					if (read == 0)
					{
						return;
					}
					await to.WriteAsync(buffer, 0, read, _stopping.Token);
					budget -= read;
				}
			}
			catch (Exception)
			{
				// the other side went away; the relay ends either way
			}
		}

		public void Dispose()
		{
			_stopping.Cancel();
			_listener.Stop();
			_accepting.Wait(TimeSpan.FromSeconds(5));
		}
	}
}
