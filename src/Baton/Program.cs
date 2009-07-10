﻿using System;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;

namespace Baton
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
				pathToRepository = Baton.Properties.Settings.Default.PathToRepository;
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

			new Runner().Run(pathToRepository);

			Baton.Properties.Settings.Default.Save();
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
					Baton.Properties.Settings.Default.PathToRepository = s;
					Baton.Properties.Settings.Default.Save();
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
