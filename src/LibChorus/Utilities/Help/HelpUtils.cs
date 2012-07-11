using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Chorus.Utilities.Help
{
	///<summary>
	/// Utilities for storing the embedded Help File on disk and retrieving it.
	///</summary>
	public class HelpUtils
	{
		/// <summary>
		/// Extracts the embedded help file and returns a filepath to it.
		/// </summary>
		/// <returns>The filepath to the help file.</returns>
		public static string GetHelpFile()
		{
			var helpFile = Application.StartupPath + @"\Send_Receive_Help.chm";

			// if (!File.Exists(helpFile))
			ExtractHelpFile();

			return helpFile;
		}

		/// <summary>
		/// Extracts the embedded help file.
		/// </summary>
		public static void ExtractHelpFile()
		{
			var helpFile = Application.StartupPath + @"\Send_Receive_Help.chm";

			var assembly = Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream("Chorus.Send_Receive_Help.chm");
			if (stream == null)
			{
				throw new NullReferenceException(); // ?
			}
			var binReader = new BinaryReader(stream);
			var binWriter = new BinaryWriter(File.Open(helpFile, FileMode.Create));

			var i = 0;
			while (i < stream.Length)
			{
				binWriter.Write(binReader.ReadByte());
				i++;
			}
			binReader.Close();
			binWriter.Close();

			if (!File.Exists(helpFile))
			{
				throw new FileNotFoundException("The help file could not be retrieved");
			}
		}
	}
}
