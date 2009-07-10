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
		string PathToFile { get; }
	}
	public interface IXmlChangeReport
	{
		XmlNode ParentNode { get; }
		XmlNode ChildNode { get; }
	}

	public abstract class ChangeReport : IChangeReport
	{
		protected Guid _guid = Guid.NewGuid();

		protected ChangeReport(string pathToFile)
		{
			PathToFile = pathToFile;
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
			get; protected set;
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

		public DefaultChangeReport(string pathToFile, string label)
			: base(pathToFile)
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

		public XmlAdditionChangeReport(string pathToFile, XmlNode addedElement)
			:base(pathToFile)
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
	}

	public class XmlDeletionChangeReport : ChangeReport, IXmlChangeReport
	{
		private readonly XmlNode _parentNode;
		private readonly XmlNode _childNode;
		private readonly XmlNode _deletedNode;

		public XmlDeletionChangeReport(string pathToFile, XmlNode parentNode, XmlNode childNode)
			: base(pathToFile)
		{
			_parentNode = parentNode;
			_childNode = childNode;
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

		public TextEditChangeReport(string pathToFile, string before, string after)
			: base(pathToFile)
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

		public XmlChangedRecordReport(string pathToFile, XmlNode parent, XmlNode child)
			: base(pathToFile)
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
