using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.merge
{
	/// <summary>
	/// gives information about a change in a file, e.g. a dictionary entry that was changed.
	/// Note that converting this raw info into human-readable, type-sensitive display
	/// is the reponsibility of implementers of IChangePresenter
	/// </summary>
	public interface IChangeReport
	{
		Guid  Guid { get; }
		string GetFullHumanReadableDescription();
		string ActionLabel { get; }
		string PathToFile { get; }
	}
	public interface IXmlChangeReport
	{
		XmlNode ParentNode { get; }
		XmlNode ChildNode { get; }
	}

	public abstract class ChangeReport : IChangeReport
	{
		public FileInRevision ChildFileInRevision { get; private set; }
		public FileInRevision ParentFileInRevision { get; private set; }
		protected Guid _guid = Guid.NewGuid();

//        public override bool Equals(object obj)
//        {
//            if(GetType() != obj.GetType())
//                return false;
//            IChangeReport other = obj as IChangeReport;
//            return PathToFile == other.PathToFile
//                && ActionLabel == other.ActionLabel;  //don't compare guids!
//        }
//        public override int GetHashCode()
//        {
//            return PathToFile.GetHashCode() + ActionLabel.GetHashCode();
//        }


//        protected ChangeReport(FileInRevision fileInRevision)
//        {
//            ChildFileInRevision = fileInRevision;
//        }
		protected ChangeReport(FileInRevision parent, FileInRevision child)
		{
			ChildFileInRevision = child;
			ParentFileInRevision = parent;
		}
		public Guid Guid
		{
			get { return _guid; }
		}

		public virtual string ActionLabel
		{
			get { return GetType().ToString(); }
		}

		public string PathToFile
		{
			get
			{
				if(null== ChildFileInRevision)
					return null;
				else
					return ChildFileInRevision.FullPath;
			}
		 }



		public override string ToString()
		{
			return GetFullHumanReadableDescription();
		}

		public virtual string GetFullHumanReadableDescription()
		{
			return ActionLabel;
		}

	}

	public class DefaultChangeReport : ChangeReport
	{
		private readonly string _label;

		public DefaultChangeReport(FileInRevision initial, string label)
			: base(null, initial)
		{
			_label = label;
		}

		public DefaultChangeReport(FileInRevision parent, FileInRevision child, string label)
			: base(parent, child)
		{
			_label = label;
		}

		public override string ActionLabel
		{
			get { return _label; }
		}
	}
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
	public class TextEditChangeReport : ChangeReport
	{
		private readonly string _before;
		private readonly string _after;

		public TextEditChangeReport(FileInRevision fileInRevision, string before, string after)
			: base(null, fileInRevision)
		{
			_before = before;
			_after = after;
		}

				//when merging, the eventual revision is unknown
		public TextEditChangeReport(string fullPath,  string before, string after)
			: this(new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), before,after )
		{

		}

		public override string ActionLabel
		{
			get { return "Text Edit"; }
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Changed '{0}' to '{1}'", _before, _after);
		}
	}

	/// <summary>
	/// THis may only be useful for quick, high-level identification that an entry changed,
	/// leaving *what* changed to a second pass, if needed by the user
	/// </summary>
	public class XmlChangedRecordReport : ChangeReport, IChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _parent;
		private readonly XmlNode _child;

		public XmlChangedRecordReport(FileInRevision parentFileInRevision, FileInRevision childFileInRevision, XmlNode parent, XmlNode child)
			: base(parentFileInRevision, childFileInRevision)
		{
			_parent = parent;
			_child = child;
		}

		public override string ActionLabel
		{
			get { return "Change"; }
		}

		public XmlNode ParentNode
		{
			get { return _parent; }
		}

		public XmlNode ChildNode
		{
			get { return _child; }
		}
	}
}
