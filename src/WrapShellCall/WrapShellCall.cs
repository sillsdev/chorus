using System;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;


namespace WrapShellCall
{
	class Program
	{
		//runhg pathWriteStandardOutputTo pathWriteStandardErrorTo thingToCall arguments

		static int Main(string[] args)
		{
			Process p = new Process();
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = args[2];
			string arguments="";
			for (int i = 3; i < args.Length; i++)
			{
				//even if they came to us with outer quotes,
				//those have been stripped off by now, so put them back
				if(args[i].Contains(" "))
				{
					arguments += "\""+args[i] + "\" ";
				}
				else
				{
					arguments += args[i] + " ";
				}
			}
			p.StartInfo.Arguments = arguments;

			//for debugging
			//File.WriteAllText("e:/temp/actualArguments.txt", p.StartInfo.Arguments);

			p.Start();
			ProcessStream processStream = new ProcessStream();
			processStream.Read(ref p);
			p.WaitForExit();

			Console.WriteLine(processStream.StandardOutput);
			Console.WriteLine(processStream.StandardError);
			File.WriteAllText(args[0], processStream.StandardOutput);
			File.WriteAllText(args[1], processStream.StandardError);
			return p.ExitCode;
		}


	}
}