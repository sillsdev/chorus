using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chorus.Utilities
{
	/// <summary>
	/// Usage:  using(new FailureSimulator("myMethod")) {.....}
	/// </summary>
	public class FailureSimulator : ShortTermEnvironmentalVariable
	{
		public const string Inducechorusfailure = "InduceChorusFailure";
		public const string InducechorusFailureTriggered = "InduceChorusFailure-Triggered";
		private string _desiredFailureLocation;

		public FailureSimulator(string desiredFailureLocation)
			: base(Inducechorusfailure, desiredFailureLocation)
		{
			_desiredFailureLocation = desiredFailureLocation;

			//using files becuase even environment variables can't be propogated up
			//from this child process to the caller
			if(File.Exists(GetTriggerIndicatorPath(desiredFailureLocation)))
				File.Delete(GetTriggerIndicatorPath(desiredFailureLocation));

			//nb: can't just us a static, because this is potentially communicating across processes
			Environment.SetEnvironmentVariable(InducechorusFailureTriggered, string.Empty);
		}

		public static void ThrowIfTestRequestsItThrowNow(string name)
		{
#if DEBUG
			string s = System.Environment.GetEnvironmentVariable(Inducechorusfailure);
			if (s != null && s == name)
			{
				File.Create(GetTriggerIndicatorPath(name));
				Environment.SetEnvironmentVariable(InducechorusFailureTriggered, name);
				throw new Exception("Exception Induced By InduceChorusFailure Environment Variable");
			}
#endif
		}

		private static string GetTriggerIndicatorPath(string name)
		{
			return Path.Combine(Path.GetTempPath(), name);
		}

		public  override void Dispose()
		{
			base.Dispose();

			if (!File.Exists(GetTriggerIndicatorPath(_desiredFailureLocation)))
			{
				throw new ApplicationException("FailureSimulator was not tiggered: "+ _desiredFailureLocation );
			}
			File.Delete(GetTriggerIndicatorPath(_desiredFailureLocation));
		}
	}
}
