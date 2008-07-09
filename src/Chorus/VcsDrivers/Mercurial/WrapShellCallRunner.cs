using System.Diagnostics;
using System.IO;
using Chorus.Utilities;

namespace Chorus.VcsDrivers.Mercurial
{
	public class WrapShellCallRunner
	{
		public static ExecutionResult Run(string commandLine)
		{
			return Run(commandLine, null);
		}

		public static ExecutionResult Run(string commandLine, string fromDirectory)
		{
			ExecutionResult result = new ExecutionResult();
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.WorkingDirectory = fromDirectory;
			p.StartInfo.FileName = "WrapShellCall";
			using (TempFile tempOut = new TempFile())
			{
				using (TempFile tempErr = new TempFile())
				{
					p.StartInfo.Arguments = "\""+tempOut.Path +"\" " + "\""+tempErr.Path + "\" " + commandLine;
					p.StartInfo.CreateNoWindow = true;
					p.Start();
					p.WaitForExit();

					result.StandardOutput = File.ReadAllText(tempOut.Path);
					result.StandardError = File.ReadAllText(tempErr.Path);
					result.ExitCode = p.ExitCode;
				}
			}
			return result;
		}
	}
}