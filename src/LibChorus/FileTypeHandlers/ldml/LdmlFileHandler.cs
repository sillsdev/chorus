using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using SIL.Extensions;
using SIL.IO;
using SIL.Progress;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.ldml
{
	///<summary>
	/// Implementation of the IChorusFileTypeHandler interface to handle LDML files
	///</summary>
	[Export(typeof(IChorusFileTypeHandler))]
	public class LdmlFileHandler : IChorusFileTypeHandler
	{
		internal LdmlFileHandler()
		{}

		private const string Extension = "ldml";

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
			return PathHelper.CheckValidPathname(pathToFile, Extension);
		}

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			if (mergeOrder == null)
				throw new ArgumentNullException(nameof(mergeOrder));

			bool addedCollationAttr;
			PreMergeFile(mergeOrder, out addedCollationAttr);

			var merger = new XmlMerger(mergeOrder.MergeSituation);
			SetupElementStrategies(merger);

			merger.EventListener = mergeOrder.EventListener;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			var result = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			using (var writer = XmlWriter.Create(mergeOrder.pathToOurs, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				var nameSpaceManager = new XmlNamespaceManager(new NameTable());
				nameSpaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
				nameSpaceManager.AddNamespace("palaso2", "urn://palaso.org/ldmlExtensions/v2");
				nameSpaceManager.AddNamespace("fw", "urn://fieldworks.sil.org/ldmlExtensions/v1");
				nameSpaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");

				var readerSettings = CanonicalXmlSettings.CreateXmlReaderSettings(ConformanceLevel.Auto);
				readerSettings.NameTable = nameSpaceManager.NameTable;
				readerSettings.XmlResolver = null;
				if (addedCollationAttr)
				{
					// Remove the optional 'key' attr we added.
					var adjustedCollation = result.MergedNode.SelectSingleNode("collations")
						.SelectNodes("collation")
						.Cast<XmlNode>().FirstOrDefault(collation => collation.Attributes["type"].Value == "standard");
					if (adjustedCollation != null)
					{
						adjustedCollation.Attributes.Remove(adjustedCollation.Attributes["type"]);
					}
				}
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
				using (var reader = XmlReader.Create(pathToFile, CanonicalXmlSettings.CreateXmlReaderSettings()))
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

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return Extension;
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

		internal static void SetupElementStrategies(XmlMerger merger)
		{
			merger.MergeStrategies.ElementToMergeStrategyKeyMapper = new LdmlElementToMergeStrategyKeyMapper();

			// See: Palaso repo: SIL.WritingSystems\LdmlDataMapper.cs
			var strategy = ElementStrategy.CreateSingletonElement();
			strategy.ContextDescriptorGenerator = new LdmlContextGenerator();
			merger.MergeStrategies.SetStrategy("ldml", strategy);
			// Child elements of ldml root.

			merger.MergeStrategies.SetStrategy("identity", ElementStrategy.CreateSingletonElement());
			// Child elements of "identity".
			merger.MergeStrategies.SetStrategy("version", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("generation", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("language", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("script", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("territory", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("variant", ElementStrategy.CreateSingletonElement());

			// sil:special can occur several times throughout the file
			merger.MergeStrategies.SetStrategy("special_xmlns:sil", new ElementStrategy(false)
			{
				MergePartnerFinder = new FindByMatchingAttributeNames(new HashSet<string> { "xmlns:sil" })
			});
			merger.MergeStrategies.SetStrategy("sil:identity", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("localeDisplayNames", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("layout", ElementStrategy.CreateSingletonElement());
			// Child element of "layout".
			merger.MergeStrategies.SetStrategy("orientation", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			});

			merger.MergeStrategies.SetStrategy("contextTransforms", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("characters", new ElementStrategy(false)
			{
				ChildOrderPolicy = new NotSignificantOrderPolicy(),
				MergePartnerFinder = new FindFirstElementWithSameName()
			});
			// handle one child element of characters - exemplarCharacters
			strategy = new ElementStrategy(false)
			{
				IsAtomic = true, // Trying to merge multiple changes to the character list would be painful
				MergePartnerFinder = new OptionalKeyAttrFinder("type", new FindFirstElementWithZeroAttributes())
			};
			merger.MergeStrategies.SetStrategy("exemplarCharacters", strategy);

			merger.MergeStrategies.SetStrategy("delimiters", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			});

			merger.MergeStrategies.SetStrategy("dates", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("numbers", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			});

			merger.MergeStrategies.SetStrategy("units", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("listPatterns", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("collations", ElementStrategy.CreateSingletonElement());
			// Child element of collations
			strategy = new ElementStrategy(false)
			{
				IsAtomic = true, // I (RBR) think it would be suicidal to try and merge this element.
				MergePartnerFinder = new FindByKeyAttribute("type")
			};
			merger.MergeStrategies.SetStrategy("collation", strategy);
			// Child of 'collation' element (They exist, but we don't care what they are, as long as the parent is 'atomic'.

			merger.MergeStrategies.SetStrategy("posix", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("segmentations", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("rbnf", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("metadata", ElementStrategy.CreateSingletonElement());


			// See: Palaso repo: SIL.WritingSystems\LdmlDataMapper.cs
			// There currently are up to three 'special' child elements of the 'ldml' root element.
			// Special "xmlns:palaso" attr
			strategy = new ElementStrategy(false)
			{
				IsAtomic = true, // May not be needed...
				MergePartnerFinder = new FindByMatchingAttributeNames(new HashSet<string> { "xmlns:palaso" })
			};
			merger.MergeStrategies.SetStrategy("special_xmlns:palaso", strategy);
			/* Not needed, as long as the parent is 'atomic'.
			// Children of 'special' xmlns:palaso
			// palaso:abbreviation
			merger.MergeStrategies.SetStrategy("palaso:abbreviation", ElementStrategy.CreateSingletonElement());
			// palaso:defaultFontFamily
			merger.MergeStrategies.SetStrategy("palaso:defaultFontFamily", ElementStrategy.CreateSingletonElement());
			// palaso:defaultFontSize
			merger.MergeStrategies.SetStrategy("palaso:defaultFontSize", ElementStrategy.CreateSingletonElement());
			// palaso:defaultKeyboard
			merger.MergeStrategies.SetStrategy("palaso:defaultKeyboard", ElementStrategy.CreateSingletonElement());
			// palaso:isLegacyEncoded
			merger.MergeStrategies.SetStrategy("palaso:isLegacyEncoded", ElementStrategy.CreateSingletonElement());
			// palaso:languageName
			merger.MergeStrategies.SetStrategy("palaso:languageName", ElementStrategy.CreateSingletonElement());
			// palaso:spellCheckingId
			merger.MergeStrategies.SetStrategy("palaso:spellCheckingId", ElementStrategy.CreateSingletonElement());
			// palaso:version
			merger.MergeStrategies.SetStrategy("palaso:version", ElementStrategy.CreateSingletonElement());
			*/

			// See: Palaso repo: SIL.WritingSystems\LdmlDataMapper.cs
			// special "xmlns:palaso2" attr: want to merge knownKeyboards child. So the root element is not atomic.
			strategy = new ElementStrategy(false)
			{
				MergePartnerFinder = new FindByMatchingAttributeNames(new HashSet<string> {"xmlns:palaso2"})
			};
			merger.MergeStrategies.SetStrategy("special_xmlns:palaso2", strategy);
			// Children of 'strategy' xmlns:palaso2
			// palaso2:knownKeyboards:
			merger.MergeStrategies.SetStrategy("palaso2:knownKeyboards", ElementStrategy.CreateSingletonElement());
			// Multiple children of "palaso2:knownKeyboards" element
			strategy = new ElementStrategy(false)
			{
				MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> {"layout", "locale"})
			};
			merger.MergeStrategies.SetStrategy("palaso2:keyboard", strategy);
			merger.MergeStrategies.SetStrategy("palaso2:version", ElementStrategy.CreateSingletonElement());

			// Special "xmlns:fw" attr (See FW source file: Src\Common\CoreImpl\PalasoWritingSystemManager.cs
			strategy = new ElementStrategy(false)
			{
				IsAtomic = true, // Really is needed. At least it is for some child elements.
				MergePartnerFinder = new FindByMatchingAttributeNames(new HashSet<string> { "xmlns:fw" })
			};
			merger.MergeStrategies.SetStrategy("special_xmlns:fw", strategy);

			// Children for top level 'special' xmlns:sil
			merger.MergeStrategies.SetStrategy("sil:external-resources", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("sil:kbd", new ElementStrategy(false)
				{
					IsAtomic = true,
					MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string>{"id", "alt"})
		}
			);
			merger.MergeStrategies.SetStrategy("sil:font", new ElementStrategy(false)
				{
					IsAtomic = true,
					MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string>{"name", "alt"})
				}
			);
			merger.MergeStrategies.SetStrategy("sil:spellcheck", new ElementStrategy(false)
				{
					IsAtomic = true,
					MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string>{"type", "alt"})
				}
			);
			merger.MergeStrategies.SetStrategy("sil:transform", new ElementStrategy(false)
				{
					IsAtomic = true,
					MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "from", "to", "type", "direction", "function", "alt" })
				}
			);
		}

		/// <summary>
		/// handles that date business, so it doesn't overwhelm the poor user with conflict reports
		/// </summary>
		/// <param name="mergeOrder"></param>
		/// <param name="addedCollationAttr"></param>
		private static void PreMergeFile(MergeOrder mergeOrder, out bool addedCollationAttr)
		{
			addedCollationAttr = false;
			var ourDoc = File.Exists(mergeOrder.pathToOurs) && File.ReadAllText(mergeOrder.pathToOurs).Contains("<ldml>") ? XDocument.Load(mergeOrder.pathToOurs) : null;
			var theirDoc = File.Exists(mergeOrder.pathToTheirs) && File.ReadAllText(mergeOrder.pathToTheirs).Contains("<ldml>") ? XDocument.Load(mergeOrder.pathToTheirs) : null;
			var commonDoc = File.Exists(mergeOrder.pathToCommonAncestor) && File.ReadAllText(mergeOrder.pathToCommonAncestor).Contains("<ldml>") ? XDocument.Load(mergeOrder.pathToCommonAncestor) : null;

			if (ourDoc == null || theirDoc == null)
				return;

			// Add optional key attr and default value on 'collation' element that has no 'type' attr.
			var ourDocDefaultCollation = GetDefaultCollationNode(ourDoc);
			var theirDocDefaultCollation = GetDefaultCollationNode(theirDoc);
			if (commonDoc != null)
			{
				var commonDocDefaultCollation = GetDefaultCollationNode(commonDoc);
				if (commonDocDefaultCollation != null)
				{
					if (ourDocDefaultCollation != null || theirDocDefaultCollation != null)
					{
						// add type attribute to the commonDoc only when we are certain it will also be added to at least one modified document
						commonDocDefaultCollation.Add(new XAttribute("type", "standard"));
						commonDoc.Save(mergeOrder.pathToCommonAncestor);
					}
				}
			}
			if (ourDocDefaultCollation != null)
			{
				ourDocDefaultCollation.Add(new XAttribute("type", "standard"));
				ourDoc.Save(mergeOrder.pathToOurs);
				addedCollationAttr = true;
			}
			if (theirDocDefaultCollation != null)
			{
				theirDocDefaultCollation.Add(new XAttribute("type", "standard"));
				theirDoc.Save(mergeOrder.pathToTheirs);
				addedCollationAttr = true;
			}

			// Pre-merge <generation> date attr to newest, plus one second.
			string ourRawGenDate;
			var ourGenDate = GetGenDate(ourDoc, out ourRawGenDate);
			string theirRawGenDate;
			var theirGenDate = GetGenDate(theirDoc, out theirRawGenDate);

			var newestGenDatePlusOneSecond = (ourGenDate == theirGenDate)
				? ourGenDate
				: ((ourGenDate > theirGenDate) ? ourGenDate : theirGenDate);
			newestGenDatePlusOneSecond = newestGenDatePlusOneSecond.AddSeconds(1);
			// date="2012-06-08T09:36:30"
			var newestRawGenDatePlusOneSecond = newestGenDatePlusOneSecond.ToISO8601TimeFormatWithUTCString();

			// Write it out as one second newer than newest of the two, since merging does change it.
			var ourData = File.ReadAllText(mergeOrder.pathToOurs).Replace(ourRawGenDate, newestRawGenDatePlusOneSecond);
			File.WriteAllText(mergeOrder.pathToOurs, ourData);
			var theirData = File.ReadAllText(mergeOrder.pathToTheirs).Replace(theirRawGenDate, newestRawGenDatePlusOneSecond);
			File.WriteAllText(mergeOrder.pathToTheirs, theirData);
		}

		private static XElement GetDefaultCollationNode(XDocument currentDocument)
		{
			var rootNode = currentDocument.Root;
			if (rootNode == null)
				return null;
			var collationsNode = rootNode.Element("collations");
			if (collationsNode == null)
				return null;
			var collationNodes = collationsNode.Elements("collation").ToList();
			if (collationNodes.Count == 0)
				return null;
			return collationNodes.FirstOrDefault(collation => collation.Attribute("type") == null);
		}

		private static DateTime GetGenDate(XDocument doc, out string rawGenDate)
		{
			rawGenDate = doc.Root.Element("identity").Element("generation").Attribute("date").Value;

			return DateTimeExtensions.ParseISO8601DateTime(rawGenDate);
		}
	}
}
