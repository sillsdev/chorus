using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using SIL.Progress;

namespace Chorus.FileTypeHandlers.test
{
	public class ChorusTestFileHandler : IChorusFileTypeHandler
	{
		internal ChorusTestFileHandler()
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
			return false;
		}

		public bool CanValidateFile(string pathToFile)
		{
			return Path.GetExtension(pathToFile)==".chorusTest";
		}
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			if (File.ReadAllText(pathToFile).Contains("invalid"))
				return "Failed to validate because it contained the word 'invalid'.";
			return null;
		}
		public static string GetInvalidContents()
		{
			return "invalid";
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			throw new NotImplementedException();
		}


		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			//this is never called because we said we don't do diffs yet; review is handled some other way
			throw new NotImplementedException();
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			//this is never called because we said we don't present diffs; review is handled some other way
			throw new NotImplementedException();
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			//this is never called because we said we don't present diffs; review is handled some other way
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "chorusTest";
		}

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		public uint MaximumFileSize
		{
			get { return LargeFileFilter.Megabyte / 1024; }
		}
	}
}