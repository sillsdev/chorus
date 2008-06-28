using System.Xml;

namespace Chorus.merge.xml.generic
{
	public interface IMergeStrategy
	{
		string MakeMergedEntry(XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry);
	}
}