using System;
using System.Collections.Generic;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// This service class performs the various high-level steps needed to do a 2-way xml diff.
	/// </summary>
	public static class Xml2WayDiffService
	{
		/// <summary>
		/// Report the differences between two versions of files in the repository.
		/// </summary>
		/// <returns>Zero or more change reports.</returns>
		public static IEnumerable<IChangeReport> ReportDifferences(HgRepository repository,
			FileInRevision parent, FileInRevision child,
			string firstElementMarker,
			string recordMarker, string identfierAttribute)
		{
			var changeAndConflictAccumulator = new ChangeAndConflictAccumulator();
			// Pulls the files out of the repository so we can read them.
			var differ = Xml2WayDiffer.CreateFromFileInRevision(
				parent,
				child,
				changeAndConflictAccumulator,
				repository,
				firstElementMarker,
				recordMarker,
				identfierAttribute);
			try
			{
				differ.ReportDifferencesToListener();
			}
			catch(Exception error)
			{
				// Eat exception.
				return new List<IChangeReport>(new[] {new ErrorDeterminingChangeReport(parent, child, null, null, error)});
			}

			return changeAndConflictAccumulator.Changes;
		}

		// Not used now, but keep it a bit longer. It was used to support tests.
		///// <summary>
		///// Report the differences between two versions of files in the repository.
		///// </summary>
		///// <returns>Zero or more change reports.</returns>
		//public static IEnumerable<IChangeReport> ReportDifferences(IMergeEventListener listener,
		//    string parentXmlData, string childXmlData,
		//    string recordMarker, string identfierAttribute)
		//{
		//    using (var tempParent = new TempFile(parentXmlData))
		//    using (var tempChild = new TempFile(childXmlData))
		//    {
		//        return ReportDifferences(tempParent.Path, tempChild.Path, listener, recordMarker, identfierAttribute);
		//    }
		//}

		/// <summary>
		/// Report the differences between two versions of files in the repository.
		/// </summary>
		/// <returns>Zero or more change reports.</returns>
		public static IEnumerable<IChangeReport> ReportDifferences(
			string parentPathname, string childPathname,
			IMergeEventListener listener,
			string firstElementMarker,
			string recordMarker, string identfierAttribute)
		{
			var changeAndConflictAccumulator = listener ?? new ChangeAndConflictAccumulator();
			var differ = Xml2WayDiffer.CreateFromFiles(
				parentPathname, childPathname,
				changeAndConflictAccumulator,
				firstElementMarker,
				recordMarker, identfierAttribute);
			try
			{
				differ.ReportDifferencesToListener();
			}
			catch
			{
				//REVIEW (from JH, 3/2012): why was this exception being swallowed?  We should always give a justification in the code. For
				//now, since I don't know why this was being swalled, I'm going to at least throw in debug mode.
				//anyone who sees it throw here should add a catch with the explicit exception, and then eventually we can get rid of this catch-all
#if DEBUG
				throw;
#endif
			}

			return changeAndConflictAccumulator is ChangeAndConflictAccumulator
					? ((ChangeAndConflictAccumulator) changeAndConflictAccumulator).Changes
					: null; // unit tests use impl class that has no "Changes" property.
		}

		/// <summary>
		/// Report the differences between two versions of files in the repository.
		/// </summary>
		/// <returns>Zero or more change reports.</returns>
		public static IEnumerable<IChangeReport> ReportDifferencesForMerge(
			string parentPathname, string childPathname,
			IMergeEventListener listener,
			string firstElementMarker,
			string recordMarker, string identfierAttribute,
			out Dictionary<string, byte[]> parentIndex)
		{
			parentIndex = null;
			var changeAndConflictAccumulator = listener ?? new ChangeAndConflictAccumulator();
			var differ = Xml2WayDiffer.CreateFromFiles(
				parentPathname, childPathname,
				changeAndConflictAccumulator,
				firstElementMarker,
				recordMarker, identfierAttribute);
			try
			{
				parentIndex = differ.ReportDifferencesToListener();
			}
			catch
			{
//REVIEW (from JH, 3/2012): why was this exception being swallowed?  We should always give a justification in the code. For
				//now, since I don't know why this was being swalled, I'm going to at least throw in debug mode.
				//anyone who sees it throw here should add a catch with the explicit exception, and then eventually we can get rid of this catch-all
#if DEBUG
				throw;
#endif
			}

			return changeAndConflictAccumulator is ChangeAndConflictAccumulator
					? ((ChangeAndConflictAccumulator) changeAndConflictAccumulator).Changes
					: null; // unit tests use impl class that has no "Changes" property.
		}

		/// <summary>
		/// Report the differences between two versions of files in the repository.
		/// </summary>
		/// <returns>Zero or more change reports.</returns>
		public static IEnumerable<IChangeReport> ReportDifferences(
			Dictionary<string, byte[]> parentIndex, string childPathname,
			IMergeEventListener listener,
			string firstElementMarker,
			string recordMarker, string identfierAttribute)
		{
			var changeAndConflictAccumulator = listener ?? new ChangeAndConflictAccumulator();
			var differ = Xml2WayDiffer.CreateFromMixed(
				parentIndex, childPathname,
				changeAndConflictAccumulator,
				firstElementMarker,
				recordMarker, identfierAttribute);
			try
			{
				differ.ReportDifferencesToListener();
			}
			catch
			{
				//REVIEW (from JH, 3/2012): why was this exception being swallowed?  We should always give a justification in the code. For
				//now, since I don't know why this was being swalled, I'm going to at least throw in debug mode.
				//anyone who sees it throw here should add a catch with the explicit exception, and then eventually we can get rid of this catch-all
#if DEBUG
				throw;
#endif
			}

			return changeAndConflictAccumulator is ChangeAndConflictAccumulator
					? ((ChangeAndConflictAccumulator) changeAndConflictAccumulator).Changes
					: null;// new List<IChangeReport>(); // unit tests use impl class that has no "Changes" property.
		}
	}
}