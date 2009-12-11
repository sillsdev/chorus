using System;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge
{
	/// <summary>
	/// for unit tests that don't need this
	/// </summary>
	public class NullMergeSituation: MergeSituation
	{
		public NullMergeSituation()
			: base(null, null, null, null, null,MergeOrder.ConflictHandlingModeChoices.WeWin)
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
		public const string kUseAlphaId= "ChorusUserAlphaId";
		public const string kUserBetaId = "ChorusUserBetaId";
		public const string kUserAlphaRevision = "ChorusUserAlphaRevision";
		public const string kUserBetaRevision = "ChorusUserBetaRevision";
		//public const string kPathToFileInRepository = "ChorusPathToFileInRepository";

		/// <summary>
		/// A relative path
		/// </summary>
		public string PathToFileInRepository{ get; set;}
	   //we don't have access to this yet: public string AncestorRevision { get; set; }
		public string UserAlphaId { get; set; }
		public string UserBetaId { get; set; }
		public string UserAlphaRevision { get; set; }
		public string UserBetaRevision { get; set; }


		public void WriteAsXml(XmlWriter writer)
		{
			writer.WriteStartElement("MergeSituation");
			writer.WriteAttributeString("userAlphaId", UserAlphaId);
			writer.WriteAttributeString("userBetaId", UserBetaId);
			writer.WriteAttributeString("userXRevision", UserAlphaRevision);
			writer.WriteAttributeString("userYRevision", UserBetaRevision);
			writer.WriteAttributeString("path", PathToFileInRepository);
			writer.WriteAttributeString("conflictHandlingMode", string.Empty, ConflictHandlingMode.ToString());
			writer.WriteEndElement();
		}

		public MergeSituation(string relativePathToFile, string firstUserId, string firstUserRevision, string secondUserId, string secondRevision, MergeOrder.ConflictHandlingModeChoices conflictHandlingMode)
		{
			ConflictHandlingMode = conflictHandlingMode;

			if (relativePathToFile != null)
				relativePathToFile = relativePathToFile.Trim(new[] {Path.DirectorySeparatorChar});

			PathToFileInRepository = relativePathToFile;

			switch (conflictHandlingMode)
			{
				case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					UserAlphaId = secondUserId;
					UserBetaId = firstUserId;
					UserBetaRevision = firstUserRevision;
					UserAlphaRevision = secondRevision;
					break;
				default:
					UserAlphaId = firstUserId;
					UserBetaId = secondUserId;
					UserBetaRevision = secondRevision;
					UserAlphaRevision = firstUserRevision;
					break;
			}


			//we don't have access to this yet:   AncestorRevision = ancestorRevision;
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
			string userAlphaId = Environment.GetEnvironmentVariable(MergeSituation.kUseAlphaId);
			string userBetaId = Environment.GetEnvironmentVariable(MergeSituation.kUserBetaId);
			string userXRevision = Environment.GetEnvironmentVariable(MergeSituation.kUserAlphaRevision);
			string userYRevision = Environment.GetEnvironmentVariable(MergeSituation.kUserBetaRevision);

			return new MergeSituation( pathToFileInRepository, userAlphaId, userXRevision, userBetaId,
									  userYRevision, mode);

		}

		/// <summary>
		/// this isn't all we want to know, but it's all the guy telling hg to merge two branches knows.
		/// This is called to put this info into env vars where we can retrieve it once chorus.exe is called by hg
		/// </summary>
		/// <param name="userXRevision"></param>
		/// <param name="userYRevision"></param>
		public static void PushRevisionsToEnvironmentVariables(string userAlphaId, string userXRevision, string userBetaId, string userYRevision)
		{
			Environment.SetEnvironmentVariable(kUseAlphaId, userAlphaId);
			Environment.SetEnvironmentVariable(kUserAlphaRevision, userXRevision);

			Environment.SetEnvironmentVariable(kUserBetaId, userBetaId);
			Environment.SetEnvironmentVariable(kUserBetaRevision, userYRevision);
		}
//
//        public void WriteXml(XmlWriter writer)
//        {
//            writer.WriteStartElement("MergeSituation");
//            writer.WriteAttributeString("PathToFileInRepository",this.PathToFileInRepository);
//            writer.WriteEndElement();
//        }


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

			return new MergeSituation(node.GetStringAttribute("path"),
									  node.GetStringAttribute("userAlphaId"),
									  node.GetStringAttribute("userXRevision"),
									  node.GetStringAttribute("userBetaId"),
									  node.GetStringAttribute("userYRevision"),
									  mode);
		}
	}
}