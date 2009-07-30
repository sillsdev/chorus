using System.Text;
using System.Xml;
using Chorus.merge.xml.lift;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// This strategy doesn't even try to put the entries together.  It just takes "their" entry
	/// and sticks it in a merge failure field
	/// </summary>
	public class PoorMansMergeStrategy : IMergeStrategy
	{
		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode unusedCommonEntry)
		{
			XmlNode mergeNoteFieldNode = ourEntry.OwnerDocument.CreateElement("field");
			LiftMerger.AddAttribute(mergeNoteFieldNode, "type", "mergeConflict");
			LiftMerger.AddDateCreatedAttribute(mergeNoteFieldNode);
			StringBuilder b = new StringBuilder();
			b.Append("<trait name='looserData'>");
			b.AppendFormat("<![CDATA[{0}]]>", theirEntry.OuterXml);
			b.Append("</trait>");
			mergeNoteFieldNode.InnerXml = b.ToString();
			ourEntry.AppendChild(mergeNoteFieldNode);
			return ourEntry.OuterXml;
		}
	}
}