using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.VcsDrivers.Mercurial
{
	/// <summary>
	/// This is for testing the simple wrapping functions, where the methods
	/// are just doing command-line queries and returning nicely packaged results
	/// </summary>
	[TestFixture]
	public class HgWrappingTests
	{

		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void GetRevisionWorkingSetIsBasedOn_NoCheckinsYet_GivesNull()
		{
			using (var testRoot = new TempFolder("ChorusHgWrappingTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				var rev = repo.GetRevisionWorkingSetIsBasedOn();
				Assert.IsNull(rev);
			}
		}

		[Test]
		public void GetRevisionWorkingSetIsBasedOn_OneCheckin_Gives0()
		{
			using (var testRoot = new TempFolder("ChorusHgWrappingTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				using(var f = testRoot.GetNewTempFile(true))
				{
					repo.AddAndCheckinFile(f.Path);
					var rev = repo.GetRevisionWorkingSetIsBasedOn();
					Assert.AreEqual("0", rev.Number.LocalRevisionNumber);
					Assert.AreEqual(12, rev.Number.Hash.Length);
				}
			}
		}

		[Test]
		public void GetRevision_RevisionDoesntExist_GivesNull()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				Assert.IsNull(setup.Repository.GetRevision("1"));
			}
		}
		[Test]
		public void GetRevision_RevisionDoesExist_Ok()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.AddAndCheckinFile("test.txt", "hello");
				Assert.IsNotNull(setup.Repository.GetRevision("0"));
			}
		}
	}
}