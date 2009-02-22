using System;
using System.Diagnostics;
using System.IO;
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
				return MergeDispatcher.Go(order);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("ChorusMerge Error: "+e.Message);
				Console.Error.WriteLine(e.StackTrace);
				return 1;
			}
		}
	}
}