using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

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
		public Dictionary<string, ElementStrategy> ElementStrategies{get;set;}

		public MergeStrategies()
		{
			ElementStrategies = new Dictionary<string, ElementStrategy>();
			ElementStrategy s = new ElementStrategy(true);//review: this says the default is to consder order relevant
			s.MergePartnerFinder = new FindTextDumb();
			this.SetStrategy("_"+XmlNodeType.Text, s);

			ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consder order relevant
			def.MergePartnerFinder = new FindByEqualityOfTree();
			this.SetStrategy("_defaultElement", def);
		}

		public void SetStrategy(string key, ElementStrategy strategy)
		{
			ElementStrategies[key] = strategy;
		}

		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			string key;
			switch (element.NodeType)
			{
				case XmlNodeType.Element:
					key = GetKeyViaHack(element);
					break;
				default:
					key = "_"+element.NodeType;
					break;
			}

			ElementStrategy strategy;
			if (!ElementStrategies.TryGetValue(key, out strategy))
			{
				return ElementStrategies["_defaultElement"];
			}
			return strategy;
		}

		private string GetKeyViaHack(XmlNode element)
		{
			var name = element.Name;
			switch (name)
			{
				default:
					// This really does stink, but I'm (RBR) not sure how to avoid it today!
					if (ElementStrategies.ContainsKey(name) || element.ParentNode == null)
						return name;
					// Combine parent name + element name as key (for new styled FW properties).
					var combinedKey = element.ParentNode.Name == "ownseq" ? element.ParentNode.Attributes["class"].Value + "_" + name : element.ParentNode.Name + "_" + name;
					if (ElementStrategies.ContainsKey(combinedKey))
						return combinedKey;
					break;
				case "special":
					var foundHack = false;
					foreach (var attrName in from XmlNode attr in element.Attributes select attr.Name)
					{
						switch (attrName)
						{
							default:
								break;
							case "xmlns:palaso":
							case "xmlns:fw":
								name += "_" + attrName;
								foundHack = true;
								break;
						}
						if (foundHack)
							break;
					}
					break;
				case "Custom": // Another hack for FW custom property elements. (If this proves to conflict with WeSay, then move preliminary processing elsewhere for FW Custom properties to get past the Custom element.
					var customPropName = element.Attributes["name"].Value;
					name += "_" + customPropName;
					var combinedCustomKey = name + element.ParentNode.Name + "_" + customPropName;
					if (ElementStrategies.ContainsKey(combinedCustomKey))
						return combinedCustomKey;
					break;
			}

			return name;
		}

		public IFindNodeToMerge GetMergePartnerFinder(XmlNode element)
		{
			return GetElementStrategy(element).MergePartnerFinder;
		}

//        private IMergeReportMaker GetDifferenceReportMaker(XmlNode element)
//        {
//            ElementStrategy strategy;
//            if (!this._mergeStrategies.ElementStrategies.TryGetValue(element.Name, out strategy))
//            {
//                return new DefaultMergeReportMaker();
//            }
//            return strategy.mergeReportMaker;
//        }

	}

	public class ElementStrategy : IElementDescriber
	{
		/// <summary>
		/// Given a node in "ours" that we want to merge with "theirs", how do we identify the one in "theirs"?
		/// </summary>
		public IFindNodeToMerge MergePartnerFinder{ get; set;}

		//is this a level of the xml file that would consitute the minimal unit conflict-understanding
		//from a user perspective?
		//e.g., in a dictionary, this is the lexical entry.  In a text, it might be  a paragraph.
		public IGenerateContextDescriptor ContextDescriptorGenerator { get; set; }

		public  ElementStrategy(bool orderIsRelevant)
		{
			OrderIsRelevant = orderIsRelevant;
			AttributesToIgnoreForMerging = new List<string>();
		}


		/// <summary>
		/// Is the order of this element among its peers relevant (this says nothing about its children)
		/// </summary>
		public bool OrderIsRelevant { get; set; }

		/// <summary>
		/// The modified data is often an attribute that is worth ignoring
		/// </summary>
		public List<string> AttributesToIgnoreForMerging{get; private set;}

		/// <summary>
		/// This is performance tweak: if you know that a keyed type element is never changed
		/// once created (e.g. chorus notes messages), then we can set this to true and not
		/// bother comparing it to the other guy's.
		/// </summary>
		public bool IsImmutable{get;set;}

		/// <summary>
		/// This allows for an element to be declared 'atomic'.
		/// When set to true, no merging will be done.
		/// If the compared elemetns are not the same,
		/// then a conflict report will be produced.
		///
		/// The default is 'false'.
		/// </summary>
		public bool IsAtomic { get; set; }

		public static ElementStrategy CreateForKeyedElement(string keyAttributeName, bool orderIsRelevant)
		{
			ElementStrategy strategy = new ElementStrategy(orderIsRelevant);
			strategy.MergePartnerFinder = new FindByKeyAttribute(keyAttributeName);
			return strategy;
		}

		/// <summary>
		/// Declare that there can only be a single element with this name in a list of children
		/// </summary>
		public static ElementStrategy CreateSingletonElement()
		{
			ElementStrategy strategy = new ElementStrategy(false);
			strategy.MergePartnerFinder = new FindFirstElementWithSameName();
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

	public interface IElementDescriber
	{
		string GetHumanDescription(XmlNode element);
	}

	//Given an element (however that is defined for a given file type (e.g. xml element for xml files)...
	//create a descriptor that can be used later to find the element again, as when reviewing conflict.
	//for an xml file, this can be an xpath.
	//I note that while this is tying to be generic, it probably won't work as-is for really simple text files,
	//which would want a line number, not the contents of the line.
	public interface IGenerateContextDescriptor
	{
		ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath);
	}

	public class ContextDescriptor
	{
		/// <summary>
		/// (XPath query for xml) Something at the right level to show in a list view, e.g. a lexical entry or chapter/verse
		/// </summary>
		public string PathToUserUnderstandableElement { get; set; }

		/// <summary>
		/// Like, what you would use to refer to the PathToUserUnderstandableElement, e.g. ("dog", or "M5:3")
		/// </summary>
		public string DataLabel { get; set; }

		public ContextDescriptor(string dataLabel, string path)
		{
			DataLabel = dataLabel;
			PathToUserUnderstandableElement = path;
		}

		public static ContextDescriptor CreateFromXml(XmlNode xmlRepresentation)
		{
			var path = xmlRepresentation.GetOptionalStringAttribute("contextPath", "missing");
			var label = xmlRepresentation.GetOptionalStringAttribute("contextDataLabel", "missing");

			return new ContextDescriptor(label, path);
		}

		public void WriteAttributes(XmlWriter writer)
		{
			writer.WriteAttributeString("contextPath", string.Empty, PathToUserUnderstandableElement);
			writer.WriteAttributeString("contextDataLabel", string.Empty, DataLabel);
		}
	}

	public class NullContextDescriptor : ContextDescriptor
	{
		public NullContextDescriptor() : base("unknown", "unknown")
		{
		}
	}
}