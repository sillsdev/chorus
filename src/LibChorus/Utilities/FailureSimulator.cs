using System;
using System.IO;

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

			//using files because even environment variables can't be propogated up
			//from this child process to the caller
			if(File.Exists(GetTriggerIndicatorPath(desiredFailureLocation)))
				File.Delete(GetTriggerIndicatorPath(desiredFailureLocation));

			//nb: can't just use a static, because this is potentially communicating across processes
			Environment.SetEnvironmentVariable(InducechorusFailureTriggered, string.Empty);
		}

		public static void IfTestRequestsItThrowNow(string name)
		{
			string s = System.Environment.GetEnvironmentVariable(Inducechorusfailure);
			if (s != null && s == name)
			{
				var triggerStream = File.Create(GetTriggerIndicatorPath(name));
				triggerStream.Close(); // Don't leave it open, or the test runner keeps hold of it, and it can't be deleted.
				Environment.SetEnvironmentVariable(InducechorusFailureTriggered, name);
				throw new Exception("Exception Induced By InduceChorusFailure Environment Variable");
			}
		}

		private static string GetTriggerIndicatorPath(string name)
		{
			return Path.Combine(Path.GetTempPath(), name);
		}

		public  override void Dispose()
		{
			base.Dispose();

			var tempPathname = GetTriggerIndicatorPath(_desiredFailureLocation);
			if (File.Exists(tempPathname))
			{
				File.Delete(tempPathname);
			}
			else
			{
				throw new ApplicationException("FailureSimulator was not tiggered: " + _desiredFailureLocation);
			}
		}
	}
}
