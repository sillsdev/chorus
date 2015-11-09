using System;
using System.Xml;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHandlers.xml
{
	public class ErrorDeterminingChangeReport : ChangeReport, IChangePresenter
	{
		private readonly XmlNode _parentNode;
		private readonly XmlNode _childNode;
 private Exception _error;
		//  private readonly XmlNode _deletedNode;

		public ErrorDeterminingChangeReport(FileInRevision parent, FileInRevision child, XmlNode parentNode, XmlNode childNode, Exception error)
			: base(parent, child)
		{

			_parentNode = parentNode;
			_childNode = childNode;
			_error = error;
		}

		public override string ActionLabel
		{
			get { return "Error"; }
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Error: >", _error.Message);
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

		#region IChangePresenter
		public string GetDataLabel()
		{
			return string.Empty;
		}

		public string GetActionLabel()
		{
			return "Error";
		}

		public string GetHtml(string style, string styleSheet)
		{
			return "<html>" + _error.Message.Replace("<", "&lt;").Replace(">", "&gt;") + "</html>";
		}

		public string GetTypeLabel()
		{
			return "error";
		}

		public string GetIconName()
		{
			return "error";
		}
		#endregion
	}
}