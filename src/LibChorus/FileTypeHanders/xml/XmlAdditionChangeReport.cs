using System.Xml;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.merge
{
	public class XmlAdditionChangeReport : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _addedElement;

		public XmlAdditionChangeReport(FileInRevision fileInRevision, XmlNode addedElement)
			:base(null, fileInRevision)
		{
			_addedElement = addedElement;
		}

		//when merging, the eventual revision is unknown
		public XmlAdditionChangeReport(string fullPath, XmlNode addedElement)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified))
		{
			_addedElement = addedElement;
		}

		public override string ActionLabel
		{
			get { return "Added"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Added a <{0}>", _addedElement.Name);
		}

		public XmlNode ParentNode
		{
			get { return null; }
		}

		public XmlNode ChildNode
		{
			get { return _addedElement; }
		}

		public override int GetHashCode()
		{
			var guid = _addedElement.GetOptionalStringAttribute("guid",string.Empty);
			if(guid!=string.Empty)
				return guid.GetHashCode();
			return base.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var guid = _addedElement.GetOptionalStringAttribute("guid",string.Empty);
			if(guid==string.Empty)
				return base.Equals(obj);

			XmlAdditionChangeReport r = obj as XmlAdditionChangeReport;
			if(r==null)
				return false;
			var otherGuid = r._addedElement.GetOptionalStringAttribute("guid",string.Empty);
			if (guid == string.Empty)
				return base.Equals(obj);

			return guid.Equals(otherGuid);
		}
	}
}