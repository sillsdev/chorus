using System.Xml;

namespace Chorus.FileTypeHandlers.xml
{
	public interface IXmlChangeReport
	{
		XmlNode ParentNode { get; }
		XmlNode ChildNode { get; }
	}
}