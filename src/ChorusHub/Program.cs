using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ChorusHub
{
	static class Program
	{
		private static bool _isClosing;
		private static HgServeRunner _hgServer;
		private static Advertiser _advertiser;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

			_hgServer = new HgServeRunner();
			_advertiser = new Advertiser();

			try
			{
				_hgServer.Start();
				_advertiser.Start();

				while (!_isClosing)
				{
					_hgServer.CheckForFailedPushes();
					Thread.Sleep(1000);
				}

			}
			finally
			{
				CloseDown();
			}

		}

		private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
		{
			// Put your own handler here

			switch (ctrlType)
			{
				case CtrlTypes.CTRL_BREAK_EVENT:
				case CtrlTypes.CTRL_CLOSE_EVENT:
				case CtrlTypes.CTRL_C_EVENT:
				case CtrlTypes.CTRL_LOGOFF_EVENT:
				case CtrlTypes.CTRL_SHUTDOWN_EVENT:
					//NB: there is reason to believe that once we return from this handler,
					//the app will die *real soon*. So we need to clean up first.
					CloseDown();
					_isClosing = true;
					break;
			}

			return true;
		}

		private static void CloseDown()
		{
			Console.WriteLine("Stopping...");
			_advertiser.Stop();
			_hgServer.Stop();
		}

		#region unmanaged
		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
		public delegate bool HandlerRoutine(CtrlTypes CtrlType);
		public enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}
		#endregion

	}
}
