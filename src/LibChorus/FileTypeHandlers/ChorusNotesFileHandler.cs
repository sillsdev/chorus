using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Chorus.notes;
using SIL.IO;
using SIL.Progress;
using SIL.Xml;

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// The Chorus Notes file is a "stand-off markup" file of annotations.
	/// It facilitate workflow of a team working on the target document, such as a dictionary.
	/// The file name is the target file + ".ChorusNotes".
	/// ChorusNotes-aware applications will make additions to let the team discuss issues and
	/// marke the status of things.  In addition, the chorus merger adds annotations when
	/// it encounters a conflict, so that the team can later review what was done by the merger and make changes.
	/// </summary>
	public class ChorusNotesFileHandler : IChorusFileTypeHandler
	{
		internal ChorusNotesFileHandler()
		{ }

		public bool CanDiffFile(string pathToFile)
		{
			return CanMergeFile(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return (System.IO.Path.GetExtension(pathToFile) == ".ChorusNotes");
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanDiffFile(pathToFile);
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
			XmlMergeService.Do3WayMerge(order,
				new ChorusNotesAnnotationMergingStrategy(order),
				false,
				null,
				"annotation", "guid");
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child, null, "annotation", "guid")
				.Where(change => !(change is XmlDeletionChangeReport)); // Remove any deletion reports.
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if ((report as IXmlChangeReport) != null)
			{
				return new NotePresenter(report as IXmlChangeReport, repository);
			}
			return new DefaultChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			var dom = new XmlDocument();
			dom.Load(file.Path);


			foreach (XmlNode e in dom.SafeSelectNodes("notes/annotation"))
			{
				yield return new XmlAdditionChangeReport(fileInRevision, e);
			}
		}

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return AnnotationRepository.FileExtension;
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