using System;
using System.IO;
using Chorus.Utilities;
using SIL.Code;
using SIL.PlatformUtilities;

namespace Chorus// DON'T MOVE THIS! It needs to be super easy for the client to find
{
	/// <summary>
	/// Used to customize where Chorus looks to run hg. If you have a Mercurial folder in with the executables, or in ../common/
	/// then it will be found for you without you ever looking at this class.
	/// </summary>
	public class MercurialLocation
	{
		public const string EnvPathToHgExecutable = "CHORUS_PATH_TO_HG_EXECUTABLE";
		public const string EnvPathToMercurialFolder = "CHORUS_PATH_TO_MERCURIAL_FOLDER";
		public const string EnvHgExe = "CHORUS_HG_EXE";

		private static string _pathToMercurialFolder;
		private static string _hgExe = Environment.GetEnvironmentVariable(EnvHgExe) ?? (Platform.IsWindows ? "hg.exe" : "hg");

		/// <summary>
		/// Clients can set this if they have their own private copy of Mercurial (recommended)
		/// </summary>
		public static string PathToMercurialFolder
		{
			get
			{
				GuessAtLocationIfNotSetAlready();
				return _pathToMercurialFolder;
			}
			set
			{
				if(string.IsNullOrEmpty(value))//was reset to default
				{
					_pathToMercurialFolder = string.Empty;
					return;
				}
				RequireThat.Directory(value).Exists();
				string expectedHgLocation=Path.Combine(value, _hgExe);
				if (!File.Exists(expectedHgLocation))
				{
					throw new FileNotFoundException(expectedHgLocation);
				}
				_pathToMercurialFolder = value;
			}
		}

		/// <summary>
		/// Will use the environment variable override if set, or will use PathToMercurialFolder if possible, otherwise will just return "hg" and rely on PATH
		/// </summary>
		public static string PathToHgExecutable
		{
			get
			{
				string path = Environment.GetEnvironmentVariable(EnvPathToHgExecutable);
				if (path != null) return path;
				GuessAtLocationIfNotSetAlready();

				if(string.IsNullOrEmpty(_pathToMercurialFolder))
					return _hgExe; //rely on the PATH
				return Path.Combine(_pathToMercurialFolder, _hgExe);
			}
		}

		private static void GuessAtLocationIfNotSetAlready()
		{
			if (!string.IsNullOrEmpty(_pathToMercurialFolder))
			{
				return;
			}

			string path = Environment.GetEnvironmentVariable(EnvPathToMercurialFolder);
			if (!string.IsNullOrEmpty(path))
			{
				PathToMercurialFolder = path;
				return;
			}

			// We now try to use the same (antique) version of Mercurial on
			// both Windows and Linux, to maintain bug-for-bug compatibility.
			var executingAssemblyPath = ExecutionEnvironment.DirectoryOfExecutingAssembly;
			var guess = CheckForMercurialSubdirectory(executingAssemblyPath);
			if (guess == null)
			{
				var grandparentPath = Directory.GetParent(executingAssemblyPath).Parent.FullName;
				guess = CheckForMercurialSubdirectory(grandparentPath);
				if (guess == null)
				{
					var greatGrandParentPath = Directory.GetParent(grandparentPath).FullName;
					guess = CheckForMercurialSubdirectory(greatGrandParentPath);
				}
			}

			PathToMercurialFolder = guess;
		}

		private static string CheckForMercurialSubdirectory(string directory)
		{
			//in case we're running off the wesay source code directory
			var guess = Path.Combine(directory, "common", "Mercurial");
			if (Directory.Exists(guess))
			{
				return guess;
			}

			//in case we're running in chorus's solution directory
			guess = Path.Combine(directory, "Mercurial");
			return Directory.Exists(guess) ? guess : null;
		}
	}
}
