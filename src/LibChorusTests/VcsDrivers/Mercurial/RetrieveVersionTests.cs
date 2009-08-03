using System;
using System.Collections.Generic;
using System.IO;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class RetrieveVersionTests
	{
		private TempFolder _testRoot;
		private TempFile _tempFile;
		private HgRepository _repo;
		private List<Revision> _changesets;
		private ConsoleProgress _progress;

		[SetUp]
		public void Setup()
		{
			_progress = new ConsoleProgress();
			_testRoot = new TempFolder("ChorusRetrieveTest");
			_tempFile = new TempFile(_testRoot);
				File.WriteAllText(_tempFile.Path,"one");

				Chorus.VcsDrivers.Mercurial.HgRepository.CreateRepositoryInExistingDir(_testRoot.Path,_progress);
			_repo = new Chorus.VcsDrivers.Mercurial.HgRepository(_testRoot.Path, new NullProgress());
			_repo.AddAndCheckinFile(_tempFile.Path);
				_repo.Commit(true, "initial");


				File.WriteAllText(_tempFile.Path, "two");
				_repo.AddAndCheckinFile(_tempFile.Path);
				_repo.Commit(true, "changed to two");

			_changesets = _repo.GetAllRevisions();
			Assert.AreEqual(2, _changesets.Count);
		}

		[TearDown]
		public void TeartDown()
		{
			_tempFile.Dispose();
			_testRoot.Dispose();
		}

		[Test]
		public void RetrieveHistoricalVersionOfFile_GetsCorrectContentsOfText()
		{

				using (var temp = TempFile.TrackExisting(_repo.RetrieveHistoricalVersionOfFile(Path.GetFileName(_tempFile.Path), _changesets[1].Number.Hash)))
				{
					var contents = File.ReadAllText(temp.Path);
					Assert.AreEqual("one", contents);
				}

				using (var temp = TempFile.TrackExisting(_repo.RetrieveHistoricalVersionOfFile(Path.GetFileName(_tempFile.Path), _changesets[0].Number.Hash)))
				{
					var contents = File.ReadAllText(temp.Path);
					Assert.AreEqual("two", contents);
				}

		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void RetrieveHistoricalVersionOfFile_BogusHash_Throws()
		{
				Assert.IsNull(
					TempFile.TrackExisting(_repo.RetrieveHistoricalVersionOfFile(Path.GetFileName(_tempFile.Path), "123456)")));
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void RetrieveHistoricalVersionOfFile_BogusFile_Throws()
		{
			Assert.IsNull(
				TempFile.TrackExisting(_repo.RetrieveHistoricalVersionOfFile("bogus.txt", _changesets[0].Number.Hash)));
		}


	}
}
