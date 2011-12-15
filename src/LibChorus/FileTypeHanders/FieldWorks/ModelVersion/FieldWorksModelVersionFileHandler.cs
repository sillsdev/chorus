using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Chorus.FileTypeHanders.FieldWorks.ModelVersion
{
	/// <summary>
	/// Handle the FieldWorks model version file.
	/// </summary>
	/// <remarks>
	/// This file uses JSON data in the form: {"modelversion": #####}
	/// </remarks>
	public class FieldWorksModelVersionFileHandler : IChorusFileTypeHandler
	{
		internal FieldWorksModelVersionFileHandler()
		{}

		private const string kExtension = "ModelVersion";

		#region Implementation of IChorusFileTypeHandler

		public bool CanDiffFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, kExtension))
				return false;

			return DoValidation(pathToFile) == null;
		}

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			// TODO: Update MDC to latest model version.

			if (mergeOrder.EventListener is NullMergeEventListener)
				mergeOrder.EventListener = new ChangeAndConflictAccumulator();

			// The bigger model number wins, no matter if it came in ours or theirs.
			var commonData = File.ReadAllText(mergeOrder.pathToCommonAncestor);
			var commonNumber = Int32.Parse(SplitData(commonData)[1]);
			var ourData = File.ReadAllText(mergeOrder.pathToOurs);
			var ourNumber = Int32.Parse(SplitData(ourData)[1]);
			var theirData = File.ReadAllText(mergeOrder.pathToTheirs);
			var theirNumber = Int32.Parse(SplitData(theirData)[1]);
			if (commonNumber == ourNumber && commonNumber == theirNumber)
				return; // No changes.
			if (ourNumber < commonNumber || theirNumber < commonNumber)
				throw new InvalidOperationException("Downgrade in model version number");

			var listener = mergeOrder.EventListener;
			if (ourNumber > theirNumber)
			{
				listener.ChangeOccurred(new FieldWorksModelVersionUpdatedReport(mergeOrder.pathToOurs, commonNumber, ourNumber));
			}
			else
			{
				// Put their number in our file. {"modelversion": #####}
				var newFileContents = "{\"modelversion\": " + theirNumber + "}";
				File.WriteAllText(mergeOrder.pathToOurs, newFileContents);
				listener.ChangeOccurred(new FieldWorksModelVersionUpdatedReport(mergeOrder.pathToTheirs, commonNumber, theirNumber));
			}
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			var diffReports = new List<IChangeReport>(1);

			// The only relevant change to report is the version number.
			var childData = child.GetFileContents(repository);
			var splitData = SplitData(childData);
			var childModelNumber = Int32.Parse(splitData[1]);
			if (parent ==  null)
			{
				diffReports.Add(new FieldWorksModelVersionAdditionChangeReport(child, childModelNumber));
			}
			else
			{
				var parentData = parent.GetFileContents(repository);
				splitData = SplitData(parentData);
				var parentModelNumber = Int32.Parse(splitData[1]);
				if (parentModelNumber != childModelNumber)
					diffReports.Add(new FieldWorksModelVersionUpdatedReport(parent, child, parentModelNumber, childModelNumber));
				else
					throw new InvalidOperationException("The version number has downgraded");
			}

			return diffReports;
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report is FieldWorksModelVersionChangeReport)
				return new FieldWorksModelVersionChangePresenter((FieldWorksModelVersionChangeReport)report);

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
		}

		/// <summary>
		/// return null if valid, otherwise nice verbose description of what went wrong
		/// </summary>
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			return DoValidation(pathToFile);
		}

		/// <summary>
		/// This is like a diff, but for when the file is first checked in.  So, for example, a dictionary
		/// handler might list any the words that were already in the dictionary when it was first checked in.
		/// </summary>
		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return kExtension;
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

		#endregion

		private static string DoValidation(string pathToFile)
		{
			try
			{
				// Uses JSON: {"modelversion": #####}
				var data = File.ReadAllText(pathToFile);
				var splitData = SplitData(data);
				if (splitData.Length == 2 && splitData[0] == "\"modelversion\"" && int.Parse(splitData[1].Trim()) >= 7000000)
					return null;
				return "Not a valid JSON model version file.";
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		private static string[] SplitData(string data)
		{
			return data.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
