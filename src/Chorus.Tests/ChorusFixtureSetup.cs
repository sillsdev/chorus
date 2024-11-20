using L10NSharp;
using NUnit.Framework;

namespace Chorus.Tests
{
   internal class ChorusFixtureSetup
   {
		[SetUpFixture]
		public class SetupFixture
		{
			[OneTimeSetUp]
			public void RunBeforeAnyTests()
			{
				LocalizationManager.StrictInitializationMode = false;
			}
		}
   }
}
