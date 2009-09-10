using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;

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
			new Runner().Run(pathToRepository);

			Properties.Settings.Default.Save();
		}

		private static void SetUpErrorHandling()
		{
			/* until we decide to require palaso.dll, we can at least make use of it if it happens
			 * to be there (as it is with WeSay)
			 */
			try
			{
				Assembly asm = Assembly.LoadFrom("Palaso.dll");
				Type errorReportType = asm.GetType("Palaso.Reporting.ErrorReport");
				PropertyInfo emailAddress = errorReportType.GetProperty("EmailAddress");
				emailAddress.SetValue(null,"issues@wesay.org",null);
				errorReportType.GetMethod("AddStandardProperties").Invoke(null, null);
				asm.GetType("Palaso.Reporting.ExceptionHandler").GetMethod("Init").Invoke(null, null);
			}
			catch(Exception)
			{
				//ah well
			}
		}


		private class Runner
		{
			public void Run(string pathToRepository)
			{

				BrowseForRepositoryEvent browseForRepositoryEvent = new BrowseForRepositoryEvent();
				browseForRepositoryEvent.Subscribe(BrowseForRepository);
				using (var bootStrapper = new BootStrapper(pathToRepository))
				{
					Application.Run(bootStrapper.CreateShell(browseForRepositoryEvent));
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
				var dlg = new FolderBrowserDialog();
				dlg.Description = "Select a chorus-enabled project to open:";
				dlg.ShowNewFolderButton = false;
				if (DialogResult.OK != dlg.ShowDialog())
					return null;
				return dlg.SelectedPath;
			}
		}
	}
}