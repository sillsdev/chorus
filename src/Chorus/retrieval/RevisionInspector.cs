using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

			revision.EnsureParentRevisionInfo();


			if (!revision.HasAtLeastOneParent)
			{
				//describe the contents of the initial checkin
				foreach (var fileInRevision in _repository.GetFilesInRevision(revision))
				{
					CollectChangesInFile(fileInRevision, null, changes);
				}
			}

			else
			{
				IEnumerable<RevisionNumber> parentRevs = revision.GetLocalNumbersOfParents();
				foreach (RevisionNumber parentRev in parentRevs)
				{
					foreach (var fileInRevision in _repository.GetFilesInRevision(revision))
					{
						CollectChangesInFile(fileInRevision, parentRev.LocalRevisionNumber, changes);
					}
				}
				if (parentRevs.Count() > 1)
				{
					changes=FilterOutChangesWeDontWantReportAgain(changes);
				}
			}

			return changes;

		}

		/// <summary>
		/// After a merge, the definition of a "change" gets more complicated.  In a sense, there are
		/// no new changes. At the moment, our algorithm is just to discard any changes that are not
		/// conflict reports.
		/// </summary>
		private List<IChangeReport> FilterOutChangesWeDontWantReportAgain(List<IChangeReport> reports)
		{
			var changes = new List<IChangeReport>();
			foreach (var report in reports)
			{
				var handler = _fileHandlerCollection.GetHandlerForDiff(report.PathToFile);
				if(handler.GetType()==typeof(ConflictFileTypeHandler))
				{
					changes.Add(report);
				}
			}

			return new List<IChangeReport>(changes.Distinct());//don't let the same change (like a new conflict) be reported twice
		}

		private void CollectChangesInFile(FileInRevision fileInRevision, string parentRev, List<IChangeReport> changes)
		{
			var handler = _fileHandlerCollection.GetHandlerForDiff(fileInRevision.FullPath);
			//find, for example, a handler that can handle .lift dictionary, or a .wav sound file
			if (handler.CanDiffFile(fileInRevision.FullPath))//review: isn't that just asking again?
			{
				if (parentRev != null && fileInRevision.ActionThatHappened == FileInRevision.Action.Modified)
				{
					var parentFileInRevision = new FileInRevision(parentRev, Path.Combine(_repository.PathToRepo, fileInRevision.FullPath),
																  fileInRevision.ActionThatHappened);

					//pull the files out of the repository so we can read them
					using (var targetFile = fileInRevision.CreateTempFile(_repository))
					using (var parentFile = parentFileInRevision.CreateTempFile(_repository))
					{
						//run the differ which the handler provides, adding the changes to the cumulative
						//list we are gathering for this whole revision
						changes.AddRange(handler.Find2WayDifferences(fileInRevision, parentFile.Path, targetFile.Path));
					}
				}
				else
				{
					try
					{
						using (var targetFile = fileInRevision.CreateTempFile(_repository))
						{
							changes.AddRange(handler.DescribeInitialContents(fileInRevision, targetFile));
						}
					}
					catch (Exception error)
					{
						changes.Add(new DefaultChangeReport(fileInRevision.FullPath,
															"Error retrieving historical version. "+error.Message));
					}
				}
			}
			else
			{
				switch (fileInRevision.ActionThatHappened)
				{
					case FileInRevision.Action.Added:
						changes.Add(new DefaultChangeReport(fileInRevision.FullPath, "Added"));
						break;
					case FileInRevision.Action.Modified:
						changes.Add(new DefaultChangeReport(fileInRevision.FullPath, "Changed"));
						break;
					case FileInRevision.Action.Deleted:
						changes.Add(new DefaultChangeReport(fileInRevision.FullPath, "Deleted"));
						break;
					default:
						Debug.Fail("Found unexpected FileInRevision Action.");
						break;

				}
			}
		}
	}
}