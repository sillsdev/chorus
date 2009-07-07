using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.retrieval
{
	/// <summary>
	/// Works with the merge/diff system to give details on what was done in the revision
	/// </summary>
	public class RevisionInspector : IMergeEventListener
	{
		private readonly HgRepository _repository;

		public IProgress ProgressIndicator { get; set; }

		public RevisionInspector(HgRepository repository)
		{
			_repository = repository;
			ProgressIndicator = new NullProgress();
		}

		public IEnumerable<IChangeReport> GetChangeRecords(Revision revision)
		{
			Changes.Clear();
			if (!revision.HasParentRevision)
			{
				return new List<IChangeReport>(new IChangeReport[]{new DummyChangeReport("Initial Checkin")});
			}

			var parentRev = revision.GetParentLocalNumber();
			foreach (var fileInRevision in _repository.GetFilesInRevision(revision))
			{
				//todo
			   if(Path.GetExtension(fileInRevision.RelativePath)!=".lift")
				   continue;
				using (var targetFile = TempFile.TrackExisting(_repository.RetrieveHistoricalVersionOfFile(fileInRevision.RelativePath, revision.LocalRevisionNumber)) )
				using (var parentFile = TempFile.TrackExisting(_repository.RetrieveHistoricalVersionOfFile(fileInRevision.RelativePath, revision.GetParentLocalNumber())) )
				{
					var order = MergeOrder.CreateForDiff(parentFile.Path, targetFile.Path, this);
					var result = MergeDispatcher.Go(order);
				}
			}
			return Changes;
			//todo: and how about conflict records?
		}

		#region IMergeEventListener

		public List<IConflict> Conflicts = new List<IConflict>();
		public List<IChangeReport> Changes = new List<IChangeReport>();
		public List<string> Contexts = new List<string>();

		public void ConflictOccurred(IConflict conflict)
		{
			Conflicts.Add(conflict);
		}

		public void ChangeOccurred(IChangeReport change)
		{
			Debug.WriteLine(change);
			Changes.Add(change);
		}

		public void EnteringContext(string context)
		{
			Contexts.Add(context);
		}
		#endregion
	}
}