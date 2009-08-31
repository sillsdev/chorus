using System;
using System.IO;
using System.Xml;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers;

namespace Chorus.merge
{

	public interface IConflict
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

		string WinnerId
		{
			get;
		}
		Guid  Guid { get; }
		MergeSituation Situation { get; set; }
		string RevisionWhereMergeWasCheckedIn { get;  }

		string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource);
		void WriteAsXml(XmlWriter writer);
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