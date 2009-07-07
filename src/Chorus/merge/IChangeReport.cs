using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

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
	}
	public interface IXmlChangeReport
	{
		XmlNode ParentNode { get; }
		XmlNode ChildNode { get; }
	}

	public abstract class ChangeReport : IChangeReport
	{
		protected Guid _guid = Guid.NewGuid();

		public Guid Guid
		{
			get { return _guid; }
		}

		public virtual string ActionLabel
		{
			get { return GetType().ToString(); }
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

	public class DummyChangeReport : ChangeReport
	{
		private readonly string _label;

		public DummyChangeReport(string label)
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

		public XmlAdditionChangeReport(XmlNode addedElement)
		{
			_addedElement = addedElement;
		}

		public override string ActionLabel
		{
			get { return "Addition"; }
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
	}

	public class XmlDeletionChangeReport : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _deletedNode;

		public XmlDeletionChangeReport(XmlNode deletedNode)
		{
			_deletedNode = deletedNode;
		}

		public override string ActionLabel
		{
			get { return "Deletion"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Deleted a <{0}>", _deletedNode.Name);
		}

		public XmlNode ParentNode
		{
			get { return _deletedNode; }
		}

		public XmlNode ChildNode
		{
			get { return null; }
		}
	}
	public class TextEditChangeReport : ChangeReport
	{
		private readonly string _before;
		private readonly string _after;

		public TextEditChangeReport(string before, string after)
		{
			_before = before;
			_after = after;
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

		public XmlChangedRecordReport(XmlNode parent, XmlNode child)
		{
			_parent = parent;
			_child = child;
		}

		public override string ActionLabel
		{
			get { return _child.Name +" Changed"; }
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
