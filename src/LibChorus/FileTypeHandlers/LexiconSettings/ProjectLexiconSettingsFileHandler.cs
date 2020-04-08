using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SIL.IO;
using SIL.Progress;
using SIL.Xml;
using Chorus.FileTypeHandlers.LexiconSettings;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using SIL.Code;

namespace Chorus.FileTypeHandlers.Settings
{
	///<summary>
	/// Implementation of the IChorusFileTypeHandler interface to handle project settings files
	///</summary>
	[Export(typeof(IChorusFileTypeHandler))]
	public class ProjectLexiconSettingsFileHandler : IChorusFileTypeHandler
	{
		internal ProjectLexiconSettingsFileHandler()
		{ }

		private const string Extension = "plsx";

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
			Guard.AgainstNull(mergeOrder, nameof(mergeOrder));

			bool addedCollationAttr;
			PreMergeFile(mergeOrder, out addedCollationAttr);

			var merger = new XmlMerger(mergeOrder.MergeSituation);
			SetupElementStrategies(merger);

			merger.EventListener = mergeOrder.EventListener;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			var result = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			using (var writer = XmlWriter.Create(mergeOrder.pathToOurs, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				var readerSettings = CanonicalXmlSettings.CreateXmlReaderSettings(ConformanceLevel.Auto);
				readerSettings.XmlResolver = null;
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
					? (IChangePresenter)report
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
					if (reader.LocalName == "ProjectLexiconSettings")
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{
						}
					}
					else
					{
						throw new InvalidOperationException("Not a project lexicon settings file.");
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
			// See: Palaso repo: SIL.LexiconUtils\ProjectLexiconSettingsDataMapper.cs
			var strategy = ElementStrategy.CreateSingletonElement();
			strategy.ContextDescriptorGenerator = new ProjectLexiconSettingsContextGenerator();
			merger.MergeStrategies.SetStrategy("ProjectLexiconSettings", strategy);
			// Child elements of lexicon project settings root.

			merger.MergeStrategies.SetStrategy("WritingSystems", ElementStrategy.CreateSingletonElement());
			// Child elements of "WritingSystems".

			merger.MergeStrategies.SetStrategy("WritingSystem", new ElementStrategy(false)
			{
				MergePartnerFinder = new FindByKeyAttribute("id")
			}
			);
			merger.MergeStrategies.SetStrategy("Abbreviation", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			}
			);
			merger.MergeStrategies.SetStrategy("LanguageName", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			}
			);
			merger.MergeStrategies.SetStrategy("ScriptName", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			}
			);
			merger.MergeStrategies.SetStrategy("RegionName", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			}
			);
			merger.MergeStrategies.SetStrategy("SpellCheckingId", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			}
			);
			merger.MergeStrategies.SetStrategy("LegacyMapping", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			}
			);
			merger.MergeStrategies.SetStrategy("Keyboard", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
			}
			);
			merger.MergeStrategies.SetStrategy("SystemCollation", new ElementStrategy(false)
			{
				IsAtomic = true,
				MergePartnerFinder = new FindFirstElementWithSameName()
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
			XDocument ourDoc = File.Exists(mergeOrder.pathToOurs) && File.ReadAllText(mergeOrder.pathToOurs).Contains("<ProjectLexiconSettings>") ? XDocument.Load(mergeOrder.pathToOurs) : null;
			XDocument theirDoc = File.Exists(mergeOrder.pathToTheirs) && File.ReadAllText(mergeOrder.pathToTheirs).Contains("<ProjectLexiconSettings>") ? XDocument.Load(mergeOrder.pathToTheirs) : null;

			if (ourDoc == null || theirDoc == null)
				return;

			var ourData = File.ReadAllText(mergeOrder.pathToOurs);
			File.WriteAllText(mergeOrder.pathToOurs, ourData);
			var theirData = File.ReadAllText(mergeOrder.pathToTheirs);
			File.WriteAllText(mergeOrder.pathToTheirs, theirData);
		}
	}
}
