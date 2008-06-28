using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// This is a really high level approach
	/// TODO: I'm not sure this is buying us anything anmore
	/// </summary>
	public interface IMergeStrategy
	{
		string MakeMergedEntry(XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry);
	}
}