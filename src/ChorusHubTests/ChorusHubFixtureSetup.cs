using L10NSharp;
using NUnit.Framework;

namespace ChorusHubTests
{
	[SetUpFixture]
	public class ChorusHubFixtureSetup
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			LocalizationManager.StrictInitializationMode = false;
		}
	}
}
