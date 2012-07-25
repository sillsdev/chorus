using System.Collections.Generic;
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

			differ.ReportDifferencesToListener();

			return changeAndConflictAccumulator.Changes;
		}

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

			differ.ReportDifferencesToListener();

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
			var changeAndConflictAccumulator = listener ?? new ChangeAndConflictAccumulator();
			var differ = Xml2WayDiffer.CreateFromFiles(
				parentPathname, childPathname,
				changeAndConflictAccumulator,
				firstElementMarker,
				recordMarker, identfierAttribute);

			parentIndex = differ.ReportDifferencesToListener();

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

			differ.ReportDifferencesToListener();

			return changeAndConflictAccumulator is ChangeAndConflictAccumulator
					? ((ChangeAndConflictAccumulator) changeAndConflictAccumulator).Changes
					: null;// new List<IChangeReport>(); // unit tests use impl class that has no "Changes" property.
		}
	}
}