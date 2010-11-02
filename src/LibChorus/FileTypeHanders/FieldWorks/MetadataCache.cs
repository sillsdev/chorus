using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.Properties;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Cache to hold metadata about the CmObject classes.
	/// </summary>
	public sealed class MetadataCache
	{
		private readonly Dictionary<string, FdoClassInfo> _classes = new Dictionary<string, FdoClassInfo>();
		private readonly IEnumerable<FdoClassInfo> _concreteClasses;
		private readonly Dictionary<string, FdoClassInfo> _classesWithCollectionProperties;

		/// <summary>
		/// Constructor.
		/// </summary>
		public MetadataCache()
		{
			AddMainClassInfo();
			SetSuperclasses();
			_concreteClasses = new List<FdoClassInfo>(from classInfo in _classes.Values
													  where !classInfo.IsAbstract
													  select classInfo);
			_classesWithCollectionProperties = new Dictionary<string, FdoClassInfo>(_concreteClasses.Count());
			foreach (var classWithCollectionProp in from classInfo in _classes.Values
													where classInfo.AllCollectionProperties.Count() > 0
													select classInfo)
			{
				_classesWithCollectionProperties.Add(classWithCollectionProp.ClassName, classWithCollectionProp);
			}
		}

		///<summary>
		/// Get the FDO class information for the given class.
		///</summary>
		///<returns>The FdoClassInfo with the given class name.</returns>
		/// <exception cref="ArgumentNullException">
		/// thrown if <param name="className"/> is null or an empty string.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if <param name="className"/> is not a recognized class name.
		/// </exception>
		public FdoClassInfo GetClassInfo(string className)
		{
			if (string.IsNullOrEmpty(className))
				throw new ArgumentNullException("classname", AnnotationImages.kNullOrEmptyString);

			return _classes[className];
		}

		internal IEnumerable<FdoClassInfo> AllConcreteClasses
		{
			get { return _concreteClasses; }
		}

		internal IDictionary<string, FdoClassInfo> ClassesWithCollectionProperties
		{
			get { return _classesWithCollectionProperties; }
		}

		/// <summary>
		/// Add a custom property to a class.
		/// </summary>
		public void AddCustomPropInfo(string className, FdoPropertyInfo propInfo)
		{
			_classes[className].AddProperty(propInfo);
		}

		private void SetSuperclasses()
		{
			foreach (var classInfo in _classes.Values.Where(classInfo => classInfo.SuperclassName != null))
			{
				classInfo.Superclass = _classes[classInfo.SuperclassName];
				classInfo.SuperclassName = null;
			}
		}

		/// <summary>
		/// This method started out as generated from the main FW FDO system.
		/// But, now it will live on its own, and as the model cahnges, it will need to be changed by hand.
		/// </summary>
		private void AddMainClassInfo()
		{
			var clsInfo = new FdoClassInfo("CmObject", true, null);
			_classes.Add("CmObject", clsInfo);

			clsInfo = new FdoClassInfo("CmProject", true, "CmObject");
			_classes.Add("CmProject", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("DateModified", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));

			clsInfo = new FdoClassInfo("CmFolder", "CmObject");
			_classes.Add("CmFolder", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("SubFolders", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Files", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("Note", "CmObject");
			_classes.Add("Note", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Content", DataType.MultiString));

			clsInfo = new FdoClassInfo("FsComplexFeature", "FsFeatDefn");
			_classes.Add("FsComplexFeature", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("CmMajorObject", true, "CmObject");
			_classes.Add("CmMajorObject", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("DateModified", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Publications", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("HeaderFooterSets", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("Segment", "CmObject");
			_classes.Add("Segment", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BeginOffset", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("FreeTranslation", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("LiteralTranslation", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Notes", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Analyses", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("CmPossibility", "CmObject");
			_classes.Add("CmPossibility", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("SubPossibilities", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("SortSpec", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Restrictions", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Confidence", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Status", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("DateModified", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("Discussion", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Researchers", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("HelpId", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ForeColor", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("BackColor", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("UnderColor", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("UnderStyle", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Hidden", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("IsProtected", DataType.Boolean));

			clsInfo = new FdoClassInfo("CmPossibilityList", "CmMajorObject");
			_classes.Add("CmPossibilityList", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Depth", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PreventChoiceAboveLevel", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("IsSorted", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("IsClosed", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("PreventDuplicates", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("PreventNodeChoices", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Possibilities", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("HelpFile", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("UseExtendedFields", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("DisplayOption", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ItemClsid", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("IsVernacular", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("WsSelector", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ListVersion", DataType.Guid));

			clsInfo = new FdoClassInfo("CmFilter", "CmObject");
			_classes.Add("CmFilter", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ClassId", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("FieldId", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("FieldInfo", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("App", DataType.Guid));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Rows", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("ColumnInfo", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ShowPrompt", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PromptText", DataType.Unicode));

			clsInfo = new FdoClassInfo("CmRow", "CmObject");
			_classes.Add("CmRow", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Cells", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("CmCell", "CmObject");
			_classes.Add("CmCell", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Contents", DataType.String));

			clsInfo = new FdoClassInfo("CmLocation", "CmPossibility");
			_classes.Add("CmLocation", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Alias", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("CmPerson", "CmPossibility");
			_classes.Add("CmPerson", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Alias", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Gender", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("DateOfBirth", DataType.GenDate));
			clsInfo.AddProperty(new FdoPropertyInfo("PlaceOfBirth", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("IsResearcher", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("PlacesOfResidence", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Education", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DateOfDeath", DataType.GenDate));
			clsInfo.AddProperty(new FdoPropertyInfo("Positions", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("StText", "CmObject");
			_classes.Add("StText", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Paragraphs", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("RightToLeft", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Tags", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("StPara", true, "CmObject");
			_classes.Add("StPara", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("StyleRules", DataType.TextPropBinary));

			clsInfo = new FdoClassInfo("StTxtPara", "StPara");
			_classes.Add("StTxtPara", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Label", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("Contents", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("ParseIsCurrent", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("TextObjects", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("AnalyzedTextObjects", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Segments", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Translations", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("StStyle", "CmObject");
			_classes.Add("StStyle", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("BasedOn", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Next", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Rules", DataType.TextPropBinary));
			clsInfo.AddProperty(new FdoPropertyInfo("IsPublishedTextStyle", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("IsBuiltIn", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("IsModified", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("UserLevel", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Context", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Structure", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Function", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Usage", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("UserView", "CmObject");
			_classes.Add("UserView", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("App", DataType.Guid));
			clsInfo.AddProperty(new FdoPropertyInfo("Records", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Details", DataType.Binary));
			clsInfo.AddProperty(new FdoPropertyInfo("System", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("SubType", DataType.Integer));

			clsInfo = new FdoClassInfo("UserViewRec", "CmObject");
			_classes.Add("UserViewRec", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Clsid", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Level", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Fields", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Details", DataType.Binary));

			clsInfo = new FdoClassInfo("UserViewField", "CmObject");
			_classes.Add("UserViewField", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Label", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("HelpString", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Flid", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Required", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Style", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("PossList", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("WsSelector", DataType.Integer));

			clsInfo = new FdoClassInfo("CmOverlay", "CmObject");
			_classes.Add("CmOverlay", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("PossList", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PossItems", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("TextTag", "CmObject");
			_classes.Add("TextTag", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BeginSegment", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("EndSegment", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("BeginAnalysisIndex", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("EndAnalysisIndex", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Tag", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("CmAgent", "CmObject");
			_classes.Add("CmAgent", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("StateInformation", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Human", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Notes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Version", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Approves", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Disapproves", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("CmAnthroItem", "CmPossibility");
			_classes.Add("CmAnthroItem", clsInfo);

			clsInfo = new FdoClassInfo("CmCustomItem", "CmPossibility");
			_classes.Add("CmCustomItem", clsInfo);

			clsInfo = new FdoClassInfo("CrossReference", "CmObject");
			_classes.Add("CrossReference", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Comment", DataType.MultiString));

			clsInfo = new FdoClassInfo("CmTranslation", "CmObject");
			_classes.Add("CmTranslation", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Translation", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Status", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("CmAgentEvaluation", "CmObject");
			_classes.Add("CmAgentEvaluation", clsInfo);

			clsInfo = new FdoClassInfo("CmAnnotation", true, "CmObject");
			_classes.Add("CmAnnotation", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("CompDetails", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Comment", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("AnnotationType", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Source", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("InstanceOf", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Text", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Features", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("DateModified", DataType.Time));

			clsInfo = new FdoClassInfo("CmAnnotationDefn", "CmPossibility");
			_classes.Add("CmAnnotationDefn", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("AllowsComment", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("AllowsFeatureStructure", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("AllowsInstanceOf", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("InstanceOfSignature", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("UserCanCreate", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("CanCreateOrphan", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("PromptUser", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("CopyCutPastable", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("ZeroWidth", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Multi", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Severity", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("MaxDupOccur", DataType.Integer));

			clsInfo = new FdoClassInfo("CmIndirectAnnotation", "CmAnnotation");
			_classes.Add("CmIndirectAnnotation", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("AppliesTo", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("CmBaseAnnotation", "CmAnnotation");
			_classes.Add("CmBaseAnnotation", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BeginOffset", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Flid", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("EndOffset", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("BeginObject", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("EndObject", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("OtherObjects", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("WsSelector", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("BeginRef", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("EndRef", DataType.Integer));

			clsInfo = new FdoClassInfo("CmMediaAnnotation", "CmAnnotation");
			_classes.Add("CmMediaAnnotation", clsInfo);

			clsInfo = new FdoClassInfo("StFootnote", "StText");
			_classes.Add("StFootnote", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("FootnoteMarker", DataType.String));

			clsInfo = new FdoClassInfo("UserConfigAcct", "CmObject");
			_classes.Add("UserConfigAcct", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Sid", DataType.Binary));
			clsInfo.AddProperty(new FdoPropertyInfo("UserLevel", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("HasMaintenance", DataType.Boolean));

			clsInfo = new FdoClassInfo("UserAppFeatAct", "CmObject");
			_classes.Add("UserAppFeatAct", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("UserConfigAcct", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ApplicationId", DataType.Guid));
			clsInfo.AddProperty(new FdoPropertyInfo("FeatureId", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ActivatedLevel", DataType.Integer));

			clsInfo = new FdoClassInfo("Publication", "CmObject");
			_classes.Add("Publication", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("PageHeight", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PageWidth", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("IsLandscape", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("GutterMargin", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("GutterLoc", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Divisions", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("FootnoteSepWidth", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PaperHeight", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PaperWidth", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("BindingEdge", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("SheetLayout", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("SheetsPerSig", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("BaseFontSize", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("BaseLineSpacing", DataType.Integer));

			clsInfo = new FdoClassInfo("PubDivision", "CmObject");
			_classes.Add("PubDivision", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("DifferentFirstHF", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("DifferentEvenHF", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("StartAt", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PageLayout", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("HFSet", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("NumColumns", DataType.Integer));

			clsInfo = new FdoClassInfo("PubPageLayout", "CmObject");
			_classes.Add("PubPageLayout", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("MarginTop", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("MarginBottom", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("MarginInside", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("MarginOutside", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PosHeader", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("PosFooter", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("IsBuiltIn", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("IsModified", DataType.Boolean));

			clsInfo = new FdoClassInfo("PubHFSet", "CmObject");
			_classes.Add("PubHFSet", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("DefaultHeader", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DefaultFooter", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FirstHeader", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FirstFooter", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("EvenHeader", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("EvenFooter", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("PubHeader", "CmObject");
			_classes.Add("PubHeader", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("InsideAlignedText", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("CenteredText", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("OutsideAlignedText", DataType.String));

			clsInfo = new FdoClassInfo("CmFile", "CmObject");
			_classes.Add("CmFile", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("OriginalPath", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("InternalPath", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Copyright", DataType.MultiString));

			clsInfo = new FdoClassInfo("CmPicture", "CmObject");
			_classes.Add("CmPicture", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("PictureFile", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Caption", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("LayoutPos", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ScaleFactor", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("LocationRangeType", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("LocationMin", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("LocationMax", DataType.Integer));

			clsInfo = new FdoClassInfo("FsFeatureSystem", "CmObject");
			_classes.Add("FsFeatureSystem", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Types", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Features", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("FsClosedFeature", "FsFeatDefn");
			_classes.Add("FsClosedFeature", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Values", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("FsClosedValue", "FsFeatureSpecification");
			_classes.Add("FsClosedValue", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Value", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("FsComplexValue", "FsFeatureSpecification");
			_classes.Add("FsComplexValue", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Value", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("FsDisjunctiveValue", "FsFeatureSpecification");
			_classes.Add("FsDisjunctiveValue", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Value", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("FsFeatDefn", true, "CmObject");
			_classes.Add("FsFeatDefn", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Default", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("GlossAbbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("RightGlossSep", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ShowInGloss", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("DisplayToRightOfValues", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("CatalogSourceId", DataType.Unicode));

			clsInfo = new FdoClassInfo("FsFeatureSpecification", true, "CmObject");
			_classes.Add("FsFeatureSpecification", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("RefNumber", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ValueState", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Feature", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("FsFeatStruc", "FsAbstractStructure");
			_classes.Add("FsFeatStruc", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("FeatureDisjunctions", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("FeatureSpecs", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("FsFeatStrucDisj", "FsAbstractStructure");
			_classes.Add("FsFeatStrucDisj", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Contents", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("FsFeatStrucType", "CmObject");
			_classes.Add("FsFeatStrucType", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Features", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("CatalogSourceId", DataType.Unicode));

			clsInfo = new FdoClassInfo("FsAbstractStructure", true, "CmObject");
			_classes.Add("FsAbstractStructure", clsInfo);

			clsInfo = new FdoClassInfo("FsNegatedValue", "FsFeatureSpecification");
			_classes.Add("FsNegatedValue", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Value", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("FsOpenFeature", "FsFeatDefn");
			_classes.Add("FsOpenFeature", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("WsSelector", DataType.Integer));

			clsInfo = new FdoClassInfo("FsOpenValue", "FsFeatureSpecification");
			_classes.Add("FsOpenValue", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Value", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("FsSharedValue", "FsFeatureSpecification");
			_classes.Add("FsSharedValue", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Value", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("FsSymFeatVal", "CmObject");
			_classes.Add("FsSymFeatVal", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("GlossAbbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("RightGlossSep", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ShowInGloss", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("CatalogSourceId", DataType.Unicode));

			clsInfo = new FdoClassInfo("CmSemanticDomain", "CmPossibility");
			_classes.Add("CmSemanticDomain", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("LouwNidaCodes", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("OcmCodes", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("OcmRefs", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("RelatedDomains", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Questions", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("CmDomainQ", "CmObject");
			_classes.Add("CmDomainQ", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Question", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ExampleWords", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ExampleSentences", DataType.MultiString));

			clsInfo = new FdoClassInfo("StJournalText", "StText");
			_classes.Add("StJournalText", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("DateModified", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("CreatedBy", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ModifiedBy", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("CmMedia", "CmObject");
			_classes.Add("CmMedia", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Label", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("MediaFile", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("CmResource", "CmObject");
			_classes.Add("CmResource", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Version", DataType.Guid));

			clsInfo = new FdoClassInfo("Scripture", "CmMajorObject");
			_classes.Add("Scripture", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ScriptureBooks", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Styles", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("RefSepr", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ChapterVerseSepr", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("VerseSepr", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Bridge", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ImportSettings", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ArchivedDrafts", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("FootnoteMarkerSymbol", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("DisplayFootnoteReference", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("RestartFootnoteSequence", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("RestartFootnoteBoundary", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("UseScriptDigits", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("ScriptDigitZero", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ConvertCVDigitsOnExport", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Versification", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("VersePunct", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ChapterLabel", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("PsalmLabel", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("BookAnnotations", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("NoteCategories", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FootnoteMarkerType", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("DisplayCrossRefReference", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("CrossRefMarkerSymbol", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("CrossRefMarkerType", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("CrossRefsCombinedWithFootnotes", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("DisplaySymbolInFootnote", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("DisplaySymbolInCrossRef", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Resources", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("ScrBook", "CmObject");
			_classes.Add("ScrBook", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Sections", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("BookId", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Title", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbrev", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("IdText", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Footnotes", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Diffs", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("UseChapterNumHeading", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("CanonicalNum", DataType.Integer));

			clsInfo = new FdoClassInfo("ScrRefSystem", "CmObject");
			_classes.Add("ScrRefSystem", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Books", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("ScrBookRef", "CmObject");
			_classes.Add("ScrBookRef", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BookName", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("BookAbbrev", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("BookNameAlt", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("ScrSection", "CmObject");
			_classes.Add("ScrSection", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Heading", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Content", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("VerseRefStart", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("VerseRefEnd", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("VerseRefMin", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("VerseRefMax", DataType.Integer));

			clsInfo = new FdoClassInfo("ScrImportSet", "CmObject");
			_classes.Add("ScrImportSet", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ImportType", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ImportProjToken", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ScriptureSources", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("BackTransSources", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("NoteSources", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ScriptureMappings", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("NoteMappings", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("ScrDraft", "CmObject");
			_classes.Add("ScrDraft", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Books", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Protected", DataType.Boolean));

			clsInfo = new FdoClassInfo("ScrDifference", "CmObject");
			_classes.Add("ScrDifference", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("RefStart", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("RefEnd", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("DiffType", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("RevMin", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("RevLim", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("RevParagraph", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("ScrImportSource", true, "CmObject");
			_classes.Add("ScrImportSource", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("NoteType", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("ScrImportP6Project", "ScrImportSource");
			_classes.Add("ScrImportP6Project", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ParatextID", DataType.Unicode));

			clsInfo = new FdoClassInfo("ScrImportSFFiles", "ScrImportSource");
			_classes.Add("ScrImportSFFiles", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("FileFormat", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Files", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("ScrMarkerMapping", "CmObject");
			_classes.Add("ScrMarkerMapping", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BeginMarker", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("EndMarker", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Excluded", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Target", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Domain", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Style", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("NoteType", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("ScrBookAnnotations", "CmObject");
			_classes.Add("ScrBookAnnotations", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Notes", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("ChkHistRecs", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("ScrScriptureNote", "CmBaseAnnotation");
			_classes.Add("ScrScriptureNote", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ResolutionStatus", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Categories", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Quote", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Discussion", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Recommendation", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Resolution", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Responses", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("DateResolved", DataType.Time));

			clsInfo = new FdoClassInfo("ScrCheckRun", "CmObject");
			_classes.Add("ScrCheckRun", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("CheckId", DataType.Guid));
			clsInfo.AddProperty(new FdoPropertyInfo("RunDate", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("Result", DataType.Integer));

			clsInfo = new FdoClassInfo("ScrTxtPara", "StTxtPara");
			_classes.Add("ScrTxtPara", clsInfo);

			clsInfo = new FdoClassInfo("ScrFootnote", "StFootnote");
			_classes.Add("ScrFootnote", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ParaContainingOrc", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("RnResearchNbk", "CmMajorObject");
			_classes.Add("RnResearchNbk", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Records", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Reminders", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("RecTypes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("CrossReferences", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("RnGenericRec", "CmObject");
			_classes.Add("RnGenericRec", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Title", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("VersionHistory", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Reminders", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Researchers", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Confidence", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Restrictions", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AnthroCodes", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("PhraseTags", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("SubRecords", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("DateModified", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("CrossReferences", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ExternalMaterials", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FurtherQuestions", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("SeeAlso", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("DateOfEvent", DataType.GenDate));
			clsInfo.AddProperty(new FdoPropertyInfo("CounterEvidence", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Status", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("SupersededBy", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("SupportingEvidence", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Conclusions", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Hypothesis", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ResearchPlan", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Locations", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Sources", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("TimeOfEvent", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Participants", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("PersonalNotes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Discussion", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("Reminder", "CmObject");
			_classes.Add("Reminder", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("Date", DataType.GenDate));

			clsInfo = new FdoClassInfo("RnRoledPartic", "CmObject");
			_classes.Add("RnRoledPartic", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Participants", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Role", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("MoStemMsa", "MoMorphSynAnalysis");
			_classes.Add("MoStemMsa", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("MsFeatures", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PartOfSpeech", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("InflectionClass", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Stratum", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ProdRestrict", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("FromPartsOfSpeech", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("LexEntry", "CmObject");
			_classes.Add("LexEntry", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("HomographNumber", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("CitationForm", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("DateCreated", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("DateModified", DataType.Time));
			clsInfo.AddProperty(new FdoPropertyInfo("MorphoSyntaxAnalyses", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Senses", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Bibliography", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Etymology", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Restrictions", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("SummaryDefinition", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("LiteralMeaning", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("MainEntriesOrSenses", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Comment", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("DoNotUseForParsing", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("ExcludeAsHeadword", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("LexemeForm", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AlternateForms", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Pronunciations", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("ImportResidue", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("EntryRefs", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("ConstChartRow", "CmObject");
			_classes.Add("ConstChartRow", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Notes", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("ClauseType", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("EndParagraph", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("EndSentence", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("StartDependentClauseGroup", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("EndDependentClauseGroup", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Cells", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Label", DataType.String));

			clsInfo = new FdoClassInfo("LexExampleSentence", "CmObject");
			_classes.Add("LexExampleSentence", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Example", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Reference", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("Translations", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));

			clsInfo = new FdoClassInfo("LexDb", "CmMajorObject");
			_classes.Add("LexDb", clsInfo);
			// Went away in DM 28 clsInfo.AddProperty(new FdoPropertyInfo("Entries", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Appendixes", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("SenseTypes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("UsageTypes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DomainTypes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("MorphTypes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("LexicalFormIndex", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AllomorphIndex", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Introduction", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("IsHeadwordCitationForm", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("IsBodyInSeparateSubentry", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("ReversalIndexes", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("References", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Resources", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("VariantEntryTypes", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ComplexEntryTypes", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("ConstituentChartCellPart", true, "CmObject");
			_classes.Add("ConstituentChartCellPart", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Column", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("MergesAfter", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("MergesBefore", DataType.Boolean));

			clsInfo = new FdoClassInfo("ConstChartWordGroup", "ConstituentChartCellPart");
			_classes.Add("ConstChartWordGroup", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BeginSegment", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("EndSegment", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("BeginAnalysisIndex", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("EndAnalysisIndex", DataType.Integer));

			clsInfo = new FdoClassInfo("ConstChartMovedTextMarker", "ConstituentChartCellPart");
			_classes.Add("ConstChartMovedTextMarker", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("WordGroup", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Preposed", DataType.Boolean));

			clsInfo = new FdoClassInfo("ConstChartClauseMarker", "ConstituentChartCellPart");
			_classes.Add("ConstChartClauseMarker", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("DependentClauses", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("ConstChartTag", "ConstituentChartCellPart");
			_classes.Add("ConstChartTag", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Tag", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("PunctuationForm", "CmObject");
			_classes.Add("PunctuationForm", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.String));

			clsInfo = new FdoClassInfo("LexPronunciation", "CmObject");
			_classes.Add("LexPronunciation", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Location", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("MediaFiles", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("CVPattern", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("Tone", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));

			clsInfo = new FdoClassInfo("LexSense", "CmObject");
			_classes.Add("LexSense", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("MorphoSyntaxAnalysis", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AnthroCodes", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Senses", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Appendixes", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Definition", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("DomainTypes", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Examples", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Gloss", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ReversalEntries", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ScientificName", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("SenseType", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ThesaurusItems", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("UsageTypes", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AnthroNote", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Bibliography", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("DiscourseNote", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("EncyclopedicInfo", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("GeneralNote", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("GrammarNote", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("PhonologyNote", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Restrictions", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("SemanticsNote", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("SocioLinguisticsNote", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Source", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("Status", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("SemanticDomains", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Pictures", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("ImportResidue", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));

			clsInfo = new FdoClassInfo("MoAdhocProhib", true, "CmObject");
			_classes.Add("MoAdhocProhib", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Adjacency", DataType.Integer));

			clsInfo = new FdoClassInfo("MoAffixAllomorph", "MoAffixForm");
			_classes.Add("MoAffixAllomorph", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("MsEnvFeatures", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PhoneEnv", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("MsEnvPartOfSpeech", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Position", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("MoAffixForm", true, "MoForm");
			_classes.Add("MoAffixForm", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("InflectionClasses", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("MoAffixProcess", "MoAffixForm");
			_classes.Add("MoAffixProcess", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Input", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Output", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("MoCompoundRule", true, "CmObject");
			_classes.Add("MoCompoundRule", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Stratum", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ToProdRestrict", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("MoDerivAffMsa", "MoMorphSynAnalysis");
			_classes.Add("MoDerivAffMsa", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("FromMsFeatures", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ToMsFeatures", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FromPartOfSpeech", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ToPartOfSpeech", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FromInflectionClass", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ToInflectionClass", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixCategory", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FromStemName", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Stratum", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FromProdRestrict", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ToProdRestrict", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("MoDerivStepMsa", "MoMorphSynAnalysis");
			_classes.Add("MoDerivStepMsa", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("PartOfSpeech", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("MsFeatures", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("InflFeats", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("InflectionClass", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ProdRestrict", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("MoEndoCompound", "MoBinaryCompoundRule");
			_classes.Add("MoEndoCompound", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("HeadLast", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("OverridingMsa", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("MoExoCompound", "MoBinaryCompoundRule");
			_classes.Add("MoExoCompound", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ToMsa", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("MoForm", true, "CmObject");
			_classes.Add("MoForm", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("MorphType", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("IsAbstract", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));

			clsInfo = new FdoClassInfo("MoInflAffixSlot", "CmObject");
			_classes.Add("MoInflAffixSlot", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Optional", DataType.Boolean));

			clsInfo = new FdoClassInfo("MoInflAffixTemplate", "CmObject");
			_classes.Add("MoInflAffixTemplate", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Slots", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Stratum", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Region", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PrefixSlots", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("SuffixSlots", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Final", DataType.Boolean));

			clsInfo = new FdoClassInfo("MoInflAffMsa", "MoMorphSynAnalysis");
			_classes.Add("MoInflAffMsa", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("InflFeats", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixCategory", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PartOfSpeech", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Slots", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("FromProdRestrict", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("MoInflClass", "CmObject");
			_classes.Add("MoInflClass", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Subclasses", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("RulesOfReferral", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("StemNames", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ReferenceForms", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("MoMorphData", "CmObject");
			_classes.Add("MoMorphData", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Strata", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("CompoundRules", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("AdhocCoProhibitions", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AnalyzingAgents", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("TestSets", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("GlossSystem", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ParserParameters", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ProdRestrict", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("MoMorphSynAnalysis", true, "CmObject");
			_classes.Add("MoMorphSynAnalysis", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Components", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("GlossString", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("GlossBundle", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));

			clsInfo = new FdoClassInfo("MoMorphType", "CmPossibility");
			_classes.Add("MoMorphType", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Postfix", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Prefix", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("SecondaryOrder", DataType.Integer));

			clsInfo = new FdoClassInfo("MoReferralRule", "CmObject");
			_classes.Add("MoReferralRule", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Input", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Output", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("MoStemAllomorph", "MoForm");
			_classes.Add("MoStemAllomorph", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("PhoneEnv", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("StemName", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("LexAppendix", "CmObject");
			_classes.Add("LexAppendix", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Contents", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("MoStemName", "CmObject");
			_classes.Add("MoStemName", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Regions", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("DefaultAffix", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DefaultStem", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("MoStratum", "CmObject");
			_classes.Add("MoStratum", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Phonemes", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("PartOfSpeech", "CmPossibility");
			_classes.Add("PartOfSpeech", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("InherFeatVal", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("EmptyParadigmCells", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("RulesOfReferral", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("InflectionClasses", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixTemplates", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixSlots", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("StemNames", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("BearableFeatures", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("InflectableFeats", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ReferenceForms", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("DefaultFeatures", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DefaultInflectionClass", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("CatalogSourceId", DataType.Unicode));

			clsInfo = new FdoClassInfo("ReversalIndex", "CmMajorObject");
			_classes.Add("ReversalIndex", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("PartsOfSpeech", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Entries", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));

			clsInfo = new FdoClassInfo("ReversalIndexEntry", "CmObject");
			_classes.Add("ReversalIndexEntry", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Subentries", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("PartOfSpeech", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ReversalForm", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("Text", "CmMajorObject");
			_classes.Add("Text", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Source", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("SoundFilePath", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Contents", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Genres", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("IsTranslated", DataType.Boolean));

			clsInfo = new FdoClassInfo("WfiAnalysis", "CmObject");
			_classes.Add("WfiAnalysis", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Category", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("MsFeatures", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Stems", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Derivation", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Meanings", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("MorphBundles", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("CompoundRuleApps", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("InflTemplateApps", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Evaluations", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("WfiGloss", "CmObject");
			_classes.Add("WfiGloss", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("WfiWordform", "CmObject");
			_classes.Add("WfiWordform", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Analyses", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("SpellingStatus", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Checksum", DataType.Integer));

			clsInfo = new FdoClassInfo("WordFormLookup", "CmObject");
			_classes.Add("WordFormLookup", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("ThesaurusCentral", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("ThesaurusItems", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AnthroCentral", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("AnthroCodes", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("WordformLookupList", "CmMajorObject");
			_classes.Add("WordformLookupList", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Wordforms", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("WritingSystem", DataType.Unicode));

			clsInfo = new FdoClassInfo("MoRuleMapping", true, "CmObject");
			_classes.Add("MoRuleMapping", clsInfo);

			clsInfo = new FdoClassInfo("MoInsertPhones", "MoRuleMapping");
			_classes.Add("MoInsertPhones", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Content", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("MoInsertNC", "MoRuleMapping");
			_classes.Add("MoInsertNC", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Content", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("MoModifyFromInput", "MoRuleMapping");
			_classes.Add("MoModifyFromInput", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Content", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Modification", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("MoDerivTrace", true, "CmObject");
			_classes.Add("MoDerivTrace", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("OutputForm", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("MoCompoundRuleApp", "MoDerivTrace");
			_classes.Add("MoCompoundRuleApp", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("LeftForm", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("RightForm", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Linker", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("MoDerivAffApp", "MoDerivTrace");
			_classes.Add("MoDerivAffApp", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("AffixForm", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixMsa", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("OutputMsa", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("MoInflAffixSlotApp", "MoDerivTrace");
			_classes.Add("MoInflAffixSlotApp", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Slot", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixForm", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixMsa", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("MoInflTemplateApp", "MoDerivTrace");
			_classes.Add("MoInflTemplateApp", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Template", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("SlotApps", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("MoPhonolRuleApp", "MoDerivTrace");
			_classes.Add("MoPhonolRuleApp", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Rule", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("VacuousApp", DataType.Boolean));

			clsInfo = new FdoClassInfo("MoStratumApp", "MoDerivTrace");
			_classes.Add("MoStratumApp", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Stratum", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("CompoundRuleApps", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("DerivAffApp", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("TemplateApp", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PRuleApps", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("PhContextOrVar", true, "CmObject");
			_classes.Add("PhContextOrVar", clsInfo);

			clsInfo = new FdoClassInfo("PhPhonContext", true, "PhContextOrVar");
			_classes.Add("PhPhonContext", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("PhIterationContext", "PhPhonContext");
			_classes.Add("PhIterationContext", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Minimum", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Maximum", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Member", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("PhSequenceContext", "PhPhonContext");
			_classes.Add("PhSequenceContext", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Members", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("PhSimpleContext", true, "PhPhonContext");
			_classes.Add("PhSimpleContext", clsInfo);

			clsInfo = new FdoClassInfo("PhSimpleContextBdry", "PhSimpleContext");
			_classes.Add("PhSimpleContextBdry", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("FeatureStructure", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("PhSimpleContextNC", "PhSimpleContext");
			_classes.Add("PhSimpleContextNC", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("FeatureStructure", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PlusConstr", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("MinusConstr", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("PhSimpleContextSeg", "PhSimpleContext");
			_classes.Add("PhSimpleContextSeg", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("FeatureStructure", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("PhVariable", "PhContextOrVar");
			_classes.Add("PhVariable", clsInfo);

			clsInfo = new FdoClassInfo("PhPhonemeSet", "CmObject");
			_classes.Add("PhPhonemeSet", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Phonemes", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("BoundaryMarkers", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));

			clsInfo = new FdoClassInfo("PhTerminalUnit", true, "CmObject");
			_classes.Add("PhTerminalUnit", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Codes", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("PhBdryMarker", "PhTerminalUnit");
			_classes.Add("PhBdryMarker", clsInfo);

			clsInfo = new FdoClassInfo("PhPhoneme", "PhTerminalUnit");
			_classes.Add("PhPhoneme", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BasicIPASymbol", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("Features", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("PhNaturalClass", true, "CmObject");
			_classes.Add("PhNaturalClass", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("PhNCFeatures", "PhNaturalClass");
			_classes.Add("PhNCFeatures", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Features", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("PhNCSegments", "PhNaturalClass");
			_classes.Add("PhNCSegments", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Segments", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("PhFeatureConstraint", "CmObject");
			_classes.Add("PhFeatureConstraint", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Feature", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("PhEnvironment", "CmObject");
			_classes.Add("PhEnvironment", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("LeftContext", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("RightContext", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AMPLEStringSegment", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("StringRepresentation", DataType.String));

			clsInfo = new FdoClassInfo("PhCode", "CmObject");
			_classes.Add("PhCode", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Representation", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("PhPhonData", "CmObject");
			_classes.Add("PhPhonData", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("PhonemeSets", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Environments", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("NaturalClasses", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Contexts", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("FeatConstraints", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("PhonRuleFeats", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PhonRules", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("MoDeriv", "CmObject");
			_classes.Add("MoDeriv", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("StemForm", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("StemMsa", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("InflectionalFeats", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("StratumApps", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("MoAlloAdhocProhib", "MoAdhocProhib");
			_classes.Add("MoAlloAdhocProhib", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Allomorphs", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("FirstAllomorph", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("RestOfAllos", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("MoMorphAdhocProhib", "MoAdhocProhib");
			_classes.Add("MoMorphAdhocProhib", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Morphemes", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("FirstMorpheme", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("RestOfMorphs", DataType.ReferenceSequence));

			clsInfo = new FdoClassInfo("MoCopyFromInput", "MoRuleMapping");
			_classes.Add("MoCopyFromInput", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Content", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("WfiWordSet", "CmObject");
			_classes.Add("WfiWordSet", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Cases", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("MoBinaryCompoundRule", "MoCompoundRule");
			_classes.Add("MoBinaryCompoundRule", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("LeftMsa", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("RightMsa", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Linker", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("MoCoordinateCompound", "MoBinaryCompoundRule");
			_classes.Add("MoCoordinateCompound", clsInfo);

			clsInfo = new FdoClassInfo("MoGlossSystem", "CmObject");
			_classes.Add("MoGlossSystem", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Glosses", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("MoGlossItem", "CmObject");
			_classes.Add("MoGlossItem", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Abbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Type", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("AfterSeparator", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ComplexNameSeparator", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("ComplexNameFirst", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("Status", DataType.Boolean));
			clsInfo.AddProperty(new FdoPropertyInfo("FeatStructFrag", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("GlossItems", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Target", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("EticID", DataType.Unicode));

			clsInfo = new FdoClassInfo("MoAdhocProhibGr", "MoAdhocProhib");
			_classes.Add("MoAdhocProhibGr", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Members", DataType.OwningCollection));

			clsInfo = new FdoClassInfo("WfiMorphBundle", "CmObject");
			_classes.Add("WfiMorphBundle", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Morph", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Msa", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Sense", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("LexEtymology", "CmObject");
			_classes.Add("LexEtymology", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Comment", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Form", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Gloss", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Source", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));

			clsInfo = new FdoClassInfo("ChkRef", "CmObject");
			_classes.Add("ChkRef", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Ref", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("KeyWord", DataType.String));
			clsInfo.AddProperty(new FdoPropertyInfo("Status", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Rendering", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Location", DataType.Integer));

			clsInfo = new FdoClassInfo("MoUnclassifiedAffixMsa", "MoMorphSynAnalysis");
			_classes.Add("MoUnclassifiedAffixMsa", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("PartOfSpeech", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("LexEntryType", "CmPossibility");
			_classes.Add("LexEntryType", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ReverseAbbr", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("LexRefType", "CmPossibility");
			_classes.Add("LexRefType", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ReverseAbbreviation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("MappingType", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Members", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ReverseName", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("LexReference", "CmObject");
			_classes.Add("LexReference", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Comment", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Targets", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));

			clsInfo = new FdoClassInfo("ChkSense", "CmObject");
			_classes.Add("ChkSense", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Explanation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Sense", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("DsChart", true, "CmMajorObject");
			_classes.Add("DsChart", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Template", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("DsConstChart", "DsChart");
			_classes.Add("DsConstChart", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("BasedOn", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Rows", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("DsDiscourseData", "CmObject");
			_classes.Add("DsDiscourseData", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("ConstChartTempl", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Charts", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ChartMarkers", DataType.OwningAtomic));

			clsInfo = new FdoClassInfo("ChkTerm", "CmPossibility");
			_classes.Add("ChkTerm", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Occurrences", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("SeeAlso", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Renderings", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("TermId", DataType.Integer));

			clsInfo = new FdoClassInfo("ChkRendering", "CmObject");
			_classes.Add("ChkRendering", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("SurfaceForm", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Meaning", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Explanation", DataType.MultiUnicode));

			clsInfo = new FdoClassInfo("LexEntryRef", "CmObject");
			_classes.Add("LexEntryRef", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("VariantEntryTypes", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("ComplexEntryTypes", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("PrimaryLexemes", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("ComponentLexemes", DataType.ReferenceSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("HideMinorEntry", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("Summary", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("LiftResidue", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("RefType", DataType.Integer));

			clsInfo = new FdoClassInfo("PhSegmentRule", "CmObject");
			_classes.Add("PhSegmentRule", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Description", DataType.MultiString));
			clsInfo.AddProperty(new FdoPropertyInfo("Name", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Direction", DataType.Integer));
			clsInfo.AddProperty(new FdoPropertyInfo("InitialStratum", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("FinalStratum", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("StrucDesc", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("PhRegularRule", "PhSegmentRule");
			_classes.Add("PhRegularRule", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("RightHandSides", DataType.OwningSequence));

			clsInfo = new FdoClassInfo("PhMetathesisRule", "PhSegmentRule");
			_classes.Add("PhMetathesisRule", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("StrucChange", DataType.String));

			clsInfo = new FdoClassInfo("PhSegRuleRHS", "CmObject");
			_classes.Add("PhSegRuleRHS", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("LeftContext", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("RightContext", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("StrucChange", DataType.OwningSequence));
			clsInfo.AddProperty(new FdoPropertyInfo("InputPOSes", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ExclRuleFeats", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ReqRuleFeats", DataType.ReferenceCollection));

			clsInfo = new FdoClassInfo("PhPhonRuleFeat", "CmPossibility");
			_classes.Add("PhPhonRuleFeat", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("Item", DataType.ReferenceAtomic));

			clsInfo = new FdoClassInfo("LangProject", "CmProject");
			_classes.Add("LangProject", clsInfo);
			clsInfo.AddProperty(new FdoPropertyInfo("EthnologueCode", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("WorldRegion", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("MainCountry", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("FieldWorkLocation", DataType.MultiUnicode));
			clsInfo.AddProperty(new FdoPropertyInfo("PartsOfSpeech", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Texts", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("TranslationTags", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Thesaurus", DataType.ReferenceAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("WordformLookupLists", DataType.ReferenceCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AnthroList", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("LexDb", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("ResearchNotebook", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AnalysisWss", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("CurVernWss", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("CurAnalysisWss", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("CurPronunWss", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("MsFeatureSystem", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("MorphologicalData", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Styles", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Filters", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ConfidenceLevels", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Restrictions", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Roles", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Status", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Locations", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("People", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Education", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("TimeOfDay", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("AffixCategories", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PhonologicalData", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Positions", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Overlays", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AnalyzingAgents", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("TranslatedScripture", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("VernWss", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("LinkedFilesRootDir", DataType.Unicode));
			clsInfo.AddProperty(new FdoPropertyInfo("Annotations", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("UserAccounts", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("ActivatedFeatures", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("AnnotationDefs", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("Pictures", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("SemanticDomainList", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("CheckLists", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("Media", DataType.OwningCollection));
			clsInfo.AddProperty(new FdoPropertyInfo("GenreList", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("DiscourseData", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("TextMarkupTags", DataType.OwningAtomic));
			clsInfo.AddProperty(new FdoPropertyInfo("PhFeatureSystem", DataType.OwningAtomic));
		}
	}
}