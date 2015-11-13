using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHandlers.xml;

namespace Chorus.merge.xml.generic
{
	public class NodeMergeResult : ChangeAndConflictAccumulator
		{
				public XmlNode MergedNode { get; internal set; }
		}

	public class XmlMerger
	{
		public IMergeEventListener EventListener { get; set; }
		public MergeSituation MergeSituation{ get; set;}
		public MergeStrategies MergeStrategies { get; set; }

		/// <summary>
		/// The nodes we were merging on the last MergeChildren call; these (specifically _oursContext) are the
		/// nodes that are the basis of the Context we set in calling the Listener's EnteringContext method.
		/// They are used to allow any Conflict objects we generate to BuildHtmlDetails.
		/// </summary>
		private XmlNode _oursContext, _theirsContext, _ancestorContext;

		private IGenerateHtmlContext _htmlContextGenerator;

		public XmlMerger(MergeSituation mergeSituation)
		{
			MergeSituation = mergeSituation;
			EventListener  = new NullMergeEventListener();
			MergeStrategies = new MergeStrategies();
		}

		public NodeMergeResult Merge(XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			SendMergeHeartbeat();
			if (ours == null && theirs == null && ancestor == null)
				throw new InvalidOperationException("At least one node has to exist.");

			var result = new NodeMergeResult();
			var listener = EventListener as DispatchingMergeEventListener;
			if (listener == null)
			{
				var dispatcher = new DispatchingMergeEventListener();
				dispatcher.AddEventListener(result);
				if (EventListener != null)
				{
					dispatcher.AddEventListener(EventListener);
				}
				EventListener = dispatcher;
			}
			else
			{
				listener.AddEventListener(result);
			}

			if (XmlMergeService.RemoveAmbiguousChildNodes)
			{
				// Remove any duplicate child nodes in all three.
				XmlMergeService.RemoveAmbiguousChildren(EventListener, MergeStrategies, ours);
				XmlMergeService.RemoveAmbiguousChildren(EventListener, MergeStrategies, theirs);
				XmlMergeService.RemoveAmbiguousChildren(EventListener, MergeStrategies, ancestor);
			}

			if (ancestor == null)
			{
				if (ours == null)
				{
					// tested
					EventListener.ChangeOccurred(new XmlAdditionChangeReport(MergeSituation.PathToFileInRepository, theirs));
					result.MergedNode = theirs;
				}
				else if (theirs == null)
				{
					// tested
					EventListener.ChangeOccurred(new XmlAdditionChangeReport(MergeSituation.PathToFileInRepository, ours));
					result.MergedNode = ours;
				}
				else
				{
					// Both added.
					if (XmlUtilities.AreXmlElementsEqual(ours, theirs))
					{
						// Same thing. (tested)
						EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(MergeSituation.PathToFileInRepository, ours));
						result.MergedNode = ours;
					}
					else
					{
						// But, not the same thing.
						if (MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
						{
							// tested
							ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(ours.Name, ours, theirs, MergeSituation, MergeStrategies.GetElementStrategy(ours), MergeSituation.AlphaUserId));
							result.MergedNode = ours;

						}
						else
						{
							// tested
							ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(theirs.Name, theirs, ours, MergeSituation, MergeStrategies.GetElementStrategy(ours), MergeSituation.BetaUserId));
							result.MergedNode = theirs;
						}
					}
				}
				return result;
			}

			// ancestor exists
			if (ours == null && theirs == null)
			{
				// tested
				EventListener.ChangeOccurred(new XmlBothDeletionChangeReport(MergeSituation.PathToFileInRepository, ancestor));
				result.MergedNode = null;
				return result;
			}
			if (ours == null)
			{
				if (XmlUtilities.AreXmlElementsEqual(ancestor, theirs))
				{
					// tested
					EventListener.ChangeOccurred(new XmlDeletionChangeReport(MergeSituation.PathToFileInRepository, ancestor, theirs));
					result.MergedNode = null;
				}
				else
				{
					// tested
					ConflictOccurred(new RemovedVsEditedElementConflict(ancestor.Name, null, theirs, ancestor, MergeSituation, MergeStrategies.GetElementStrategy(ancestor), MergeSituation.BetaUserId));
					result.MergedNode = theirs;
				}
				return result;
			}
			if (theirs == null)
			{
				if (XmlUtilities.AreXmlElementsEqual(ancestor, ours))
				{
					// tested
					EventListener.ChangeOccurred(new XmlDeletionChangeReport(MergeSituation.PathToFileInRepository, ancestor, ours));
					result.MergedNode = null;
				}
				else
				{
					// tested
					ConflictOccurred(new EditedVsRemovedElementConflict(ancestor.Name, ours, null, ancestor, MergeSituation, MergeStrategies.GetElementStrategy(ancestor), MergeSituation.AlphaUserId));
					result.MergedNode = ours;
				}
				return result;
			}

