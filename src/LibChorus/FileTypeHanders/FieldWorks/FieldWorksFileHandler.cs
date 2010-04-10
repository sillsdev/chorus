using System;
using System.Collections.Generic;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// File handler for a FieldWorks 7.0+ xml file.
	/// </summary>
	public class FieldWorksFileHandler : IChorusFileTypeHandler
	{
		#region Implementation of IChorusFileTypeHandler

		public bool CanDiffFile(string pathToFile)
		{
			throw new NotImplementedException();
		}

		public bool CanMergeFile(string pathToFile)
		{
			throw new NotImplementedException();
		}

		public bool CanPresentFile(string pathToFile)
		{
			throw new NotImplementedException();
		}

		public bool CanValidateFile(string pathToFile)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			throw new NotImplementedException();
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// return null if valid, otherwise nice verbose description of what went wrong
		/// </summary>
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// This is like a diff, but for when the file is first checked in.  So, for example, a dictionary
		/// handler might list any the words that were already in the dictionary when it was first checked in.
		/// </summary>
		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
