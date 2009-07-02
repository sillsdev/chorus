using System;
using System.Windows.Forms;

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


			string settingsPath =null;
			if(args.Length > 0)
			{
				settingsPath = args[0];
			}
//            string s = RepositoryManager.GetEnvironmentReadinessMessage("en");
//            if(!string.IsNullOrEmpty(s))
//            {
//                MessageBox.Show(s, "Chorus", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
//                return;
//            }

			Application.Run(new BootStrapper(settingsPath).CreateShell());
		}
	}
}
