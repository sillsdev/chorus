using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Chorus;
using L10NSharp;

namespace SampleApp
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			LocalizationManager.StrictInitializationMode = false;
			var installedTranslations = Path.Combine(AppContext.BaseDirectory, "localizations");
			if (Directory.Exists(Path.Combine(installedTranslations, "Chorus")))
			{
				var userTranslations = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"SIL", "ChorusSampleApp");
				Directory.CreateDirectory(userTranslations);
				ChorusSystem.SetUpLocalization("en", installedTranslations, userTranslations);
			}

			string dataDirectory = Path.Combine(Path.GetTempPath(), "ChorusSampleApp");
			if(Directory.Exists(dataDirectory ))
				DeleteFolderThatMayBeInUse(dataDirectory);
			Directory.CreateDirectory(dataDirectory);


			Application.Run(new Form1(dataDirectory));
		}

		public static void DeleteFolderThatMayBeInUse(string folder)//review: this is a strange place for this... isn't this in palaso?
		{
			if (Directory.Exists(folder))
			{
				try
				{
					Directory.Delete(folder, true);
				}
				catch (Exception e)
				{
					try
					{
						Console.WriteLine(e.Message);
						//maybe we can at least clear it out a bit
						string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
						foreach (string s in files)
						{
							File.Delete(s);
						}
						//sleep and try again (seems to work)
						Thread.Sleep(1000);
						Directory.Delete(folder, true);
					}
					catch (Exception)
					{
					}
				}
			}
		}
	}
}
