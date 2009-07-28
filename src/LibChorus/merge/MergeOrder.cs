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
		public enum ConflictHandlingModeChoices { WeWin, TheyWin, LcdPlusPartials}

	  //  public ConflictHandlingModeChoices ConflictHandlingMode{ get; set;}
		public IMergeEventListener EventListener{ get; set;}


		public MergeOrder( string pathToOurs, string pathToCommon, string pathToTheirs,
			MergeSituation situation)
		{
			this.pathToOurs = pathToOurs;
			this.pathToTheirs = pathToTheirs;
			MergeSituation = situation;
			pathToCommonAncestor = pathToCommon;

		 //   ConflictHandlingMode = mode;
			EventListener = new NullMergeEventListener();//client can put something useful in if it needs one
		}

		public static MergeOrder CreateUsingEnvironmentVariables(string pathToOurs, string pathToCommon,  string pathToTheirs )
		{
			string pathToRepository = Environment.GetEnvironmentVariable(MergeOrder.kPathToRepository);

			string pathInRepository = pathToOurs.Replace(pathToRepository, "").Trim(new[]{Path.DirectorySeparatorChar});//REVIEW

			return new MergeOrder(pathToOurs,pathToCommon, pathToTheirs , MergeSituation.CreateFromEnvironmentVariables(pathInRepository));
		}

		public static void PushToEnvironmentVariables(string pathToRepository)
		{
			Environment.SetEnvironmentVariable(kPathToRepository, pathToRepository);
		}
	}
}