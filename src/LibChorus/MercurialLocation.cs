using System;
using System.Collections.Generic;
using System.IO;
using Chorus.Utilities.code;

namespace Chorus// DON'T MOVE THIS! It needs to be super easy for the client to find
{
	/// <summary>
	/// Used to customize where Chorus looks to run hg
	/// </summary>
	public class MercurialLocation
	{
		private static string _pathToMercurialFolder;

		/// <summary>
		/// Clients can set this if they have their own private copy of Mercurial (recommended)
		/// </summary>
		public static string PathToMercurialFolder
		{
			get { return _pathToMercurialFolder; }
			set
			{
				if(string.IsNullOrEmpty(value))//was reset to default
				{
					_pathToMercurialFolder = string.Empty;
					return;
				}
				RequireThat.Directory(value).Exists();
				string expectedHgLocation=Path.Combine(value, "hg.exe");
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
				if(string.IsNullOrEmpty(_pathToMercurialFolder))
					return "hg"; //rely on the PATH
				return Path.Combine(_pathToMercurialFolder, "hg");
			}
		}
	}
}
