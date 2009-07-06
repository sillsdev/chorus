using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.retrieval
{
	public class RevisionInfoProvider : IMergeEventListener
	{
		private readonly HgRepository _repository;

		public IProgress ProgressIndicator { get; set; }

		public RevisionInfoProvider(HgRepository repository)
		{
			_repository = repository;
			ProgressIndicator = new NullProgress();
		}

		public IEnumerable<IChangeReport> GetChangeRecords(Revision revision)
		{
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
			Changes.Add(change);
		}

		public void EnteringContext(string context)
		{
			Contexts.Add(context);
		}
		#endregion
	}
}