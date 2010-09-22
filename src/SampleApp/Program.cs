using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;
using Palaso.IO;

namespace SampleApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var path = @"c:\dev\temp\testChorusData";
			ProjectFolderConfiguration project = new ProjectFolderConfiguration(path);
			var synchronizer = new Chorus.sync.Synchronizer(path, project, new NullProgress());
			synchronizer.SyncNow(new SyncOptions());
			//Application.Run(new Form1());
		}
	}
}
