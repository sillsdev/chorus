using System.Diagnostics;

namespace Chorus.Utilities
{
	public class ExecutionResult
	{
		public int ExitCode;
		public string StandardError;
		public string StandardOutput;

		public ExecutionResult()
		{

		}
		public ExecutionResult(Process proc)
		{
			ProcessStream ps = new ProcessStream();
			ps.Read(ref proc);
			StandardOutput = ps.StandardOutput;
			StandardError = ps.StandardError;
			ExitCode = proc.ExitCode;
		}
	}
}