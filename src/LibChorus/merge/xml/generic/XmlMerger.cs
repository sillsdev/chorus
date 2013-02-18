using System.Collections.Generic;
using System.IO;
using System.Xml;

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
			var result = new NodeMergeResult();
			if (EventListener is DispatchingMergeEventListener)
			{
				((DispatchingMergeEventListener)EventListener).AddEventListener(result);
			}
			else
			{
				DispatchingMergeEventListener dispatcher = new DispatchingMergeEventListener();
				dispatcher.AddEventListener(result);
				if (EventListener != null)
				{
					dispatcher.AddEventListener(EventListener);
				}
				EventListener = dispatcher;
			}

			// Remove any duplicate child nodes in all three.
			XmlMergeService.RemoveAmbiguousChildren(EventListener, MergeStrategies, ours);
			XmlMergeService.RemoveAmbiguousChildren(EventListener, MergeStrategies, theirs);
			//XmlMergeService.RemoveAmbiguousChildren(EventListener, MergeStrategies, ancestor);

			MergeInner(ref ours, theirs, ancestor);
			result.MergedNode = ours;
			return result;
		}

		public XmlNode Merge(IMergeEventListener eventListener, XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			EventListener = eventListener;
			MergeInner(ref ours, theirs, ancestor);
			return ours;
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
			IGenerateHtmlContext htmlGenerator = contextDescriptorGenerator as IGenerateHtmlContext;
			if (htmlGenerator == null)
				htmlGenerator = new SimpleHtmlGenerator();

			XmlMergeService.AddConflictToListener(
				EventListener,
				conflict,
				_oursContext,
				_theirsContext,
				_ancestorContext,
				htmlGenerator,
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
			_oursContext = ours;
			_theirsContext = theirs;
			_ancestorContext = ancestor;

			var elementStrat = MergeStrategies.GetElementStrategy(ours ?? theirs ?? ancestor);

			// Do anything special the strategy wants to do before regular merging. This may modify the nodes.
			elementStrat.PreMerge(ours, theirs, ancestor);

			if (elementStrat.IsImmutable)
				return; // Can't merge something that can't change.

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

			MergeXmlAttributesService.MergeAttributes(this, ref ours, theirs, ancestor);

			// It could be possible for the elements to have no children, in which case, there is nothing more to merge, so just return.
			if (ours != null && !ours.HasChildNodes && theirs != null && !theirs.HasChildNodes && ancestor != null && !ancestor.HasChildNodes)
				return;

			var generator = elementStrat.ContextDescriptorGenerator;
			if (generator != null)
			{
				//review: question: does this not get called at levels below the entry?
				//this would seem to fail at, say, a sense. I'm confused. (JH 30june09)
				ContextDescriptor descriptor;
				descriptor = GetContextDescriptor(ours, generator);
				EventListener.EnteringContext(descriptor);
				_htmlContextGenerator = (generator as IGenerateHtmlContext); // null is OK.
			}

			if (XmlUtilities.IsTextLevel(ours, theirs, ancestor))
			{
				DoTextMerge(ref ours, theirs, ancestor, elementStrat);
			}
			else
			{
				switch (elementStrat.NumberOfChildren)
				{
					case NumberOfChildrenAllowed.Zero:
					case NumberOfChildrenAllowed.ZeroOrOne:
						MergeLimitedChildrenService.Run(this, elementStrat, ref ours, theirs, ancestor);
						break;
					case NumberOfChildrenAllowed.ZeroOrMore:
						//is this a level of the xml file that would consitute the minimal unit conflict-understanding
						//from a user perspecitve?
						//e.g., in a dictionary, this is the lexical entry.  In a text, it might be  a paragraph.
						new MergeChildrenMethod(ours, theirs, ancestor, this).Run();
						break;
				}
			}
			// At some point, it may be necessary here to restore the pre-existing values of
			// _oursContext, _theirsContext, _ancestorContext, and _htmlContextGenerator.
			// and somehow restore the EventListener's Context.
			// Currently however no client generates further conflicts after calling MergeChildren.
		}

		internal ContextDescriptor GetContextDescriptor(XmlNode ours, IGenerateContextDescriptor generator)
		{
			ContextDescriptor descriptor;
			if (generator == null)
				return null; // can't produce one.
			if (generator is IGenerateContextDescriptorFromNode)
			{
				// If the generator prefers the XmlNode, get the context that way.
				descriptor = ((IGenerateContextDescriptorFromNode) generator).GenerateContextDescriptor(ours,
					MergeSituation.PathToFileInRepository);
			}
			else
			{
				descriptor = generator.GenerateContextDescriptor(ours.OuterXml, MergeSituation.PathToFileInRepository);
			}
			return descriptor;
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
			//Debug.Fail("time to attach");
			XmlDocument ourDoc = new XmlDocument();
			ourDoc.Load(ourPath);
			XmlNode ourNode = ourDoc.DocumentElement;

			XmlDocument theirDoc = new XmlDocument();
			theirDoc.Load(theirPath);
			XmlNode theirNode = theirDoc.DocumentElement;

			XmlNode ancestorNode = null;
			if (File.Exists(ancestorPath)) // it's possible for the file to be created independently by each user, with no common ancestor
			{
				XmlDocument ancestorDoc = new XmlDocument();
				try
				{
					ancestorDoc.Load(ancestorPath);
					ancestorNode = ancestorDoc.DocumentElement;
				}
				catch (XmlException e)
				{
					if(File.ReadAllText(ancestorPath).Length>1 )
					{
						throw e;
					}
					//otherwise, it's likely an artifact of how hg seems to create an empty file
					//for the ancestor, if there wasn't one there before, and empty = not well-formed xml!
				}
			 }

			return Merge(ourNode, theirNode, ancestorNode);
		}
	}
}