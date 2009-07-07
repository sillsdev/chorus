using System.Collections.Generic;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.retrieval
{
	/// <summary>
	/// Works with the merge/diff system to give details on what was done in the revision
	/// </summary>
	public class RevisionInspector
	{
		private readonly HgRepository _repository;
		private readonly List<IChorusFileTypeHandler> _fileTypeHandlers;

		public IProgress ProgressIndicator { get; set; }

		public RevisionInspector(HgRepository repository, List<IChorusFileTypeHandler> fileTypeHandlers)
		{
			_repository = repository;
			_fileTypeHandlers = fileTypeHandlers;
			ProgressIndicator = new NullProgress();
		}

		public IEnumerable<IChangeReport> GetChangeRecords(Revision revision)
		{
			var changes = new List<IChangeReport>();

			if (!revision.HasParentRevision)
			{
				return new List<IChangeReport>(new IChangeReport[]{new DummyChangeReport(string.Empty, "Initial Checkin")});
			}

			var parentRev = revision.GetParentLocalNumber();
			foreach (var fileInRevision in _repository.GetFilesInRevision(revision))
			{
				foreach (var handler in _fileTypeHandlers)
				{
					//find, for example, a handler that can handle .lift dictionary, or a .wav sound file
					if (handler.CanHandleFile(fileInRevision.RelativePath))
					{
						var parentFileInRevision = new FileInRevision(parentRev, fileInRevision.RelativePath, fileInRevision.ActionThatHappened);

						//pull the files out of the repository so we can read them
						using (var targetFile = fileInRevision.CreateTempFile(_repository))
						using (var parentFile = parentFileInRevision.CreateTempFile(_repository))
						{
							//run the differ which the handler provides, adding the changes to the cumulative
							//list we are gathering for this hole revision
							changes.AddRange(handler.Find2WayDifferences(parentFile.Path, targetFile.Path));
						}
						break; //only the first handler gets a shot at it
					}
				}
			}
			return changes;
			//todo: and how about conflict records?
		}

	}
}