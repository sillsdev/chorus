using System.Xml;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHandlers.xml
{
	public class XmlBothAddedSameChangeReport : XmlAdditionChangeReport
	{
		public XmlBothAddedSameChangeReport(FileInRevision fileInRevision, XmlNode addedElement)
			: base(fileInRevision, addedElement)
		{
		}

		public XmlBothAddedSameChangeReport(FileInRevision fileInRevision, XmlNode addedElement, string url)
			: base(fileInRevision, addedElement, url)
		{
		}

		//when merging, the eventual revision is unknown
		public XmlBothAddedSameChangeReport(string fullPath, XmlNode addedElement)
			: base(fullPath, addedElement)
		{
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Both added <{0}>", ChildNode.Name);
		}
	}
}