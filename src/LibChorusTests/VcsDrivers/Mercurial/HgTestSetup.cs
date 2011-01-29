using System;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class HgTestSetup  :IDisposable
	{
		public TempFolder Root;
		public HgRepository Repository;
		private ConsoleProgress _progress;


		public HgTestSetup()
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

		public void ChangeAndCheckinFile(string path, string contents)
		{
			File.WriteAllText(path, contents);
			Repository.Commit(false, "{0}-->{1}", Path.GetFileName(path), contents);
		}

		public void WriteLogToConsole()
		{
			Debug.WriteLine(Repository.GetLog(-1));
		}
	}
}