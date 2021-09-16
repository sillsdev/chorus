using System.Collections.Generic;
using System.Diagnostics;

namespace LibChorus.TestUtilities
{
	public class SystemAssertListener : TraceListener
	{
		public List<string> Messages { get; } = new List<string>();

		public override void Write(string message)
		{
			Messages.Add(message);
		}

		public override void WriteLine(string message)
		{
			Messages.Add(message);
		}
	}
}
