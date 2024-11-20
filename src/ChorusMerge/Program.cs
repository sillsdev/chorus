//#define RUNINDEBUGGER
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using L10NSharp;

// Allow redirecting Console.Error for unit tests (Avoid spurious build failures)
[assembly: InternalsVisibleTo("ChorusMerge.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001005f4452c387d979e3cba05fd73bb9aebe8f8830874663d66a7869f614a8f5e8def658d5c5920fae609d28aa005d5a9af5bd758ca8f19ad0347b7aa76e1f723f8994792136f5ceff9fb6f719d4337f65da2e1d66a85cc5e28e4656a1a30c2ff513440393177625c725d3fb156dc3c11610ea5936b9404ab9d51f7eb71ac0aa27bd")]

namespace ChorusMerge
{
	/// <summary>
	/// arguments are: {pathToOurs,  pathToCommon,   pathToTheirs}
	/// This is used as the starting point for all merging using Chorus.
	/// It will dispatch to file-format-specific mergers.  Note that
	/// we can't control the argument list or get more arguments, so
	/// anything beyond the 3 files must be specified in environment variables.
	/// See MergeOrder and MergeSituation for a description of those variables and their possible values.
	/// </summary>
	/// <remarks>
	/// The arguments used to be presented in utf-8 encoding presented via CP1252, but Mercurial 6.5.1 and
	/// Python 3 have made that unnecessary. Unicode arguments are now passed correctly without needing
	/// to play games with encoding.
	/// </remarks>
	public class Program
	{
		internal static TextWriter ErrorWriter = Console.Error;

		public static int Main(string[] args)
		{
			try
			{
				LocalizationManager.StrictInitializationMode = false;
				string ourFilePath = args[0];
				string commonFilePath = args[1];
				string theirFilePath = args[2];

				//this was originally put here to test if console writes were making it out to the linux log or not
				Console.WriteLine("ChorusMerge: {0}, {1}, {2}", ourFilePath, commonFilePath, theirFilePath);

#if RUNINDEBUGGER
				var order = new MergeOrder(ourFilePath, commonFilePath, theirFilePath, new MergeSituation(ourFilePath, "Me", "CHANGETHIS", "YOU", "CHANGETHIS", MergeOrder.ConflictHandlingModeChoices.WeWin));
#else
				MergeOrder order = MergeOrder.CreateUsingEnvironmentVariables(ourFilePath, commonFilePath, theirFilePath);
#endif
				var handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers();
				var handler = handlers.GetHandlerForMerging(order.pathToOurs);

				//DispatchingMergeEventListener listenerDispatcher = new DispatchingMergeEventListener();
				//using (HumanLogMergeEventListener humanListener = new HumanLogMergeEventListener(order.pathToOurs + ".ChorusNotes.txt"))
				using (var xmlListener = new ChorusNotesMergeEventListener(order.pathToOurs + ".NewChorusNotes"))
				{
//                    listenerDispatcher.AddEventListener(humanListener);
//                    listenerDispatcher.AddEventListener(xmlListener);
					order.EventListener = xmlListener;

					handler.Do3WayMerge(order);
				}
			}
			catch (Exception e)
			{
				ErrorWriter.WriteLine("ChorusMerge Error: " + e.Message);
				ErrorWriter.WriteLine(e.StackTrace);
				return 1;
			}
			return 0;//no error
		}
	}
}