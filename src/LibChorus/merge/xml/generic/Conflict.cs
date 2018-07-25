using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Autofac;
using Chorus.VcsDrivers;
using L10NSharp;
using SIL.IO;
using SIL.Xml;
using SIL.Code;
using SIL.Providers;

namespace Chorus.merge.xml.generic
{
	/* Merge conflicts are dealt with automatically, and a record of the conflict is added to a conflicts
	 * file.  Later, a history UI can retrieve these records to show the user what happened and allow
	 * them to change the automatic decision.
	 */

	/// <summary>
	/// Base class for Conflicts detected in merging changes made by different users.
	/// NB: Be sure to register any new concrete subclasses in this assembly in the ConflictFactory getter.
	/// Register any Conflict types added by clients using Conflict.RegisterConflictClass().
	/// </summary>
	public abstract class Conflict : IConflict, IEquatable<Conflict>
	{
		[Obsolete("Use TimeFormatWithTimeZone instead, as TimeFormatNoTimeZone produces incorrect results when used with DateTime.Now")]
		public static string TimeFormatNoTimeZone = @"yyyy-MM-ddTHH:mm:ssZ";
		public static string TimeFormatWithTimeZone = @"yyyy-MM-ddTHH:mm:ssK";

		private ContextDescriptor _context = new NullContextDescriptor();

		protected Guid _guid = GuidProvider.Current.NewGuid();
		// The value used for the "class" attribute in Annotation XML created to wrap conflicts other than notifications.
		public const string ConflictAnnotationClassName = @"mergeConflict";
		// The value used for the "class" attribute in Annotation XML created to wrap conflicts that are notifications.
		public const string NotificationAnnotationClassName = @"notification";

		/// <summary>
		/// The value that should be used for the "class" attribute in Annotation XML created to wrap this conflict.
		/// </summary>
		public string AnnotationClassName
		{
			get { return IsNotification ? NotificationAnnotationClassName : ConflictAnnotationClassName; }
		}

		public string RelativeFilePath { get { return Situation.PathToFileInRepository; } }

		public abstract string GetFullHumanReadableDescription();
		public abstract string Description { get; }

		public string HtmlDetails { get; set; }

		public MergeSituation Situation { get; set; }
		public string RevisionWhereMergeWasCheckedIn { get; private set; }

		public ContextDescriptor Context
		{
			get { return _context; }
			set
			{
				_context = value ?? new NullContextDescriptor();
			}
		}

		protected string _whoWon;

		/// <summary>
		/// Notifications are low-priority conflicts.
		/// Typically where both users added something, we aren't quite sure of the order, but no actual data loss
		/// has occurred. Override this in classes which you think it is not too unreasonable for users to ignore.
		/// </summary>
		public virtual bool IsNotification
		{
			get { return false; }
		}

		protected Conflict(XmlNode xmlRepresentation)
		{
			Situation = MergeSituation.FromXml(xmlRepresentation.SafeSelectNodes(@"MergeSituation")[0]);
			_guid = new Guid(xmlRepresentation.GetOptionalStringAttribute(@"guid", string.Empty));
			//PathToUnitOfConflict = xmlRepresentation.GetOptionalStringAttribute("pathToUnitOfConflict", string.Empty);
			Context = ContextDescriptor.CreateFromXml(xmlRepresentation);
			// _shortDataDescription = xmlRepresentation.GetOptionalStringAttribute("shortElementDescription", string.Empty);
			_whoWon = xmlRepresentation.GetOptionalStringAttribute(@"whoWon", string.Empty);
			HtmlDetails = xmlRepresentation.GetOptionalStringAttribute(@"htmlDetails", string.Empty);
		}


		protected Conflict(MergeSituation situation)
		{
			Situation = situation;
		}

		protected Conflict(MergeSituation situation, string whoWon)
		{
			Situation = situation;
		}

		public override bool Equals(object obj)
		{
			IConflict otherGuy = obj as IConflict;
			return _guid == otherGuy.Guid;
		}

		public Guid Guid
		{
			get { return _guid; }
		}

		public abstract string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource);

		protected string GetWhoWonText()
		{
			return string.Format(KeptChangePattern, _whoWon);
		}

		protected static string KeptChangePattern
		{
			get { return LocalizationManager.GetString(@"Conflict.KeptChange", "The merger kept the change made by {0}.", "{0} is a person's ID, typically a name"); }
		}

