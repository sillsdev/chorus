using System;
using System.IO;
using Chorus.Utilities;

namespace Chorus.VcsDrivers.Mercurial
{
	public class FileInRevision
	{
		private readonly string _revisionNumber;
		public Action ActionThatHappened { get; private set; }

		public enum Action
		{
			Added, Deleted, Modified,
			Unknown,
			NoChanges,
			Parent
		}
		public string FullPath { get; private set; }
		public FileInRevision(string revisionNumber, string fullPath, Action action)
		{
			_revisionNumber = revisionNumber;
			ActionThatHappened = action;
			FullPath = fullPath;
		}

		/// <summary>
		/// Make sure to dispose of this
		/// </summary>
		/// <returns>An IDisposable TempFile</returns>
		public TempFile CreateTempFile(HgRepository repository)
		{
			var path = repository.RetrieveHistoricalVersionOfFile(FullPath, _revisionNumber);
			return TempFile.TrackExisting(path);
		}

		public string GetFileContents(HgRepository repository)
		{
			var path = repository.RetrieveHistoricalVersionOfFile(FullPath, _revisionNumber);
			try
			{
				return File.ReadAllText(path);
			}
			finally
			{
				 File.Delete(path);
			}
		}

		public byte[] GetFileContentsAsBytes(HgRepository repository)
		{
			var path = repository.RetrieveHistoricalVersionOfFile(FullPath, _revisionNumber);
			try
			{
				return File.ReadAllBytes(path);
			}
			finally
			{
				File.Delete(path);
			}
		}
	}

	public class FileInUnknownRevision : FileInRevision
	{
		public FileInUnknownRevision(string fullPath, Action action):base(string.Empty, fullPath, action)
		{

		}
	}
}