			// All three nodes exist.
			MergeInner(ref ours, theirs, ancestor);
			result.MergedNode = ours;

			return result;
		}

		public XmlNode Merge(IMergeEventListener eventListener, XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			SendMergeHeartbeat();
			EventListener = eventListener;
			MergeInner(ref ours, theirs, ancestor);
			return ours;
		}

		/// <summary>
		/// Writes out a merge heartbeat to the stream that the process that launched hg is listening on.
		/// This lets us detect that work is still happening on a merge request.
		/// </summary>
		private static void SendMergeHeartbeat()
		{
			Console.Out.WriteLine(Properties.Resources.MergeHeartbeat);
		}

		internal void ConflictOccurred(IConflict conflict)
		{
			if (_htmlContextGenerator == null)
				_htmlContextGenerator = new SimpleHtmlGenerator();

			XmlMergeService.AddConflictToListener(
				EventListener,
				conflict,
				_oursContext,
				_theirsContext,
				_ancestorContext,
				_htmlContextGenerator);
		}

		internal void ConflictOccurred(IConflict conflict, XmlNode nodeToFindGeneratorFrom)
		{
			var contextDescriptorGenerator = GetContextDescriptorGenerator(nodeToFindGeneratorFrom);
			_htmlContextGenerator = (contextDescriptorGenerator as IGenerateHtmlContext) ?? new SimpleHtmlGenerator();

			XmlMergeService.AddConflictToListener(
				EventListener,
				conflict,
				_oursContext,
				_theirsContext,
				_ancestorContext,
				_htmlContextGenerator,
				this,
				nodeToFindGeneratorFrom);
		}

		/// <summary>
		/// Get a context based on the given node. This should only be used when we are at the outer level (typically the EventListener has no context to add).
		/// </summary>
		/// <param name="nodeToFindGeneratorFrom"></param>
		/// <returns></returns>
		internal ContextDescriptor GetContext(XmlNode nodeToFindGeneratorFrom)
		{
			return GetContextDescriptor(nodeToFindGeneratorFrom, GetContextDescriptorGenerator(nodeToFindGeneratorFrom));
		}

		private IGenerateContextDescriptor GetContextDescriptorGenerator(XmlNode nodeToFindGeneratorFrom)
		{
			return MergeStrategies.GetElementStrategy(nodeToFindGeneratorFrom).ContextDescriptorGenerator;
		}

		internal void WarningOccurred(IConflict warning)
		{
			if (_htmlContextGenerator == null)
				_htmlContextGenerator = new SimpleHtmlGenerator();

			XmlMergeService.AddWarningToListener(
				EventListener,
				warning,
				_oursContext,
				_theirsContext,
				_ancestorContext,
				_htmlContextGenerator);
		}

		/// <summary>
		/// This method does the actual work for the various public entry points of XmlMerge
		/// and from the various Method-type classes, as it processes child nodes, if any.
		/// </summary>
		internal void MergeInner(ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			SendMergeHeartbeat();
			_oursContext = ours;
			_theirsContext = theirs;
			_ancestorContext = ancestor;

			var elementStrat = MergeStrategies.GetElementStrategy(ours ?? theirs ?? ancestor);

			// Step 0: Do anything special the strategy wants to do before regular merging. This may modify the nodes.
			// For instance clinets may want to ensure 'our' and 'their' have the latest date stamp available.
			elementStrat.Premerger.Premerge(EventListener, ref ours, theirs, ancestor);

			// Step 0.1: Set up a context, if available.
			// Listeners are set to use NullContextDescriptor as the default context,
			var contextDescriptorGenerator = elementStrat.ContextDescriptorGenerator; // May be null, which is fine in code, below, that uses it.
			//review: question: does this not get called at levels below the entry?
			//this would seem to fail at, say, a sense. I'm confused. (JH 30june09)
			var descriptor = GetContextDescriptor(ours, contextDescriptorGenerator);
			EventListener.EnteringContext(descriptor); // TODO: If the context is ever redone as a stack, then this call with push the context onto the stack.
			_htmlContextGenerator = (contextDescriptorGenerator as IGenerateHtmlContext) ?? new SimpleHtmlGenerator();

			// Step 1: If the current set of nodes are immutable,
			// then make sure no changes took place (among a few other things).
			if (elementStrat.IsImmutable)
			{
				ImmutableElementMergeService.DoMerge(this, ref ours, theirs, ancestor);
				return; // Don't go any further, since it is immutable.
			}

			// Step 2: If the current set of elements is 'atomic'
			// (only one user can make changes to the node, or anything it contains),
			// then make sure only set of changes have been made.
			if (elementStrat.IsAtomic)
			{
				if (elementStrat.AllowAtomicTextMerge && XmlUtilities.IsTextLevel(ours, theirs, ancestor))
				{
					DoTextMerge(ref ours, theirs, ancestor, elementStrat);
					return;
				}
				MergeAtomicElementService.Run(this, ref ours, theirs, ancestor);
				return;
			}

			// Step 3: Go ahead and merge the attributes, as needed.
			MergeXmlAttributesService.MergeAttributes(this, ref ours, theirs, ancestor);

			// Step 4: Hmm, trouble is, a node might be required to have one or more child nodes.
			// I suppose a real validator would have picked up on that, and not allowed
			// the first commit, so we wouldn't then be doing a merge operation.
			// So, not to worry here, if the validator doesn't care enough to prevent the first commit.
			// It could be possible for the elements to have no children, in which case, there is nothing more to merge, so just return.
			if (ours != null && !ours.HasChildNodes && theirs != null && !theirs.HasChildNodes && ancestor != null && !ancestor.HasChildNodes)
				return;

			// Step 5: Do some kind of merge on the child node.
			if (XmlUtilities.IsTextLevel(ours, theirs, ancestor))
			{
				// Step 5A: Merge the text element.
				DoTextMerge(ref ours, theirs, ancestor, elementStrat);
			}
			else
			{
				switch (elementStrat.NumberOfChildren)
				{
					case NumberOfChildrenAllowed.Zero:
					case NumberOfChildrenAllowed.ZeroOrOne:
						// Step 5B: Merge the "special needs" nodes.
						MergeLimitedChildrenService.Run(this, elementStrat, ref ours, theirs, ancestor);
						break;
					case NumberOfChildrenAllowed.ZeroOrMore:
						// Step 5B:
						// Q: is this a level of the xml file that would consitute the minimal unit conflict-understanding
						// from a user perspecitve?
						// e.g., in a dictionary, this is the lexical entry.  In a text, it might be  a paragraph.
						// A (RandyR): Definitely it may not be such a level of node.
						new MergeChildrenMethod(ours, theirs, ancestor, this).Run();
						break;
				}
			}

			// At some point, it may be necessary here to restore the pre-existing values of
			// _oursContext, _theirsContext, _ancestorContext, and _htmlContextGenerator.
			// and somehow restore the EventListener's Context.
			// Currently however no client generates further conflicts after calling MergeChildren.

			// Step 6: TODO: If the context is ever redone as a stack, then pop the stack here to return to the outer context via some new LeavingContext method on EventListener.
		}

		private ContextDescriptor GetContextDescriptor(XmlNode ours, IGenerateContextDescriptor generator)
		{
			if (generator == null)
				return new NullContextDescriptor(); // Can't produce one from 'generator', so use default.

			var contextDescriptorFromNode = generator as IGenerateContextDescriptorFromNode;
			var retval = (contextDescriptorFromNode != null)
				? contextDescriptorFromNode.GenerateContextDescriptor(ours, MergeSituation.PathToFileInRepository)
				: generator.GenerateContextDescriptor(ours.OuterXml, MergeSituation.PathToFileInRepository);
			return retval ?? new NullContextDescriptor();
		}

		private void DoTextMerge(ref XmlNode ours, XmlNode theirs, XmlNode ancestor, ElementStrategy elementStrat)
		{
			new MergeTextNodesMethod(this, elementStrat, new HashSet<XmlNode>(), ref ours, new List<XmlNode>(), theirs,
				new List<XmlNode>(), ancestor, new List<XmlNode>()).Run();
		}

		public NodeMergeResult Merge(string ourXml, string theirXml, string ancestorXml)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourXml, doc);
			XmlNode theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirXml, doc);
			XmlNode ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(ancestorXml, doc);

			return Merge(ourNode, theirNode, ancestorNode);
		}

		public NodeMergeResult MergeFiles(string ourPath, string theirPath, string ancestorPath)
		{
			// Either user (or both users) could have created/edited/deleted the document.
			var ancestorNode = LoadXmlDocumentAndGetRootNode(ancestorPath);
			var ourNode = LoadXmlDocumentAndGetRootNode(ourPath);
			var theirNode = LoadXmlDocumentAndGetRootNode(theirPath);

			return Merge(ourNode, theirNode, ancestorNode);
		}

		private static XmlNode LoadXmlDocumentAndGetRootNode(string pathname)
		{
			XmlNode documentRootNode = null;
			if (File.Exists(pathname))
			{
				var fileInfo = new FileInfo(pathname);
				if (fileInfo.Length > 0)
				{
					var doc = new XmlDocument();
					doc.Load(pathname); // Will throw XmlException, if the file is not valid xml.
					documentRootNode = doc.DocumentElement;
				}
				// Otherwise file is empty. Perhaps because of how Hg creates one for ancestor,
				// or because it was deleted (ours/theirs)
			}
			return documentRootNode;
		}
	}
}