using Chorus.FileTypeHandlers.lift;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.FileHandlers.Lift
{
	[TestFixture]
	public class LiftSynchronizerAdjunctTests
	{
		private const string testLift13File =
			@"<?xml version='1.0' encoding='UTF-8'?>
				<lift version='0.13'>
			</lift>";

		[Test]
		public void LiftSynchronizerReadsLift13VersionCorrectly()
		{
			// Setup
			using (var myfile = new TempFile(testLift13File))
			{
				// SUT
				var syncAdjunct = new LiftSynchronizerAdjunct(myfile.Path);

				// Verification
				Assert.AreEqual("default", syncAdjunct.BranchName, "BranchName should be 'default' for LIFT0.13");
			}
		}

		private const string testLift15File =
			@"<?xml version='1.0' encoding='UTF-8'?>
				<lift version='0.15'>
			</lift>";

		[Test]
		public void LiftSynchronizerReadsLift15VersionCorrectly()
		{
			// Setup
			using (var myfile = new TempFile(testLift15File))
			{
				// SUT
				var syncAdjunct = new LiftSynchronizerAdjunct(myfile.Path);

				// Verification
				Assert.AreEqual("LIFT0.15", syncAdjunct.BranchName, "BranchName should be 'LIFT0.15'");
			}
		}
	}
}
