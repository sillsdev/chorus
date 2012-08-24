using Chorus.FileTypeHanders.lift;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.FileHandlers.Lift
{
	class LiftSynchronizerAdjunctTests
	{
		private const string testLiftFile =
			@"<?xml version='1.0' encoding='UTF-8'?>
				<lift version='0.13'>
			</lift>";

		[Test]
		public void LiftSynchronizerReadsLiftVersionCorrectly()
		{
			// Setup
			using (var myfile = new TempFile(testLiftFile))
			{
				// SUT
				var syncAdjunct = new LiftSynchronizerAdjunct(myfile.Path);

				// Verification
				Assert.AreEqual("LIFT0.13", syncAdjunct.BranchName, "BranchName should be 'LIFT0.13'");
			}
		}
	}
}
