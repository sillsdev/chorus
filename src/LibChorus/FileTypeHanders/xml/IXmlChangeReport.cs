using System.Xml;

namespace Chorus.FileTypeHanders.xml
{
	public interface IXmlChangeReport
	{
		XmlNode ParentNode { get; }
		XmlNode ChildNode { get; }
	}
}