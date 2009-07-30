using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus.merge;
using Chorus.merge.text;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders
{
	public class TextFileTypeHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return (Path.GetExtension(pathToFile) == ".txt");
		}

		public bool CanMergeFile(string pathToFile)
		{
			return (Path.GetExtension(pathToFile) == ".txt");
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanMergeFile(pathToFile);
		}

		public void Do3WayMerge(MergeOrder order)
		{
			//TODO: this is not yet going to deal with conflicts at all!
			var contents = GetRawMerge(order.pathToOurs, order.pathToCommonAncestor, order.pathToTheirs);
			File.WriteAllText(order.pathToOurs, contents);
		}


		public static string GetRawMerge(string oursPath, string commonPath, string theirPath)
		{
			return RunProcess("diff3/bin/diff3.exe", "-m " + oursPath + " " + commonPath + " " + theirPath);
		}


		public static string RunProcess(string filePath, string arguments)
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;

			//NB: there is a post-build step in this project which copies the diff3 folder into the output
			p.StartInfo.FileName = filePath;
			p.StartInfo.Arguments = arguments;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			p.WaitForExit();
			ExecutionResult result = new ExecutionResult(p);
			if (result.ExitCode == 2)//0 and 1 are ok
			{
				throw new ApplicationException("Got error " + result.ExitCode + " " + result.StandardOutput + " " + result.StandardError);
			}
			Debug.WriteLine(result.StandardOutput);
			return result.StandardOutput;
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			yield return new TextEditChangeReport(parent, child, parent.GetFileContents(repository), child.GetFileContents(repository));
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report is TextEditChangeReport)
			{
				return new TextEditChangePresenter(report as TextEditChangeReport, repository);
			}
			return new DefaultChangePresenter(report, repository);
		}



		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

	}


	public class TextEditChangePresenter : IChangePresenter
	{
		private readonly TextEditChangeReport _report;
		private readonly HgRepository _repository;

		public TextEditChangePresenter(TextEditChangeReport report, HgRepository repository)
		{
			_report = report;
			_repository = repository;
		}

		public string GetDataLabel()
		{
			return Path.GetFileName(_report.PathToFile);
		}

		public string GetActionLabel()
		{
			return _report.ActionLabel;
		}

		public virtual string GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head><body>");

			if (style == "normal")
				builder.AppendFormat("<p>The file: '{0}' was {1}.</p>", Path.GetFileName(_report.PathToFile), GetActionLabel().ToLower());
			else
			{
				AppendRawDiffOfFiles(builder);
			}

			builder.Append("</body></html>");
			return builder.ToString();
		}

		protected void AppendRawDiffOfFiles(StringBuilder builder)
		{
			builder.AppendFormat("<p>The file: '{0}' was {1}.</p>", Path.GetFileName(_report.PathToFile), GetActionLabel().ToLower());

			try
			{
				AppendDiffOfTextFile(builder, _report);
			}
			catch (Exception error)
			{
				builder.Append("Could not retrieve or diff the file: " + error.Message);
			}

		}

		private void AppendDiffOfTextFile(StringBuilder builder, TextEditChangeReport r)
		{
			var modified = r.ChildFileInRevision.GetFileContents(_repository);

			if (r.ParentFileInRevision != null) // will be null when this file was just added
			{
				var original = r.ParentFileInRevision.GetFileContents(_repository);
				var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
				builder.Append(m.merge());
			}
			else
			{
				builder.Append(modified);
			}
		}



		public string GetTypeLabel()
		{
			return "--";
		}

		public virtual string GetIconName()
		{
			return "file";
		}
	}
}