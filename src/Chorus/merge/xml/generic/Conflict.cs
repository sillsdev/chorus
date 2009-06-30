using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Chorus.retrieval;
using Chorus.Utilities;

namespace Chorus.merge.xml.generic
{
//
//    public class ConflictFactory
//    {
//        private readonly MergeSituation _mergeSituation;
//
//        public ConflictFactory(MergeSituation mergeSituation)
//        {
//            _mergeSituation = mergeSituation;
//        }
//
//        public T Create<T>()
//            where T:IConflict, new()
//        {
//            T conflict = new T();
//            conflict.MergeSituation = _mergeSituation;
//            return conflict;
//        }
//    }


	public abstract class AttributeConflict : IConflict
	{
		protected readonly string _attributeName;
		protected readonly string _ourValue;
		protected readonly string _theirValue;
		protected readonly string _ancestorValue;
		protected readonly MergeSituation _mergeSituation;
		private Guid _guid = Guid.NewGuid();
		public string XPathOrOtherDescriptorOfConflictingElement { get; set; }

		public AttributeConflict(string attributeName, string ourValue, string theirValue, string ancestorValue, MergeSituation mergeSituation)
		{
			_attributeName = attributeName;
			_ourValue = ourValue;
			_theirValue = theirValue;
			_ancestorValue = ancestorValue;
			_mergeSituation = mergeSituation;
		}

		public string AttributeDescription
		{
			get
			{
				return string.Format("{0}", _attributeName);
			}
		}

		public string WhatHappened
		{
			get
			{
				string ancestor = string.IsNullOrEmpty(_ancestorValue) ? "<didn't exist>" : _ancestorValue;
				string ours = string.IsNullOrEmpty(_ourValue) ? "<removed>" : _ourValue;
				string theirs = string.IsNullOrEmpty(_theirValue) ? "<removed>" : _theirValue;
				return string.Format("When we last synchronized, the value was {0}. Since then, we changed it to {1}, while they changed it to {2}.",
									 ancestor, ours, theirs);
			}
		}



		public virtual string GetFullHumanReadableDescription()
		{
			return string.Format("{0} ({1}): {2}", ConflictTypeHumanName, AttributeDescription, WhatHappened);
		}
		public virtual string GetXmlOfConflict()
		{
			return string.Format("<conflict type='{0}'/>", this.GetType().Name);
		}
		public abstract string ConflictTypeHumanName
		{
			get;
		}

		public Guid Guid
		{
			get { return _guid; }
		}

		public string GetRawDataFromConflictVersion(IRetrieveFile fileRetriever, ThreeWayMergeSources.Source mergeSource, string recordLevel)
		{
			string revision=null;
		   // string elementId = null;
			switch (mergeSource)
			{
				case ThreeWayMergeSources.Source.Ancestor:
					throw new ApplicationException("Ancestor retrieval not implemented yet.");
				case ThreeWayMergeSources.Source.UserX:
					revision = _mergeSituation.UserXRevision;
				 //   elementId = _userXElementId;
					break;
				case ThreeWayMergeSources.Source.UserY:
					revision = _mergeSituation.UserYRevision;
				 //    elementId = _userYElementId;
				   break;

			}
			using(var f = TempFile.TrackExisting(fileRetriever.RetrieveHistoricalVersionOfFile(_mergeSituation.PathToFileInRepository, revision)))
			{
				var doc = new XmlDocument();
				doc.Load(f.Path);
				var element = doc.SelectSingleNode(XPathOrOtherDescriptorOfConflictingElement);
				if(element == null)
				{
					throw new ApplicationException("Could not find the element specified by the context, " + XPathOrOtherDescriptorOfConflictingElement);
				}
				return element.OuterXml;
			}
		}
	}

	public class RemovedVsEditedAttributeConflict : AttributeConflict
	{
		public RemovedVsEditedAttributeConflict(string attributeName, string ourValue, string theirValue, string ancestorValue, MergeSituation mergeSituation)
			: base(attributeName, ourValue, theirValue, ancestorValue, mergeSituation)
		{
		}
		public override string ConflictTypeHumanName
		{
			get { return string.Format("Removed Vs Edited Attribute Conflict"); }
		}
	}

