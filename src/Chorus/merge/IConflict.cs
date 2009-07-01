using System;
using Chorus.merge.xml.generic;
using Chorus.retrieval;

namespace Chorus.merge
{
	public interface IConflict
	{
		//store a descriptor that can be used later to find the element again, as when reviewing conflict.
		//for xml files, this context descriptor can be an xpath which returns the element
		string XPathOrOtherDescriptorOfConflictingElement { get; set; }

		string GetFullHumanReadableDescription();
		string ConflictTypeHumanName
		{
			get;
		}

		Guid  Guid { get; }

		string GetRawDataFromConflictVersion(IRetrieveFile fileRetriever, ThreeWayMergeSources.Source mergeSource, string recordLevel);
	}

	public class ThreeWayMergeSources
	{
		public enum Source
		{
			Ancestor, UserX, UserY
		}
	}

}