using System.Xml;

namespace Chorus.merge
{
	public interface IXmlChangeReport
	{
		XmlNode ParentNode { get; }
		XmlNode ChildNode { get; }
	}
}