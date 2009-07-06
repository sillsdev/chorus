using System;
using System.IO;
using Chorus.merge.xml.generic;

namespace Chorus.merge
{
	/// <summary>
	/// The instructions Chorus.exe was given: what files to merge, any parameters on how to do it.
	/// </summary>
	public class MergeOrder
	{
		public const string kConflictHandlingModeEnvVarName = "ChorusConflictHandlingMode";
		private const string kPathToRepository = "ChorusPathToRepository";
		public string pathToOurs;
		public string pathToCommonAncestor;
		public string pathToTheirs;
		public MergeSituation MergeSituation { get; set; }

		/// <summary>
		/// If the LcdPlusPartials is specified, the merger must
		/// produce 3 files:  LeastCommonDenominator,
		/// OurPartial, and TheirPartial files.
		///
		/// The LCD one is returned as the result of the merge, the paths to all three
		/// are appended to a special file that the Chorus syncing method can later read.
		/// It is then the Chorus syncing methods job to take the two partials and insert
		/// them into the repository history.
		/// </summary>
		public enum ConflictHandlingModeChoices { WeWin, TheyWin, LcdPlusPartials,
			DifferenceOnly
		}

		public ConflictHandlingModeChoices ConflictHandlingMode{ get; set;}
		public IMergeEventListener EventListener{ get; set;}


		public MergeOrder(ConflictHandlingModeChoices mode, string pathToOurs, string pathToCommon, string pathToTheirs,
			MergeSituation situation)
		{
			this.pathToOurs = pathToOurs;
			this.pathToTheirs = pathToTheirs;
			MergeSituation = situation;
			pathToCommonAncestor = pathToCommon;

			ConflictHandlingMode = mode;
		}

		public static MergeOrder CreateUsingEnvironmentVariables(string pathToOurs, string pathToCommon,  string pathToTheirs )
		{
			MergeOrder.ConflictHandlingModeChoices mode = MergeOrder.ConflictHandlingModeChoices.WeWin;

			//we have to get this argument out of the environment variables because we have not control of the arguments
			//the dvcs system is going to use to call us. So whoever invokes the dvcs needs to set this variable ahead of time
			string modeString = Environment.GetEnvironmentVariable(MergeOrder.kConflictHandlingModeEnvVarName);
			if (!string.IsNullOrEmpty(modeString))
			{

				mode =
					(MergeOrder.ConflictHandlingModeChoices)
					Enum.Parse(typeof(MergeOrder.ConflictHandlingModeChoices), modeString);
			}
			string pathToRepository = Environment.GetEnvironmentVariable(MergeOrder.kPathToRepository);

			string pathInRepository = pathToOurs.Replace(pathToRepository, "");//REVIEW

			return new MergeOrder(mode, pathToOurs,pathToCommon, pathToTheirs , MergeSituation.CreateFromEnvironmentVariables(pathInRepository));
		}

		public static void PushToEnvironmentVariables(string pathToRepository)
		{
			Environment.SetEnvironmentVariable(kPathToRepository, pathToRepository);
		}

		/// <summary>
		/// Factory used when we don't actually want to merge, we just want to diff between a parent and child.
		/// The diff is accessed by setting up the listener and, well, listening to the diffs.
		/// </summary>
		public static MergeOrder CreateForDiff(string parentPath, string childPath, IMergeEventListener listener)
		{
			return new MergeOrder(ConflictHandlingModeChoices.DifferenceOnly, childPath, parentPath, parentPath, new NullMergeSituation())
						{EventListener = listener};
		}
	}
}