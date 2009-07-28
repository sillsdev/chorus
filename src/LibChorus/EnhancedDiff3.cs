using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus.VCS.Utilities;

namespace Chorus.VCS
{
	public class EnhancedDiff3
	{
		public static string GetVersion()
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "diff3";
			p.StartInfo.Arguments = "--version";
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			p.WaitForExit();
			ExecutionResult result = new ExecutionResult(p);
			Debug.WriteLine(result.StandardOutput);
			return result.StandardOutput;
		}

		private enum State
		{
			unknown,
			ours,
			common,
			theirs,
			normal
		}

		public static void Merge(string common, string ours, string theirs,
			string lcdOutputPath, string ourPartialOutputPath, string theirPartialOutputPath)
		{
			string merge = GetRawMerge(ours, common, theirs);

			StreamWriter lcd=null;
			StreamWriter ourPartial=null;
			StreamWriter theirPartial=null;

			try
			{
				lcd = File.CreateText(lcdOutputPath);
				ourPartial = File.CreateText(ourPartialOutputPath);
				theirPartial = File.CreateText(theirPartialOutputPath);

				ReadLines(merge, ourPartial, lcd, theirPartial);
			}
			finally
			{
				if (ourPartial != null)
				{
					ourPartial.Close();
					ourPartial.Dispose();
				}
				if (theirPartial != null)
				{
					theirPartial.Close();
					theirPartial.Dispose();
				}
				if (lcd != null)
				{
					lcd.Close();
					lcd.Dispose();
				}
			}
		}

		private static void ReadLines(string merge, StreamWriter ourPartial, StreamWriter lcd, StreamWriter theirPartial)
		{
			StringReader r = new StringReader(merge);
			string line = null;
			State state = State.normal;
			do
			{
				line = r.ReadLine();
				if (line == null)
				{
					break;
				}
				if (line.StartsWith("<<<<<<<"))
				{
					state = State.ours;
					continue;
				}
				else if (line.StartsWith("||||||"))
				{
					state = State.common;
					continue;
				}
				else if (line.StartsWith("======="))
				{
					state = State.theirs;
					continue;
				}
				else if (line.StartsWith(">>>>>>>"))
				{
					state = State.normal;
					continue;
				}

				switch (state)
				{
					case State.ours:
						ourPartial.WriteLine(line);
						break;
					case State.common:
						lcd.WriteLine(line);
						break;
					case State.theirs:
						theirPartial.WriteLine(line);
						break;
					default:
						ourPartial.WriteLine(line);
						lcd.WriteLine(line);
						theirPartial.WriteLine(line);
						break;
				}
			} while (line != null);
		}

		public static string GetRawMerge(string oursPath, string commonPath, string theirPath)
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "diff3/bin/diff3.exe";
			p.StartInfo.Arguments = "-m " + oursPath + " "+commonPath+" "+theirPath;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			p.WaitForExit();
			ExecutionResult result = new ExecutionResult(p);
			if (result.ExitCode == 2)//0 and 1 are ok
			{
				throw new ApplicationException("Got error "+result.ExitCode + " " +result.StandardOutput +" "+ result.StandardError);
			}
			Debug.WriteLine(result.StandardOutput);
			return result.StandardOutput;
		}
	}
}
