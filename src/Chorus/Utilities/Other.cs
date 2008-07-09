using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Chorus.Utilities
{
	public class Other
	{
		public static string DirectoryOfExecutingAssembly
		{
			get
			{
				string path;
				bool unitTesting = Assembly.GetEntryAssembly() == null;
				if (unitTesting)
				{
					path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
					path = Uri.UnescapeDataString(path);
				}
				else
				{
					path = Assembly.GetExecutingAssembly().Location;
				}
				return Directory.GetParent(path).FullName;
			}
		}

		protected static string GetTopAppDirectory()
		{
			string path;

			path = DirectoryOfExecutingAssembly;

			if (path.ToLower().IndexOf("output") > -1)
			{
				//go up to output
				path = Directory.GetParent(path).FullName;
				//go up to directory containing output
				path = Directory.GetParent(path).FullName;
			}
			return path;
		}
	}
}