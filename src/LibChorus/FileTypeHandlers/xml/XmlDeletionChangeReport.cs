using System.Xml;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHandlers.xml
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
			: this(new FileInUnknownRevision(fullPath, FileInRevision.Action.Deleted), parentNode,  childNode)
		{
		}

		public override string ActionLabel
		{
			get { return "Deleted"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Deleted a <{0}>", ParentNode.Name);
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

	public class XmlBothDeletionChangeReport : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _childNode;
		//  private readonly XmlNode _deletedNode;

		public XmlBothDeletionChangeReport(FileInRevision fileInRevision, XmlNode deletedNode)
			: base(null, fileInRevision)
		{
			_childNode = deletedNode;
		}

		//when merging, the eventual revision is unknown
		public XmlBothDeletionChangeReport(string fullPath, XmlNode deletedNode)
			: this(new FileInUnknownRevision(fullPath, FileInRevision.Action.Deleted), deletedNode)
		{
		}

		public override string ActionLabel
		{
			get { return "Deleted"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Both deleted the <{0}>", ChildNode.Name);
		}

		public XmlNode ParentNode
		{
			get { return _childNode.ParentNode; }
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