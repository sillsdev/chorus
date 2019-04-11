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
		private static string _pathToMercurialFolder;

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
				string expectedHgLocation=Path.Combine(value, Platform.IsWindows ? "hg.exe" : "hg");
				if (!File.Exists(expectedHgLocation))
				{
					throw new FileNotFoundException(expectedHgLocation);
				}
				_pathToMercurialFolder = value;
			}
		}

		/// <summary>
		/// Will use the PathToMercurialFolder, otherwise will just return "hg"
		/// </summary>
		public static string PathToHgExecutable
		{
			get
			{
				GuessAtLocationIfNotSetAlready();

				if(string.IsNullOrEmpty(_pathToMercurialFolder))
					return "hg"; //rely on the PATH
				return Path.Combine(_pathToMercurialFolder, "hg");
			}
		}

		private static void GuessAtLocationIfNotSetAlready()
		{
			if (!string.IsNullOrEmpty(_pathToMercurialFolder))
			{
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
