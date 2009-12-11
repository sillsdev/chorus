using System;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers;

namespace Chorus.merge
{
	[TypeGuid("18C7E1A2-2F69-442F-9057-6B3AC9833675")]
	public class UnmergableFileTypeConflict :Conflict
	{
		public UnmergableFileTypeConflict(MergeSituation situation )
			: base(situation)
		{
		}

		public UnmergableFileTypeConflict(XmlNode xmlRepresentation)
			: base(xmlRepresentation)
		{
			Context.DataLabel = Situation.PathToFileInRepository;
		}


		public override string GetFullHumanReadableDescription()
		{
			var b = new StringBuilder();
			b.AppendFormat("Chorus did not have the ability to merge both user's version of the file '{0}'.", Situation.PathToFileInRepository);
			b.AppendLine();

			string loserId = (Situation.ConflictHandlingMode != MergeOrder.ConflictHandlingModeChoices.TheyWin)
								 ?
									 Situation.BetaUserId
								 :Situation.AlphaUserId;

			string loserRev = (Situation.ConflictHandlingMode != MergeOrder.ConflictHandlingModeChoices.TheyWin)
								 ?
									 Situation.BetaUserRevision
								 :Situation.AlphaUserRevision;

			b.AppendFormat("The merger gave both users the copy from '{0}'.", WinnerId);
			b.AppendLine();
			b.AppendFormat("The version from '{0}' is not lost; it is available in the repository at revision {1}", loserId, loserRev);
			return b.ToString();
		}



		public override string Description
		{
			get { return "Merge Failure"; }
		}

		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new NotImplementedException();
		}
	}
}