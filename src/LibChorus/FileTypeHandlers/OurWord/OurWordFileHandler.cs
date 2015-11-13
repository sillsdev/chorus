using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using SIL.Progress;

namespace Chorus.FileTypeHandlers.OurWord
{
	public class OurWordFileHandler : IChorusFileTypeHandler
	{
		internal OurWordFileHandler()
		{}

		static MethodInfo RetrieveRemoteMethod(string remoteMethodName)
		{
			var ourWordPath = Path.Combine(
				ExecutionEnvironment.DirectoryOfExecutingAssembly, "OurWordData.dll");
			var ourWordAssembly = Assembly.LoadFrom(ourWordPath);

			var mergerType = ourWordAssembly.GetType("OurWordData.Synchronize.Merger");

			return mergerType.GetMethod(remoteMethodName);
		}

		private bool OurWordAssemblyIsAvailable
		{
			get {
				return File.Exists(Path.Combine(
									   ExecutionEnvironment.DirectoryOfExecutingAssembly, "OurWordData.dll"));
			}
		}

		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			if (!OurWordAssemblyIsAvailable)
				return false;

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

		public bool CanValidateFile(string pathToFile)
		{
			return false;
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			if (!OurWordAssemblyIsAvailable)
			{
				throw new ApplicationException("OurWord Dll is not available to do the merge (so this should not have been called).");
			}


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

		public string ValidateFile(string pathToFile, IProgress progress)
		{
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
			if (!OurWordAssemblyIsAvailable)
			{
				return new string[] {};
			}

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
}
