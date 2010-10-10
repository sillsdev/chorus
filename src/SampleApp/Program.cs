using System;
using System.IO;
using System.Windows.Forms;

namespace SampleApp
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			string dataDirectory = Path.Combine(Path.GetTempPath(), "ChorusSampleApp");
			if(Directory.Exists(dataDirectory ))
				Directory.Delete(dataDirectory, true);
			Directory.CreateDirectory(dataDirectory);

			var dataPath = Path.Combine(dataDirectory, "shoppingList.txt");
			File.WriteAllText(dataPath,"");
			Application.Run(new Form1(dataDirectory, dataPath));
		}
	}
}
