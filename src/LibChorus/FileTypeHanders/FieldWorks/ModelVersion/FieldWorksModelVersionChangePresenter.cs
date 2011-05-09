using System.Text;
using Chorus.merge;

namespace Chorus.FileTypeHanders.FieldWorks.ModelVersion
{
	///<summary>
	///</summary>
	public class FieldWorksModelVersionChangePresenter : IChangePresenter
	{
		private readonly FieldWorksModelVersionChangeReport _report;

		/// <summary>
		/// Constructor
		/// </summary>
		public FieldWorksModelVersionChangePresenter(FieldWorksModelVersionChangeReport report)
		{
			_report = report;
		}

		#region Implementation of IChangePresenter

		public string GetDataLabel()
		{
			return "FDO model version";
		}

		public string GetActionLabel()
		{
			return _report.ActionLabel;
		}

		public string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head>");

			switch (_report.GetType().Name)
			{
				default:
					builder.Append("Don't know how to display a report of type: " + _report.GetType().Name + " <p/>");
					break;
				case "FieldWorksModelVersionAdditionChangeReport":
					var additionChangeReport = (FieldWorksModelVersionAdditionChangeReport)_report;
					builder.AppendFormat("Added the version file for the following model version: {0} <p/>", additionChangeReport.NewModelVersion);
					break;
				case "FieldWorksModelVersionUpdatedReport":
					var changedRecordReport = (FieldWorksModelVersionUpdatedReport)_report;
					builder.AppendFormat("Updated the model version number from {0} to {1}. <p/>", changedRecordReport.OldModelVersion, changedRecordReport.NewModelVersion);
					break;
			}
			builder.Append("</html>");
			return builder.ToString();
		}

		public string GetTypeLabel()
		{
			return "FieldWorks data model version";
		}

		public string GetIconName()
		{
			return "file";
		}

		#endregion
	}
}
