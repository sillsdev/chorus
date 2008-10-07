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
	/// See MergeOrder for a description of those variables and their possible.
	/// </summary>
	class Program
	{
		static int Main(string[] args)
		{
			try
			{

				//Debug.Fail("attach now");

//                string path = Path.Combine(System.Environment.GetEnvironmentVariable("temp"),
//               @"chorusMergeArgs.txt");
//                string argcontents = "";
//                foreach (string s in args)
//                {
//                    argcontents += s+", ";
//                }
//                File.AppendAllText(path, argcontents);


				 MergeOrder.ConflictHandlingMode mode = MergeOrder.ConflictHandlingMode.WeWin;

				//we have to get this argument out of the environment variables because we have not control of the arguments
				//the dvcs system is going to use to call us. So whoever invokes the dvcs needs to set this variable ahead of time
				string modeString = Environment.GetEnvironmentVariable(MergeOrder.kConflictHandlingModeEnvVarName);
				if (!string.IsNullOrEmpty(modeString))
				{

					mode =
						(MergeOrder.ConflictHandlingMode)
						Enum.Parse(typeof (MergeOrder.ConflictHandlingMode), modeString);
				}

				MergeOrder order = new MergeOrder(mode, args[0], args[1], args[2]);
				return MergeDispatcher.Go(order);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("ChorusMerge Error: "+e.Message);
				Console.Error.WriteLine(e.StackTrace);
				return -1;
			}
		}
	}
}