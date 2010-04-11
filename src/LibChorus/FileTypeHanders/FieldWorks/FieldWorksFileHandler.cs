using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.Utilities.code;
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
			//if (!CheckValidPathname(pathToFile))
			//    return false;

			//using (var reader = File.OpenText(pathToFile))
			//{
			//    while (!reader.EndOfStream)
			//    {
			//        var line = reader.ReadLine();
			//        if (line != null && line.Contains("<languageproject"))
			//            return true;
			//    }
			//}
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
			return CheckValidPathname(pathToFile);
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
			return XmlValidation.ValidateFile(pathToFile, progress);
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
			yield return "xml";
		}

		#endregion

		private static bool CheckValidPathname(string pathToFile)
		{
			return !string.IsNullOrEmpty(pathToFile) && File.Exists(pathToFile) && Path.GetExtension(pathToFile).ToLower() == ".xml";
		}
	}
}
