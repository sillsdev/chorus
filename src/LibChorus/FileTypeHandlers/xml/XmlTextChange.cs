using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.Properties;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.xml
{
	/// <summary>
	/// Abstract class that supports the three concrete type of XML elements that contain an XmlNodeType.Text element.
	/// </summary>
	public abstract class XmlTextChange : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _affectedNode;
		private readonly string _url;

		protected XmlTextChange(FileInRevision parent, FileInRevision child, XmlNode affectedNode, string url)
			: base(parent, child)
		{
			if (affectedNode.HasChildNodes && affectedNode.FirstChild.NodeType != XmlNodeType.Text)
				throw new ArgumentException(AnnotationImages.kElementNotTextElement, "affectedNode");

			_affectedNode = affectedNode;
			_url = url;
		}

		protected abstract string FormattedMessageForFullHumanReadableDescription { get; }

		#region Implementation of IXmlChangeReport

		public XmlNode ParentNode
		{
			get { return ChildNode.ParentNode; }
		}

		public XmlNode ChildNode
		{
			get { return _affectedNode; }
		}

		#endregion

		#region Implementation of IChangeReport

		public override string UrlOfItem
		{
			get { return _url; }
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format(FormattedMessageForFullHumanReadableDescription, ChildNode.Name, ParentNode != null ? ParentNode.Name : "");
		}

		#endregion
	}

	/// <summary>
	/// Change report for Xml Text element (parent of XmlNodeType.Text element) that changed.
	/// </summary>
	public sealed class XmlTextChangedReport : XmlTextChange
	{
		public XmlTextChangedReport(FileInRevision parentFileInRevision, FileInRevision childFileInRevision, XmlNode editedElement, string url)
			: base(parentFileInRevision, childFileInRevision, editedElement, url)
		{
		}

		public XmlTextChangedReport(FileInRevision parentFileInRevision, FileInRevision childFileInRevision, XmlNode editedElement)
			: this(parentFileInRevision, childFileInRevision, editedElement, string.Empty)
		{
		}

		public XmlTextChangedReport(string fullPath, XmlNode editedElement)
			: this(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), editedElement, string.Empty)
		{
		}

		#region Overrides of ChangeReport

		public override string ActionLabel
		{
			get { return "Changed"; }
		}

		#endregion

		#region Overrides of XmlTextChange

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Modified <{0}> of <{1}>"; }
		}

		#endregion
	}

	/// <summary>
	/// Change report for Xml Text element (parent of XmlNodeType.Text element) that was added.
	/// </summary>
	public sealed class XmlTextAddedReport : XmlTextChange
	{
		// When merging, the eventual revision is unknown.
		public XmlTextAddedReport(string fullPath, XmlNode addedElement)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Added), addedElement, string.Empty)
		{
		}

		#region Overrides of XmlTextChange

		public override string ActionLabel
		{
			get { return "Added"; }
		}

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Added <{0}> to <{1}>"; }
		}

		#endregion

		public override int GetHashCode()
		{
			var guid = ChildNode.GetOptionalStringAttribute("guid", string.Empty);
			return (guid == string.Empty)
				? base.GetHashCode()
				: guid.ToLowerInvariant().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var otherReport = obj as XmlTextAddedReport;
			if (otherReport == null)
				return false;

			var guid = ChildNode.GetOptionalStringAttribute("guid", string.Empty);
			var otherGuid = otherReport.ChildNode.GetOptionalStringAttribute("guid", string.Empty);
			if (guid == string.Empty || otherGuid == string.Empty)
				return base.Equals(obj);

			return String.Equals(guid, otherGuid, StringComparison.OrdinalIgnoreCase);
		}
	}

	/// <summary>
	/// Change report for Xml Text element (parent of XmlNodeType.Text node) that was deleted.
	/// </summary>
	public sealed class XmlTextDeletedReport : XmlTextChange
	{
		public XmlTextDeletedReport(FileInRevision fileInRevision, XmlNode deletedChildNode, string url)
			: base(null, fileInRevision, deletedChildNode, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlTextDeletedReport(string fullPath, XmlNode deletedChildNode)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Deleted),  deletedChildNode, string.Empty)
		{
		}

		public override string ActionLabel
		{
			get { return "Deleted"; }
		}

		#region Overrides of XmlTextChange

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Deleted <{0}> from <{1}>"; }
		}

		#endregion
	}

	/// <summary>
	/// Change report for Xml Text element (parent of XmlNodeType.Text node) that was deleted by both people.
	/// </summary>
	public sealed class XmlTextBothDeletedReport : XmlTextChange
	{
		public XmlTextBothDeletedReport(FileInRevision fileInRevision, XmlNode deletedChildNode, string url)
			: base(null, fileInRevision, deletedChildNode, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlTextBothDeletedReport(string fullPath, XmlNode deletedChildNode)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Deleted), deletedChildNode, string.Empty)
		{
		}

		public override string ActionLabel
		{
			get { return "Both Deleted"; }
		}

		#region Overrides of XmlTextChange

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Both people deleted <{0}> from <{1}>"; }
		}

		#endregion
	}

	/// <summary>
	/// Change report for Xml Text element (parent of XmlNodeType.Text node) that was added by both people.
	/// </summary>
	public sealed class XmlTextBothAddedReport : XmlTextChange
	{
		public XmlTextBothAddedReport(FileInRevision fileInRevision, XmlNode addedChildNode, string url)
			: base(null, fileInRevision, addedChildNode, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlTextBothAddedReport(string fullPath, XmlNode addedChildNode)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Added), addedChildNode, string.Empty)
		{
		}

		public override string ActionLabel
		{
			get { return "Both Added"; }
		}

		#region Overrides of XmlTextChange

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Both people added <{0}> to <{1}>"; }
		}

		#endregion
	}

	/// <summary>
	/// Change report for Xml Text element (parent of XmlNodeType.Text node) that was added by both people.
	/// </summary>
	public sealed class XmlTextBothMadeSameChangeReport : XmlTextChange
	{
		public XmlTextBothMadeSameChangeReport(FileInRevision fileInRevision, XmlNode editedChildNode, string url)
			: base(null, fileInRevision, editedChildNode, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlTextBothMadeSameChangeReport(string fullPath, XmlNode editedChildNode)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), editedChildNode, string.Empty)
		{
		}

		public override string ActionLabel
		{
			get { return "Both Made Same Change"; }
		}

		#region Overrides of XmlTextChange

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Both people changed <{0}> in <{1}>"; }
		}

		#endregion
	}
}
