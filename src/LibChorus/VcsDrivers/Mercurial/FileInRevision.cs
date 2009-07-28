using System;
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
			NoChanges
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

	}
}