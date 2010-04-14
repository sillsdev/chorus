using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
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
		private const string kExtension = "xml";

		#region Implementation of IChorusFileTypeHandler

		public bool CanDiffFile(string pathToFile)
		{
			//return CheckThatInputIsValidFieldWorksFile(pathToFile);
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			//return CheckThatInputIsValidFieldWorksFile(pathToFile);
			return false;
		}

		public bool CanPresentFile(string pathToFile)
		{
			//return CheckThatInputIsValidFieldWorksFile(pathToFile);
			return false;
		}

		public bool CanValidateFile(string pathToFile)
		{
			if (!CheckValidPathname(pathToFile))
				return false;

			try
			{
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					return reader.LocalName == "languageproject" && reader.MoveToAttribute("version");
				}
			}
			catch
			{
				return false;
			}
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
			try
			{
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					if (reader.LocalName == "languageproject" && reader.MoveToAttribute("version"))
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{}
					}
					else
					{
						throw new InvalidOperationException("Not a FieldWorks file.");
					}
				}
			}
			catch (Exception error)
			{
				return error.Message;
			}
			return null;
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

		#endregion

		private bool CheckThatInputIsValidFieldWorksFile(string pathToFile)
		{
			if (!CheckValidPathname(pathToFile))
				return false;

			return ValidateFile(pathToFile, null) == null;
		}

		private static bool CheckValidPathname(string pathToFile)
		{
			// Just because all of this is true, doesn't mean it is a FW 7.0 xml file. :-(

			return !string.IsNullOrEmpty(pathToFile) // No null or empty string can be valid.
				&& File.Exists(pathToFile) // There has to be an actual file,
				&& Path.GetExtension(pathToFile).ToLowerInvariant() == "." + kExtension; // It better have kExtension for its extension.
		}
	}
}