	internal class BothEdittedAttributeConflict : AttributeConflict
	{
		public BothEdittedAttributeConflict(string attributeName, string ourValue, string theirValue, string ancestorValue, MergeSituation mergeSituation)
			: base(attributeName, ourValue, theirValue, ancestorValue, mergeSituation)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return string.Format("Both Edited Attribute Conflict"); }
		}
	}

	internal class BothEdittedTextConflict : AttributeConflict
	{
		public BothEdittedTextConflict(XmlNode ours, XmlNode theirs, XmlNode ancestor, MergeSituation mergeSituation)
			: base("text", ours.InnerText, theirs.InnerText,
				   ancestor == null ? string.Empty : ancestor.InnerText,
				   mergeSituation)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return string.Format("Both Edited Text Field Conflict"); }
		}
	}

	public class RemovedVsEdittedTextConflict : AttributeConflict
	{
		public RemovedVsEdittedTextConflict(XmlNode ours, XmlNode theirs, XmlNode ancestor, MergeSituation mergeSituation)
			: base("text", ours == null ? string.Empty : ours.InnerText,
				   theirs == null ? string.Empty : theirs.InnerText,
				   ancestor.InnerText,
				   mergeSituation)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return string.Format("Both Edited Text Field Conflict"); }
		}
	}

	public abstract class ElementConflict : IConflict
	{
		protected readonly string _elementName;
		protected readonly XmlNode _ourElement;
		protected readonly XmlNode _theirElement;
		protected readonly XmlNode _ancestorElement;
		protected readonly  MergeSituation _mergeSituation;
		private Guid _guid = Guid.NewGuid();
		private MergeStrategies _mergeStrategies;
		public string XPathOrOtherDescriptorOfConflictingElement { get; set; }

		public ElementConflict(string elementName, XmlNode ourElement, XmlNode theirElement, XmlNode ancestorElement,
							   MergeSituation mergeSituation, MergeStrategies mergeStrategies)
		{
			_elementName = elementName;
			_ourElement = ourElement;
			_theirElement = theirElement;
			_ancestorElement = ancestorElement;
			_mergeStrategies = mergeStrategies;
			_mergeSituation = mergeSituation;
		}


		public void SetContextDescriptor(string contextDescriptor)
		{
			XPathOrOtherDescriptorOfConflictingElement = contextDescriptor;
		}

		public virtual string GetFullHumanReadableDescription()
		{
			//enhance: this is a bit of a hack to pick some element that isn't null
			XmlNode element = _ourElement == null ? _ancestorElement : _ourElement;
			if(element == null)
			{
				element = _theirElement;
			}

			return string.Format("{0} ({1}): {2}", ConflictTypeHumanName, _mergeStrategies.GetElementStrategy(element).GetHumanDescription(element), WhatHappened);
		}



		public Guid Guid
		{
			get { return _guid; }
		}

		public string GetRawDataFromConflictVersion(IRetrieveFile fileRetriever, ThreeWayMergeSources.Source mergeSource, string recordLevel)
		{
			//fileRetriever.RetrieveHistoricalVersionOfFile(_file, userSources[]);
			return null;
		}


		public abstract string ConflictTypeHumanName
		{
			get;
		}
		public abstract string WhatHappened
		{
			get;
		}
	}

	internal class RemovedVsEditedElementConflict : ElementConflict
	{
		public RemovedVsEditedElementConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement, MergeSituation mergeSituation, MergeStrategies mergeStrategies)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, mergeStrategies)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return "Removed Vs Edited Element Conflict"; }
		}

		public override string WhatHappened
		{
			get
			{
				if (_theirElement == null)
				{
					return "Since we last synchronized, they deleted this element, while you or the program you were using edited it.";
				}
				else
				{
					return "Since we last synchronized, you deleted this element, while they or the program they were using edited it.";
				}
			}
		}
	}

	internal class BothReorderedElementConflict : ElementConflict
	{
		public BothReorderedElementConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement, MergeSituation mergeSituation, MergeStrategies mergeStrategies)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, mergeStrategies)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return "Both Reordered Conflict"; }
		}

		public override string WhatHappened
		{
			get { return "Since we last synchronized, you and they both reordered the children of this element in different ways"; }
		}
	}

	internal class AmbiguousInsertConflict : ElementConflict
	{
		public AmbiguousInsertConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement, MergeSituation mergeSituation, MergeStrategies mergeStrategies)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, mergeStrategies)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return "Ambiguous Insert Warning"; }
		}

		public override string WhatHappened
		{
			get { return "Since we last synchronized, you and they both inserted material in this element in the same place. We cannot be sure of the correct order for the inserted material."; }
		}
	}
	internal class AmbiguousInsertReorderConflict : ElementConflict
	{
		public AmbiguousInsertReorderConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement, MergeSituation mergeSituation, MergeStrategies mergeStrategies)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, mergeStrategies)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return "Ambiguous Insert Reorder Warning"; }
		}

		public override string WhatHappened
		{
			get { return "Since we last synchronized, someone inserted material in this element, but the other user re-ordered things. We cannot be sure of the correct position for the inserted material."; }
		}
	}
}