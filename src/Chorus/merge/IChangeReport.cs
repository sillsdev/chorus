using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Chorus.merge
{
	/// <summary>
	/// gives information about a change in a file, e.g. a dictionary entry that was changed
	/// </summary>
	public interface IChangeReport
	{
		Guid  Guid { get; }
		string GetFullHumanReadableDescription();
		string HumanNameOfChangeType { get; }
	}

	public abstract class ChangeReport : IChangeReport
	{
		protected Guid _guid = Guid.NewGuid();

		public Guid Guid
		{
			get { return _guid; }
		}

		public virtual string HumanNameOfChangeType
		{
			get { return GetType().ToString(); }
		}

		public override string ToString()
		{
			return GetFullHumanReadableDescription();
		}

		public virtual string GetFullHumanReadableDescription()
		{
			return HumanNameOfChangeType;
		}
	}

	public class DummyChangeReport : ChangeReport
	{
		private readonly string _label;

		public DummyChangeReport(string label)
		{
			_label = label;
		}
		public override string HumanNameOfChangeType
		{
			get { return _label; }
		}
	}
	public class AdditionChangeReport : ChangeReport
	{
		private readonly XmlNode _addedElement;

		public AdditionChangeReport(XmlNode addedElement)
		{
			_addedElement = addedElement;
		}

		public override string HumanNameOfChangeType
		{
			get { return "Addition"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Added a <{0}>", _addedElement.Name);
		}
	}

	public class DeletionChangeReport : ChangeReport
	{
		private readonly XmlNode _deletedNode;

		public DeletionChangeReport(XmlNode deletedNode)
		{
			_deletedNode = deletedNode;
		}

		public override string HumanNameOfChangeType
		{
			get { return "Deletion"; }
		}
		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Deleted a <{0}>", _deletedNode.Name);
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

		public override string HumanNameOfChangeType
		{
			get { return "Text Edit"; }
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Changed '{0}' to '{1}'", _before, _after);
		}
	}
}
