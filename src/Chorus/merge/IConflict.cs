using System;
using Chorus.merge.xml.generic;
using Chorus.retrieval;

namespace Chorus.merge
{
	public interface IConflict
	{
		//store a descriptor that can be used later to find the element again, as when reviewing conflict.
		//for xml files, this would be an xpath which returns the element which you'd use to
		//show the difference to the user
		string PathToUnitOfConflict { get; set; }

		string GetFullHumanReadableDescription();
		string ConflictTypeHumanName
		{
			get;
		}

		Guid  Guid { get; }

		string GetConflictingRecordOutOfSourceControl(IRetrieveFile fileRetriever, ThreeWayMergeSources.Source mergeSource);
	}

	public class ThreeWayMergeSources
	{
		public enum Source
		{
			Ancestor, UserX, UserY
		}
	}

}