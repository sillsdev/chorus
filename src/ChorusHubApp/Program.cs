using System;
using System.IO;
using System.Windows.Forms;
using Chorus.ChorusHub;
using ChorusHubApp.Properties;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;

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
			ExceptionHandler.Init(new WinFormsExceptionHandler());
		}
	}
}
