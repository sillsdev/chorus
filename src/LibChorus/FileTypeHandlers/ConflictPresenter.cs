using System;
using System.IO;
using System.Text;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers;
using SIL.Xml;

/********************
 *
 *
 *
 * This stuff is defunct. Conflicts now are displayed by the notes system,
 * not the change presentation system.
 *
 * However, I (John) have so burnt ot on this stuff that I haven't moved
 * the functionality here over to the new system, so it sits her waiting.
 *
 *
 ***************************/

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// The key thing to understand here is that the conflict file only *grows*... it doesn't get edited
	/// normally.  So the only changes we're interested in are *additions* of conflict reports to the file.
	/// </summary>
	public class ConflictPresenter : IChangePresenter
	{
		private readonly IRetrieveFileVersionsFromRepository _fileRetriever;
		private readonly IXmlChangeReport _report;
		private IConflict _conflict;

		public ConflictPresenter(IXmlChangeReport report, IRetrieveFileVersionsFromRepository fileRetriever)
		{
			_fileRetriever = fileRetriever;
			_report = report;// as XmlAdditionChangeReport;
			if (_report == null)
			{
				_conflict = new UnreadableConflict(report.ChildNode);
			}
			else
			{
				if (_report.ChildNode.Name == "conflict") // old style situation, only on Tok Pisin before Oct 2009
				{
					_conflict = Conflict.CreateFromConflictElement(_report.ChildNode);
				}
				else
				{
					var conflictNode = _report.ChildNode.SelectSingleNode("data/conflict");
					if (conflictNode != null)
					{
						_conflict = Conflict.CreateFromConflictElement(conflictNode);
					}
					else
					{
						_conflict = new UnreadableConflict(_report.ChildNode);
					}
				}
			}
		}

		public string GetDataLabel()
		{
			return _conflict.Context.DataLabel;
		}

		public string GetActionLabel()
		{
			var label = _report.ChildNode.GetOptionalStringAttribute("class", string.Empty);//the annotation class
			if (label == string.Empty)
			{   //handle pre-oct09 TokPisin experiment format, which had the conflict in their own file, not wrapped in annotations
				label = _report.ChildNode.GetOptionalStringAttribute("type", string.Empty);
			}
			return label;
		}

		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>"+styleSheet +"</head>");
			if (style == "normal")
			{
				if (_conflict is UnreadableConflict)
				{
					builder.Append(((UnreadableConflict)_conflict).ConflictNode.OuterXml);
				}
				else if (_conflict is UnmergableFileTypeConflict)
				{
					builder.Append(((UnmergableFileTypeConflict)_conflict).GetFullHumanReadableDescription());
				}
				else if (_report is XmlAdditionChangeReport)
				{
					builder.AppendFormat(
						"<p>{0} and {1} both edited {2} in the file {3} in a way that could not be automatically merged.</p>",
						_conflict.Situation.AlphaUserId, _conflict.Situation.BetaUserId, _conflict.Context.DataLabel,
						_conflict.RelativeFilePath);

					builder.AppendFormat("<p></p>");

					// XmlUtilities.GetXmlForShowingInHtml(_report.ChildNode.OuterXml));
				}

			}
			else
			{
				if (_conflict is UnreadableConflict)
				{
					builder.Append(((UnreadableConflict) _conflict).ConflictNode.OuterXml);
				}
				else if (_conflict is UnmergableFileTypeConflict)
				{
					builder.Append(((UnmergableFileTypeConflict) _conflict).GetFullHumanReadableDescription());
				}
				else
				{

					builder.AppendFormat(
						"{0} and {1} both edited {2} in a way that could not be merged. Where they conflicted, {3}'s version was kept.<br/>",
						_conflict.Situation.AlphaUserId, _conflict.Situation.BetaUserId, _conflict.Context.DataLabel,
						_conflict.WinnerId);

					builder.AppendFormat(
						"The kind of conflict was: {0}", _conflict.Description);
					//                var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
					//                builder.Append(m.merge());

					builder.AppendFormat("<h3>Original Record</h3>");
					var ancestor = _conflict.GetConflictingRecordOutOfSourceControl(_fileRetriever,
																					ThreeWayMergeSources.Source.Ancestor);
					builder.Append(XmlUtilities.GetXmlForShowingInHtml(ancestor));
					builder.AppendFormat("<h3>{0}'s version</h3>", _conflict.Situation.AlphaUserId);
					var userXVersion = _conflict.GetConflictingRecordOutOfSourceControl(_fileRetriever,
																						ThreeWayMergeSources.Source.
																							UserX);
					builder.Append(XmlUtilities.GetXmlForShowingInHtml(userXVersion));
					builder.AppendFormat("</p><h3>{0}'s version</h3>", _conflict.Situation.BetaUserId);
					var userYVersion = _conflict.GetConflictingRecordOutOfSourceControl(_fileRetriever,
																						ThreeWayMergeSources.Source.
																							UserY);
					builder.Append(XmlUtilities.GetXmlForShowingInHtml(userYVersion));
					builder.AppendFormat("</p><h3>Resulting version</h3>", _conflict.Situation.BetaUserId);

					string resulting = "";
					try
					{
						resulting = _fileRetriever.RetrieveHistoricalVersionOfFile(_conflict.RelativeFilePath,
																					   _conflict.
																						   RevisionWhereMergeWasCheckedIn);
						builder.Append(XmlUtilities.GetXmlForShowingInHtml(resulting));
					}
					catch (Exception error)
					{
						if (File.Exists(resulting))
							File.Delete(resulting);
						builder.Append("Could not retrieve the file. The error was: " + error.Message);
					}
				}
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
			return "conflict";
		}
	}
}