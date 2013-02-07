using System;
using System.IO;
using System.Windows.Forms;
using ChorusHub.Properties;
using Palaso.Reporting;
using CommandLine;

namespace ChorusHub
{
	static class Program
	{

		[STAThread]
		static void Main(string[] args)
		{
			var parameters = new ChorusHubParameters();
			if(Parser.ParseHelp(args))
			{
				MessageBox.Show(Parser.ArgumentsUsage(parameters.GetType()),"Chorus Hub Command Line Parameters");
				return;
			}
			if (!Parser.ParseArguments(args, parameters, ShowCommandLineError))
			{
				return;
			}

			string parentOfRoot = Path.GetDirectoryName(parameters.RootDirectory);
			if(!Path.IsPathRooted(parameters.RootDirectory))
			{
				ErrorReport.NotifyUserOfProblem("You supplied '{0}' for the root directory, but that doesn't have a drive letter.",
																	parameters.RootDirectory);
				return;
			}
			if(!Directory.Exists(parentOfRoot))
			{
				ErrorReport.NotifyUserOfProblem("In order to use '{0}', '{1}' must already exist",
																	parameters.RootDirectory, parentOfRoot);
				return;
			}
			var server = new ChorusHubClient().FindServer();
			if (server != null)
			{
				ErrorReport.NotifyUserOfProblem("Only one ChorusHub can be run on a network and there is already one running on {0}",
												server.HostName);
				return;
			}

			SetupErrorHandling(parameters);
			SetUpReporting();

			Application.Run(mainForm: new ChorusHubWindow(parameters));
		}

		private static void ShowCommandLineError(string e)
		{
			MessageBox.Show(e + Environment.NewLine + Environment.NewLine + "Command Line Arguments are: "+ Environment.NewLine+Parser.ArgumentsUsage(typeof(ChorusHubParameters)), "Chorus Hub Command Line Problem");
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

		private static void SetupErrorHandling(ChorusHubParameters chorusHubParameters)
		{
			ErrorReport.EmailAddress = "issues@chorus.palaso.org";
			ErrorReport.AddProperty("Application", "ChorusHub");
			ErrorReport.AddProperty("Directory", chorusHubParameters.RootDirectory);
			ErrorReport.AddProperty("AdvertisingPort", ChorusHubParameters.kAdvertisingPort.ToString());
			ErrorReport.AddProperty("MercurialPort", ChorusHubParameters.kMercurialPort.ToString());
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
