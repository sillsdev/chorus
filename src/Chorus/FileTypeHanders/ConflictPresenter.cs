using System;
using System.Text;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.retrieval;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// The key thing to understand here is that the conflict file only *grows*... it doesn't get editted
	/// normally.  So the only changes we're interested in are *additions* of conflict reports to the file.
	/// </summary>
	public class ConflictPresenter : IChangePresenter
	{
		private readonly IRetrieveFileVersionsFromRepository _fileRetriever;
		private readonly XmlAdditionChangeReport _report;
		private IConflict _conflict;

		public ConflictPresenter(IXmlChangeReport report, IRetrieveFileVersionsFromRepository fileRetriever)
		{
			_fileRetriever = fileRetriever;
			_report = report as XmlAdditionChangeReport;
			if (_report == null)
			{
				_conflict = new UnreadableConflict(_report.ChildNode);
			}
			else
			{
				_conflict = Conflict.CreateFromXml(_report.ChildNode);
			}
		}

		public string GetDataLabel()
		{
			return _conflict.Context.DataLabel;
		}

		public string GetActionLabel()
		{
			return XmlUtilities.GetStringAttribute(_report.ChildNode, "type");
		}

		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>"+styleSheet +"</head>");
			if (style == "normal")
			{
				if (_report is XmlAdditionChangeReport)
				{
					builder.AppendFormat(
						"<p>{0} and {1} both editted {2} in the file {3} in a way that could not be automatically merged.</p>",
						_conflict.Situation.UserXId, _conflict.Situation.UserYId, _conflict.Context.DataLabel,
						_conflict.RelativeFilePath);

					builder.AppendFormat("<p></p>");

					// XmlUtilities.GetXmlForShowingInHtml(_report.ChildNode.OuterXml));
				}

			}
			else
			{
				builder.AppendFormat(
					"{0} and {1} both editted {2} in a way that could not be merged. Where they conflicted, {3}'s version was kept.<br/>",
					_conflict.Situation.UserXId, _conflict.Situation.UserYId, _conflict.Context.DataLabel, _conflict.WinnerId);

				builder.AppendFormat(
					"The kind of conflict was: {0}", _conflict.ConflictTypeHumanName);
				//                var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
//                builder.Append(m.merge());

				builder.AppendFormat("<h3>Original Record</h3>");
			   var ancestor = _conflict.GetConflictingRecordOutOfSourceControl(_fileRetriever, ThreeWayMergeSources.Source.Ancestor);
				builder.Append(XmlUtilities.GetXmlForShowingInHtml(ancestor));
				builder.AppendFormat("<h3>{0}'s version</h3>", _conflict.Situation.UserXId);
				var userXVersion = _conflict.GetConflictingRecordOutOfSourceControl(_fileRetriever, ThreeWayMergeSources.Source.UserX);
				builder.Append(XmlUtilities.GetXmlForShowingInHtml(userXVersion));
				builder.AppendFormat("</p><h3>{0}'s version</h3>", _conflict.Situation.UserYId);
				var userYVersion = _conflict.GetConflictingRecordOutOfSourceControl(_fileRetriever, ThreeWayMergeSources.Source.UserY);
				builder.Append(XmlUtilities.GetXmlForShowingInHtml(userYVersion));
				builder.AppendFormat("</p><h3>Resulting version</h3>", _conflict.Situation.UserYId);
				var resulting = _fileRetriever.RetrieveHistoricalVersionOfFile(_conflict.RelativeFilePath,
																			   _conflict.RevisionWhereMergeWasCheckedIn);
				builder.Append(XmlUtilities.GetXmlForShowingInHtml(resulting));
			}
			builder.Append("</html>");
			return builder.ToString();

		}

		public string GetTypeLabel()
		{
			return "conflict";
		}

		public string GetIconName()
		{
			return "warning";
		}
	}
}