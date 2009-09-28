using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.lift
{
	public class WeSayConfigFileHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			return false;
		}

		public bool CanPresentFile(string pathToFile)
		{
			return (System.IO.Path.GetExtension(pathToFile).ToLower() == ".wesayconfig");

		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return new IChangeReport[] {new DefaultChangeReport(parent, child,"Editted")};
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return new WeSayConfigChangePresenter(report, repository);
		}


		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "WeSayConfig";
			yield return "xml";
			yield return "css";
		}
	}

	public class WeSayConfigChangePresenter : DefaultChangePresenter
	{
		public WeSayConfigChangePresenter(IChangeReport report, HgRepository repository):base(report, repository)
		{
		}



		public override string GetIconName()
		{
			return "WesayConfig";
		}
		public override string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head><body>");

			if (style == "normal")
				builder.AppendFormat("The configuration file for the WeSay project was editted.  This tool cannot present what changed in a friendly way.  However a 'raw' view of the changes is available.");
			else
			{
				AppendRawDiffOfFiles(builder);
			}

			builder.Append("</body></html>");
			return builder.ToString();
		}

//        public string GetHtml(string style, string styleSheet)
//        {
//            var builder = new StringBuilder();
//            builder.Append("<html><head>" + styleSheet + "</head>");
//
//
//            else if (_report is XmlChangedRecordReport)
//            {
//                GetHtmlForChange(style, builder);
//            }
//            builder.Append("</html>");
//            return builder.ToString();
//        }

	}
}