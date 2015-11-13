using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
using SIL.Progress;

namespace Chorus.retrieval
{
	/// <summary>
	/// Works with the merge/diff system to give details on what was done in the revision
	/// </summary>
	public class RevisionInspector
	{
		public  HgRepository Repository{get;private set;}
		private readonly ChorusFileTypeHandlerCollection _fileHandlerCollection;

		public IProgress ProgressIndicator { get; set; }

		public RevisionInspector(HgRepository repository, ChorusFileTypeHandlerCollection fileHandlerCollection)
		{
			Repository = repository;
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
				foreach (var fileInRevision in Repository.GetFilesInRevision(revision))
				{
					CollectChangesInFile(fileInRevision, null, changes);
				}
			}

			else
			{
				IEnumerable<RevisionNumber> parentRevs = revision.GetLocalNumbersOfParents();
				foreach (RevisionNumber parentRev in parentRevs)
				{
					foreach (var fileInRevision in Repository.GetFilesInRevision(revision)
						.Where(fileInRevision => parentRevs.Count() == 1
							|| fileInRevision.FullPath.ToLowerInvariant().EndsWith(".chorusnotes")))
					{
						CollectChangesInFile(fileInRevision, parentRev.LocalRevisionNumber, changes);
					}
				}
				if (parentRevs.Count() > 1)
					changes = new List<IChangeReport>(changes.Distinct());
			}

			return changes;
		}

		private void CollectChangesInFile(FileInRevision fileInRevision, string parentRev, List<IChangeReport> changes)
		{
			var handler = _fileHandlerCollection.GetHandlerForDiff(fileInRevision.FullPath);
			//find, for example, a handler that can handle .lift dictionary, or a .wav sound file
			if (handler.CanDiffFile(fileInRevision.FullPath))//review: isn't that just asking again?
			{
				if (parentRev != null && fileInRevision.ActionThatHappened == FileInRevision.Action.Modified)
				{
					var parentFileInRevision = new FileInRevision(parentRev, Path.Combine(Repository.PathToRepo, fileInRevision.FullPath),
																  fileInRevision.ActionThatHappened);

					//pull the files out of the repository so we can read them
//                    using (var targetFile = fileInRevision.CreateTempFile(Repository))
//                    using (var parentFile = parentFileInRevision.CreateTempFile(Repository))
					{
						//run the differ which the handler provides, adding the changes to the cumulative
						//list we are gathering for this whole revision
						changes.AddRange(handler.Find2WayDifferences(parentFileInRevision, fileInRevision, Repository));
					}
				}
				else
				{
					try
					{
						using (var targetFile = fileInRevision.CreateTempFile(Repository))
						{
							changes.AddRange(handler.DescribeInitialContents(fileInRevision, targetFile));
						}
					}
					catch (Exception error)
					{
						changes.Add(new DefaultChangeReport(fileInRevision,
															"Error retrieving historical version. "+error.Message));
					}
				}
			}
			else
			{
				switch (fileInRevision.ActionThatHappened)
				{
					case FileInRevision.Action.Added:
						changes.Add(new DefaultChangeReport(fileInRevision, "Added"));
						break;
					case FileInRevision.Action.Modified:
						var parentFileInRevision = new FileInRevision(parentRev, Path.Combine(Repository.PathToRepo, fileInRevision.FullPath),
																	  FileInRevision.Action.Parent);
						changes.Add(new DefaultChangeReport(parentFileInRevision, fileInRevision, "Changed"));
						break;
					case FileInRevision.Action.Deleted:
						changes.Add(new DefaultChangeReport(fileInRevision, "Deleted"));
						break;
					default:
						Debug.Fail("Found unexpected FileInRevision Action.");
						break;

				}
			}
		}
	}
}