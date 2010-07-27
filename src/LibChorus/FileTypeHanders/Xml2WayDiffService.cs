using System.Collections.Generic;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
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
			string recordMarker, string identfierAttribute)
		{
			var changeAndConflictAccumulator = new ChangeAndConflictAccumulator();
			// Pulls the files out of the repository so we can read them.
			var differ = Xml2WayDiffer.CreateFromFileInRevision(
				parent,
				child,
				changeAndConflictAccumulator,
				repository,
				recordMarker,
				identfierAttribute);
			try
			{
				differ.ReportDifferencesToListener();
			}
			catch
			{
				// Eat exception.
			}

			return changeAndConflictAccumulator.Changes;
		}

		/// <summary>
		/// Report the differences between two versions of files in the repository.
		/// </summary>
		/// <returns>Zero or more change reports.</returns>
		public static IEnumerable<IChangeReport> ReportDifferences(IMergeEventListener listener,
			string parentXmlData, string childXmlData,
			string recordMarker, string identfierAttribute)
		{
			using (var tempParent = new TempFile(parentXmlData))
			{
				using (var tempChild = new TempFile(childXmlData))
				{
					var changeAndConflictAccumulator = listener ?? new ChangeAndConflictAccumulator();
					// Pulls the files out of the repository so we can read them.
					var differ = Xml2WayDiffer.CreateFromFiles(
						tempParent.Path,
						tempChild.Path,
						changeAndConflictAccumulator,
						recordMarker,
						identfierAttribute);
					try
					{
						differ.ReportDifferencesToListener();
					}
					catch
					{
						// Eat exception.
					}

					if (changeAndConflictAccumulator is ChangeAndConflictAccumulator)
						return ((ChangeAndConflictAccumulator) changeAndConflictAccumulator).Changes;
					return null; // unit tests use impl class that has no "Changes" property.
				}
			}
		}
	}
}