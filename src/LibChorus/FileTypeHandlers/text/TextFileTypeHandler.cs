using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Progress;

namespace Chorus.FileTypeHandlers.text
{
	[Export(typeof(IChorusFileTypeHandler))]
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
		   // Debug.Fail("hello");
			// FailureSimulator is only used by tests to force a failure.
			FailureSimulator.IfTestRequestsItThrowNow("TextMerger");

			//trigger on a particular file name
			// FailureSimulator is only used by tests to force a failure.
			FailureSimulator.IfTestRequestsItThrowNow("TextMerger-"+Path.GetFileName(order.pathToOurs));

			//Throws on conflict
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
			if(result.ExitCode == 1)//0 and 1 are ok
			{
				throw new ApplicationException("Could not merge text files without conflict: " + result.StandardOutput);
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

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
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
		private static extern int GetShortPathName(
			[MarshalAs(UnmanagedType.LPTStr)] string path,
			[MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
			int shortPathLength);


		public static string GetShortPath(string path)
		{
			if (Platform.IsMono)
				return path;

			var shortPath = new StringBuilder(255);

			GetShortPathName(path, shortPath, shortPath.Capacity);
			return shortPath.ToString();
		}
	}
}