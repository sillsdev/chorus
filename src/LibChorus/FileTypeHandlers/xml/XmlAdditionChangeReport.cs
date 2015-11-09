using System;
using System.Xml;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.xml
{
	public class XmlAdditionChangeReport : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _addedElement;
		private readonly string _url;

		public XmlAdditionChangeReport(FileInRevision fileInRevision, XmlNode addedElement)
			:base(null, fileInRevision)
		{
			_addedElement = addedElement;
		}

		public XmlAdditionChangeReport(FileInRevision fileInRevision, XmlNode addedElement, string url)
			: base(null, fileInRevision)
		{
			_addedElement = addedElement;
			_url = url;
		}

		//when merging, the eventual revision is unknown
		public XmlAdditionChangeReport(string fullPath, XmlNode addedElement)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified))
		{
			_addedElement = addedElement;
		}

		public override string UrlOfItem
		{
			get
			{
				return _url;
			}
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
			return (guid != string.Empty)
				? guid.ToLowerInvariant().GetHashCode()
				: base.GetHashCode();
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
			return otherGuid == string.Empty ? base.Equals(obj) : String.Equals(guid, otherGuid, StringComparison.OrdinalIgnoreCase);
		}
	}
}