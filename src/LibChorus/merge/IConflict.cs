using System;
using System.Xml;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers;

namespace Chorus.merge
{
	public interface IConflict // NB: Be sure to register any concrete implementations in CreateFromConflictElement method.
	{
		//store a descriptor that can be used later to find the element again, as when reviewing conflict.
		//for xml files, this would be an xpath which returns the element which you'd use to
		//show the difference to the user
   //     string PathToUnitOfConflict { get; set; }
		string RelativeFilePath { get; }

		ContextDescriptor Context { get; set; }
		string GetFullHumanReadableDescription();
		string Description
		{
			get;
		}

		/// <summary>
		/// This should be a string which can be set as the entire DocumentText of a System.Windows.Forms.WebBrowser.
		/// </summary>
		string HtmlDetails { get; }

		/// <summary>
		/// This method should be called to initialize the HtmlDetails. It is passed the three versions of the context
		/// node that has the conflict (one may be null for some conflict types) and an object which can come up
		/// with an HTML version of the data.
		/// </summary>
		/// <param name="oursContext"></param>
		/// <param name="theirsContext"></param>
		/// <param name="ancestorContext"></param>
		/// <param name="htmlMaker"></param>
		void MakeHtmlDetails(XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext,
			IGenerateHtmlContext htmlMaker);

		string WinnerId
		{
			get;
		}
		Guid  Guid { get; }
		MergeSituation Situation { get; set; }
		string RevisionWhereMergeWasCheckedIn { get;  }

		string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource);
		void WriteAsChorusNotesAnnotation(XmlWriter writer);
	}

	public class TypeGuidAttribute : Attribute
	{
		public TypeGuidAttribute(string guid)
		{
			GuidString = guid;
		}
		public string GuidString { get; private set; }
	}

	public class ThreeWayMergeSources
	{
		public enum Source
		{
			Ancestor, UserX, UserY
		}
	}

}