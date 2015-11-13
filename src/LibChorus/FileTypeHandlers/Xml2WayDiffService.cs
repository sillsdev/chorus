using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge.xml.generic.xmldiff;

namespace Chorus.FileTypeHandlers
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
		public static IEnumerable<IChangeReport> ReportDifferences(
			FileInRevision parentFileInRevision, Dictionary<string, byte[]> parentIndex,
			FileInRevision childFileInRevision, Dictionary<string, byte[]> childIndex)
		{
			const string deletedAttr = "dateDeleted=";
			var changeReports = new List<IChangeReport>();
			var enc = Encoding.UTF8;
			var parentDoc = new XmlDocument();
			var childDoc = new XmlDocument();
			foreach (var kvpParent in parentIndex)
			{
				var parentKey = kvpParent.Key;
				var parentValue = kvpParent.Value;
				byte[] childValue;
				if (childIndex.TryGetValue(parentKey, out childValue))
				{
					childIndex.Remove(parentKey);
					// It is faster to skip this and just turn them into strings and then do the check.
					//if (!parentValue.Where((t, i) => t != childValue[i]).Any())
					//    continue; // Bytes are all the same.

					var parentStr = enc.GetString(parentValue);
					var childStr = enc.GetString(childValue);
					if (parentStr == childStr)
						continue; // Route tested

					// May have added 'dateDeleted' attr, in which case treat it as deleted, not changed.
					// NB: This is only for Lift diffing, not FW diffing,
					// so figure a way to have the client do this kind of check.
					if (childStr.Contains(deletedAttr))
					{
						// Only report it as deleted, if it is not already marked as deleted in the parent.
						if (!parentStr.Contains(deletedAttr))
						{
							// Route tested
							changeReports.Add(new XmlDeletionChangeReport(
															parentFileInRevision,
															XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(kvpParent.Value), parentDoc),
															XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));

						}
					}
					else
					{
						try
						{
							if (XmlUtilities.AreXmlElementsEqual(new XmlInput(childStr), new XmlInput(parentStr)))
								continue; // Route tested
						}
						catch (Exception error)
						{
							// Route not tested, and I don't know how to get XmlUtilities.AreXmlElementsEqual to throw.
							changeReports.Add(new ErrorDeterminingChangeReport(
															parentFileInRevision,
															childFileInRevision,
															XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
															XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc),
															error));
							continue;
						}
						// NB: This comment is from the class description of XmlChangedRecordReport
						// This may only be useful for quick, high-level identification that an entry changed,
						// leaving *what* changed to a second pass, if needed by the user
						// I (RBR), believe this can overproduce, otherwise useless change reports in a merge, if the merger uses it.
						// Route tested
						changeReports.Add(new XmlChangedRecordReport(
														parentFileInRevision,
														childFileInRevision,
														XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
														XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));
					}
				}
				else
				{
					//don't report deletions where there was a tombstone, but then someone removed the entry (which is what FLEx does)
					var parentStr = enc.GetString(parentValue);
					if (parentStr.Contains(deletedAttr))
					{
						// Route tested
						continue;
					}
					// Route tested
					changeReports.Add(new XmlDeletionChangeReport(
													parentFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(kvpParent.Value), parentDoc),
													null)); // Child Node? How can we put it in, if it was deleted?
				}
			}

			// Values that are still in childIndex are new,
			// since values that were not new have been removed by now.
			foreach (var child in childIndex.Values)
			{
				// Route tested
				changeReports.Add(new XmlAdditionChangeReport(
												childFileInRevision,
											XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(child), childDoc)));
			}

			return changeReports;
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