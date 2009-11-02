using System;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class HgSetup  :IDisposable
	{
		public TempFolder Root;
		public HgRepository Repository;
		private ConsoleProgress _progress;


		public HgSetup()
		{
			_progress = new ConsoleProgress();
			Root = new TempFolder("ChorusHgWrappingTest");
			HgRepository.CreateRepositoryInExistingDir(Root.Path,_progress);
			Repository = new HgRepository(Root.Path, new NullProgress());
		}

		public void Dispose()
		{
			Root.Dispose();

		}

		public IDisposable GetWLock()
		{
			return TempFile.CreateAt(Root.Combine(".hg", "wlock"), "blah");
		}
		public IDisposable GetLock()
		{
			return TempFile.CreateAt(Root.Combine(".hg", "store", "lock"), "blah");
		}


		public void AssertLocalNumberOfTip(string number)
		{
			Assert.AreEqual(number, Repository.GetTip().Number.LocalRevisionNumber);
		}

		public void AssertHeadOfWorkingDirNumber(string localNumber)
		{
			Assert.AreEqual(localNumber, Repository.GetRevisionWorkingSetIsBasedOn().Number.LocalRevisionNumber);
		}

		public void AssertHeadCount(int expectedCount)
		{
			Assert.AreEqual(expectedCount, Repository.GetHeads().Count);
		}

		public void AssertCommitMessageOfRevision(string localNumber, string expectedCommitMessage)
		{
		   Assert.AreEqual(expectedCommitMessage, Repository.GetRevision(localNumber).Summary);
		}
	}
}