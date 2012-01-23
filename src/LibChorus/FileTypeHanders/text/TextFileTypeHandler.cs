using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Chorus.FileTypeHanders.text
{


	public class TextFileTypeHandler : IChorusFileTypeHandler
	{
		internal TextFileTypeHandler()
		{}

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

		public bool CanValidateFile(string pathToFile)
		{
			return false;
		}
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			throw new NotImplementedException();
		}

		public void Do3WayMerge(MergeOrder order)
		{
#if DEBUG
		   // Debug.Fail("hello");
			FailureSimulator.IfTestRequestsItThrowNow("TextMerger");

			//trigger on a particular file name
			FailureSimulator.IfTestRequestsItThrowNow("TextMerger-"+Path.GetFileName(order.pathToOurs));
#endif


			//TODO: this is not yet going to deal with conflicts at all!
			var contents = GetRawMerge(order.pathToOurs, order.pathToCommonAncestor, order.pathToTheirs);
			File.WriteAllText(order.pathToOurs, contents);
		}


		public static string GetRawMerge(string oursPath, string commonPath, string theirPath)
		{
			//NB: surrounding with quotes didn't cut it to get past paths with spaces

			return RunProcess("diff3/bin/diff3.exe", "-m " + LongToShortConverter.GetShortPath(oursPath) + " " +
				LongToShortConverter.GetShortPath(commonPath) + " " +
				LongToShortConverter.GetShortPath(theirPath));
		}

		protected static string SurroundWithQuotes(string path)
		{
			return "\"" + path + "\"";
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

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "txt";
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

	//Todo: not gonna work in Linux
	class LongToShortConverter
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]

		public static extern int GetShortPathName(

				 [MarshalAs(UnmanagedType.LPTStr)]

				   string path,

				 [MarshalAs(UnmanagedType.LPTStr)]

				   StringBuilder shortPath,

				 int shortPathLength

				 );



		public static string GetShortPath(string path)
		{

			StringBuilder shortPath = new StringBuilder(255);

			GetShortPathName(path, shortPath, shortPath.Capacity);
			return shortPath.ToString();
		}
	}
}