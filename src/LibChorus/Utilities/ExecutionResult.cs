using System.Diagnostics;

namespace Chorus.Utilities
{
	public class ExecutionResult
	{
		public int ExitCode;
		public string StandardError;
		public string StandardOutput;
		public bool DidTimeOut { get { return ExitCode == ProcessOutputReader.kTimedOut; } }
		public bool UserCancelled { get { return ExitCode == ProcessOutputReader.kCancelled; } }

		public ExecutionResult()
		{

		}
		public ExecutionResult(Process proc)
		{
			ProcessStream ps = new ProcessStream();
			ps.Read(ref proc, 30);
			StandardOutput = ps.StandardOutput;
			StandardError = ps.StandardError;
			ExitCode = proc.ExitCode;
		}
	}
}