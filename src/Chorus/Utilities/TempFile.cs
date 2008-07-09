using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chorus.Utilities
{
	public class TempFile : IDisposable
	{
		private string _path;

		public TempFile()
		{
			_path = System.IO.Path.GetTempFileName();
		}


		public TempFile(string contents)
			: this()
		{
			File.WriteAllText(_path, contents);
		}

		public TempFile(string[] contentLines)
			: this()
		{
			File.WriteAllLines(_path, contentLines);
		}

		public string Path
		{
			get { return _path; }
		}
		public void Dispose()
		{
			File.Delete(_path);
		}


//        public static TempFile TrackExisting(string path)
//        {
//            return new TempFile(path, false);
//        }
		public static TempFile CopyOf(string pathToExistingFile)
		{
			TempFile t = new TempFile();
			File.Copy(pathToExistingFile, t.Path, true);
			return t;
		}
	}
}