using System;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;

namespace Chorus.merge.text
{
	/// <summary>
	/// Do a 3-way Merge of a text file, producing least-common-denominator and 2 partial merge files
	/// </summary>
	public class PartialTextMerger
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
			File.WriteAllText(Path.Combine(Path.GetTempPath(), "ChorusDiff3Raw.txt"), merge);
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
			// Debug.Fail("break");
			StringReader r = new StringReader(merge);
			string line = null;
			State state = State.normal;
			string useNextTimeAround = null;
			do
			{
				if (useNextTimeAround != null)
				{
					line = useNextTimeAround;
					useNextTimeAround = null;
				}
				else
				{
					line = r.ReadLine();

					if (line == null)
					{
						break;
					}
					//this is to hand a case I ran into where the file didn't end in a carriage return,
					//so we had diffe outputing:
					//  This is the end of the test file=======
					//instead of the expected
					//  This is the end of the test file
					//  =======
					int startOfSep = line.IndexOf("=======");
					if(startOfSep > 0)
					{
						line = line.Substring(0, startOfSep);
						useNextTimeAround = "=======";
					}
					startOfSep = line.IndexOf(">>>>>>>");
					if (startOfSep > 0)
					{
						line = line.Substring(0, startOfSep);
						useNextTimeAround = ">>>>>>>";
					}
					startOfSep = line.IndexOf("|||||||");
					if (startOfSep > 0)
					{
						line = line.Substring(0, startOfSep);
						useNextTimeAround = "|||||||";
					}
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
						ourPartial.Write(line);
						if (r.Peek() >-1)
							ourPartial.WriteLine();
						break;
					case State.common:
						lcd.Write(line);
						if (r.Peek() > -1)
							lcd.WriteLine();
						break;
					case State.theirs:
						theirPartial.Write(line);
						if (r.Peek() > -1)
							theirPartial.WriteLine();
						break;
					default:
						ourPartial.Write(line);
						lcd.Write(line);
						theirPartial.Write(line);
						if (r.Peek() > -1)
						{
							lcd.WriteLine();
							theirPartial.WriteLine();
							ourPartial.WriteLine();
						}
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

			//NB: there is a post-build step in this project which copies the diff3 folder into the output
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