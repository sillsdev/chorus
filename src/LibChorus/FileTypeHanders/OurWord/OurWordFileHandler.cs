using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.OurWord
{
	public class OurWordFileHandler : IChorusFileTypeHandler
	{
		static MethodInfo RetrieveRemoteMethod(string remoteMethodName)
		{
			var ourWordPath = Path.Combine(
				Other.DirectoryOfExecutingAssembly, "OurWordData.dll");
			var ourWordAssembly = Assembly.LoadFrom(ourWordPath);

			var mergerType = ourWordAssembly.GetType("OurWordData.Synchronize.Merger");

			return mergerType.GetMethod(remoteMethodName);
		}

		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			try
			{
				var method = RetrieveRemoteMethod("CanMergeFile");
				return (bool)method.Invoke(null, new object[] { pathToFile });
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}

		public bool CanPresentFile(string pathToFile)
		{
			return false;
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			// Debug.Fail("LibChorus.FileTypeHandlers.OurWord.Do3WayMerge - For debugging.");

			var method = RetrieveRemoteMethod("Do3WayMerge");
			method.Invoke(null, new object[]{mergeOrder});
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
			try
			{
				var method = RetrieveRemoteMethod("GetExtensionsOfKnownTextFileTypes");
				return  (IEnumerable<string>) method.Invoke(null, null);
			}
			catch(Exception)
			{
				return null;
			}
		}
	}
}
