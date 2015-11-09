using System;
using System.Xml;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.xml
{
	public class BothChangedAtomicElementReport : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _modifiedElement;
		private readonly string _url;

		public BothChangedAtomicElementReport(FileInRevision fileInRevision, XmlNode modifiedElement)
			:base(null, fileInRevision)
		{
			_modifiedElement = modifiedElement;
		}

		public BothChangedAtomicElementReport(FileInRevision fileInRevision, XmlNode modifiedElement, string url)
			: base(null, fileInRevision)
		{
			_modifiedElement = modifiedElement;
			_url = url;
		}

		//when merging, the eventual revision is unknown
		public BothChangedAtomicElementReport(string fullPath, XmlNode modifiedElement)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified))
		{
			_modifiedElement = modifiedElement;
		}

		#region Implementation of IXmlChangeReport

		public XmlNode ParentNode
		{
			get { return _modifiedElement.ParentNode; }
		}

		public XmlNode ChildNode
		{
			get { return _modifiedElement; }
		}

		#endregion

		public override string UrlOfItem
		{
			get
			{
				return _url;
			}
		}

		public override string ActionLabel
		{
			get { return "Modified"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Both made same change(s) to <{0}>", _modifiedElement.Name);
		}

		public override int GetHashCode()
		{
			var guid = _modifiedElement.GetOptionalStringAttribute("guid", string.Empty);
			return (guid != string.Empty)
				? guid.ToLowerInvariant().GetHashCode()
				: base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var guid = _modifiedElement.GetOptionalStringAttribute("guid", string.Empty);
			if (guid == string.Empty)
				return base.Equals(obj);

			var r = obj as BothChangedAtomicElementReport;
			if (r == null)
				return false;
			var otherGuid = r._modifiedElement.GetOptionalStringAttribute("guid", string.Empty);
			return otherGuid == string.Empty ? base.Equals(obj) : String.Equals(guid, otherGuid, StringComparison.OrdinalIgnoreCase);
		}
	}
}