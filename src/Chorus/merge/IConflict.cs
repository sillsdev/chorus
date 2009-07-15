using System;
using System.IO;
using System.Text;
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

	public class TypeGuidAttribute : Attribute
	{
		public TypeGuidAttribute(string guid)
		{
			GuidString = guid;
		}
		public string GuidString { get; private set; }
	}

	[TypeGuid("18C7E1A2-2F69-442F-9057-6B3AC9833675")]
	public class UnmergableFileTypeConflict :Conflict, IConflict
	{
		private MergeOrder _order;

		public UnmergableFileTypeConflict(MergeOrder order)
			: base(order.MergeSituation)
		{
			_order = order;
		}


		public string GetFullHumanReadableDescription()
		{
			var b = new StringBuilder();
			b.AppendFormat("Chorus did not have the ability to merge both user's version of the file {0}", _mergeSituation.PathToFileInRepository);
			b.AppendLine();
			string winnerId = (_order.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.TheyWin)
								  ?
									  _order.MergeSituation.UserYId
								  : _order.MergeSituation.UserXId;

			string loserId = (_order.ConflictHandlingMode != MergeOrder.ConflictHandlingModeChoices.TheyWin)
								  ?
									  _order.MergeSituation.UserYId
								  : _order.MergeSituation.UserXId;

			b.AppendFormat("The merger gave both users the copy from '{0}'.", winnerId);
			b.AppendLine();
			b.AppendFormat("The version from '{0}' is not lost; it is available in the Chorus repository", loserId);
			return b.ToString();
		}

		public string ConflictTypeHumanName
		{
			get { return "Merge Failure"; }
		}

		public string GetConflictingRecordOutOfSourceControl(IRetrieveFile fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new NotImplementedException();
		}
	}

	public class ThreeWayMergeSources
	{
		public enum Source
		{
			Ancestor, UserX, UserY
		}
	}

}