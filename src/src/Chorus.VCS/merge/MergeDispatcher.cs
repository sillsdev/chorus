using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using Chorus.Utilities;
namespace Chorus.merge
{
	public class MergeDispatcher
	{
		static public int Go(MergeOrder order)
		{
			try
			{
				//Debug.Fail("ehlo");
				switch (Path.GetExtension(order.pathToOurs))
				{
					default:
						//todo: we don't know how to handle this file type, so pick one and report a conflict
						Console.Error.WriteLine("ChorusMerge doesn't know how to merge files of type" + Path.GetExtension(order.pathToOurs));
						return -1;
						break;
					case ".lift":
						return MergeLiftFiles(order);
						break;
					case ".txt":
						return MergeTextFiles(order);
						break;
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("ChorusMerge Error: " + e.Message);
				Console.Error.WriteLine(e.StackTrace);
				return -1;
			}
		}

		private static int MergeLiftFiles(MergeOrder order)
		{
		  //;  Debug.Fail("hello");
			Chorus.merge.xml.lift.LiftMerger merger;
			switch (order.conflictHandlingMode)
			{
				default:
					throw new ArgumentException("The Lift merger cannot handle the requested conflict handling mode");
					break;
				case MergeOrder.ConflictHandlingMode.WeWin:

					   merger= new LiftMerger(new DropTheirsMergeStrategy(), order.pathToOurs, order.pathToTheirs, order.pathToCommonAncestor);
					break;
				case MergeOrder.ConflictHandlingMode.TheyWin:
					merger = new LiftMerger(new DropTheirsMergeStrategy(), order.pathToTheirs, order.pathToOurs, order.pathToCommonAncestor);
					break;
			}

			string newContents = merger.GetMergedLift();
			File.WriteAllText(order.pathToOurs, newContents);
			return 0;
		}

		private static int MergeTextFiles(MergeOrder order)
		{
			using (TempFile lcd = new TempFile())//this one gets used, not left for the caller
			{
				TempFile ourPartial = new TempFile();
				TempFile theirPartial = new TempFile();
				// Debug.Fail("hi");

				//Debug.Fail("(Not really a failure) chorus merge : "+pathToOurs);
				int code = ChorusMerge.TextMerger.Merge(order.pathToCommonAncestor, order.pathToOurs, order.pathToTheirs, lcd.Path, ourPartial.Path,
													   theirPartial.Path);
				if (code == 0)
				{
					// insert a single comma-delimited line
					//listing {user's path, path to ourPartial, paht to theirPartial}
					StreamWriter f = File.AppendText(Path.Combine(Path.GetTempPath(), "chorusMergePaths.txt"));
					f.Write(order.pathToOurs);
					f.Write("," + ourPartial.Path);
					f.WriteLine("," + theirPartial.Path);
					f.Close();
					f.Dispose();

					switch (order.conflictHandlingMode)
					{
						case MergeOrder.ConflictHandlingMode.WeWin:
							File.Copy(ourPartial.Path, order.pathToOurs, true);
							ourPartial.Dispose();
							theirPartial.Dispose();
							break;
						case MergeOrder.ConflictHandlingMode.TheyWin:
							File.Copy(theirPartial.Path, order.pathToOurs, true);
							ourPartial.Dispose();
							theirPartial.Dispose();
							break;
						case MergeOrder.ConflictHandlingMode.LcdPlusPartials:
							//Make the result of the merge be the LCD (It's critical that the calling process
							//will be following this up RIGHT AWAY by appending the partials to their respective
							//branches! Otherwise conflicting changes will be lost to both parties.
							File.Copy(lcd.Path, order.pathToOurs, true);
							//leave the other two temp files for the caller to work with and delete
							break;
						default:
							throw new ArgumentException(
								"The text merge dispatcher does not understand this conflictHandlingMode");
					}
				}
				return code;
			}
		}

		public class MergeOrder
		{
			public const string kConflictHandlingModeEnvVarName = "ChorusConflictHandlingMode";
			public string pathToOurs;
			public string pathToCommonAncestor;
			public string pathToTheirs;

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
			public enum ConflictHandlingMode { WeWin, TheyWin, LcdPlusPartials }

			public ConflictHandlingMode conflictHandlingMode;

			public MergeOrder(string ours, string common, string theirs)
			{
				pathToOurs = ours;
				pathToTheirs = theirs;
				pathToCommonAncestor = common;

				conflictHandlingMode = (ConflictHandlingMode)Enum.Parse(typeof(ConflictHandlingMode),
					Environment.GetEnvironmentVariable(kConflictHandlingModeEnvVarName));
			}
		}
	}
}
