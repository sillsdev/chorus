using System;
using System.Collections.Generic;
using System.Text;

namespace Chorus.Utilities
{
	/// <summary>
	/// Usage:  using(new FailureSimulator("myMethod")) {.....}
	/// </summary>
	public class FailureSimulator : ShortTermEnvironmentalVariable
	{
		public FailureSimulator(string desiredFailureLocation)
			: base("InduceChorusFailure", desiredFailureLocation)
		{
		}

		public static void ThrowIfTestRequestsItThrowNow(string name)
		{
#if DEBUG
			string s = System.Environment.GetEnvironmentVariable("InduceChorusFailure");
			if (s != null && s == name)
			{
				throw new Exception("Exception Induced By InduceChorusFailure Environment Variable");
			}
#endif
		}
	}
}
