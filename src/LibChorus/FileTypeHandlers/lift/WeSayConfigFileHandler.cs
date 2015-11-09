using System;
using System.Collections.Generic;
using System.Text;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using SIL.Progress;

namespace Chorus.FileTypeHandlers.lift
{
	public class WeSayConfigFileHandler : IChorusFileTypeHandler
	{
		internal WeSayConfigFileHandler()
		{}

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

		public bool CanValidateFile(string pathToFile)
		{
			return false;
		}
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			throw new NotImplementedException();
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return new IChangeReport[] {new DefaultChangeReport(parent, child,"Edited")};
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return new WeSayConfigChangePresenter(report, repository);
		}


		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "WeSayConfig";
			yield return "xml";
			yield return "css";
		}

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		public uint MaximumFileSize
		{
			get { return UInt32.MaxValue; }
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
				builder.AppendFormat("The configuration file for the WeSay project was edited.  This tool cannot present what changed in a friendly way.  However a 'raw' view of the changes is available.");
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