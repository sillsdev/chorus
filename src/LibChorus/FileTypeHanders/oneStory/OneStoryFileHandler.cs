using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.xml;
using Chorus.Utilities;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Chorus.FileTypeHanders.oneStory
{
	public class OneStoryFileHandler : IChorusFileTypeHandler
	{
		internal OneStoryFileHandler()
		{}

		public const string CstrAppName = "StoryEditor.exe";

		private bool OneStoryAssemblyIsAvailable
		{
			get
			{
				return File.Exists(Path.Combine(
									   ExecutionEnvironment.DirectoryOfExecutingAssembly, CstrAppName));
			}
		}

		protected bool HasOneStoryExtension(string strPathToFile)
		{
			return (Path.GetExtension(strPathToFile).ToLower() == ".onestory");
		}

		public bool CanDiffFile(string pathToFile)
		{
			return OneStoryAssemblyIsAvailable && HasOneStoryExtension(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return HasOneStoryExtension(pathToFile);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return OneStoryAssemblyIsAvailable && HasOneStoryExtension(pathToFile);
		}

		public bool CanValidateFile(string pathToFile)
		{
			return false;
		}
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			throw new NotImplementedException();
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation);
			SetupElementStrategies(merger);

			merger.EventListener = mergeOrder.EventListener;
			var result = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);

			// use linq to write the merged XML out, so it is as much as possible like the format that the OneStory editor
			//  (which uses linq also) writes out. Do this so we maximally keep indentation the same, so that if you do
			//  "view changesets" in TortoiseHG (a line-by-line differencer) it will highlight bona fide differences as much
			//  as possible.
			XDocument doc = XDocument.Parse(result.MergedNode.OuterXml);
			doc.Save(mergeOrder.pathToOurs);
		}

		private void SetupElementStrategies(XmlMerger merger)
		{
			merger.MergeStrategies.SetStrategy("StoryProject", ElementStrategy.CreateSingletonElement());

			//this handles the meta data
			merger.MergeStrategies.SetStrategy("Members", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Member", ElementStrategy.CreateForKeyedElement("memberKey", false));

			merger.MergeStrategies.SetStrategy("Languages", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("LanguageInfo", ElementStrategy.CreateForKeyedElement("lang", false));

			// AdaptIt configuration settings (we use AI for BT)
			merger.MergeStrategies.SetStrategy("AdaptItConfigurations", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("AdaptItConfiguration", ElementStrategy.CreateForKeyedElement("BtDirection", false));

			// Language and culture notes
			merger.MergeStrategies.SetStrategy("LnCNotes", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("LnCNote", ElementStrategy.CreateForKeyedElement("guid", false));

			// story sets and stories
			merger.MergeStrategies.SetStrategy("stories", ElementStrategy.CreateForKeyedElement("SetName", false));

			var elementStrategyStory = ElementStrategy.CreateForKeyedElement("guid", false);
			elementStrategyStory.AttributesToIgnoreForMerging.Add("stageDateTimeStamp");
			merger.MergeStrategies.SetStrategy("story", elementStrategyStory);

			// the rest is used only if the same story was edited by two or more people at the same time
			//  not supposed to happen, but let's be safer
			merger.MergeStrategies.SetStrategy("CraftingInfo", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("StoryCrafter", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("ProjectFacilitator", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Consultant", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Coach", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("StoryPurpose", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("ResourcesUsed", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("MiscellaneousStoryInfo", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("BackTranslator", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("TestsRetellings", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("TestRetelling", ElementStrategy.CreateForKeyedElement("memberID", true));
			merger.MergeStrategies.SetStrategy("TestsTqAnswers", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("TestTqAnswer", ElementStrategy.CreateForKeyedElement("memberID", true));

			merger.MergeStrategies.SetStrategy("TransitionHistory", ElementStrategy.CreateSingletonElement());

			// this doesn't need a merge strategy, because it's always add-only (and somehow on one user's machine
			//  OSE was spitting out a whole bunch of these with the same TransitionDateTime...
			// merger.MergeStrategies.SetStrategy("StateTransition", ElementStrategy.CreateForKeyedElement("TransitionDateTime", true));

			merger.MergeStrategies.SetStrategy("Verses", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Verse", ElementStrategy.CreateForKeyedElement("guid", true));

			merger.MergeStrategies.SetStrategy("StoryLine", ElementStrategy.CreateForKeyedElement("lang", false));

			merger.MergeStrategies.SetStrategy("Anchors", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Anchor", ElementStrategy.CreateForKeyedElement("jumpTarget", false));

			merger.MergeStrategies.SetStrategy("ExegeticalHelps", ElementStrategy.CreateSingletonElement());
			// there can be multiple exegeticalHelp elements, but a) their order doesn't matter and b) they don't need a key
			//  I think if I left this uncommented, then it would only allow one and if another user added one, it
			//  would just replace the one that's there... (i.e. I think that's what ElementStrategy.CreateSingletonElement
			//  means, so... commenting out):
			// merger.MergeStrategies.SetStrategy("exegeticalHelp", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("TestQuestions", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("TestQuestion", ElementStrategy.CreateForKeyedElement("guid", false));
			merger.MergeStrategies.SetStrategy("TestQuestionLine", ElementStrategy.CreateForKeyedElement("lang", false));

			merger.MergeStrategies.SetStrategy("Answers", ElementStrategy.CreateSingletonElement());
			// now the answer and retelling have a 2nd attribute which uniquely defines a singleton
#if !UseSingleAttribute
			var strategy = new ElementStrategy(true)
									{
										MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "memberID", "lang" })
									};
			merger.MergeStrategies.ElementStrategies.Add("Answer", strategy);
#else
			merger.MergeStrategies.SetStrategy("answer", ElementStrategy.CreateForKeyedElement("memberID", true));
#endif

			merger.MergeStrategies.SetStrategy("Retellings", ElementStrategy.CreateSingletonElement());
			// now the answer and retelling have a 2nd attribute which uniquely defines a singleton
#if !UseSingleAttribute
			strategy = new ElementStrategy(true)
									{
										MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "memberID", "lang" })
									};
			merger.MergeStrategies.ElementStrategies.Add("Retelling", strategy);
#else
			merger.MergeStrategies.SetStrategy("Retelling", ElementStrategy.CreateForKeyedElement("memberID", true));
#endif

			merger.MergeStrategies.SetStrategy("ConsultantNotes", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("ConsultantConversation", ElementStrategy.CreateForKeyedElement("guid", true));

			var elementStrategyConNote = ElementStrategy.CreateForKeyedElement("guid", true);
			elementStrategyConNote.AttributesToIgnoreForMerging.Add("timeStamp");
			merger.MergeStrategies.SetStrategy("ConsultantNote", elementStrategyConNote);

			merger.MergeStrategies.SetStrategy("CoachNotes", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("CoachConversation", ElementStrategy.CreateForKeyedElement("guid", true));

			var elementStrategyCoaNote = ElementStrategy.CreateForKeyedElement("guid", true);
			elementStrategyCoaNote.AttributesToIgnoreForMerging.Add("timeStamp");
			merger.MergeStrategies.SetStrategy("CoachNote", elementStrategyCoaNote);
		}

		private XmlNode _projFile;
		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			var listener = new ChangeAndConflictAccumulator();
			//pull the files out of the repository so we can read them
			using (var childFile = child.CreateTempFile(repository))
			using (var parentFile = parent.CreateTempFile(repository))
			{
				var differ = OneStoryDiffer.CreateFromFiles(parent, child, repository.PathToRepo, parentFile.Path, childFile.Path, listener);
				differ.ReportDifferencesToListener(out _projFile);
				return listener.Changes;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if ((report as IXmlChangeReport) != null)
				return new OneStoryChangePresenter(report as IXmlChangeReport, _projFile, repository.PathToRepo);

			return new DefaultChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			//this is never called because we said we don't present diffs; review is handled some other way
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "onestory";
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
	}
}
