using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;

namespace Chorus
{
	static class Program
	{

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			SetUpErrorHandling();

			//Xpcom.Initialize(XULRunnerLocator.GetXULRunnerLocation());
			//GeckoPreferences.User["gfx.font_rendering.graphite.enabled"] = true;
			//Application.ApplicationExit += (sender, e) => { Xpcom.Shutdown(); };

			//is mercurial set up?
			var s = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(s))
			{
				MessageBox.Show(s, "Chorus", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}

			//did they give us a path on the command line?
			string pathToRepository =null;
			if(args.Length > 0)
			{
				pathToRepository = args[0];
			}

			if (string.IsNullOrEmpty(pathToRepository) || !Directory.Exists(pathToRepository))
			{
				//do we have a valid path from last time?
				pathToRepository = Properties.Settings.Default.PathToRepository;
			}

			if (string.IsNullOrEmpty(pathToRepository) || !Directory.Exists(pathToRepository))
			{
				//can they find a repository for us?
				pathToRepository = Runner.BrowseForRepository();
			}
			if (string.IsNullOrEmpty(pathToRepository) || !Directory.Exists(pathToRepository))
			{
				return; //give up
			}


			Properties.Settings.Default.PathToRepository = pathToRepository;
			Properties.Settings.Default.Save();
			new Runner().Run(pathToRepository, new Arguments(args));

			Properties.Settings.Default.Save();
			Application.Exit ();
		}

		private static void SetUpErrorHandling()
		{
			try
			{
//				SIL.Reporting.ErrorReport.AddProperty("EmailAddress", "issues@wesay.org");
//				SIL.Reporting.ErrorReport.AddStandardProperties();
//				SIL.Reporting.ExceptionHandler.Init();

			/* until we decide to require SIL.Core.dll, we can at least make use of it if it happens
			 * to be there (as it is with WeSay)
			 */
				Assembly asm = Assembly.LoadFrom("SIL.Core.dll");
				Type errorReportType = asm.GetType("SIL.Reporting.ErrorReport");
				PropertyInfo emailAddress = errorReportType.GetProperty("EmailAddress");
				emailAddress.SetValue(null,"issues@wesay.org",null);
				errorReportType.GetMethod("AddStandardProperties").Invoke(null, null);
				asm.GetType("SIL.Reporting.ExceptionHandler").GetMethod("Init").Invoke(null, null);
			}
			catch(Exception)
			{
				//ah well
			}
		}


		internal class Runner
		{
			public void Run(string pathToRepository, Arguments arguments)
			{

				BrowseForRepositoryEvent browseForRepositoryEvent = new BrowseForRepositoryEvent();
				browseForRepositoryEvent.Subscribe(BrowseForRepository);
				using (var bootStrapper = new BootStrapper(pathToRepository))
				using (var shell = bootStrapper.CreateShell(browseForRepositoryEvent, arguments))
				{
					Application.Run(shell);
				}
			}

			public  void BrowseForRepository(string dummy)
			{
				var s = BrowseForRepository();
				if (!string.IsNullOrEmpty(s) && Directory.Exists(s))
				{
					//NB: if this was run from a Visual Studio debug session, these settings
					//are going to be saved in a different place, so on
					//restart, we won't really open the one we wanted.
					//We'll instead open the last project that was opened when
					//running outside of Visual Studio.
					Properties.Settings.Default.PathToRepository = s;
					Properties.Settings.Default.Save();
					Application.Restart();
				}
			}

			public static string BrowseForRepository()
			{
				using (var dlg = new FolderBrowserDialog())
				{
					dlg.Description = LocalizationManager.GetString("Messages.SelectChorusProject",
						"Select a chorus-enabled project to open:");
					dlg.ShowNewFolderButton = false;
					if (DialogResult.OK != dlg.ShowDialog())
						return null;
					return dlg.SelectedPath;
				}
			}
		}
	}
}
