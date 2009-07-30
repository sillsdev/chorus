using System.Xml;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.merge
{
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