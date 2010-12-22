using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// File handler for a FieldWorks 7.0 xml file.
	/// </summary>
	public class FieldWorksFileHandler : IChorusFileTypeHandler
	{
		private const string kExtension = "fwdata";
		private readonly Dictionary<string, bool> _filesChecked = new Dictionary<string, bool>();
		private readonly MetadataCache _mdc = new MetadataCache();

		/// <summary>
		/// For testing only.
		/// </summary>
		internal MetadataCache Mdc
		{
			get { return _mdc; }
		}

		#region Implementation of IChorusFileTypeHandler

		public bool CanDiffFile(string pathToFile)
		{
			return CheckThatInputIsValidFieldWorksFile(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return CheckThatInputIsValidFieldWorksFile(pathToFile);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CheckThatInputIsValidFieldWorksFile(pathToFile);
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
			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksMergingStrategy(mergeOrder.MergeSituation, _mdc),
				"AdditionalFields",
				"rt", "guid", WritePreliminaryInformation);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				"AdditionalFields",
				"rt", "guid");
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report is IXmlChangeReport)
				return new FieldWorksChangePresenter((IXmlChangeReport)report);

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
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
						{
							// Populate _mdc with optional custom properties.
							//if (reader.LocalName != "CustomField")
							//	continue;
							/*
<AdditionalFields>
<CustomField name="Certified" class="WfiWordform" type="Boolean" />
</AdditionalFields>
								*/
							while (reader.LocalName == "CustomField")
							{
								reader.MoveToAttribute("name");
								var propName = reader.Value;
								reader.MoveToAttribute("class");
								var className = reader.Value;
								reader.MoveToAttribute("type");
								var propDataType = reader.Value;
								// Add custom property to MDC.
								_mdc.AddCustomPropInfo(className, new FdoPropertyInfo(propName, propDataType));


								reader.MoveToElement();
								reader.Read();
							}
						}
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

			bool seenBefore;
			if (_filesChecked.TryGetValue(pathToFile, out seenBefore))
				return seenBefore;
			var retval = ValidateFile(pathToFile, null) == null;
			_filesChecked.Add(pathToFile, retval);
			return retval;
		}

		private static bool CheckValidPathname(string pathToFile)
		{
			// Just because all of this is true, doesn't mean it is a FW 7.0 xml file. :-(
			return !string.IsNullOrEmpty(pathToFile) // No null or empty string can be valid.
				&& File.Exists(pathToFile) // There has to be an actual file,
				&& Path.GetExtension(pathToFile).ToLowerInvariant() == "." + kExtension; // It better have kExtension for its extension.
		}

		private static void WritePreliminaryInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("languageproject");
			reader.MoveToAttribute("version");
			writer.WriteAttributeString("version", reader.Value);
			reader.MoveToElement();
			reader.Read();
		}
	}
}
