using System.Collections.Generic;
using System.Xml;
using Palaso.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// each element-type can have custom merging code
	/// </summary>
	public class MergeStrategies
	{
		private Dictionary<string, ElementStrategy> _elementStrategies;
		private readonly HashSet<string> _elementStrategyKeys = new HashSet<string>();

		/// <summary>
		/// the list of custom strategies that have been installed
		/// </summary>
		public Dictionary<string, ElementStrategy> ElementStrategies
		{
			get { return _elementStrategies; }
			set
			{
				_elementStrategies = value;
				_elementStrategyKeys.UnionWith(_elementStrategies.Keys);
			}
		}

		public MergeStrategies()
		{
			ElementStrategies = new Dictionary<string, ElementStrategy>();
			ElementStrategy s = new ElementStrategy(true);//review: this says the default is to consider order relevant
			s.MergePartnerFinder = new FindTextDumb();
			SetStrategy("_"+XmlNodeType.Text, s);

			ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
			def.MergePartnerFinder = new FindByEqualityOfTree();
			SetStrategy("_defaultElement", def);

			ElementToMergeStrategyKeyMapper = new DefaultElementToMergeStrategyKeyMapper();
		}

		/// <summary>
		/// Get or set the IKeyFinder implementation that is used by some domain to find the key to use to get the correct ElementStrategy.
		///
		/// It starts out using the DefaultKeyFinder, which uses the element's name.
		/// </summary>
		public IElementToMergeStrategyKeyMapper ElementToMergeStrategyKeyMapper { get; set; }

		public void SetStrategy(string key, ElementStrategy strategy)
		{
			ElementStrategies[key] = strategy;
			_elementStrategyKeys.Add(key);
		}

		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			string key;
			switch (element.NodeType)
			{
				case XmlNodeType.Element:
					key = ElementToMergeStrategyKeyMapper.GetKeyFromElement(_elementStrategyKeys, element);
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
		public ElementStrategy(bool orderIsRelevant)
		{
			OrderIsRelevant = orderIsRelevant;
			AttributesToIgnoreForMerging = new List<string>();
			NumberOfChildren = NumberOfChildrenAllowed.ZeroOrMore;
			ChildOrderPolicy = new AskChildrenOrderPolicy();
			Premerger = new DefaultPremerger();
		}

		/// <summary>
		/// Given a node in "ours" that we want to merge with "theirs", how do we identify the one in "theirs"?
		/// </summary>
		public IFindNodeToMerge MergePartnerFinder{ get; set;}

		//is this a level of the xml file that would consitute the minimal unit conflict-understanding
		//from a user perspective?
		//e.g., in a dictionary, this is the lexical entry.  In a text, it might be  a paragraph.
		public IGenerateContextDescriptor ContextDescriptorGenerator { get; set; }

		/// <summary>
		/// Get or set the number of allowed child elements. Default is: <see cref="NumberOfChildrenAllowed.ZeroOrMore"/>.
		/// </summary>
		public NumberOfChildrenAllowed NumberOfChildren { get; set; }

		/// <summary>
		/// Is the order of this element among its peers relevant (this says nothing about its children).
		/// This may be overridden if the parent element specifies a ChildOrderPolicy other than AskChildren (the default).
		/// </summary>
		public bool OrderIsRelevant { get; set; }

		/// <summary>
		/// Set this to one of the three simple policies or something more complex to determine the
		/// relevance of the order of the children of this element. This takes priority over the OrderIsRelevant
		/// value for the children (unless the policy returns AskChildren, the default).
		/// </summary>
		public IChildOrderPolicy ChildOrderPolicy { get; set; }

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
		/// If the compared elements are not the same,
		/// then a conflict report will be produced.
		///
		/// The default is 'false'.
		/// </summary>
		public bool IsAtomic { get; set; }

		/// <summary>
		/// Allow clients to do something special to the nodes before regular merging takes place.
		/// </summary>
		public IPremerger Premerger { internal get; set; }

		/// <summary>
		/// This is relevant only when IsAtomic is true. It allows a special case when the atomic element has only text children,
		/// which is common for nodes like [text] nodes in LIFT, so that we can get more helpful text conflict reports etc.
		/// We fall back to the atomic strategy if the element has non-text children, e.g., [span] elements within [text].
		/// </summary>
		public bool AllowAtomicTextMerge { get; set; }

		public static ElementStrategy CreateForKeyedElement(string keyAttributeName, bool orderIsRelevant)
		{
			var strategy = new ElementStrategy(orderIsRelevant)
				{
					MergePartnerFinder = new FindByKeyAttribute(keyAttributeName)
				};
			return strategy;
		}

		public static ElementStrategy CreateForKeyedElementInList(string keyAttributeName)
		{
			var strategy = new ElementStrategy(true)
				{
					MergePartnerFinder = new FindByKeyAttributeInList(keyAttributeName)
				};
			return strategy;
		}

		/// <summary>
		/// Declare that there can only be a single element with this name in a list of children
		/// </summary>
		public static ElementStrategy CreateSingletonElement()
		{
			var strategy = new ElementStrategy(false)
				{
					MergePartnerFinder = new FindFirstElementWithSameName()
				};
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

	/// <summary>
	/// The number of chldren allowed in some xml element.
	/// </summary>
	public enum NumberOfChildrenAllowed
	{
		/// <summary>
		/// Allow zero or more (no limit) child elements.
		/// </summary>
		ZeroOrMore,
		/// <summary>
		/// Allows no children at all.
		/// </summary>
		Zero,
		/// <summary>
		/// Allows one optional child element.
		/// </summary>
		ZeroOrOne
	}

	public interface IElementDescriber
	{
		string GetHumanDescription(XmlNode element);
	}

	/// <summary>
	/// Interface that allows for pre-merging work to be done on eleemnts, before any other merging work is done.
	/// </summary>
	public interface IPremerger
	{
		/// <summary>
		/// Premerge the given elements.
		/// </summary>
		void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor);
	}

	internal class DefaultPremerger : IPremerger
	{
		public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{ /* Do nothing at all. */ }
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

	/// <summary>
	/// This interface is expected to be an additional interface implemented by a context generator, that is,
	/// a class that implements IGenerateContextDescriptor, or more probably, IGenerateContextDescriptorFromNode.
	/// It is used to get a human-readable HTML representation of the ancestor or one of the leaf contexts for
	/// a conflicting change.
	/// In general the context generated by IGenerateContextDescriptorFromNode and the HtmlContext generated
	/// by this method should be consistent. For example, suppose the node in question is the definition
	/// of a sense of a lexical entry. If HtmlContext shows the whole LexEntry, the ContextDescriptor's
	/// DataLabel should just be the name of the entry. If HtmlContext shows just the content of the definition, the DataLabel
	/// could well be something like Entry myWord Sense 2 Definition.
	/// </summary>
	public interface IGenerateHtmlContext
	{
		/// <summary>
		/// Passed a node at the same level in the hierarchy as GenerateContextDescriptor, this returns a
		/// human-readable HTML representation of the object contents.
		/// </summary>
		/// <param name="mergeElement"></param>
		/// <returns></returns>
		string HtmlContext(XmlNode mergeElement);

		/// <summary>
		/// Return whatever should go INSIDE the "style type='text/css'" element in the header of the HTML document
		/// for describing the specified element. (Typically the implementation ignores the particular element,
		/// it's just provided for compatibility.)
		/// </summary>
		/// <param name="mergeElement"></param>
		/// <returns></returns>
		string HtmlContextStyles(XmlNode mergeElement);
	}

	/// <summary>
	/// If the ContextDescriptorGenerator implements this interface, it will be called instead of
	/// the IGenerateContextDescriptor version.
	/// </summary>
	public interface IGenerateContextDescriptorFromNode
	{
		ContextDescriptor GenerateContextDescriptor(XmlNode mergeElement, string filePath);
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

	/// <summary>
	/// Responses that an implementation of IChildOrderPolicy may give to control how the children are ordered.
	/// </summary>
	public enum ChildOrder
	{
		AskChildren, // Obtain a strategy for each child and let this determine whether order is significant
		Significant, // Order is significant for all children
		NotSignificant // Order is not significant for any children
	}

	/// <summary>
	/// Policy which may be implemented to allow a parent to control the significance of the order of its children.
	/// </summary>
	public interface IChildOrderPolicy
	{
		ChildOrder OrderSignificance(XmlNode parent);
	}

	/// <summary>
	/// The default policy is to ask the children.
	/// </summary>
	public class AskChildrenOrderPolicy : IChildOrderPolicy
	{
		public ChildOrder OrderSignificance(XmlNode parent)
		{
			return ChildOrder.AskChildren;
		}
	}

	/// <summary>
	/// This is another simple policy that can be used to avoid asking each child
	/// </summary>
	public class SignificantOrderPolicy : IChildOrderPolicy
	{
		public ChildOrder OrderSignificance(XmlNode parent)
		{
			return ChildOrder.Significant;
		}
	}

	/// <summary>
	/// This is another simple policy that can be used to avoid asking each child
	/// </summary>
	public class NotSignificantOrderPolicy : IChildOrderPolicy
	{
		public ChildOrder OrderSignificance(XmlNode parent)
		{
			return ChildOrder.NotSignificant;
		}
	}
}