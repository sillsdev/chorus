using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Class that does the bulk of the 3-way merging for FieldWorks 7.0 data.
	/// </summary>
	internal class FieldWorksMerger
	{
		private readonly MergeOrder _mergeOrder;
		private readonly IMergeStrategy _mergeStategy;
		private readonly string _pathToWinner;
		private readonly string _pathToLoser;
		private readonly string _pathToCommonAncestor;
		private readonly string _winnerId;

		internal FieldWorksMerger(MergeOrder mergeOrder, IMergeStrategy mergeStategy, string pathToWinner, string pathToLoser, string pathToCommonAncestor, string winnerId)
		{
			_mergeOrder = mergeOrder;
			_mergeStategy = mergeStategy;
			_pathToWinner = pathToWinner;
			_pathToLoser = pathToLoser;
			_pathToCommonAncestor = pathToCommonAncestor;
			_winnerId = winnerId;
		}

		internal void DoMerge(string outputPathname)
		{
			XmlMergeService.Do3WayMerge(_mergeStategy, _mergeOrder, EventListener,
									_pathToWinner, _pathToLoser, _pathToCommonAncestor,
									outputPathname, _winnerId, "rt", "guid", WritePreliminaryInformation);
		}

		private static void WritePreliminaryInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("languageproject");
			reader.MoveToAttribute("version");
			writer.WriteAttributeString("version", reader.Value);
			reader.MoveToElement();

			// Deal with optional custom field declarations.
			if (reader.LocalName == "AdditionalFields")
			{
				writer.WriteNode(reader, false);
			}
		}

		internal IMergeEventListener EventListener
		{ get; set; }
	}
}