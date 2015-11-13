using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// This class does some high level 2-way diffing between parent and child sets of XML data.
	/// If the xml for individual main elements is not the same byte array,
	/// then this class calls into the more detailed xml diffing code, the entry point of which is
	/// the XmlUtilities.AreXmlElementsEqual method.
	/// </summary>
	public class Xml2WayDiffer
	{
		private enum DiffingMode
		{
			FromFileInRevisions,
			FromPathnames,
			FromMixed
		}

		private readonly IMergeEventListener _eventListener;
		private readonly HgRepository _repository;
		private readonly FileInRevision _parentFileInRevision;
		private readonly FileInRevision _childFileInRevision;
		private readonly string _startTag;
		private readonly string _firstElementTag;
		private readonly string _identfierAttribute;
		private readonly string _parentPathname;
		private readonly string _childPathname;
		private readonly DiffingMode _diffingMode;
		private readonly Dictionary<string, byte[]> _parentIndex;

		/// <summary>
		/// Create instance of Xml2WayDiffer
		/// </summary>
		public static Xml2WayDiffer CreateFromFileInRevision(FileInRevision parent, FileInRevision child,
			IMergeEventListener eventListener, HgRepository repository,
			string firstElementMarker,
			string startTag, string identfierAttribute)
		{
			return new Xml2WayDiffer(repository, eventListener, parent, child,
				firstElementMarker,
				startTag, identfierAttribute);
		}

		private Xml2WayDiffer(HgRepository repository, IMergeEventListener eventListener, FileInRevision parent, FileInRevision child,
			string firstElementMarker,
			string startTag, string identfierAttribute)
		{
			_diffingMode = DiffingMode.FromFileInRevisions;
			_repository = repository;
			_parentFileInRevision = parent;
			_childFileInRevision = child;
			if (!string.IsNullOrEmpty(firstElementMarker))
				_firstElementTag = firstElementMarker.Trim();
			_startTag = "<" + startTag.Trim();
			_identfierAttribute = identfierAttribute;
			_eventListener = eventListener;
		}

		/// <summary>
		/// Create instance of Xml2WayDiffer
		/// </summary>
		public static Xml2WayDiffer CreateFromFiles(string parentPathname, string childPathname,
			IMergeEventListener eventListener,
			string firstElementMarker,
			string startTag, string identfierAttribute)
		{
			return new Xml2WayDiffer(eventListener, parentPathname, childPathname,
				firstElementMarker,
				startTag, identfierAttribute);
		}

		private Xml2WayDiffer(IMergeEventListener eventListener, string parentPathname, string childPathname,
			string firstElementMarker,
			string startTag, string identfierAttribute)
		{
			_diffingMode = DiffingMode.FromPathnames;
			_parentPathname = parentPathname;
			_childPathname = childPathname;
			if (!string.IsNullOrEmpty(firstElementMarker))
				_firstElementTag = firstElementMarker.Trim();
			_startTag = "<" + startTag.Trim();
			_identfierAttribute = identfierAttribute;
			_eventListener = eventListener;
		}

		/// <summary>
		/// Create instance of Xml2WayDiffer
		/// </summary>
		public static Xml2WayDiffer CreateFromMixed(Dictionary<string, byte[]> parentIndex, string childPathname,
			IMergeEventListener eventListener,
			string firstElementMarker,
			string startTag, string identfierAttribute)
		{
			return new Xml2WayDiffer(eventListener, parentIndex, childPathname,
				firstElementMarker,
				startTag, identfierAttribute);
		}

		private Xml2WayDiffer(IMergeEventListener eventListener, Dictionary<string, byte[]> parentIndex, string childPathname,
			string firstElementMarker,
			string startTag, string identfierAttribute)
		{
			_diffingMode = DiffingMode.FromMixed;
			_parentIndex = parentIndex;
			_childPathname = childPathname;
			if (!string.IsNullOrEmpty(firstElementMarker))
				_firstElementTag = firstElementMarker.Trim();
			_startTag = "<" + startTag.Trim();
			_identfierAttribute = identfierAttribute;
			_eventListener = eventListener;
		}

		///<summary>
		/// Given the complete parent and child xml,
		/// this method compares 'records' (main xml elements) in the parent and child data sets
		/// at a high level of difference detection.
		///
		/// Low-level xml differences are handed to XmlUtilities.AreXmlElementsEqual for processing differences
		/// of each 'record'.
		///</summary>
		public Dictionary<string, byte[]> ReportDifferencesToListener()
		{
			Dictionary<string, byte[]> childIndex;
			Dictionary<string, byte[]> parentIndex;
			switch (_diffingMode)
			{
				default:
					throw new InvalidEnumArgumentException("Diffing mode not recognized.");
				case DiffingMode.FromFileInRevisions:
					PrepareIndicesUsingFilesInRepositorySource(out parentIndex, out childIndex);
					break;
				case DiffingMode.FromPathnames:
					PrepareIndicesUsingPathNameSource(out parentIndex, out childIndex);
					break;
				case DiffingMode.FromMixed:
					parentIndex = _parentIndex;
					PrepareIndexUsingPathNameSource(out childIndex);
					break;
			}

			foreach (var difference in Xml2WayDiffService.ReportDifferences(
								_parentFileInRevision, parentIndex,
								_childFileInRevision, childIndex))
			{
				_eventListener.ChangeOccurred(difference);
			}
			return parentIndex;
		}

		private void PrepareIndexUsingPathNameSource(out Dictionary<string, byte[]> childIndex)
		{
			const int estimatedObjectCount = 400;
			var fileInfo = new FileInfo(_childPathname);
			childIndex = new Dictionary<string, byte[]>((int)(fileInfo.Length / estimatedObjectCount), StringComparer.OrdinalIgnoreCase);
			using (var prepper = new MakeRecordDictionary(childIndex, _childPathname,
				_firstElementTag,
				_startTag, _identfierAttribute))
			{
				prepper.ShouldContinueAfterDuplicateKey = s =>
															{
																_eventListener.WarningOccurred(new MergeWarning(_childPathname + ": " + s));
																return true;
															};
				prepper.Run();
			}
		}

		private void PrepareIndicesUsingPathNameSource(out Dictionary<string, byte[]> parentIndex, out Dictionary<string, byte[]> childIndex)
		{
			PrepareIndicesUsingPathNameSource(_parentPathname, out parentIndex, _childPathname, out childIndex);
		}

		private void PrepareIndicesUsingFilesInRepositorySource(out Dictionary<string, byte[]> parentIndex, out Dictionary<string, byte[]> childIndex)
		{
			using (var parentTempFile = _parentFileInRevision.CreateTempFile(_repository))
			using (var childTempFile = _childFileInRevision.CreateTempFile(_repository))
			{
				PrepareIndicesUsingPathNameSource(parentTempFile.Path, out parentIndex, childTempFile.Path, out childIndex);
			}
		}

		private void PrepareIndicesUsingPathNameSource(string parentPathname, out Dictionary<string, byte[]> parentIndex,
			string childPathname, out Dictionary<string, byte[]> childIndex)
		{
			// This arbitrary length (400) is based on two large databases,
			// one 360M with 474 bytes/object, and one 180M with 541.
			// It's probably not perfect, but we're mainly trying to prevent
			// fragmenting the large object heap by growing it MANY times.
			const int estimatedObjectCount = 400;
			var fileInfo = new FileInfo(parentPathname);
			parentIndex = new Dictionary<string, byte[]>((int)(fileInfo.Length / estimatedObjectCount), StringComparer.OrdinalIgnoreCase);
			using (var prepper = new MakeRecordDictionary(parentIndex, parentPathname,
				_firstElementTag,
				_startTag, _identfierAttribute))
			{
				prepper.ShouldContinueAfterDuplicateKey = s =>
				{
					_eventListener.WarningOccurred(new MergeWarning(parentPathname +": "+s));
					return true;
				};

				prepper.Run();
			}
			fileInfo = new FileInfo(childPathname);
			childIndex = new Dictionary<string, byte[]>((int)(fileInfo.Length / estimatedObjectCount), StringComparer.OrdinalIgnoreCase);
			using (var prepper = new MakeRecordDictionary(childIndex, childPathname,
				_firstElementTag,
				_startTag, _identfierAttribute))
			{
				prepper.ShouldContinueAfterDuplicateKey = s =>
				{
					_eventListener.WarningOccurred(new MergeWarning(childPathname + ": " + s));
					return true;
				};

				prepper.Run();
			}
		}
	}
}
