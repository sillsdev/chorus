using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Autofac;
using Chorus.retrieval;
using Chorus.Utilities;

namespace Chorus.merge.xml.generic
{
	/* Merge conflicts are dealt with automatically, and a record of the conflict is added to a conflicts
	 * file.  Later, a history UI can retrieve these records to show the user what happened and allow
	 * them to change the automatic decision.
	 */

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

	public abstract class Conflict :IConflict, IEquatable<Conflict>
	{
		static public string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";

	   // protected string _shortDataDescription;
		protected Guid _guid = Guid.NewGuid();
	   // public string PathToUnitOfConflict { get; set; }
		public string RelativeFilePath { get { return Situation.PathToFileInRepository; } }

		public abstract string GetFullHumanReadableDescription();
		public abstract string ConflictTypeHumanName { get; }
		public MergeSituation Situation{get;set;}
		public string RevisionWhereMergeWasCheckedIn { get;private set;}

		public ContextDescriptor Context { get; set; }

		public Conflict(XmlNode xmlRepresentation)
		{
			Situation =  MergeSituation.FromXml(xmlRepresentation.SafeSelectNodes("MergeSituation")[0]);
			_guid = new Guid(xmlRepresentation.GetOptionalStringAttribute("guid", string.Empty));
			//PathToUnitOfConflict = xmlRepresentation.GetOptionalStringAttribute("pathToUnitOfConflict", string.Empty);
			Context  = ContextDescriptor.CreateFromXml(xmlRepresentation);
		   // _shortDataDescription = xmlRepresentation.GetOptionalStringAttribute("shortElementDescription", string.Empty);
	   }


		protected Conflict(MergeSituation situation)
		{
			Situation = situation;
		}

		public override bool Equals(object obj)
		{
			IConflict otherGuy = obj as IConflict;
			return _guid == otherGuy.Guid;
//            return base.Equals(obj);
		}

		public Guid Guid
		{
			get { return _guid; }
		}

		public abstract string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource);

		public void WriteAsXml(XmlWriter writer)
		{
			writer.WriteStartElement("conflict");
			WriteAttributes(writer);

			writer.WriteString(GetFullHumanReadableDescription());

			Situation.WriteAsXml(writer);

			writer.WriteEndElement();
		}

		protected virtual void WriteAttributes(XmlWriter writer)
		{
			writer.WriteAttributeString("typeGuid", string.Empty, GetTypeGuid());
			writer.WriteAttributeString("class", string.Empty, this.GetType().FullName);
			writer.WriteAttributeString("relativeFilePath", string.Empty, RelativeFilePath);
			//writer.WriteAttributeString("pathToUnitOfConflict", string.Empty, PathToUnitOfConflict);
			writer.WriteAttributeString("type", string.Empty, ConflictTypeHumanName);
			writer.WriteAttributeString("guid", string.Empty, Guid.ToString());
			writer.WriteAttributeString("date", string.Empty, DateTime.UtcNow.ToString(TimeFormatNoTimeZone));
		  //  writer.WriteAttributeString("shortDataDescription", _shortDataDescription);

			if (Context != null)
			{
				Context.WriteAttributes(writer);
			}
		}

		private string GetTypeGuid()
		{
			return GetTypeGuid(GetType());
		}

		public static string GetTypeGuid(Type t)
		{
			var attribute = t.GetCustomAttributes(true).FirstOrDefault(
					   a => a.GetType() == typeof(TypeGuidAttribute)) as TypeGuidAttribute;

			Guard.AgainstNull(attribute,
				  "The Conflict type " + t.ToString() + " needs a guid attribute");

			return attribute.GuidString;
		}

		public string WinnerId
		{
			get
			{
				return (this.Situation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.TheyWin)
						   ?
							   Situation.UserYId
						   : Situation.UserXId;
			}
		}

		public static IConflict CreateFromXml(XmlNode conflictNode)
		{
			try
			{

			var builder = new Autofac.Builder.ContainerBuilder();

			Register<RemovedVsEditedElementConflict>(builder);
			Register<AmbiguousInsertConflict>(builder);
			Register<AmbiguousInsertReorderConflict>(builder);
			Register<BothEdittedAttributeConflict>(builder);
			Register<BothEdittedTextConflict>(builder);
			Register<BothReorderedElementConflict>(builder);
			Register<RemovedVsEditedElementConflict>(builder);
			Register<RemovedVsEditedAttributeConflict>(builder);
			Register<RemovedVsEdittedTextConflict>(builder);

			var container = builder.Build();

			var typeGuid = conflictNode.GetStringAttribute("typeGuid");
			return container.Resolve<IConflict>(typeGuid, new Parameter[]{new TypedParameter(typeof(XmlNode),conflictNode)});
			}
			catch (Exception)
			{
				return new UnreadableConflict(conflictNode);
			}
		}


		private static void Register<T>(Autofac.Builder.ContainerBuilder builder)
		{
			builder.Register<T>().Named(GetTypeGuid(typeof(T)));
		}

		public bool Equals(Conflict other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other._guid.Equals(_guid);
		}

