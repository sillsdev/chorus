using System;
using System.Collections.Generic;
using System.Text;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.retrieval
{
	//do we want this?

	/// <summary>
	/// Adds chorus-specific stuff to the standard revision class,
	/// like awareness of whether the revision was a merge, had
	/// conflicts, and what the changes were
	/// </summary>
//    public class ChorusRevision: Revision
//    {
//
//        public IEnumerable<IChangeReport> GetChangeRecords(Revision descriptor)
//        {
//            var revisions = _repositoryManager.GetAllRevisions(ProgressIndicator);
//
//            //foreach(files in the revision)
//            //            {
//            //                using (
//            //                    var f = TempFile.TrackExisting(
//            //                        fileRetriever.RetrieveHistoricalVersionOfFile(.PathToFileInRepository, revision)) )
//            //                {
//            //                    //Set up a listener to get the changes
//            //
//            //                    //do a compare of this file to its ancestor
//            //                }
//            //            }
//
//            //todo: and how about conflict records?
//
//            return new List<IChangeReport>();
//        }
//    }
}
