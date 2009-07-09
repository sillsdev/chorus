using System.Collections.Generic;
using System.Diagnostics;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
namespace Chorus.retrieval
{
	/// <summary>
	/// Works with the merge/diff system to give details on what was done in the revision
	/// </summary>
	public class RevisionInspector
	{
		private readonly HgRepository _repository;
		private readonly ChorusFileTypeHandlerCollection _fileHandlerCollection;

		public IProgress ProgressIndicator { get; set; }

		public RevisionInspector(HgRepository repository, ChorusFileTypeHandlerCollection fileHandlerCollection)
		{
			_repository = repository;
			_fileHandlerCollection = fileHandlerCollection;
			ProgressIndicator = new NullProgress();
		}

		public IEnumerable<IChangeReport> GetChangeRecords(Revision revision)
		{
			var changes = new List<IChangeReport>();

//            if (!revision.HasParentRevision)
//            {
//                return new List<IChangeReport>(new IChangeReport[]{new DefaultChangeReport(string.Empty, "Initial Checkin")});
//            }

			string parentRev = null;
			if (revision.HasParentRevision)
			{
				parentRev = revision.GetParentLocalNumber();
			}


				foreach (var fileInRevision in _repository.GetFilesInRevision(revision))
				{
					var handler = _fileHandlerCollection.GetHandler(fileInRevision.RelativePath);
					//find, for example, a handler that can handle .lift dictionary, or a .wav sound file
					if (handler.CanDiffFile(fileInRevision.RelativePath))
					{
						if (parentRev != null)
						{
							var parentFileInRevision = new FileInRevision(parentRev, fileInRevision.RelativePath,
																		  fileInRevision.ActionThatHappened);

							//pull the files out of the repository so we can read them
							using (var targetFile = fileInRevision.CreateTempFile(_repository))
							using (var parentFile = parentFileInRevision.CreateTempFile(_repository))
							{
								//run the differ which the handler provides, adding the changes to the cumulative
								//list we are gathering for this hole revision
								changes.AddRange(handler.Find2WayDifferences(fileInRevision, parentFile.Path, targetFile.Path));
							}
						}
						else
						{
							using (var targetFile = fileInRevision.CreateTempFile(_repository))
							{
								changes.AddRange(handler.DescribeInitialContents(fileInRevision, targetFile));
							}
						}
					}
					else
					{
						switch (fileInRevision.ActionThatHappened)
						{
							case FileInRevision.Action.Added:
								changes.Add(new DefaultChangeReport(fileInRevision.RelativePath, "Added"));
								break;
							case FileInRevision.Action.Modified:
								changes.Add(new DefaultChangeReport(fileInRevision.RelativePath, "Changed"));
								break;
							case FileInRevision.Action.Deleted:
								changes.Add(new DefaultChangeReport(fileInRevision.RelativePath, "Deleted"));
								break;
							default:
								Debug.Fail("Found unexpected FileInRevision Action.");
								break;

						}
					}
				}

			return changes;
			//todo: and how about conflict records?
		}

	}
}