using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// each element-type can have custom merging code
	/// </summary>
	public class MergeStrategies
	{
		/// <summary>
		/// the list of custom strategies that have been installed
		/// </summary>
		public Dictionary<string, ElementStrategy> _elementStrategies = new Dictionary<string, ElementStrategy>();

		public MergeStrategies()
		{
			ElementStrategy s = new ElementStrategy();
			s.MergePartnerFinder = new FindTextDumb();
			this.SetStrategy("_"+XmlNodeType.Text, s);

			ElementStrategy def = new ElementStrategy();
			def.MergePartnerFinder = new FindByEqualityOfTree();
			this.SetStrategy("_defaultElement", def);
		}

		public void SetStrategy(string key, ElementStrategy strategy)
		{
			_elementStrategies[key] = strategy;
		}

		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			string name;
			switch (element.NodeType)
			{
				case XmlNodeType.Element:
					name = element.Name;
					break;
				default:
					name = "_"+element.NodeType;
					break;
			}

			ElementStrategy strategy;
			if (!_elementStrategies.TryGetValue(name, out strategy))
			{
				return _elementStrategies["_defaultElement"];
			}
			return strategy;
		}

		public IFindNodeToMerge GetMergePartnerFinder(XmlNode element)
		{
			return GetElementStrategy(element).MergePartnerFinder;
		}

//        private IMergeReportMaker GetDifferenceReportMaker(XmlNode element)
//        {
//            ElementStrategy strategy;
//            if (!this._mergeStrategies._elementStrategies.TryGetValue(element.Name, out strategy))
//            {
//                return new DefaultMergeReportMaker();
//            }
//            return strategy.mergeReportMaker;
//        }

	}

	public class ElementStrategy
	{
		/// <summary>
		/// Given a node in "ours" that we want to merge with "theirs", how do we identify the one in "theirs"?
		/// </summary>
		public IFindNodeToMerge MergePartnerFinder{ get; set;}

		//is this a level of the xml file that would consitute the minimal unit conflict-understanding
		//from a user perspecitve?
		//e.g., in a dictionary, this is the lexical entry.  In a text, it might be  a paragraph.
		public IGenerateContextDescriptor ContextDescriptorGenerator { get; set; }

		public static ElementStrategy CreateForKeyedElement(string keyAttributeName)
		{
			ElementStrategy strategy = new ElementStrategy();
			strategy.MergePartnerFinder = new FindByKeyAttribute(keyAttributeName);
			return strategy;
		}

		public string GetHumanDescription(XmlNode element)
		{
			return "not implemented";
		}

//        public string GetConflictContextIfAppropriate(XmlNode element)
//        {
//            return null;
//        }
	}

	//Given an element (however that is defined for a given file type (e.g. xml element for xml files)...
	//create a descriptor that can be used later to find the element again, as when reviewing conflict.
	//I note that while this is tying to be generic, it probably won't work as-is for really simple text files,
	//which would want a line number, not the contents of the line.
	public interface IGenerateContextDescriptor
	{
		string GenerateContextDescriptor(string mergeElement);
	}
}