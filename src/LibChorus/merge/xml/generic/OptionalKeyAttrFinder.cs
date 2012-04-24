using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// This class should be used when there is an optional attribute that would provide our match,
	/// but some other finder strategy is needed when it isn't there.
	/// </summary>
	public class OptionalKeyAttrFinder : IFindNodeToMerge
	{
		private readonly string _key;
		private readonly IFindNodeToMerge _backup;

		public OptionalKeyAttrFinder(string key, IFindNodeToMerge backupFinder)
		{
			_key = key;
			_backup = backupFinder;
		}

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null || nodeToMatch == null)
				return null;
			var keyVal = nodeToMatch.Attributes[_key];
			if( keyVal != null)
			{
				var match = parentToSearchIn.SelectSingleNode(nodeToMatch.LocalName + "[" + keyVal + "]");
				if (match != null)
					return match;
			}

			return _backup.GetNodeToMerge(nodeToMatch, parentToSearchIn);

		}
	}
}