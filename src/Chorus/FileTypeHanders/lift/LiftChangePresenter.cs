using System.Xml;
using Chorus.merge;

namespace Chorus.FileTypeHanders.lift
{
	public class LiftChangePresenter : IChangePresenter
	{
		private readonly IXmlChangeReport _report;

		public LiftChangePresenter(IXmlChangeReport report)
		{
			_report = report;
		}

		public string GetActionLabel()
		{
			return ((IChangeReport) _report).ActionLabel;
		}

		public string GetDataLabel()
		{
			//Enhance: this is just a lexeme form, not the headword
			var nodes = FirstNonNullNode.SelectNodes("//lexical-unit/form/text");
			if (nodes == null || nodes.Count == 0)
				return "??";
			return nodes[0].InnerText;

		}
		private XmlNode FirstNonNullNode
		{
			get
			{
				if (_report.ChildNode == null)
					return _report.ParentNode;
				return _report.ChildNode;
			}
		}
	}
}