using System;
using System.IO;
using Chorus;
using Chorus.Utilities;

namespace LibChorus.Tests
{
	/// <summary>
	/// this lets us safely use this static without messing up other tests
	/// </summary>
	public class ShortTermMercurialPathSetting : IDisposable
	{
		private readonly string _oldValue;

		public ShortTermMercurialPathSetting(string path)
		{
			_oldValue = MercurialLocation.PathToMercurialFolder;
			MercurialLocation.PathToMercurialFolder = path;
		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		public virtual void Dispose()
		{
			MercurialLocation.PathToMercurialFolder = _oldValue;
		}
	}

/*NO LONGER NEEDED, AS THE SYSTEM DETECTS AND CHOOSES APPROPRIATELY

	/// <summary>
	/// most tests should create this in setup and remove it in teardown
	/// </summary>
	public class UseMercurialInChorusCodeDirectory : ShortTermMercurialPathSetting
	{
		public UseMercurialInChorusCodeDirectory()
			: base(Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly + "\\..\\common", "mercurial"))
		{
		}
	}
 */
}
