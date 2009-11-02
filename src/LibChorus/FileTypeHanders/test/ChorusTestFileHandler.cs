using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.test
{
	public class ChorusTestFileHandler : IChorusFileTypeHandler
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
			return false;
		}

		public bool CanValidateFile(string pathToFile)
		{
			return Path.GetExtension(pathToFile)==".chorusTest";
		}
		public bool GetFileIsValid(string pathToFile, IProgress progress)
		{
			return !File.ReadAllText(pathToFile).Contains("invalid");
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

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "chorusTest";
		}
	}
}