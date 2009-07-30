using System.Xml;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.merge
{
	public class XmlDeletionChangeReport : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _parentNode;
		private readonly XmlNode _childNode;
		//  private readonly XmlNode _deletedNode;

		public XmlDeletionChangeReport(FileInRevision fileInRevision, XmlNode parentNode, XmlNode childNode)
			: base(null, fileInRevision)
		{
			_parentNode = parentNode;
			_childNode = childNode;
		}

		//when merging, the eventual revision is unknown
		public XmlDeletionChangeReport(string fullPath, XmlNode parentNode, XmlNode childNode)
			: this(new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), parentNode,  childNode)
		{
		}

		public override string ActionLabel
		{
			get { return "Deleted"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Deleted a <{0}>", _parentNode.Name);
		}

		public XmlNode ParentNode
		{
			get { return _parentNode; }
		}

		/// <summary>
		/// yes, we may have a child, if it's still there, just marked as deleted
		/// </summary>
		public XmlNode ChildNode
		{
			get { return _childNode; }
		}
	}
}