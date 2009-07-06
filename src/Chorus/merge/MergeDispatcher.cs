using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using Chorus.Utilities;
namespace Chorus.merge
{
	/// <summary>
	/// Does a merge of the files after figuring out the correct class or program to do it.
	/// </summary>
	public class MergeDispatcher
	{
		static public int Go(MergeOrder order)
		{
			//Debug.Fail("Use this break to attach to ChorusMerge.exe so you can step into this code, which is called by Hg");
			try
			{
				switch (Path.GetExtension(order.pathToOurs))
				{
					default:
						//todo: we don't know how to handle this file type, so pick one and report a conflict
						Console.Error.WriteLine("ChorusMerge doesn't know how to merge files of type" + Path.GetExtension(order.pathToOurs));
						return 1; //DON'T USE -1! HG SWALLOWS IT
					case ".lift":
						//TODO: this is a hack
						if (order.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.DifferenceOnly)
						{
							DiffLiftFiles(order);
							return 0;//review
						}
						else
						{
							return MergeLiftFiles(order);
						}
						break;
					case ".conflicts":
						return MergeConflictFiles(order);
					case ".txt":
						return MergeTextFiles(order);
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("ChorusMerge Error: " + e.Message);
				Console.Error.WriteLine(e.StackTrace);
				return 1;
			}
		}

		private static void DiffLiftFiles(MergeOrder order)
		{
			var merger = new LiftMerger(new LiftEntryMergingStrategy(order.MergeSituation), order.pathToOurs, order.pathToTheirs,
						order.pathToCommonAncestor);
			merger.EventListener = order.EventListener;
			merger.Do2WayDiffOfLift();
		}

		private static int MergeConflictFiles(MergeOrder order)
		{
			XmlMerger merger  = new XmlMerger(order.MergeSituation);
			try
			{
				NodeMergeResult r = merger.MergeFiles(order.pathToOurs, order.pathToTheirs, order.pathToCommonAncestor);
				File.WriteAllText(order.pathToOurs, r.MergedNode.OuterXml);
			}
			catch (Exception error)
			{
				return 1;
			}
			return 0;
		}

		private static int MergeLiftFiles(MergeOrder order)
		{
			DispatchingMergeEventListener listenerDispatcher = new DispatchingMergeEventListener();

			//Debug.Fail("hello");
			//review: where should these really go?
			string dir = Path.GetDirectoryName(order.pathToOurs);
			using(HumanLogMergeEventListener humanListener = new HumanLogMergeEventListener(order.pathToOurs+".conflicts.txt"))
			using (XmlLogMergeEventListener xmlListener = new XmlLogMergeEventListener(order.pathToOurs+".conflicts"))
			{
				listenerDispatcher.AddEventListener(humanListener);
				listenerDispatcher.AddEventListener(xmlListener);
				order.EventListener = listenerDispatcher;

				//;  Debug.Fail("hello");
				Chorus.merge.xml.lift.LiftMerger merger;
				switch (order.ConflictHandlingMode)
				{
					default:
						throw new ArgumentException("The Lift merger cannot handle the requested conflict handling mode");
					case MergeOrder.ConflictHandlingModeChoices.WeWin:

						merger = new LiftMerger(new LiftEntryMergingStrategy(order.MergeSituation), order.pathToOurs, order.pathToTheirs,
												order.pathToCommonAncestor);
						break;
					case MergeOrder.ConflictHandlingModeChoices.TheyWin:
						merger = new LiftMerger(new LiftEntryMergingStrategy(order.MergeSituation), order.pathToTheirs, order.pathToOurs,
												order.pathToCommonAncestor);
						break;
				}
				merger.EventListener = order.EventListener;

				string newContents = merger.GetMergedLift();
				File.WriteAllText(order.pathToOurs, newContents);
				return 0;
			}
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

					switch (order.ConflictHandlingMode)
					{
						case MergeOrder.ConflictHandlingModeChoices.WeWin:
							File.Copy(ourPartial.Path, order.pathToOurs, true);
							ourPartial.Dispose();
							theirPartial.Dispose();
							break;
						case MergeOrder.ConflictHandlingModeChoices.TheyWin:
							File.Copy(theirPartial.Path, order.pathToOurs, true);
							ourPartial.Dispose();
							theirPartial.Dispose();
							break;
						case MergeOrder.ConflictHandlingModeChoices.LcdPlusPartials:
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
	}
}
