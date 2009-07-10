using System;
using System.Diagnostics;
using System.IO;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace ChorusMerge
{
	/// <summary>
	/// This is used as the starting point for all merging using Chorus.
	/// It will dispatch to file-format-specific mergers.  Note that
	/// we can't control the argument list or get more arguments, so
	/// anything beyond the 3 files must be specified in environment variables.
	/// See MergeOrder and MergeSituation for a description of those variables and their possible values.
	/// </summary>
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				MergeOrder order = MergeOrder.CreateUsingEnvironmentVariables(args[0], args[1], args[2]);
				var handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers();
				var handler = handlers.GetHandlerForMerging(order.pathToOurs);
				if (handler is DefaultFileTypeHandler)
				{
					//todo: we don't know how to handle this file type, so pick one and report a conflict
					Console.Error.WriteLine("ChorusMerge doesn't know how to merge files of type" + Path.GetExtension(order.pathToOurs));
					return 1;
				}
				handler.Do3WayMerge(order);
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