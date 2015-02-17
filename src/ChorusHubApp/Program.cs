using System;
using System.IO;
using System.Windows.Forms;
using Chorus.ChorusHub;
using ChorusHubApp.Properties;
using SIL.Reporting;

namespace ChorusHubApp
{
	static class Program
	{

		[STAThread]
		static void Main(string[] args)
		{
			var parentOfRoot = Path.GetDirectoryName(ChorusHubOptions.RootDirectory);
			if(!Directory.Exists(parentOfRoot))
			{
				ErrorReport.NotifyUserOfProblem("In order to use '{0}', '{1}' must already exist", ChorusHubOptions.RootDirectory, parentOfRoot);
				return;
			}
			var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
			if (chorusHubServerInfo != null)
			{
				ErrorReport.NotifyUserOfProblem("Only one ChorusHub can be run on a network but there is already one running on {0}",
												chorusHubServerInfo.HostName);
				return;
			}

			SetupErrorHandling();
			SetUpReporting();

			Application.Run(new ChorusHubWindow());
		}

		private static void SetUpReporting()
		{
			if (Settings.Default.Reporting == null)
			{
				Settings.Default.Reporting = new ReportingSettings();
				Settings.Default.Save();
			}
		 //TODO: set up Google Analytics account
//            UsageReporter.Init(Settings.Default.Reporting, "hub.chorus.palaso.org", "UA-22170471-6",
//#if DEBUG
// true
//#else
// false
//#endif
//        );
//            UsageReporter.AppNameToUseInDialogs = "Chorus Hub";
//            UsageReporter.AppNameToUseInReporting = "ChorusHub";
		}

		private static void SetupErrorHandling()
		{
			ErrorReport.EmailAddress = "issues@chorus.palaso.org";
			ErrorReport.AddProperty("Application", "ChorusHub");
			ErrorReport.AddProperty("Directory", ChorusHubOptions.RootDirectory);
			ErrorReport.AddProperty("AdvertisingPort", ChorusHubOptions.AdvertisingPort.ToString());
			ErrorReport.AddProperty("MercurialPort", ChorusHubOptions.MercurialPort.ToString());
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init();
		}

//
//
//            try
//            {
//                _service.Start(true);
//                while (!_isClosing)
//                {
//                    _service.Tick();
//                    Thread.Sleep(1000);
//                }
//            }
//            finally
//            {
//                CloseDown();
//            }
//        }
//
//        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
//        {
//            // Put your own handler here
//
//            switch (ctrlType)
//            {
//                case CtrlTypes.CTRL_BREAK_EVENT:
//                case CtrlTypes.CTRL_CLOSE_EVENT:
//                case CtrlTypes.CTRL_C_EVENT:
//                case CtrlTypes.CTRL_LOGOFF_EVENT:
//                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
//                    //NB: there is reason to believe that once we return from this handler,
//                    //the app will die *real soon*. So we need to clean up first.
//                    CloseDown();
//                    _isClosing = true;
//                    break;
//            }
//
//            return true;
//        }
//
//        private static void CloseDown()
//        {
//            Console.WriteLine("Stopping...");
//            _service.Stop();
//        }
//
//        #region unmanaged
//        [DllImport("Kernel32")]
//        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
//        public delegate bool HandlerRoutine(CtrlTypes CtrlType);
//        public enum CtrlTypes
//        {
//            CTRL_C_EVENT = 0,
//            CTRL_BREAK_EVENT,
//            CTRL_CLOSE_EVENT,
//            CTRL_LOGOFF_EVENT = 5,
//            CTRL_SHUTDOWN_EVENT
//        }
//        #endregion

	}
}
