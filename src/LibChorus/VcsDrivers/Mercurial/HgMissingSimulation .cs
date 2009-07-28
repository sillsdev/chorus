using System;

namespace Chorus.VcsDrivers.Mercurial
{
	/// <summary>
	/// to use: Wrap test code in a using(new HgMissingSimulation()){}
	/// </summary>
	public class HgMissingSimulation : IDisposable
	{
		private string _originalPath;

		public HgMissingSimulation()
		{
			_originalPath = System.Environment.GetEnvironmentVariable("PATH");
			//this is just for testing, hence nothing fancy...
			Environment.SetEnvironmentVariable("PATH", _originalPath.Replace(@"TortoiseHg", "XTortoiseHg").Replace(@"Hg", "XHg").Replace(@"Mercurial", "XMercurial"));
			var check = System.Environment.GetEnvironmentVariable("PATH");
		}

		public void Dispose()
		{
			System.Environment.SetEnvironmentVariable("PATH", _originalPath);
		}
	}
}
