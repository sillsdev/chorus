using System;
using System.IO;
using System.Xml;
using SIL.Xml;

namespace Chorus.merge
{
	/// <summary>
	/// for unit tests that don't need this
	/// </summary>
	public class NullMergeSituation : MergeSituation
	{
		public NullMergeSituation()
			: base(null, null, null, null, null, MergeOrder.ConflictHandlingModeChoices.WeWin)
		{
		}
	}

	/// <summary>
	/// for unit tests that don't need this
	/// </summary>
	public class NullMergeSituationTheyWin : MergeSituation
	{
		public NullMergeSituationTheyWin()
			: base(null, null, null, null, null, MergeOrder.ConflictHandlingModeChoices.TheyWin)
		{
		}
	}

	/// <summary>
	/// The context for the conflict that occurred.
	/// This information will be useful to help the user later determined exactly what happened.
	/// </summary>
	public class MergeSituation
	{
		public MergeOrder.ConflictHandlingModeChoices ConflictHandlingMode { get; private set; }

		//we don't have access to this yet: public const string kAncestorRevision = "ChorusAncestorRevision";
		public const string kAlphaUserId= "ChorusUserAlphaId";
		public const string kBetaUserId = "ChorusUserBetaId";
		public const string kAlphaUserRevision = "ChorusUserAlphaRevision";
		public const string kBetaUserRevision = "ChorusUserBetaRevision";
		//public const string kPathToFileInRepository = "ChorusPathToFileInRepository";

		/// <summary>
		/// A relative path
		/// </summary>
		public string PathToFileInRepository{ get; set;}
	   //we don't have access to this yet: public string AncestorRevision { get; set; }

		/// <summary>
		/// Here, "alpha" is the guy who wins when there's no better way to decide, and "beta" is the loser.
		/// </summary>
		public string AlphaUserId { get; set; }
		public string BetaUserId { get; set; }
		public string AlphaUserRevision { get; set; }
		public string BetaUserRevision { get; set; }


		public void WriteAsXml(XmlWriter writer)
		{
			writer.WriteStartElement("MergeSituation");
			writer.WriteAttributeString("alphaUserId", AlphaUserId);
			writer.WriteAttributeString("betaUserId", BetaUserId);
			writer.WriteAttributeString("alphaUserRevision", AlphaUserRevision);
			writer.WriteAttributeString("betaUserRevision", BetaUserRevision);
			writer.WriteAttributeString("path", PathToFileInRepository);
			writer.WriteAttributeString("conflictHandlingMode", string.Empty, ConflictHandlingMode.ToString());
			writer.WriteEndElement();
		}

		public MergeSituation(string relativePathToFile, string firstUserId, string firstUserRevision, string secondUserId, string secondRevision, MergeOrder.ConflictHandlingModeChoices conflictHandlingMode)
			:this(relativePathToFile, conflictHandlingMode)
		{
			switch (conflictHandlingMode)
			{
				case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					AlphaUserId = secondUserId;
					BetaUserId = firstUserId;
					BetaUserRevision = firstUserRevision;
					AlphaUserRevision = secondRevision;
					break;
				default:
					AlphaUserId = firstUserId;
					BetaUserId = secondUserId;
					BetaUserRevision = secondRevision;
					AlphaUserRevision = firstUserRevision;
					break;
			}


			//we don't have access to this yet:   AncestorRevision = ancestorRevision;
		}

		//this one is only for deserializing
		private MergeSituation(string relativePathToFile, MergeOrder.ConflictHandlingModeChoices conflictHandlingMode)
		{
			ConflictHandlingMode = conflictHandlingMode;

			if (relativePathToFile != null)
				relativePathToFile = relativePathToFile.Trim(new[] { Path.DirectorySeparatorChar });

			PathToFileInRepository = relativePathToFile;
		}

		public static MergeSituation CreateFromEnvironmentVariables(string pathToFileInRepository)
		{
			var mode = MergeOrder.ConflictHandlingModeChoices.WeWin;

			//we have to get this argument out of the environment variables because we have not control of the arguments
			//the dvcs system is going to use to call us. So whoever invokes the dvcs needs to set this variable ahead of time
			string modeString = Environment.GetEnvironmentVariable(MergeOrder.kConflictHandlingModeEnvVarName);
			if (!string.IsNullOrEmpty(modeString))
			{

				mode =
					(MergeOrder.ConflictHandlingModeChoices)
					Enum.Parse(typeof(MergeOrder.ConflictHandlingModeChoices), modeString);
			}

			//NB: these aren't needed to do the merge; we're given the actual 3 files.  But they are needed
			//for the conflict record, so that we can later look up exactly what were the 3 inputs at the time of merging.
			//string pathToFileInRepository = Environment.GetEnvironmentVariable(MergeSituation.kPathToFileInRepository);
			//we don't have access to this yet: string ancestorRevision = Environment.GetEnvironmentVariable(MergeSituation.kAncestorRevision);
			string alphaId = Environment.GetEnvironmentVariable(MergeSituation.kAlphaUserId);
			string betaId = Environment.GetEnvironmentVariable(MergeSituation.kBetaUserId);
			string alphaUserRevision = Environment.GetEnvironmentVariable(MergeSituation.kAlphaUserRevision);
			string betaUserRevision = Environment.GetEnvironmentVariable(MergeSituation.kBetaUserRevision);

			return new MergeSituation( pathToFileInRepository, alphaId, alphaUserRevision, betaId,
									  betaUserRevision, mode);

		}

		/// <summary>
		/// this isn't all we want to know, but it's all the guy telling hg to merge two branches knows.
		/// This is called to put this info into env vars where we can retrieve it once chorus.exe is called by hg
		/// </summary>
		public static void PushRevisionsToEnvironmentVariables(string userAlphaId, string alphaUserRevision, string userBetaId, string betaUserRevision)
		{
			Environment.SetEnvironmentVariable(kAlphaUserId, userAlphaId);
			Environment.SetEnvironmentVariable(kAlphaUserRevision, alphaUserRevision);

			Environment.SetEnvironmentVariable(kBetaUserId, userBetaId);
			Environment.SetEnvironmentVariable(kBetaUserRevision, betaUserRevision);
		}


		public static MergeSituation FromXml(XmlNode node)
		{
			var modeLabel = node.GetOptionalStringAttribute("conflictHandlingMode",
															string.Empty);

			MergeOrder.ConflictHandlingModeChoices mode;

			try
			{
				mode =
					(MergeOrder.ConflictHandlingModeChoices)
					Enum.Parse(typeof (MergeOrder.ConflictHandlingModeChoices),
							   modeLabel);
			}
			catch (Exception)
			{
				mode = MergeOrder.ConflictHandlingModeChoices.Unknown;
			}

			//Note, we can't use the normal constructor, because it switches who is alpha/beta
			//depending on the conflict handling mode.  We don't want to switch them again
			//when we're re-constituting the situation
			var situation = new MergeSituation(node.GetStringAttribute("path"), mode);
			situation.AlphaUserId = node.GetStringAttribute("alphaUserId");
			situation.AlphaUserRevision = node.GetStringAttribute("alphaUserRevision");
			situation.BetaUserId = node.GetStringAttribute("betaUserId");
			situation.BetaUserRevision = node.GetStringAttribute("betaUserRevision");
			return situation;
		}
	}
}