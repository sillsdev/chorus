using L10NSharp;
using NUnit.Framework;

namespace ChorusMerge.Tests
{
	[SetUpFixture]
	public class ChorusMergeFixtureSetup
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			LocalizationManager.StrictInitializationMode = false;
		}
	}
}
