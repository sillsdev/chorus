using System;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.lift
{
	/// <summary>
	/// This is used by version control systems to do an intelligent 3-way merge of lift files.
	///
	/// This class is lift-specific.  It walks through each entry, applying a merger
	/// (which is currenlty, normally, something that uses the Chorus xmlMerger).
	///
	/// TODO: A confusing part here is the mix of levels we got from how this was built historically:
	/// file, lexentry, ultimately the chorus xml merger on the parts of the lexentry.  Each level seems to have some strategies.
	/// I (JH) wonder if we could move more down to the generic level.
	///
	/// Eventually, we may want a non-dom way to handle the top level, in which case having this class would be handy.
	/// </summary>
	public class LiftMerger
	{
		private IMergeStrategy _mergingStrategy;
		public IMergeEventListener EventListener = new NullMergeEventListener();
		private readonly MergeOrder _mergeOrder;
		private readonly string _pathToWinner;
		private readonly string _pathToLoser;
		private readonly string _pathToCommonAncestor;
		private readonly string _winnerId;

		/// <summary>
		/// Here, "alpha" is the guy who wins when there's no better way to decide, and "beta" is the loser.
		/// </summary>
		public LiftMerger(MergeOrder mergeOrder, string alphaLiftPath, string betaLiftPath, IMergeStrategy mergeStrategy, string ancestorLiftPath, string winnerId)
		{
			_mergeOrder = mergeOrder;
			_pathToWinner = alphaLiftPath;
			_pathToLoser = betaLiftPath;
			_pathToCommonAncestor = ancestorLiftPath;
			_winnerId = winnerId;
			_mergingStrategy = mergeStrategy;
		}

		public void DoMerge(string outputPathname)
		{
			XmlMergeService.Do3WayMerge(_mergingStrategy, _mergeOrder, EventListener,
				_pathToWinner, _pathToLoser, _pathToCommonAncestor,
				outputPathname, _winnerId, "entry", "id", WritePreliminaryInformation);
		}

		private static void WritePreliminaryInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("lift");
			if (reader.MoveToAttribute("version"))
				writer.WriteAttributeString("version", reader.Value);
			if (reader.MoveToAttribute("producer"))
				writer.WriteAttributeString("producer", reader.Value);
			reader.MoveToElement();
		}

		internal static void AddDateCreatedAttribute(XmlNode elementNode)
		{
			AddAttribute(elementNode, "dateCreated", DateTime.Now.ToString(LiftUtils.LiftTimeFormatNoTimeZone));
		}

		internal static void AddAttribute(XmlNode element, string name, string value)
		{
			XmlAttribute attr = element.OwnerDocument.CreateAttribute(name);
			attr.Value = value;
			element.Attributes.Append(attr);
		}
	}
}