		public override int GetHashCode()
		{
		   unchecked
			{
				return _guid.GetHashCode();
			}
		}
	}

	public class UnreadableConflict : IConflict
	{
		public UnreadableConflict(XmlNode node)
		{
		}

		public string PathToUnitOfConflict
		{
			get { return string.Empty; }
			set { ; }
		}

		public string RelativeFilePath
		{
			get { return string.Empty; }
		}

		public ContextDescriptor Context
		{
			get { return new ContextDescriptor("??",string.Empty); }
			set { ; }
		}

		public string GetFullHumanReadableDescription()
		{
			return "Unreadable Conflict";
		}

		public string ConflictTypeHumanName
		{
			get { return "Unreadable Conflict"; }
		}

		public string WinnerId
		{
			get {   return string.Empty;}
		}

		public Guid Guid
		{
			get { throw new NotImplementedException(); }
		}

		public MergeSituation Situation
		{
			get { return new NullMergeSituation(); }
			set { }
		}

		public string RevisionWhereMergeWasCheckedIn
		{
			get {
				return string.Empty;}
		}

		public string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new NotImplementedException();
		}

		public void WriteAsXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}
	}

	public abstract class AttributeConflict : Conflict, IConflict
	{
		protected readonly string _attributeName;
		protected readonly string _ourValue;
		protected readonly string _theirValue;
		protected readonly string _ancestorValue;

		public AttributeConflict(string attributeName, string ourValue, string theirValue, string ancestorValue,MergeSituation mergeSituation)
			:base(mergeSituation)
		{
			_attributeName = attributeName;
			_ourValue = ourValue;
			_theirValue = theirValue;
			_ancestorValue = ancestorValue;
		}

		public AttributeConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{
			_attributeName = xmlRepresentation.GetOptionalStringAttribute("attributeName", "unknown");
			_ourValue = xmlRepresentation.GetOptionalStringAttribute("ourValue", string.Empty);
			_theirValue = xmlRepresentation.GetOptionalStringAttribute("theirValue", string.Empty);
			_ancestorValue = xmlRepresentation.GetOptionalStringAttribute("ancestorValue", string.Empty);
		}

		protected override void WriteAttributes(XmlWriter writer)
		{
			base.WriteAttributes(writer);
			writer.WriteAttributeString("attributeName", _attributeName);
			writer.WriteAttributeString("ourValue", _ourValue);
			writer.WriteAttributeString("theirValue", _theirValue);
			writer.WriteAttributeString("ancestorValue", _ancestorValue);
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
				return string.Format("When they last synchronized, the value was {0}. Since then, one changed it to {1}, while the other changed it to {2}.",
									 ancestor, ours, theirs);
			}
		}



		public override string GetFullHumanReadableDescription()
		{
			return string.Format("{0} ({1}): {2}", ConflictTypeHumanName, AttributeDescription, WhatHappened);
		}
		public virtual string GetXmlOfConflict()
		{
			return string.Format("<conflict type='{0}'/>", this.GetType().Name);
		}


		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			string revision=null;
		   // string elementId = null;
			switch (mergeSource)
			{
				case ThreeWayMergeSources.Source.Ancestor:
					revision =fileRetriever.GetCommonAncestorOfRevisions(this.Situation.UserXRevision, Situation.UserYRevision);
					break;
				case ThreeWayMergeSources.Source.UserX:
					revision = Situation.UserXRevision;
				 //   elementId = _userXElementId;
					break;
				case ThreeWayMergeSources.Source.UserY:
					revision = Situation.UserYRevision;
				 //    elementId = _userYElementId;
				   break;

			}
			using(var f = TempFile.TrackExisting(fileRetriever.RetrieveHistoricalVersionOfFile(Situation.PathToFileInRepository, revision)))
			{
				var doc = new XmlDocument();
				doc.Load(f.Path);
				var element = doc.SelectSingleNode(Context.PathToUserUnderstandableElement);
				if(element == null)
				{
					throw new ApplicationException("Could not find the element specified by the context, " + Context.PathToUserUnderstandableElement);
				}
				return element.OuterXml;
			}
		}
	}

	[TypeGuid("B11ABA8C-DFB9-4E37-AF35-8AFDB86F00B7")]
	sealed public class RemovedVsEditedAttributeConflict : AttributeConflict
	{
		public RemovedVsEditedAttributeConflict(string attributeName, string ourValue, string theirValue, string ancestorValue,MergeSituation mergeSituation)
			: base(attributeName, ourValue, theirValue, ancestorValue, mergeSituation)
		{
		}
		public override string ConflictTypeHumanName
		{
			get { return string.Format("Removed Vs Edited Attribute Conflict"); }
		}
	}

	[TypeGuid("5BBDF4F6-953A-4F79-BDCD-0B1F733DA4AB")]
	sealed public class BothEdittedAttributeConflict : AttributeConflict
	{
		public BothEdittedAttributeConflict(string attributeName, string ourValue, string theirValue, string ancestorValue,MergeSituation mergeSituation)
			: base(attributeName, ourValue, theirValue, ancestorValue, mergeSituation)
		{
		}

		public override string ConflictTypeHumanName
		{
			get { return string.Format("Both Edited Attribute Conflict"); }
		}
	}

	[TypeGuid("0507DE36-13A3-449D-8302-48F5213BD92E")]
	sealed public class BothEdittedTextConflict : AttributeConflict
	{
		public BothEdittedTextConflict(XmlNode ours, XmlNode theirs, XmlNode ancestor,MergeSituation mergeSituation)
			: base("text", ours.InnerText, theirs.InnerText,
				   ancestor == null ? string.Empty : ancestor.InnerText,
				   mergeSituation)
		{
		}

		public BothEdittedTextConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{

		}

		public override string ConflictTypeHumanName
		{
			get { return string.Format("Both Edited Text Field Conflict"); }
		}
	}

	[TypeGuid("E1CCC59B-46E5-4D24-A1B1-5B621A0F8870")]
	sealed public class RemovedVsEdittedTextConflict : AttributeConflict
	{
		public RemovedVsEdittedTextConflict(XmlNode ours, XmlNode theirs, XmlNode ancestor,MergeSituation mergeSituation)
			: base("text", ours == null ? string.Empty : ours.InnerText,
				   theirs == null ? string.Empty : theirs.InnerText,
				   ancestor.InnerText,
				   mergeSituation)
		{
		}


		public RemovedVsEdittedTextConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{

		}

		public override string ConflictTypeHumanName
		{
			get { return string.Format("Both Edited Text Field Conflict"); }
		}
	}

	[TypeGuid("DC5D3236-9372-4965-9E34-386182675A5C")]
	public abstract class ElementConflict : Conflict, IConflict
	{
		protected readonly string _elementName;
//        protected readonly XmlNode _ourElement;
//        protected readonly XmlNode _theirElement;
//        protected readonly XmlNode _ancestorElement;

		public ElementConflict(string elementName, XmlNode ourElement, XmlNode theirElement, XmlNode ancestorElement,
							  MergeSituation mergeSituation, IElementDescriber elementDescriber)
			: base(mergeSituation)
		{
			_elementName = elementName;
//            _ourElement = ourElement;
//            _theirElement = theirElement;
//            _ancestorElement = ancestorElement;

			//nb: we need to make use of the describer now, because it won't make it through the xml serialization/deserialization
			//_shortDataDescription = elementDescriber.GetHumanDescription(ourElement);
		}

		public ElementConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{
		}

		public override string GetFullHumanReadableDescription()
		{
			//enhance: this is a bit of a hack to pick some element that isn't null
//            XmlNode element = _ourElement == null ? _ancestorElement : _ourElement;
//            if(element == null)
//            {
//                element = _theirElement;
//            }

			return string.Format("{0} ({1}): {2}", ConflictTypeHumanName, Context.DataLabel, WhatHappened);
		}


		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			//fileRetriever.RetrieveHistoricalVersionOfFile(_file, userSources[]);
			return null;
		}


		public abstract string WhatHappened
		{
			get;
		}

		protected override void WriteAttributes(XmlWriter writer)
		{
			base.WriteAttributes(writer);
		}
	}

	[TypeGuid("56F9C347-C4FA-48F4-8028-729F3CFF48EF")]
	internal class RemovedVsEditedElementConflict : ElementConflict
	{
		public RemovedVsEditedElementConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement,MergeSituation mergeSituation, IElementDescriber elementDescriber)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, elementDescriber)
		{
		}


		public RemovedVsEditedElementConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
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
//                if (_theirElement == null)
//                {
//                    return "Since we last synchronized, they deleted this element, while you or the program you were using edited it.";
//                }
//                else
//                {
//                    return "Since we last synchronized, you deleted this element, while they or the program they were using edited it.";
//                }
				return "One user deleted this element, while another edited it.";
			}
		}
	}

	[TypeGuid("14262878-270A-4E27-BA5F-7D232B979D6B")]
	internal class BothReorderedElementConflict : ElementConflict
	{
		public BothReorderedElementConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement,MergeSituation mergeSituation, IElementDescriber elementDescriber)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, elementDescriber)
		{
		}

		public BothReorderedElementConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
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

	[TypeGuid("B77C0D86-2368-4380-B2E4-7943F3E7553C")]
	internal class AmbiguousInsertConflict : ElementConflict
	{
		public AmbiguousInsertConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement,MergeSituation mergeSituation, IElementDescriber elementDescriber)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, elementDescriber)
		{
		}

		public AmbiguousInsertConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
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
		public override string ToString()
		{
			return GetType().ToString() + ":" + _elementName+" (or lower?)";
		}
	}

	[TypeGuid("A5CE68F5-ED0D-4732-BAA8-A04A99ED35B3")]
	internal class AmbiguousInsertReorderConflict : ElementConflict
	{
		public AmbiguousInsertReorderConflict(string elementName, XmlNode ourElement, XmlNode theirElement,
			XmlNode ancestorElement,MergeSituation mergeSituation, IElementDescriber elementDescriber)
			: base(elementName, ourElement, theirElement, ancestorElement, mergeSituation, elementDescriber)
		{
		}

		public AmbiguousInsertReorderConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
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