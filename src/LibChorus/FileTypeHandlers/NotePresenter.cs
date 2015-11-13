using System.Xml.Linq;
using Chorus.FileTypeHandlers.xml;
using Chorus.notes;
using Chorus.VcsDrivers;
using SIL.Code;


namespace Chorus.FileTypeHandlers
{
	public class NotePresenter : IChangePresenter
	{
		private readonly IRetrieveFileVersionsFromRepository _fileRetriever;
		private readonly IXmlChangeReport _report;
		private Annotation _annotation;

		public NotePresenter(IXmlChangeReport report, IRetrieveFileVersionsFromRepository fileRetriever)
		{
			Guard.AgainstNull(report,"report");
			_fileRetriever = fileRetriever;
			_report = report;// as XmlAdditionChangeReport;
			 _annotation = new Annotation(XElement.Parse(report.ChildNode.OuterXml));
		}

		public string GetDataLabel()
		{
			return _annotation.GetLabelFromRef("?");
		}

		public string GetActionLabel()
		{
			return _annotation.ClassName;
		}

		public string GetHtml(string style, string styleSheet)
		{
			if (style == "normal")
			{
				return _annotation.GetTextForToolTip();
			}
			else
			{
				return _annotation.Element.ToString();
			}
		}

		public string GetTypeLabel()
		{
			return _annotation.ClassName;
		}

		public string GetIconName()
		{
			return _annotation.ClassName;
		}
	}
}