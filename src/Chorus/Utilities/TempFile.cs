using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Chorus.Utilities
{
	public class TempLiftFile : TempFile
	{
		public TempLiftFile(string xmlOfEntries)
			: this(xmlOfEntries, /*LiftIO.Validation.Validator.LiftVersion*/ "0.12")
		{
		}
	   public TempLiftFile(string xmlOfEntries, string claimedLiftVersion)
			: this(null, xmlOfEntries, claimedLiftVersion)
		{
		 }

		public TempLiftFile(TempFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			:base(false)
		{
			if (parentFolder != null)
			{
				_path = parentFolder.GetPathForNewTempFile(false)+".lift";
			}
			else
			{
				_path = System.IO.Path.GetRandomFileName()+".lift";
			}

			string liftContents = string.Format("<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>", claimedLiftVersion, xmlOfEntries);
			File.WriteAllText(_path, liftContents);
		}

	}


	public class TempFile : IDisposable
	{
		protected string _path;

		public TempFile()
		{
			_path = System.IO.Path.GetTempFileName();
		}

		internal TempFile(bool dontMakeMeAFile)
		{
		}

		public TempFile(TempFolder parentFolder)
		{
			if (parentFolder != null)
			{
				_path = parentFolder.GetPathForNewTempFile(true);
			}
			else
			{
				_path = System.IO.Path.GetTempFileName();
			}

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

		private TempFile(string existingPath, bool dummy)
		{
			_path = existingPath;
		}

		public static TempFile TrackExisting(string path)
		{
			return new TempFile(path, false);
		}
	}

	public class TempFolder : IDisposable
	{
		private readonly string _folderPath;

		public TempFolder(string testName)
		{
			_folderPath = Path.Combine(Path.GetTempPath(), testName);
			if (Directory.Exists(_folderPath))
			{
				TestUtilities.DeleteFolderThatMayBeInUse(_folderPath);
			}
			Directory.CreateDirectory(_folderPath);
		}

		public string FolderPath
		{
			get { return _folderPath; }
		}

		public void Dispose()
		{
			TestUtilities.DeleteFolderThatMayBeInUse(_folderPath);
		}

		public string GetPathForNewTempFile(bool doCreateTheFile)
		{
			string s = Path.GetRandomFileName();
			s = Path.Combine(_folderPath, s);
			if (doCreateTheFile)
			{
				File.Create(s).Close();
			}
			return s;
		}

		public TempFile GetNewTempFile(bool doCreateTheFile)
		{
			string s = Path.GetRandomFileName();
			s = Path.Combine(_folderPath, s);
			if (doCreateTheFile)
			{
				File.Create(s).Close();
			}
			return TempFile.TrackExisting(s);
		}
		public string Combine(string innerFileName)
		{
			return Path.Combine(_folderPath, innerFileName);
		}
	}

	public class TestUtilities
	{
		public static void DeleteFolderThatMayBeInUse(string folder)
		{
			if (Directory.Exists(folder))
			{
				for (int i = 0; i < 50; i++)//wait up to five seconds
				{
					try
					{
						Directory.Delete(folder, true);
						return;
					}
					catch (Exception)
					{
					}
					Thread.Sleep(100);
				}
				//maybe we can at least clear it out a bit
				try
				{
					Debug.WriteLine("TestUtilities.DeleteFolderThatMayBeInUse(): gave up trying to delete the whole folder. Some files may be abandoned in your temp folder.");

					string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
					foreach (string s in files)
					{
						File.Delete(s);
					}
					//sleep and try again
					Thread.Sleep(1000);
					Directory.Delete(folder, true);
				}
				catch (Exception)
				{
				}

			}
		}
	}

}