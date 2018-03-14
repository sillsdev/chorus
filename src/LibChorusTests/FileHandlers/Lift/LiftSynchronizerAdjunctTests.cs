using System.IO;
using Chorus.FileTypeHandlers.lift;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;
using SIL.WritingSystems.Tests;

namespace LibChorus.Tests.FileHandlers.Lift
{
	[TestFixture]
	public class LiftSynchronizerAdjunctTests
	{
		private const string TestLift13File =
			@"<?xml version='1.0' encoding='UTF-8'?>
				<lift version='0.13'>
			</lift>";

		[Test]
		public void LiftSynchronizerReadsLift13VersionCorrectly()
		{
			// Setup
			using (var liftProject = new TemporaryFolder("TempProj_LIFT_NOLDML"))
			using (var liftFile = new TempFileFromFolder(liftProject, "proj.lift", TestLift13File))
			{
				// SUT
				var syncAdjunct = new LiftSynchronizerAdjunct(liftFile.Path);

				// Verification
				Assert.AreEqual("default", syncAdjunct.BranchName, "BranchName should be 'default' for LIFT0.13");
			}
		}

		[Test]
		public void CorrectlyReturnsDefaultBranchNameOnLdml2Files()
		{
			using (var liftProject = new TemporaryFolder("TempProj_LIFT"))
			using (var liftFile = new TempFileFromFolder(liftProject, "proj.lift", TestLift13File))
			{
				var wsDirectory = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(liftFile.Path), "WritingSystems"));
				using (var writingSystemsDir =
					TempFileFromFolder.CreateAt(liftProject.Combine("WritingSystems", "lang.ldml"), LdmlContentForTests.Version2("en", "", "", "")))
				{
					var syncAdjunct = new LiftSynchronizerAdjunct(liftFile.Path);
					Assert.AreEqual("default", syncAdjunct.BranchName, "BranchName should be 'default' with version 2 ldml files");
				}
			}
		}

		[Test]
		public void CorrectlyAppendsLdmlVersion3ToBranchName()
		{
			using (var liftProject = new TemporaryFolder("TempProj_LIFT"))
			using (var liftFile = new TempFileFromFolder(liftProject, "proj.lift", TestLift13File))
			{
				var wsDirectory = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(liftFile.Path), "WritingSystems"));
				using (var writingSystemsDir =
					TempFileFromFolder.CreateAt(liftProject.Combine("WritingSystems", "lang.ldml"), LdmlContentForTests.Version3("en", "", "", "")))
				{
					var syncAdjunct = new LiftSynchronizerAdjunct(liftFile.Path);
					Assert.AreEqual("LIFT0.13_ldml3", syncAdjunct.BranchName, "BranchName should be 'LIFT0.13_ldml3' with version 3 ldml files");
				}
			}
		}

		private const string TestLift15File =
			@"<?xml version='1.0' encoding='UTF-8'?>
				<lift version='0.15'>
			</lift>";

		[Test]
		public void LiftSynchronizerReadsLift15VersionCorrectly()
		{
			// Setup
			using (var myfile = new TempFile(TestLift15File))
			{
				// SUT
				var syncAdjunct = new LiftSynchronizerAdjunct(myfile.Path);

				// Verification
				Assert.AreEqual("LIFT0.15", syncAdjunct.BranchName, "BranchName should be 'LIFT0.15'");
			}
		}
	}
}
