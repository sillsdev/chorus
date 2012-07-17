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
// ReSharper disable AssignNullToNotNullAttribute
			// If there is no executing assembly, we have other problems.
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Chorus_Help.chm");
// ReSharper restore AssignNullToNotNullAttribute
		}
	}
}
