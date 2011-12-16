using System;
using System.Linq;
using System.Text;
using System.Xml;
using Autofac;
using Chorus.Utilities.code;
using Chorus.VcsDrivers;
using Palaso.IO;

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
		public  const string ConflictAnnotationClassName="mergeConflict";
		// public string PathToUnitOfConflict { get; set; }
		public string RelativeFilePath { get { return Situation.PathToFileInRepository; } }

		public abstract string GetFullHumanReadableDescription();
		public abstract string Description { get; }
		public MergeSituation Situation{get;set;}
		public string RevisionWhereMergeWasCheckedIn { get;private set;}

		public ContextDescriptor Context { get; set; }
		protected  string _whoWon;

		protected Conflict(XmlNode xmlRepresentation)
		{
			Situation =  MergeSituation.FromXml(xmlRepresentation.SafeSelectNodes("MergeSituation")[0]);
			_guid = new Guid(xmlRepresentation.GetOptionalStringAttribute("guid", string.Empty));
			//PathToUnitOfConflict = xmlRepresentation.GetOptionalStringAttribute("pathToUnitOfConflict", string.Empty);
			Context  = ContextDescriptor.CreateFromXml(xmlRepresentation);
		   // _shortDataDescription = xmlRepresentation.GetOptionalStringAttribute("shortElementDescription", string.Empty);
			_whoWon = xmlRepresentation.GetOptionalStringAttribute("whoWon", string.Empty);
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

		protected string GetWhoWonText()
		{
			return string.Format("The automated merger kept the change made by {0}.", _whoWon);
		}
		public void WriteAsChorusNotesAnnotation(XmlWriter writer)
		{
			writer.WriteStartElement("annotation");
			writer.WriteAttributeString("class", string.Empty, Conflict.ConflictAnnotationClassName);
			Guard.AgainstNull(Context,"Context");
			Guard.AgainstNull(Context.PathToUserUnderstandableElement, "Context.PathToUserUnderstandableElement");
			writer.WriteAttributeString("ref", Context.PathToUserUnderstandableElement);
			writer.WriteAttributeString("guid", Guid.NewGuid().ToString()); //nb: this is the guid of the enclosing annotation, not the conflict;

			writer.WriteStartElement("message");
			writer.WriteAttributeString("author", string.Empty, "merger");
			writer.WriteAttributeString("status", string.Empty, "open");
			writer.WriteAttributeString("guid", string.Empty, Guid.ToString());//nb: ok to have the same guid with the conflict, as they are in 1-1 relation and eventually we'll remove the one on conflict
			writer.WriteAttributeString("date", string.Empty, DateTime.UtcNow.ToString(TimeFormatNoTimeZone));
			writer.WriteString(GetFullHumanReadableDescription());

			//we embedd this xml inside the CDATA section so that it pass a more generic schema without
			//resorting to the complexities of namespaces
			var b = new StringBuilder();
			using (var embeddedWriter = XmlWriter.Create(b, Palaso.Xml.CanonicalXmlSettings.CreateXmlWriterSettings(ConformanceLevel.Fragment)))
			{
				embeddedWriter.WriteStartElement("conflict");
				WriteAttributes(embeddedWriter);

				Situation.WriteAsXml(embeddedWriter);

				embeddedWriter.WriteEndElement();
			}

			writer.WriteCData(b.ToString());


			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		protected virtual void WriteAttributes(XmlWriter writer)
		{
			writer.WriteAttributeString("typeGuid", string.Empty, GetTypeGuid());
			writer.WriteAttributeString("class", string.Empty, this.GetType().FullName);
			writer.WriteAttributeString("relativeFilePath", string.Empty, RelativeFilePath);
			//writer.WriteAttributeString("pathToUnitOfConflict", string.Empty, PathToUnitOfConflict);
			writer.WriteAttributeString("type", string.Empty, Description);
			writer.WriteAttributeString("guid", string.Empty, Guid.ToString());
			writer.WriteAttributeString("date", string.Empty, DateTime.UtcNow.ToString(TimeFormatNoTimeZone));
		  //  writer.WriteAttributeString("shortDataDescription", _shortDataDescription);
			writer.WriteAttributeString("whoWon", _whoWon);

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
							   Situation.BetaUserId
						   : Situation.AlphaUserId;
			}
		}

		public static IConflict CreateFromConflictElement(XmlNode conflictNode)
		{
			try
			{

			var builder = new Autofac.Builder.ContainerBuilder();

			Register<RemovedVsEditedElementConflict>(builder);
			Register<AmbiguousInsertConflict>(builder);
			Register<AmbiguousInsertReorderConflict>(builder);
			Register<BothEditedAttributeConflict>(builder);
			Register<BothEditedTextConflict>(builder);
			Register<BothReorderedElementConflict>(builder);
			Register<RemovedVsEditedElementConflict>(builder);
			Register<RemovedVsEditedAttributeConflict>(builder);
			Register<RemovedVsEditedTextConflict>(builder);
			Register<BothEditedDifferentPartsOfDependentPiecesOfDataWarning>(builder);
			Register<UnmergableFileTypeConflict>(builder);

			var container = builder.Build();

			var typeGuid = conflictNode.GetStringAttribute("typeGuid");
			return container.Resolve<IConflict>(typeGuid, new Parameter[]{new TypedParameter(typeof(XmlNode),conflictNode)});
			}
			catch (Exception error)
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

		public static IConflict CreateFromChorusNotesAnnotation(string annotationXml)
		{
			var dom = new XmlDocument();
			dom.LoadXml(annotationXml);
			var msg = dom.SelectSingleNode("//message");
			return CreateFromConflictElement(GetFirstCDataChild(msg));
		}
		private static XmlNode GetFirstCDataChild(XmlNode messageNode)
		{
			foreach (XmlNode node in messageNode)
			{
				if (node.NodeType == XmlNodeType.CDATA)
				{
					XmlDocument x = new XmlDocument();
					x.LoadXml(node.InnerText);
					return x.SelectSingleNode("conflict");
				}
			}
			return null;
		}
	}

	/// <summary>
	/// this exists for presentation only, in the case where we couldn't deserialize the conflict record
	/// </summary>
	public class UnreadableConflict : IConflict
	{
		public XmlNode ConflictNode { get; private set; }

		public UnreadableConflict(XmlNode node)
		{
			ConflictNode = node;
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

		public string Description
		{
			get { return "Unreadable Conflict"; }
		}

		public string WinnerId
		{
			get {   return string.Empty;}
		}

		public Guid Guid
		{
			get { return new Guid(ConflictNode.GetOptionalStringAttribute("guid", string.Empty)); }
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
			throw new NotImplementedException("Unable to retrieve from source control.");
		}

		public void WriteAsChorusNotesAnnotation(XmlWriter writer)
		{
			throw new NotImplementedException("UnreadableConflict is not intended to be ever saved");
		}
	}

	public abstract class AttributeConflict : Conflict
	{
		protected readonly string _attributeName;
		protected readonly string _alphaValue;
		protected readonly string _betaValue;
		protected readonly string _ancestorValue;

		protected AttributeConflict(string attributeName, string alphaValue, string betaValue, string ancestorValue, MergeSituation mergeSituation, string whoWon)
			:base(mergeSituation)
		{
			_whoWon = whoWon;
			_attributeName = attributeName;
			_alphaValue = alphaValue;
			_betaValue = betaValue;
			_ancestorValue = ancestorValue;
		}

		protected AttributeConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{
			_attributeName = xmlRepresentation.GetOptionalStringAttribute("attributeName", "unknown");
			_alphaValue = xmlRepresentation.GetOptionalStringAttribute("alphaValue", string.Empty);
			_betaValue = xmlRepresentation.GetOptionalStringAttribute("betaValue", string.Empty);
			_ancestorValue = xmlRepresentation.GetOptionalStringAttribute("ancestorValue", string.Empty);
		}

		protected override void WriteAttributes(XmlWriter writer)
		{
			base.WriteAttributes(writer);
			writer.WriteAttributeString("attributeName", _attributeName);
			writer.WriteAttributeString("alphaValue", _alphaValue);
			writer.WriteAttributeString("betaValue", _betaValue);
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
				string ancestor = string.IsNullOrEmpty(_ancestorValue) ? "empty" : "'"+_ancestorValue+"'";
				string alpha = string.IsNullOrEmpty(_alphaValue) ? "(empty)" : "'"+_alphaValue+"'";
				string beta = string.IsNullOrEmpty(_betaValue) ? "(empty)" : "'" + _betaValue+"'";
				return string.Format("{1} changed {0} to {2}, while {3} changed it to {4}. ",
									 ancestor, Situation.AlphaUserId, alpha, Situation.BetaUserId, beta)+GetWhoWonText();
			}
		}



		public override string GetFullHumanReadableDescription()
		{
			return string.Format("{0} ({1}): {2}", Description, AttributeDescription, WhatHappened);
		}
		public virtual string GetXmlOfConflict()
		{
			return string.Format("<annotation type='{0}'/>", this.GetType().Name);
		}


		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			string revision=null;
		   // string elementId = null;
			switch (mergeSource)
			{
				case ThreeWayMergeSources.Source.Ancestor:
					revision =fileRetriever.GetCommonAncestorOfRevisions(this.Situation.AlphaUserRevision, Situation.BetaUserRevision);
					break;
				case ThreeWayMergeSources.Source.UserX:
					revision = Situation.AlphaUserRevision;
				 //   elementId = _userXElementId;
					break;
				case ThreeWayMergeSources.Source.UserY:
					revision = Situation.BetaUserRevision;
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
		public RemovedVsEditedAttributeConflict(string attributeName, string alphaValue, string betaValue, string ancestorValue, MergeSituation mergeSituation, string whoWon)
			: base(attributeName, alphaValue, betaValue, ancestorValue, mergeSituation, whoWon)
		{
		}
		public override string Description
		{
			get { return string.Format("Removed Vs Edited Attribute Conflict"); }
		}
	}

	[TypeGuid("5BBDF4F6-953A-4F79-BDCD-0B1F733DA4AB")]
	sealed public class BothEditedAttributeConflict : AttributeConflict
	{
		public BothEditedAttributeConflict(string attributeName, string alphaValue, string betaValue, string ancestorValue, MergeSituation mergeSituation, string whoWon)
			: base(attributeName, alphaValue, betaValue, ancestorValue, mergeSituation, whoWon)
		{
		}

		public override string Description
		{
			get { return string.Format("Both Edited Attribute Conflict"); }
		}
	}

	[TypeGuid("0507DE36-13A3-449D-8302-48F5213BD92E")]
	sealed public class BothEditedTextConflict : AttributeConflict
	{
		public BothEditedTextConflict(XmlNode alphaNode, XmlNode betaNode, XmlNode ancestor, MergeSituation mergeSituation, string whoWon)
			: base("text", alphaNode.InnerText, betaNode.InnerText,
				   ancestor == null ? string.Empty : ancestor.InnerText,
				   mergeSituation, whoWon)
		{
		}

		public BothEditedTextConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{

		}

		public override string Description
		{
			get { return string.Format("Both Edited Text Field Conflict"); }
		}
	}

	[TypeGuid("E1CCC59B-46E5-4D24-A1B1-5B621A0F8870")]
	sealed public class RemovedVsEditedTextConflict : AttributeConflict
	{
		public RemovedVsEditedTextConflict(XmlNode alphaNode, XmlNode betaNode, XmlNode ancestor,MergeSituation mergeSituation, string whoWon)
			: base("text", alphaNode == null ? string.Empty : alphaNode.InnerText,
				   betaNode == null ? string.Empty : betaNode.InnerText,
				   ancestor.InnerText,
				   mergeSituation, whoWon)
		{
		}


		public RemovedVsEditedTextConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{

		}

		public override string Description
		{
			get { return string.Format("Both Edited Text Field Conflict"); }
		}
	}

	[TypeGuid("DC5D3236-9372-4965-9E34-386182675A5C")]
	public abstract class ElementConflict : Conflict
	{
		protected readonly string _elementName;


		protected ElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(mergeSituation)
		{
			_elementName = elementName;
			_whoWon = whoWon;
		}

		protected ElementConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format("{0} ({1}): {2}", Description, Context.DataLabel, WhatHappened);
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

	}

	[TypeGuid("56F9C347-C4FA-48F4-8028-729F3CFF48EF")]
	public class RemovedVsEditedElementConflict : ElementConflict
	{
		public RemovedVsEditedElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}


		public RemovedVsEditedElementConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{

		}
		public override string Description
		{
			get { return "Removed Vs Edited Element Conflict"; }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format("{0} deleted this element, while {1} edited it. ", Situation.AlphaUserId, Situation.BetaUserId, _whoWon)+GetWhoWonText();
			}
		}

	}

	[TypeGuid("3d9ba4ac-4a25-11df-9879-0800200c9a66")]
	public class EditedVsRemovedElementConflict : ElementConflict
	{
		public EditedVsRemovedElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public EditedVsRemovedElementConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{

		}
		public override string Description
		{
			get { return "Edited Vs Removed Element Conflict"; }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format("{0} edited this element, while {1} deleted it. ", Situation.AlphaUserId, Situation.BetaUserId) + GetWhoWonText();
			}
		}

	}

	[TypeGuid("14262878-270A-4E27-BA5F-7D232B979D6B")]
	public class BothReorderedElementConflict : ElementConflict
	{
		public BothReorderedElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement,MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public BothReorderedElementConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{

		}
		public override string Description
		{
			get { return "Both Reordered Conflict"; }
		}

		public override string WhatHappened
		{
			get { return string.Format("{0} and {1} both re-ordered the children of this element in different ways.",
				Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	[TypeGuid("B77C0D86-2368-4380-B2E4-7943F3E7553C")]
	public class AmbiguousInsertConflict : ElementConflict
	{
		public AmbiguousInsertConflict(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public AmbiguousInsertConflict(XmlNode xmlRepresentation):base(xmlRepresentation)
		{

		}
		public override string Description
		{
			get { return "Ambiguous Insert Warning"; }
		}

		public override string WhatHappened
		{
			get { return string.Format("{0} and {1} both inserted material in this element in the same place. The automated merger cannot be sure of the correct order for the inserted material.",
				Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
		public override string ToString()
		{
			return GetType().ToString() + ":" + _elementName+" (or lower?)";
		}
	}

	[TypeGuid("A5CE68F5-ED0D-4732-BAA8-A04A99ED35B3")]
	public class AmbiguousInsertReorderConflict : ElementConflict
	{
		public AmbiguousInsertReorderConflict(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public AmbiguousInsertReorderConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{

		}


		public override string Description
		{
			get { return "Ambiguous Insert Reorder Warning"; }
		}

		public override string WhatHappened
		{
			get { return string.Format("{0} inserted material in this element, but {1} re-ordered things. The automated merger cannot be sure of the correct position for the inserted material.",
				Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	/// <summary>
	/// Used when, say, one guy adds a translation of an the example sentence,
	/// but meanwhile the other guy changed the example sentence, so the translation is
	/// suspect.  This could be a "warning", if we had such a thing.
	/// </summary>
	[TypeGuid("71636317-A94F-4814-8665-1D0F83DF388F")]
	public class BothEditedDifferentPartsOfDependentPiecesOfDataWarning : ElementConflict
	{
		public BothEditedDifferentPartsOfDependentPiecesOfDataWarning(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public BothEditedDifferentPartsOfDependentPiecesOfDataWarning(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{

		}


		public override string Description
		{
			get { return "Both Edited Different Parts Of Dependent Pieces Of Data Warning"; }
		}

		public override string WhatHappened
		{
			get {
				return
					string.Format("{0} edited one part of this element, while {1} edited another part. Since these two pieces of data are thought to be dependent on each other, someone needs to verify that the resulting merge is ok.",
					Situation.AlphaUserId, Situation.BetaUserId); }
		}
	}

	/// <summary>
	/// Used when, say, one guy adds a translation of an the example sentence,
	/// but meanwhile the other guy changed the example sentence, so the translation is
	/// suspect.  This could be a "warning", if we had such a thing.
	/// </summary>
	[TypeGuid("3d9ba4ae-4a25-11df-9879-0800200c9a66")]
	public class BothEditedTheSameElement : ElementConflict
	{
		public BothEditedTheSameElement(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public BothEditedTheSameElement(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{

		}


		public override string Description
		{
			get { return "Both Edited the Same Element"; }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format("{0} and {1} edited this element.", Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}
}