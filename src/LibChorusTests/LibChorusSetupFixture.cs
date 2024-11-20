using L10NSharp;
using NUnit.Framework;

namespace LibChorus.Tests
{
	[SetUpFixture]
	public class LibChorusSetupFixture
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			LocalizationManager.StrictInitializationMode = false;
		}
	}
}