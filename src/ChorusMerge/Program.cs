using System;
using System.Diagnostics;
using System.IO;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;

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
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				//this was originally put here to test if console writes were making it out to the linux log or not
				Console.WriteLine("ChorusMerge({0}, {1}, {2}", args[0], args[1], args[2]);

				// Debug.Fail("hello");
				MergeOrder order = MergeOrder.CreateUsingEnvironmentVariables(args[0], args[1], args[2]);
				var handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers();
				var handler = handlers.GetHandlerForMerging(order.pathToOurs);

				//DispatchingMergeEventListener listenerDispatcher = new DispatchingMergeEventListener();
				//using (HumanLogMergeEventListener humanListener = new HumanLogMergeEventListener(order.pathToOurs + ".ChorusML.txt"))
				using (ChorusMLMergeEventListener xmlListener = new ChorusMLMergeEventListener(order.pathToOurs + ".ChorusML"))
				{
//                    listenerDispatcher.AddEventListener(humanListener);
//                    listenerDispatcher.AddEventListener(xmlListener);
					order.EventListener = xmlListener;

					handler.Do3WayMerge(order);
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("ChorusMerge Error: "+e.Message);
				Console.Error.WriteLine(e.StackTrace);
				return 1;
			}
			return 0;//no error
		}
	}
}