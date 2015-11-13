using System;
using System.Xml;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHandlers.xml
{
	/// <summary>
	/// Abstract class that supports the three concrete type of XML elements that contain an XmlNodeType.Text element.
	/// </summary>
	public abstract class XmlAttributeChange : ChangeReport, IXmlChangeReport
	{
		private readonly XmlAttribute _affectedAttribute;
		private readonly string _url;

		protected XmlAttributeChange(FileInRevision parent, FileInRevision child, XmlAttribute affectedAttribute, string url)
			: base(parent, child)
		{
			if (affectedAttribute == null) throw new ArgumentNullException("affectedAttribute");
			_affectedAttribute = affectedAttribute;
			_url = url;
		}

		protected abstract string FormattedMessageForFullHumanReadableDescription { get; }

		#region Implementation of IXmlChangeReport

		public XmlNode ParentNode
		{
			get { return _affectedAttribute.OwnerElement; }
		}

		public XmlNode ChildNode
		{
			get { return _affectedAttribute; }
		}

		#endregion

		#region Implementation of IChangeReport

		public override string UrlOfItem
		{
			get { return _url; }
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format(FormattedMessageForFullHumanReadableDescription, ChildNode.Name, ParentNode.Name);
		}

		#endregion
	}

	/// <summary>
	/// Change report for an XmlAttribute that changed.
	/// </summary>
	public sealed class XmlAttributeChangedReport : XmlAttributeChange
	{
		public XmlAttributeChangedReport(FileInRevision parentFileInRevision, FileInRevision childFileInRevision, XmlAttribute editedAttribute, string url)
			: base(parentFileInRevision, childFileInRevision, editedAttribute, url)
		{
		}

		public XmlAttributeChangedReport(FileInRevision parentFileInRevision, FileInRevision childFileInRevision, XmlAttribute editedAttribute)
			: this(parentFileInRevision, childFileInRevision, editedAttribute, string.Empty)
		{
		}

		public XmlAttributeChangedReport(string fullPath, XmlAttribute editedAttribute)
			: this(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), editedAttribute, string.Empty)
		{
		}

		#region Overrides of ChangeReport

		public override string ActionLabel
		{
			get { return "Changed Attribute"; }
		}

		#endregion

		#region Overrides of XmlTextChange

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Modified <{0}> attribute of <{1}>"; }
		}

		#endregion
	}
	/// <summary>
	/// Change report for XmlAttribute element that was added.
	/// </summary>
	public sealed class XmlAttributeAddedReport : XmlAttributeChange
	{
		// When merging, the eventual revision is unknown.
		public XmlAttributeAddedReport(string fullPath, XmlAttribute addedAttribute)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Added), addedAttribute, string.Empty)
		{
		}

		#region Overrides of XmlTextChange

		public override string ActionLabel
		{
			get { return "Added"; }
		}

		protected override string FormattedMessageForFullHumanReadableDescription
		{
			get { return "Added <{0}> attribute to <{1}>"; }
		}

		#endregion

		public override int GetHashCode()
		{
			return ChildNode.Value.ToLowerInvariant().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var otherReport = obj as XmlAttributeAddedReport;
			return otherReport != null && String.Equals(ChildNode.Value, otherReport.ChildNode.Value, StringComparison.OrdinalIgnoreCase);
		}
	}

	/// <summary>
	/// Change report for XmlAttribute element that was added by both people with the same value.
	/// </summary>
	public sealed class XmlAttributeBothAddedReport : XmlAttributeChange
	{
		public XmlAttributeBothAddedReport(FileInRevision fileInRevision, XmlAttribute addedChildAttribute, string url)
			: base(null, fileInRevision, addedChildAttribute, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlAttributeBothAddedReport(string fullPath, XmlAttribute addedChildAttribute)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Added), addedChildAttribute, string.Empty)
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
	/// Change report for identical XmlAttribute edit that was made by both people.
	/// </summary>
	public sealed class XmlAttributeBothMadeSameChangeReport : XmlAttributeChange
	{
		public XmlAttributeBothMadeSameChangeReport(FileInRevision fileInRevision, XmlAttribute editedChilAttribute, string url)
			: base(null, fileInRevision, editedChilAttribute, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlAttributeBothMadeSameChangeReport(string fullPath, XmlAttribute editedChilAttribute)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), editedChilAttribute, string.Empty)
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

	/// <summary>
	/// Change report for XmlAttribute that was deleted by both people.
	/// </summary>
	public sealed class XmlAttributeBothDeletedReport : XmlAttributeChange
	{
		public XmlAttributeBothDeletedReport(FileInRevision fileInRevision, XmlAttribute ancestorAttribute, string url)
			: base(null, fileInRevision, ancestorAttribute, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlAttributeBothDeletedReport(string fullPath, XmlAttribute ancestorAttribute)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Deleted), ancestorAttribute, string.Empty)
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
	/// Change report for XmlAttribute element that was deleted by either person.
	/// </summary>
	public sealed class XmlAttributeDeletedReport : XmlAttributeChange
	{
		public XmlAttributeDeletedReport(FileInRevision fileInRevision, XmlAttribute ancestorAttribute, string url)
			: base(null, fileInRevision, ancestorAttribute, url)
		{
		}

		// When merging, the eventual revision is unknown
		public XmlAttributeDeletedReport(string fullPath, XmlAttribute ancestorAttribute)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Deleted), ancestorAttribute, string.Empty)
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
}