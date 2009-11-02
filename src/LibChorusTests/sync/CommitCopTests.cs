using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.test;
using Chorus.sync;
using NUnit.Framework;

namespace LibChorus.Tests.sync
{
	[TestFixture]
	public class CommitCopTests
	{
		[Test]
		public void NoMatchingFileHandlers_DoesNothing()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				using(var cop = new CommitCop(bob.Repository , ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(), bob.Progress))
				{
					bob.AddAndCheckinFile("test.abc", "hello");
				}
				bob.AssertLocalRevisionNumber(0);
			}
		}

		[Test]
		public void HasFileHandlers_Validates_DoesNothing()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				using (var cop = new CommitCop(bob.Repository, ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(), bob.Progress))
				{
					bob.AddAndCheckinFile("test.chorusTest", "hello");
				}
				bob.AssertLocalRevisionNumber(0);
			}
		}


		[Test]
		public void SecondCheckin_Invalid_BacksOut()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				bob.AddAndCheckinFile("test.chorusTest", "hello");
				bob.ChangeFile("test.chorusTest",ChorusTestFileHandler.GetInvalidContents());
				using (var cop = new CommitCop(bob.Repository, ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(), bob.Progress))
				{
					Assert.IsFalse(string.IsNullOrEmpty(cop.ValidationResult));
					bob.Repository.Commit(false, "bad data");
				}
				Debug.WriteLine(bob.Repository.GetLog(-1));
				bob.AssertHeadCount(1);
				bob.AssertLocalRevisionNumber(2);
				bob.AssertFileContents("test.chorusTest", "hello");
				bob.ShowInTortoise();
			}
		}
	}
}