		public void WriteAsChorusNotesAnnotation(XmlWriter writer)
		{
			Guard.AgainstNull(writer, @"writer");
			Guard.AgainstNull(Context.PathToUserUnderstandableElement, @"Context.PathToUserUnderstandableElement");

			writer.WriteStartElement(@"annotation");
			writer.WriteAttributeString(@"class", string.Empty, AnnotationClassName);
			writer.WriteAttributeString(@"ref", Context.PathToUserUnderstandableElement);
			writer.WriteAttributeString(@"guid", GuidProvider.Current.NewGuid().ToString()); //nb: this is the guid of the enclosing annotation, not the conflict;

			writer.WriteStartElement(@"message");
			writer.WriteAttributeString(@"author", string.Empty, Author);
			writer.WriteAttributeString(@"status", string.Empty, @"open");
			writer.WriteAttributeString(@"guid", string.Empty, Guid.ToString());//nb: ok to have the same guid with the conflict, as they are in 1-1 relation and eventually we'll remove the one on conflict
			writer.WriteAttributeString(@"date", string.Empty, DateTimeProvider.Current.UtcNow.ToString(TimeFormatWithTimeZone));
			writer.WriteString(GetFullHumanReadableDescription());

			//we embed this xml inside the CDATA section so that it pass a more generic schema without
			//resorting to the complexities of namespaces
			var b = new StringBuilder();
			using (var embeddedWriter = XmlWriter.Create(b, SIL.Xml.CanonicalXmlSettings.CreateXmlWriterSettings(ConformanceLevel.Fragment)))
			{
				embeddedWriter.WriteStartElement(@"conflict");
				WriteAttributes(embeddedWriter);

				Situation.WriteAsXml(embeddedWriter);

				embeddedWriter.WriteEndElement();
			}

			writer.WriteCData(b.ToString());


			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		protected virtual string Author
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.MergerAgent", "merger",
					"Used as the name of an agent responsible for a change (that is, one made by the merge process itself)");
			}
		}

		protected virtual void WriteAttributes(XmlWriter writer)
		{
			writer.WriteAttributeString(@"typeGuid", string.Empty, GetTypeGuid());
			writer.WriteAttributeString(@"class", string.Empty, this.GetType().FullName);
			writer.WriteAttributeString(@"relativeFilePath", string.Empty, RelativeFilePath);
			writer.WriteAttributeString(@"type", string.Empty, Description);
			writer.WriteAttributeString(@"guid", string.Empty, Guid.ToString());
			writer.WriteAttributeString(@"date", string.Empty, DateTimeProvider.Current.UtcNow.ToString(TimeFormatWithTimeZone));
			writer.WriteAttributeString(@"whoWon", _whoWon);
			writer.WriteAttributeString(@"htmlDetails", HtmlDetails);

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

		public virtual void MakeHtmlDetails(XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext, IGenerateHtmlContext htmlMaker)
		{
			StringBuilder sb = new StringBuilder(@"<head><style type='text/css'>");
			sb.Append(htmlMaker.HtmlContextStyles(oursContext));
			sb.Append(@"</style></head><body><div class='description'>");
			sb.Append(GetFullHumanReadableDescription());
			sb.Append(@"</div>");
			var ancestorHtml = @"";
			if (ancestorContext != null)
			{
				ancestorHtml = htmlMaker.HtmlContext(ancestorContext);
			}
			AppendAlternative(sb, oursContext, ancestorContext, ancestorHtml, htmlMaker, OursLabel);
			AppendAlternative(sb, theirsContext, ancestorContext, ancestorHtml, htmlMaker, TheirsLabel);
			sb.Append(@"<div class='mergechoice'>");
			AppendWhatHappened(sb);
			sb.Append(@"</div>");
			sb.Append(@"</body>");
			HtmlDetails = sb.ToString();
		}

		/// <summary>
		/// Append to the builder a description of how the conflict was resolved. The two changed versions have already
		/// been reported. Usually it is enough to say who won, but in some cases (such as ambiguous insert) we need to say
		/// that we inserted both in some specific order.
		/// </summary>
		/// <param name="sb"></param>
		protected virtual void AppendWhatHappened(StringBuilder sb)
		{
			var winnerId = WinnerId;
			if (!string.IsNullOrEmpty(_whoWon) && _whoWon != winnerId)
				winnerId = _whoWon; // Can happen if loser edited and winner deleted.
			sb.Append(string.Format(KeptChangePattern, winnerId));
		}

		private void AppendAlternative(StringBuilder sb, XmlNode changedContext, XmlNode ancestorContext,
			string ancestorHtml, IGenerateHtmlContext htmlMaker, string label)
		{
			if (changedContext != null)
			{
				var oursHtml = htmlMaker.HtmlContext(changedContext);
				if (ancestorContext != null)
				{
					try
					{
						var diffReport = new Rainbow.HtmlDiffEngine.Merger(ancestorHtml, oursHtml).merge();
						sb.Append(@"<div class='alternative'>");
						sb.Append(string.Format(LocalizationManager.GetString(@"Conflict.XChanges", "{0}'s changes: ",
							"{0} is a user ID; this string labels a block of text with changes highlighted"), label));
						sb.Append(diffReport);
						sb.Append(@"</div>");
						return;
					}
					catch (Exception)
					{
						// Diff sometimes fails; I've had IndexOutOfRange exceptions when one input is just <a></a> for example.
						// For example, you can reach this point by executing XmlMergerTests.OneEditedDeepChildOfElementOtherDeleted
					}
				}
				// fall-back strategy
				sb.Append(@"<div class='alternative'>");
				sb.Append(string.Format(LocalizationManager.GetString(@"Conflict.XVersion", "{0}'s version: ", "{0} is a user ID; this string labels his version, when we were not able to highlight changes"), label));
				sb.Append(oursHtml);
				sb.Append(@"</div>");
			}
		}

		string ContextDataLabel
		{
			get
			{
				if (Context != null)
					return Context.DataLabel;
				return @"";
			}
		}

		string OursLabel
		{
			get
			{
				if (Situation == null)
					return LocalizationManager.GetString(@"Conflict.OneUser", "One user", "Used in place of a user name when we don't have one");
				return Situation.AlphaUserId;
			}
		}

		string TheirsLabel
		{
			get
			{
				if (Situation == null)
					return LocalizationManager.GetString(@"Conflict.AnotherUser", "Another user", "Used in place of a user name when we don't have one");
				return Situation.BetaUserId;
			}
		}

		public string WinnerId
		{
			get
			{
				return (this.Situation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.TheyWin)
					? Situation.BetaUserId
					: Situation.AlphaUserId;
			}
		}

		private static IContainer _conflictFactory;
		static List<Type> _additionalConflictTypes = new List<Type>();

		/// <summary>
		/// Notify the system that the specified type of conflict may need to be created by CreateFromConflictElement.
		/// NOTE that the indicated type MUST have a TypeGuid. See declarations of concrete classes in this file.
		/// </summary>
		/// <param name="type"></param>
		internal static void RegisterContextClass(Type type)
		{
			_additionalConflictTypes.Add(type);
			_conflictFactory = null; // regenerate when next needed
		}

		private static IContainer ConflictFactory
		{
			get
			{
				if (_conflictFactory == null)
				{
					var builder = new Autofac.ContainerBuilder();
					//moved this down into Register for autofac 2: builder.SetDefaultScope(InstanceScope.Factory);

					Register<RemovedVsEditedElementConflict>(builder);
					Register<EditedVsRemovedElementConflict>(builder);

					Register<AmbiguousInsertConflict>(builder);
					Register<AmbiguousInsertReorderConflict>(builder);

					Register<BothEditedAttributeConflict>(builder);
					Register<BothEditedTextConflict>(builder);
					Register<BothEditedTheSameAtomicElement>(builder);
					Register<XmlTextBothEditedTextConflict>(builder);
					Register<XmlTextBothAddedTextConflict>(builder);

					Register<BothReorderedElementConflict>(builder);
					Register<BothInsertedAtDifferentPlaceConflict>(builder);
					Register<BothAddedMainElementButWithDifferentContentConflict>(builder);

					Register<RemovedVsEditedAttributeConflict>(builder);
					Register<EditedVsRemovedAttributeConflict>(builder);
					Register<BothAddedAttributeConflict>(builder);

					Register<RemovedVsEditedTextConflict>(builder);
					Register<EditedVsRemovedTextConflict>(builder);
					Register<XmlTextEditVsRemovedConflict>(builder);
					Register<XmlTextRemovedVsEditConflict>(builder);

					Register<IncompatibleMoveConflict>(builder);

					Register<BothEditedDifferentPartsOfDependentPiecesOfDataWarning>(builder);
					Register<UnmergableFileTypeConflict>(builder);
					Register<MergeWarning>(builder);

					foreach (var conflictType in _additionalConflictTypes)
					{
						Register(builder, conflictType);
					}
					_conflictFactory = builder.Build();
				}
				return _conflictFactory;
			}
		}

		protected static string EmptyLiteral
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.Empty", "(empty)",
					"string used in place of element content when the element is missing");
			}
		}

		protected static string WhatHappenedPattern
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.WhatHappened", "{0} changed {1} to {2}, while {3} changed it to {4}.  {5}",
					"{0} and {3} are names; {1}, {2}, and {4} are data; {5} is a sentence saying which change won");
			}
		}

		protected static string ConflictDesciptionPattern
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.Description", "{0} ({1}): {2}",
					"{0} is a general description of the conflict, {1} gives more detail, {2} says how it was resolved");
			}
		}

		public static IConflict CreateFromConflictElement(XmlNode conflictNode)
		{
			try
			{
				var typeGuid = conflictNode.GetStringAttribute(@"typeGuid");
				return ConflictFactory.ResolveNamed<IConflict>(typeGuid, new TypedParameter(typeof(XmlNode), conflictNode));
			}
			catch (Exception error)
			{
				return new UnreadableConflict(conflictNode);
			}
		}

		private static void Register<T>(Autofac.ContainerBuilder builder)
		{
			builder.RegisterType<T>().As<IConflict>().Named<IConflict>(GetTypeGuid(typeof(T))).InstancePerDependency();
		}

		private static void Register(Autofac.ContainerBuilder builder, Type type)
		{
			builder.RegisterType(type).As<IConflict>().Named<IConflict>(GetTypeGuid(type)).InstancePerDependency();
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
			var msg = dom.SelectSingleNode(@"//message");
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
					return x.SelectSingleNode(@"conflict");
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
			set { }
		}

		public string RelativeFilePath
		{
			get { return string.Empty; }
		}

		public ContextDescriptor Context
		{
			get { return new ContextDescriptor(@"??", string.Empty); }
			set { }
		}

		public string GetFullHumanReadableDescription()
		{
			return Description;
		}

		public string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.AnotherUser", "Unreadable Conflict"); }
		}

		public string HtmlDetails
		{
			get { return @"<body>" + LocalizationManager.GetString(@"Conflict.CantInterpretConflict", "The system does not know how to interpret this conflict report.") + @"</body>"; }
		}

		public void MakeHtmlDetails(XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext, IGenerateHtmlContext htmlMaker)
		{
			throw new NotImplementedException("MakeHtmlDetails should never be called for UnreadableConflict; they are never made in the conflict detection phase");
		}

		public string WinnerId
		{
			get { return string.Empty; }
		}

		public Guid Guid
		{
			get { return new Guid(ConflictNode.GetOptionalStringAttribute(@"guid", string.Empty)); }
		}

		public MergeSituation Situation
		{
			get { return new NullMergeSituation(); }
			set { }
		}

		public string RevisionWhereMergeWasCheckedIn
		{
			get
			{
				return string.Empty;
			}
		}

		public string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new NotImplementedException("Unable to retrieve from source control.");
		}

		public void WriteAsChorusNotesAnnotation(XmlWriter writer)
		{
			throw new NotImplementedException("UnreadableConflict is not intended to be ever saved");
		}

		public bool IsNotification { get { return false; } }
	}

	/// <summary>
	/// This conflict indicates that one user made a change to a file and another user deleted that whole file.
	/// The default behavior of Chorus is to keep the modified file and report that we did so.
	/// <note>At the time we are detecting this we do not have the same version information as we do for conflicts that
	/// are internal to a file.</note>
	/// </summary>
	public class FileChangedVsFileDeletedConflict : IConflict
	{
		private ContextDescriptor _contextDescriptor = new ContextDescriptor("File Deleted", "unknown");

		public FileChangedVsFileDeletedConflict(string changedVsDeletedFile)
		{
			RelativeFilePath = changedVsDeletedFile;
		}

		public string RelativeFilePath { get; private set; }
		public ContextDescriptor Context
		{
			get { return _contextDescriptor; }
			set { _contextDescriptor = value; }
		}

		public string GetFullHumanReadableDescription()
		{
			return HtmlDetails;
		}

		public string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.FileChangedVsFileDeleted", "File Deleted vs File Edited Conflict"); }
		}
		public string HtmlDetails { get { return String.Format("One user deleted {0} when the other changed it, the file was kept with its changes.", RelativeFilePath); } }
		public void MakeHtmlDetails(XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext, IGenerateHtmlContext htmlMaker)
		{
			throw new NotImplementedException("The FileChangedVsFileDeletedConflict is created before ChorusMerge, this method is not needed.");
		}

		public string WinnerId { get; private set; }
		public Guid Guid { get; private set; }
		public MergeSituation Situation { get; set; }
		public string RevisionWhereMergeWasCheckedIn { get; private set; }
		public string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new NotImplementedException();
		}

		public void WriteAsChorusNotesAnnotation(XmlWriter writer)
		{
			Guard.AgainstNull(writer, @"writer");
			writer.WriteStartElement(@"annotation");
			writer.WriteAttributeString(@"class", "mergeconflict");
			writer.WriteAttributeString(@"guid", GuidProvider.Current.NewGuid().ToString());
			writer.WriteAttributeString(@"ref", String.Format("sil://localhost?&label=File {0} deleted and changed", Path.GetFileName(RelativeFilePath)));
			writer.WriteStartElement(@"message");
			writer.WriteAttributeString(@"author", string.Empty, "merger");
			writer.WriteAttributeString(@"status", string.Empty, @"open");
			writer.WriteAttributeString(@"guid", string.Empty, Guid.ToString());
			writer.WriteAttributeString(@"date", string.Empty, DateTimeProvider.Current.UtcNow.ToString(Conflict.TimeFormatWithTimeZone));
			writer.WriteString(GetFullHumanReadableDescription());

			//we embed the conflict xml inside the CDATA section so that it pass a more generic schema without
			//resorting to the complexities of namespaces
			var b = new StringBuilder();
			using (var embeddedWriter = XmlWriter.Create(b, CanonicalXmlSettings.CreateXmlWriterSettings(ConformanceLevel.Fragment)))
			{
				embeddedWriter.WriteStartElement(@"conflict");
				embeddedWriter.WriteAttributeString(@"class", string.Empty, this.GetType().FullName);
				embeddedWriter.WriteAttributeString(@"relativeFilePath", string.Empty, RelativeFilePath);
				embeddedWriter.WriteAttributeString(@"type", string.Empty, Description);
				embeddedWriter.WriteAttributeString(@"guid", string.Empty, Guid.ToString());
				embeddedWriter.WriteAttributeString(@"date", string.Empty, DateTimeProvider.Current.UtcNow.ToString(Conflict.TimeFormatWithTimeZone));

				if (Context != null)
				{
					Context.WriteAttributes(embeddedWriter);
				}
				embeddedWriter.WriteEndElement();
			}
			writer.WriteCData(b.ToString());
			writer.WriteEndElement(); // </message>
			writer.WriteEndElement();// </annotation>
		}

		public bool IsNotification { get; private set; }
	}

	#region TextConflicts

	public abstract class TextConflict : Conflict // NB: Be sure to register any new concrete subclasses in CreateFromConflictElement method.
	{
		protected readonly string _elementName;
		protected readonly string _alphaValue;
		protected readonly string _betaValue;
		protected readonly string _ancestorValue;

		protected TextConflict(XmlNode alpha, XmlNode beta, XmlNode ancestor, MergeSituation mergeSituation, string whoWon)
			: base(mergeSituation)
		{
			var extantNode = alpha ?? beta ?? ancestor;
			_whoWon = whoWon;
			_elementName = extantNode.LocalName;
			_alphaValue = (alpha == null) ? string.Empty : alpha.InnerText;
			_betaValue = (beta == null) ? string.Empty : beta.InnerText;
			_ancestorValue = (ancestor == null) ? string.Empty : ancestor.InnerText;
		}

		protected TextConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
			var element = xmlRepresentation.FirstChild;
			if (element == null)
			{
				_elementName = string.Empty;
				_alphaValue = string.Empty;
				_betaValue = string.Empty;
				_ancestorValue = string.Empty;
			}
			else
			{
				_elementName = element.LocalName;
				_alphaValue = element.SelectSingleNode(@"alphaValue").InnerText;
				_betaValue = element.SelectSingleNode(@"betaValue").InnerText;
				_ancestorValue = element.SelectSingleNode(@"ancestorValue").InnerText;
			}
		}

		protected override void WriteAttributes(XmlWriter writer)
		{
			base.WriteAttributes(writer);

			if (_elementName == string.Empty)
				return;

			writer.WriteStartElement(_elementName);
			writer.WriteElementString(@"alphaValue", _alphaValue);
			writer.WriteElementString(@"betaValue", _betaValue);
			writer.WriteElementString(@"ancestorValue", _ancestorValue);
			writer.WriteEndElement();
		}

		private string ElementDescription
		{
			get
			{
				return string.Format(@"{0}", _elementName);
			}
		}

		public virtual string WhatHappened
		{
			get
			{
				var empty = EmptyLiteral;
				var ancestor = string.IsNullOrEmpty(_ancestorValue) ? empty : "'" + _ancestorValue + "'";
				var alpha = string.IsNullOrEmpty(_alphaValue) ? empty : "'" + _alphaValue + "'";
				var beta = string.IsNullOrEmpty(_betaValue) ? empty : "'" + _betaValue + "'";
				return string.Format(WhatHappenedPattern,
					Situation.AlphaUserId, ancestor, alpha,
					Situation.BetaUserId, beta,
					GetWhoWonText());
			}
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format(ConflictDesciptionPattern,
				Description, ElementDescription, WhatHappened);
		}

		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			string revision = null;
			// string elementId = null;
			switch (mergeSource)
			{
				case ThreeWayMergeSources.Source.Ancestor:
					revision = fileRetriever.GetCommonAncestorOfRevisions(Situation.AlphaUserRevision, Situation.BetaUserRevision);
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
			using (var f = TempFile.TrackExisting(fileRetriever.RetrieveHistoricalVersionOfFile(Situation.PathToFileInRepository, revision)))
			{
				var doc = new XmlDocument();
				doc.Load(f.Path);
				var element = doc.SelectSingleNode(Context.PathToUserUnderstandableElement);
				if (element == null)
					throw new ApplicationException("Could not find the element specified by the context, " + Context.PathToUserUnderstandableElement);
				return element.OuterXml;
			}
		}
	}

	[TypeGuid(@"E1CCC59B-46E5-4D24-A1B1-5B621A0F8870")]
	public sealed class RemovedVsEditedTextConflict : TextConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public RemovedVsEditedTextConflict(XmlNode alphaNode, XmlNode betaNode, XmlNode ancestor, MergeSituation mergeSituation, string whoWon)
			: base(alphaNode, betaNode, ancestor, mergeSituation, whoWon)
		{
		}

		public RemovedVsEditedTextConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.RemovedVsEditedElt", "Removed Vs Edited Element Conflict"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(
					LocalizationManager.GetString(@"Conflict.RemovedVsEditedEltIds", // used twice
						"{0} deleted this element, while {1} edited it. {2}",
						"{0} and {1} are user names, {2} describes who won"),
					Situation.AlphaUserId, Situation.BetaUserId, GetWhoWonText());
			}
		}
	}

	[TypeGuid(@"c1ed6dbb-e382-11de-8a39-0800200c9a66")]
	public sealed class EditedVsRemovedTextConflict : TextConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public EditedVsRemovedTextConflict(XmlNode alphaNode, XmlNode betaNode, XmlNode ancestor, MergeSituation mergeSituation, string whoWon)
			: base(alphaNode, betaNode, ancestor, mergeSituation, whoWon)
		{
		}

		public EditedVsRemovedTextConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.RemovedVsEditedElt", "Removed Vs Edited Element Conflict"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(
					LocalizationManager.GetString(@"Conflict.RemovedVsEditedEltIds", // used twice
						"{0} deleted this element, while {1} edited it. {2}",
						"{0} and {1} are user names, {2} describes who won"),
					Situation.BetaUserId, Situation.AlphaUserId, GetWhoWonText());
			}
		}
	}

	[TypeGuid(@"0507DE36-13A3-449D-8302-48F5213BD92E")]
	public sealed class BothEditedTextConflict : TextConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public BothEditedTextConflict(XmlNode alphaNode, XmlNode betaNode, XmlNode ancestor, MergeSituation mergeSituation, string whoWon)
			: base(alphaNode, betaNode, ancestor, mergeSituation, whoWon)
		{
		}

		public BothEditedTextConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothEditedText", "Both edited a text field conflict"); }
		}
	}
	#endregion TextConflicts

	#region AttributeConflicts

	public abstract class AttributeConflict : Conflict // NB: Be sure to register any new concrete subclasses in CreateFromConflictElement method.
	{
		protected readonly string _attributeName;
		protected readonly string _alphaValue;
		protected readonly string _betaValue;
		protected readonly string _ancestorValue;

		protected AttributeConflict(string attributeName, string alphaValue, string betaValue, string ancestorValue, MergeSituation mergeSituation, string whoWon)
			: base(mergeSituation)
		{
			_whoWon = whoWon;
			_attributeName = attributeName;
			_alphaValue = alphaValue;
			_betaValue = betaValue;
			_ancestorValue = ancestorValue;
		}

		protected AttributeConflict(XmlNode xmlRepresentation) : base(xmlRepresentation)
		{
			_attributeName = xmlRepresentation.GetOptionalStringAttribute(@"attributeName", @"unknown");
			_alphaValue = xmlRepresentation.GetOptionalStringAttribute(@"alphaValue", string.Empty);
			_betaValue = xmlRepresentation.GetOptionalStringAttribute(@"betaValue", string.Empty);
			_ancestorValue = xmlRepresentation.GetOptionalStringAttribute(@"ancestorValue", string.Empty);
		}

		protected override void WriteAttributes(XmlWriter writer)
		{
			base.WriteAttributes(writer);
			writer.WriteAttributeString(@"attributeName", _attributeName);
			writer.WriteAttributeString(@"alphaValue", _alphaValue);
			writer.WriteAttributeString(@"betaValue", _betaValue);
			writer.WriteAttributeString(@"ancestorValue", _ancestorValue);
		}

		public string AttributeDescription
		{
			get
			{
				return string.Format(@"{0}", _attributeName);
			}
		}

		public string WhatHappened
		{
			get
			{
				var empty = EmptyLiteral;
				var ancestor = string.IsNullOrEmpty(_ancestorValue) ? empty : "'" + _ancestorValue + "'";
				var alpha = string.IsNullOrEmpty(_alphaValue) ? empty : "'" + _alphaValue + "'";
				var beta = string.IsNullOrEmpty(_betaValue) ? empty : "'" + _betaValue + "'";
				return string.Format(WhatHappenedPattern,
									 Situation.AlphaUserId, ancestor, alpha,
									 Situation.BetaUserId, beta,
									 GetWhoWonText());
			}
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format(ConflictDesciptionPattern, Description, AttributeDescription, WhatHappened);
		}

		public virtual string GetXmlOfConflict()
		{
			// REVIEW JohnH(RandyR): What is this supposed to be doing?
			return string.Format(@"<annotation type='{0}'/>", GetType().Name);
		}

		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			string revision = null;
			// string elementId = null;
			switch (mergeSource)
			{
				case ThreeWayMergeSources.Source.Ancestor:
					revision = fileRetriever.GetCommonAncestorOfRevisions(this.Situation.AlphaUserRevision, Situation.BetaUserRevision);
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
			using (var f = TempFile.TrackExisting(fileRetriever.RetrieveHistoricalVersionOfFile(Situation.PathToFileInRepository, revision)))
			{
				var doc = new XmlDocument();
				doc.Load(f.Path);
				var element = doc.SelectSingleNode(Context.PathToUserUnderstandableElement);
				if (element == null)
				{
					throw new ApplicationException("Could not find the element specified by the context, " + Context.PathToUserUnderstandableElement);
				}
				return element.OuterXml;
			}
		}
	}

	[TypeGuid(@"c1ed6dc1-e382-11de-8a39-0800200c9a66")]
	sealed public class BothAddedAttributeConflict : AttributeConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public BothAddedAttributeConflict(string attributeName, string alphaValue, string betaValue, MergeSituation mergeSituation, string whoWon)
			: base(attributeName, alphaValue, betaValue, null, mergeSituation, whoWon)
		{
		}

		// Constructor required for regenerating conflict object from XML.
		public BothAddedAttributeConflict(XmlNode xmlRepresentation) :
			base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get { return string.Format(LocalizationManager.GetString(@"Conflict.BothAddedAttr", "Both Added Attribute Conflict")); }
		}
	}

	[TypeGuid(@"B11ABA8C-DFB9-4E37-AF35-8AFDB86F00B7")]
	sealed public class RemovedVsEditedAttributeConflict : AttributeConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public RemovedVsEditedAttributeConflict(string attributeName, string alphaValue, string betaValue, string ancestorValue, MergeSituation mergeSituation, string whoWon)
			: base(attributeName, alphaValue, betaValue, ancestorValue, mergeSituation, whoWon)
		{
		}
		// Constructor required for regenerating conflict object from XML.
		public RemovedVsEditedAttributeConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}
		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.RemovedVsEditedAttr", "Removed Vs Edited Attribute Conflict"); }
		}
	}

	[TypeGuid(@"c1ed6dc0-e382-11de-8a39-0800200c9a66")]
	sealed public class EditedVsRemovedAttributeConflict : AttributeConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public EditedVsRemovedAttributeConflict(string attributeName, string alphaValue, string betaValue, string ancestorValue, MergeSituation mergeSituation, string whoWon)
			: base(attributeName, alphaValue, betaValue, ancestorValue, mergeSituation, whoWon)
		{
		}
		// Constructor required for regenerating conflict object from XML.
		public EditedVsRemovedAttributeConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}
		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.RemovedVsEditedAttr", "Removed Vs Edited Attribute Conflict"); }
		}
	}

	[TypeGuid(@"5BBDF4F6-953A-4F79-BDCD-0B1F733DA4AB")]
	sealed public class BothEditedAttributeConflict : AttributeConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public BothEditedAttributeConflict(string attributeName, string alphaValue, string betaValue, string ancestorValue, MergeSituation mergeSituation, string whoWon)
			: base(attributeName, alphaValue, betaValue, ancestorValue, mergeSituation, whoWon)
		{
		}
		// Constructor required for regenerating conflict object from XML.
		public BothEditedAttributeConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}
		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothEditedAttr", "Both Edited Attribute Conflict"); }
		}
	}

	#endregion AttributeConflicts

	#region ElementConflicts

	[TypeGuid(@"DC5D3236-9372-4965-9E34-386182675A5C")]
	public abstract class ElementConflict : Conflict // NB: Be sure to register any new concrete subclasses in CreateFromConflictElement method.
	{
		protected readonly string _elementName;


		protected ElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(mergeSituation)
		{
			_elementName = elementName;
			_whoWon = whoWon;
		}

		protected ElementConflict(XmlNode xmlRepresentation) : base(xmlRepresentation)
		{
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format("{0}{1}", (Context == null ? "" : Context.DataLabel + ": "), WhatHappened);
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

		protected static string DeletedVsEditedPattern
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.DeletedVsEditedElt", "{0} deleted this element, while {1} edited it.",
					"{0} and {1} are user names");
			}
		}

		protected static string RemovedVsEditedTxtPattern
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.RemovedVsEditedTxtName",
					"{0} deleted this element, while {1} edited its text content. {2}");
			}
		}
	}

	[TypeGuid(@"56F9C347-C4FA-48F4-8028-729F3CFF48EF")]
	public class RemovedVsEditedElementConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		private XmlNode _theirs; // node we deleted. only set by original constructor, not from XML
		private XmlNode _ancestor;  // ancestor they changed, we deleted. only set by original constructor, not from XML
		public RemovedVsEditedElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
			_theirs = betaNode;
			_ancestor = ancestorElement;
		}

		/// <summary>
		/// For a removed vs edited, we get a better version of this by passing as context the actual node they modified (and null as the first
		/// argument, indicating we deleted. This produces just one subsection of changes, corresponding to the claim that they edited.
		/// The context nodes are typiclly one layer further out, and passing them leads to two sets of changes.
		///
		/// </summary>
		/// <param name="oursContext"></param>
		/// <param name="theirsContext"></param>
		/// <param name="ancestorContext"></param>
		/// <param name="htmlMaker"></param>
		public override void MakeHtmlDetails(XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext, IGenerateHtmlContext htmlMaker)
		{
			// (Minimally) route tested, XmlMergerTests.OneEditedDeepChildOfElementOtherDeleted.
			base.MakeHtmlDetails(null, _theirs, _ancestor, htmlMaker);
		}


		public RemovedVsEditedElementConflict(XmlNode xmlRepresentation) : base(xmlRepresentation)
		{

		}
		public override string Description
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.RemovedVsEditedElt", // used twice
					"Removed Vs Edited Element Conflict");
			}
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(DeletedVsEditedPattern + " ", Situation.AlphaUserId, Situation.BetaUserId) + GetWhoWonText();
			}
		}
	}

	[TypeGuid(@"3d9ba4ac-4a25-11df-9879-0800200c9a66")]
	public class EditedVsRemovedElementConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		private XmlNode _ours; // node they deleted. only set by original constructor, not from XML
		private XmlNode _ancestor;  // ancestor we changed, they deleted. only set by original constructor, not from XML
		public EditedVsRemovedElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
			_ours = alphaNode;
			_ancestor = ancestorElement;
		}

		public EditedVsRemovedElementConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{

		}
		/// <summary>
		/// For a edited vs removed, we get a better version of this by passing as context the actual node we modified (and null as the second
		/// argument, indicating they deleted. This produces just one subsection of changes, corresponding to the claim that we edited.
		/// The context nodes are typiclly one layer further out, and passing them leads to two sets of changes.
		///
		/// </summary>
		/// <param name="oursContext"></param>
		/// <param name="theirsContext"></param>
		/// <param name="ancestorContext"></param>
		/// <param name="htmlMaker"></param>
		public override void MakeHtmlDetails(XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext, IGenerateHtmlContext htmlMaker)
		{
			// (Minimally) route tested, XmlMergerTests.OneEditedDeepChildOfElementOtherDeleted.
			base.MakeHtmlDetails(_ours, null, _ancestor, htmlMaker);
		}

		public override string Description
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.RemovedVsEditedElt", // used twice
					"Removed Vs Edited Element Conflict");
			}
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(DeletedVsEditedPattern + " ", Situation.BetaUserId, Situation.AlphaUserId) + GetWhoWonText();
			}
		}
	}

	[TypeGuid(@"c1ed94d6-e382-11de-8a39-0800200c9a66")]
	public class BothAddedMainElementButWithDifferentContentConflict : ElementConflict
	{
		public BothAddedMainElementButWithDifferentContentConflict(string elementName, XmlNode alphaNode, XmlNode betaNode,
			MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, null, mergeSituation, elementDescriber, whoWon)
		{
		}

		public BothAddedMainElementButWithDifferentContentConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothAddedDifferent", "Both added the same element, but with different content conflict"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.BothAddedDiffNames", "{0} and {1} added the same element, but with different content."),
					Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	[TypeGuid(@"14262878-270A-4E27-BA5F-7D232B979D6B")]
	public class BothReorderedElementConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public BothReorderedElementConflict(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public BothReorderedElementConflict(XmlNode xmlRepresentation) : base(xmlRepresentation)
		{

		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothReordered", "Both Reordered Conflict"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.BothReorderedNames",
					"{0} and {1} both re-ordered the children of this element in different ways."),
					Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	[TypeGuid(@"46423E4C-34EF-4F19-B62F-07AB2634E53B")]
	public class BothInsertedAtDifferentPlaceConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public BothInsertedAtDifferentPlaceConflict(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public BothInsertedAtDifferentPlaceConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{

		}
		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.InsertedDiff", "Both Inserted at different places Conflict"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.InsertedDiffNames",
					"{0} and {1} both added the same children to this element in different places."),
					Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
		public override bool IsNotification
		{
			get { return true; }
		}
	}
	[TypeGuid(@"B77C0D86-2368-4380-B2E4-7943F3E7553C")]
	public class AmbiguousInsertConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public AmbiguousInsertConflict(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber,
			LocalizationManager.GetString(@"Conflict.BothUsers", "both users",
			"this phrase is inserted as a desciption of 'who won', in a case where we were able to keep both changes"))
		{
		}

		public AmbiguousInsertConflict(XmlNode xmlRepresentation) : base(xmlRepresentation)
		{

		}
		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.AmbiguousInsert", "Ambiguous Insert Warning"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.AmbiguousInsertNames",
					"{0} and {1} both inserted material in this element in the same place. The automated merger cannot be sure of the correct order for the inserted material, but kept both of them."),
					Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
		public override string ToString()
		{
			return GetType() + ":" + _elementName + @" (or lower?)";
		}

		public override bool IsNotification
		{
			get { return true; }
		}
	}

	[TypeGuid(@"A5CE68F5-ED0D-4732-BAA8-A04A99ED35B3")]
	public class AmbiguousInsertReorderConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
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
			get { return LocalizationManager.GetString(@"Conflict.AmbiguousInsertReorder", "Ambiguous Insert Reorder Warning"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.AmbiguousInsertReorderNames",
					"{0} inserted material in this element, but {1} re-ordered things. The automated merger cannot be sure of the correct position for the inserted material."),
					Situation.AlphaUserId, Situation.BetaUserId);
			}
		}

		public override bool IsNotification
		{
			get { return true; }
		}
	}

	/// <summary>
	/// This not really a conflict but is used to store warnings that occur during merge
	/// </summary>
	[TypeGuid(@"2E7B7307-B316-4644-8565-1B667372E269")]
	public class MergeWarning : Conflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		private readonly string _message;

		public MergeWarning(string message)
			: base(new NullMergeSituation(), string.Empty)
		{
			_message = message;
		}

		public MergeWarning(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{

		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.MergeWarning", "Merge Warning", "default description of less serious conflicts; possibly never seen"); }
		}

		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new NotImplementedException();
		}

		public override string GetFullHumanReadableDescription()
		{
			return _message;
		}

		protected override void AppendWhatHappened(StringBuilder sb)
		{
			if (string.IsNullOrWhiteSpace(_whoWon))
			{
				return; // Do nothing about showing who won.
			}
			base.AppendWhatHappened(sb);
		}
	}

	/// <summary>
	/// Used when, say, one guy adds a translation of an the example sentence,
	/// but meanwhile the other guy changed the example sentence, so the translation is
	/// suspect.  This could be a "warning", if we had such a thing.
	/// </summary>
	[TypeGuid(@"71636317-A94F-4814-8665-1D0F83DF388F")]
	public class BothEditedDifferentPartsOfDependentPiecesOfDataWarning : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
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
			get { return LocalizationManager.GetString(@"Conflict.BothEditedDependent", "Both Edited Different Parts Of Dependent Pieces Of Data Warning"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.BothEditedDependentNames",
					"{0} edited one part of this element, while {1} edited another part. Since these two pieces of data are thought to be dependent on each other, someone needs to verify that the resulting merge is ok."),
					Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	/// <summary>
	/// Used when, say, one guy adds a translation of an the example sentence,
	/// but meanwhile the other guy changed the example sentence, so the translation is
	/// suspect.  This could be a "warning", if we had such a thing.
	/// (Or maybe not. Currently in FlexBridge, multistring alternatives are marked atomic, so this is the one that
	/// comes up for both editing, say, the definition of a sense.)
	/// </summary>
	[TypeGuid(@"3d9ba4ae-4a25-11df-9879-0800200c9a66")]
	public class BothEditedTheSameAtomicElement : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method.
	{
		public BothEditedTheSameAtomicElement(string elementName, XmlNode alphaNode, XmlNode betaNode,
			XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public BothEditedTheSameAtomicElement(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}


		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothEditedAtomic", "Both Edited the Same Atomic Element", "atomic in this context means indivisible; we don't know how to merge changes to different parts of it"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.BothEditedAtomic", "{0} and {1} edited the same part of this data."), Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	[TypeGuid(@"c1ed6dbc-e382-11de-8a39-0800200c9a66")]
	public sealed class XmlTextEditVsRemovedConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method
	{
		public XmlTextEditVsRemovedConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public XmlTextEditVsRemovedConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.RemovedVsEditedTxt", // used twice
					"Removed Vs Edited Xml Text Element Conflict");
			}
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(RemovedVsEditedTxtPattern,
					Situation.BetaUserId, Situation.AlphaUserId, GetWhoWonText());
			}
		}
	}

	[TypeGuid(@"c1ed6dbd-e382-11de-8a39-0800200c9a66")]
	public sealed class XmlTextRemovedVsEditConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method
	{
		public XmlTextRemovedVsEditConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public XmlTextRemovedVsEditConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get
			{
				return LocalizationManager.GetString(@"Conflict.RemovedVsEditedTxt", // used twice
					"Removed Vs Edited Xml Text Element Conflict");
			}
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(RemovedVsEditedTxtPattern, Situation.AlphaUserId, Situation.BetaUserId, GetWhoWonText());
			}
		}
	}

	[TypeGuid(@"c1ed6dbe-e382-11de-8a39-0800200c9a66")]
	public sealed class XmlTextBothEditedTextConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method
	{
		public XmlTextBothEditedTextConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, XmlNode ancestorElement, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, ancestorElement, mergeSituation, elementDescriber, whoWon)
		{
		}

		public XmlTextBothEditedTextConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothEditedTxt", "Both Edited Xml Text Element Conflict"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.BothEditedTxtName", "{0} and {1} edited this element."), Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	[TypeGuid(@"c1ed6dbf-e382-11de-8a39-0800200c9a66")]
	public sealed class XmlTextBothAddedTextConflict : ElementConflict // NB: Be sure to register any new instances in CreateFromConflictElement method
	{
		public XmlTextBothAddedTextConflict(string elementName, XmlNode alphaNode, XmlNode betaNode, MergeSituation mergeSituation, IElementDescriber elementDescriber, string whoWon)
			: base(elementName, alphaNode, betaNode, null, mergeSituation, elementDescriber, whoWon)
		{
		}

		public XmlTextBothAddedTextConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothAddedTxt", "Both Added Xml Text Conflict"); }
		}

		public override string WhatHappened
		{
			get
			{
				return string.Format(LocalizationManager.GetString(@"Conflict.BothAddedTxtName",
					"{0} and {1} added different text to this element."),
					Situation.AlphaUserId, Situation.BetaUserId);
			}
		}
	}

	[TypeGuid(@"c1ed6dc2-e382-11de-8a39-0800200c9a66")]
	public sealed class IncompatibleMoveConflict : Conflict // NB: Be sure to register any new instances in CreateFromConflictElement method
	{
		private readonly string _elementName;
		private readonly XmlNode _alphaNode;
		private readonly XmlNode _betaNode;
		private readonly IElementDescriber _elementDescriber;

		public IncompatibleMoveConflict(string elementName, XmlNode alphaNode)
			: base(null, LocalizationManager.GetString(@"Conflict.Dunno", "Dunno", "A version of 'who won' used when we don't know"))
		{
			_elementName = elementName;
			_alphaNode = alphaNode;
			_betaNode = null;
			_elementDescriber = null;
		}

		public IncompatibleMoveConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
		}

		#region Overrides of Conflict

		protected override string Author
		{
			get { return "FLExBridge"; }
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format(LocalizationManager.GetString(@"Conflict.CopyWasMade",
				"{0}: A copy was made so that the object can be in both places.",
				"{0} is Conflict.BothAddedDesc"), Description);
		}

		public override string Description
		{
			get { return LocalizationManager.GetString(@"Conflict.BothAddedDesc", "Two users both moved something, but to different locations"); }
		}

		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			// Borrowed from ElementConflict.
			//fileRetriever.RetrieveHistoricalVersionOfFile(_file, userSources[]);
			return null;
		}

		#endregion
	}

	#endregion ElementConflicts
}