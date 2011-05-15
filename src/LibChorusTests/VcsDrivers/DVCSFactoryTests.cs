using System;
using System.IO;
using Chorus.VcsDrivers;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers
{
	/// <summary>
	/// Test the DVCS factory.
	/// </summary>
	[TestFixture]
	public class DVCSFactoryTests
	{
		private StringBuilderProgress _stringBuilderProgress;
		private IProgress _progress;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_stringBuilderProgress = new StringBuilderProgress();
			_progress = new MultiProgress(new IProgress[] { new ConsoleProgress() { ShowVerbose = true }, _stringBuilderProgress });
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_stringBuilderProgress = null;
			_progress = null;
		}

		[Test]
		public void NullPathShouldThrow()
		{
			Assert.Throws<ArgumentNullException>(() => DVCSFactory.Create(null, _progress));
		}

		[Test]
		public void EmptyPathShouldThrow()
		{
			Assert.Throws<ArgumentNullException>(() => DVCSFactory.Create("", _progress));
		}

		[Test]
		public void NonExistantPathShouldThrow()
		{
			Assert.Throws<ArgumentException>(() => DVCSFactory.Create(@"BogusPath", _progress));
		}

		[Test]
		public void ShouldCreateHgRepository()
		{
			using (var tempFolder = new TemporaryFolder("HgProject"))
			{
				Directory.CreateDirectory(Path.Combine(tempFolder.Path, ".hg"));
				DoChecks(DVCSFactory.Create(tempFolder.Path, _progress), "HgRepository");
			}
		}

		[Test]
		public void ShouldCreateGitRepository()
		{
			using (var tempFolder = new TemporaryFolder("GitProject"))
			{
				Directory.CreateDirectory(Path.Combine(tempFolder.Path, ".git"));
				DoChecks(DVCSFactory.Create(tempFolder.Path, _progress), "GitRepository");
			}
		}

		[Test]
		public void ShouldCreateDefaultRepository()
		{
			using (var tempFolder = new TemporaryFolder("DefaultProject"))
			{
				DoChecks(DVCSFactory.Create(tempFolder.Path, _progress), "HgRepository");
			}
		}

		private static void DoChecks(IDVCSRepository repository, string expectedTypeName)
		{
			Assert.IsNotNull(repository);
			Assert.IsTrue(repository.GetType().Name == expectedTypeName);
		}
	}
}
