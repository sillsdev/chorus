using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// This strategy doesn't even try to put the entries together.  It just takes "their" entry
	/// and sticks it in a merge failure field
	/// </summary>
	public class PoorMansMergeStrategy : IMergeStrategy
	{
		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode unusedCommonEntry)
		{
			XmlNode mergeNoteFieldNode = ourEntry.OwnerDocument.CreateElement("field");
			XmlUtilities.AddAttribute(mergeNoteFieldNode, "type", Conflict.ConflictAnnotationClassName);
			XmlUtilities.AddDateCreatedAttribute(mergeNoteFieldNode);
			StringBuilder b = new StringBuilder();
			b.Append("<trait name='looserData'>");
			b.AppendFormat("<![CDATA[{0}]]>", theirEntry.OuterXml);
			b.Append("</trait>");
			mergeNoteFieldNode.InnerXml = b.ToString();
			ourEntry.AppendChild(mergeNoteFieldNode);
			return ourEntry.OuterXml;
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			var def = new ElementStrategy(true)
			{
				MergePartnerFinder = new FindByEqualityOfTree()
			};
			return def;
		}

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		public MergeStrategies GetStrategies()
		{
			var merger = new XmlMerger(new MergeSituation(null, null, null, null, null, MergeOrder.ConflictHandlingModeChoices.WeWin));
			var def = new ElementStrategy(true)
			{
				MergePartnerFinder = new FindByEqualityOfTree()
			};
			merger.MergeStrategies.SetStrategy("def", def);
			return merger.MergeStrategies;
		}

		public HashSet<string> SuppressIndentingChildren()
		{
			return new HashSet<string>();
		}

		public bool ShouldCreateConflictReport(XmlNode ancestor, XmlNode survivor)
		{
			return true;
		}
	}
}