//#define RUNINDEBUGGER
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;

// Allow redirecting Console.Error for unit tests (Avoid spurious build failures)
[assembly: InternalsVisibleTo("ChorusMerge.Tests")]

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
	/// The arguments are presumed to be presented in utf-8 encoding presented via CP1252. This is a departure
	/// from the norm on Windows of UCS2. However, python has issues in calling out to processes using UCS2
	/// so gives utf8, which is then mangled via CP1252.  This can all be decoded to give ChorusMerge the
	/// ucs2 args it expects.
	/// </remarks>
	public class Program
	{
		internal static TextWriter ErrorWriter = Console.Error;

		public static int Main(string[] args)
		{
			try
			{
#if MONO
				var ourFilePath = args[0];
				var commonFilePath = args[1];
				var theirFilePath = args[2];
#else
				// Convert the input arguments from cp1252 -> utf8 -> ucs2
				// It always seems to be 1252, even when the input code page is actually something else. CP 2012-03
				// var inputEncoding = Console.InputEncoding;
				var inputEncoding = Encoding.GetEncoding(1252);
				var ourFilePath = Encoding.UTF8.GetString(inputEncoding.GetBytes(args[0]));
				var commonFilePath = Encoding.UTF8.GetString(inputEncoding.GetBytes(args[1]));
				var theirFilePath = Encoding.UTF8.GetString(inputEncoding.GetBytes(args[2]));
				Console.WriteLine("ChorusMerge: Input encoding {0}", inputEncoding.EncodingName);
#endif

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