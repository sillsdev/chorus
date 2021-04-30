using System;
using System.IO;
using System.Reflection;
using SIL.PlatformUtilities;

namespace Chorus.Utilities
{
	public class ExecutionEnvironment
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

		internal static string ChorusMergeFilePath()
		{
			if (Platform.IsMono)
			{
				// We need to use a shell script wrapper on Linux to ensure the correct mono is called.
				var chorusMergeFilePath =
					Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly,
						"chorusmerge");
				// The replace is only useful for use with the MonoDevelop environment which doesn't
				// honor $(Configuration) in the csproj files. When this is exported as an environment
				// var it needs escaping to prevent the shell from replacing it with an empty string.
				// When MonoDevelop is fixed this can be removed.
				return chorusMergeFilePath.Replace("$(Configuration)", "\\$(Configuration)");
			}
			return Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly, "ChorusMerge.exe");
		}
	}
}