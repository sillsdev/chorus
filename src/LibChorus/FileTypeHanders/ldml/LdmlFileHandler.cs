using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;
using Palaso.Xml;

namespace Chorus.FileTypeHanders.ldml
{
	///<summary>
	/// Implementation of the IChorusFileTypeHandler interface to handle LDML files
	///</summary>
	public class LdmlFileHandler : IChorusFileTypeHandler
	{
		internal LdmlFileHandler()
		{}

		private const string kExtension = "ldml";

		#region Implementation of IChorusFileTypeHandler

		public bool CanDiffFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, kExtension);
		}

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			if (mergeOrder == null)
				throw new ArgumentNullException("mergeOrder");

			var merger = new XmlMerger(mergeOrder.MergeSituation);
			SetupElementStrategies(merger);

			merger.EventListener = mergeOrder.EventListener;
			var result = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			using (var writer = XmlWriter.Create(mergeOrder.pathToOurs, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				var nameSpaceManager = new XmlNamespaceManager(new NameTable());
				nameSpaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
				nameSpaceManager.AddNamespace("fw", "urn://fieldworks.sil.org/ldmlExtensions/v1");

				var readerSettings = new XmlReaderSettings
										{
											NameTable = nameSpaceManager.NameTable,
											IgnoreWhitespace = true,
											ConformanceLevel = ConformanceLevel.Auto,
											ValidationType = ValidationType.None,
											XmlResolver = null,
											CloseInput = true,
											ProhibitDtd = false
										};
				using (var nodeReader = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(result.MergedNode.OuterXml)), readerSettings))
				{
					writer.WriteNode(nodeReader, false);
				}
			}
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return new IChangeReport[] { new DefaultChangeReport(parent, child, "Edited") };
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			// TODO: Add better presenter.
			return report is ErrorDeterminingChangeReport
					? (IChangePresenter) report
					: new DefaultChangePresenter(report, repository);
		}

		/// <summary>
		/// return null if valid, otherwise nice verbose description of what went wrong
		/// </summary>
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			try
			{
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					if (reader.LocalName == "ldml")
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{
						}
					}
					else
					{
						throw new InvalidOperationException("Not an LDML file.");
					}
				}
			}
			catch (Exception error)
			{
				return error.Message;
			}
			return null;
		}

		/// <summary>
		/// This is like a diff, but for when the file is first checked in.  So, for example, a dictionary
		/// handler might list any the words that were already in the dictionary when it was first checked in.
		/// </summary>
		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return kExtension;
		}

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		public uint MaximumFileSize
		{
			get { return UInt32.MaxValue; }
		}

		#endregion

		private static void SetupElementStrategies(XmlMerger merger)
		{
			merger.MergeStrategies.SetStrategy("ldml", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("identity", ElementStrategy.CreateSingletonElement());
			// Child elements of "identity".
			merger.MergeStrategies.SetStrategy("version", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("generation", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("language", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("script", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("territory", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("variant", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("collations", ElementStrategy.CreateSingletonElement());
			// Child element of collations
			var strategy = ElementStrategy.CreateSingletonElement();
			strategy.IsAtomic = true; // I (RBR) think it would be suicidal to try and merge this element.
			merger.MergeStrategies.SetStrategy("collation", strategy);
			// Special "xmlns:palaso"
			strategy = new ElementStrategy(false)
			{
				IsAtomic = true, // May not be needed...
				MergePartnerFinder = new FindByMatchingAttributeNames(new HashSet<string> { "xmlns:palaso" })
			};
			merger.MergeStrategies.SetStrategy("special_xmlns:palaso", strategy);
			// Special "xmlns:fw"
			strategy = new ElementStrategy(false)
			{
				IsAtomic = true, // Really is needed. At least it is for some child elements.
				MergePartnerFinder = new FindByMatchingAttributeNames(new HashSet<string> { "xmlns:fw" })
			};
			merger.MergeStrategies.SetStrategy("special_xmlns:fw", strategy);
		}
	}
}
