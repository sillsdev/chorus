using System;
using System.Diagnostics;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.test;
using Chorus.sync;
using LibChorus.TestUtilities;
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
					Assert.That(cop.ValidationResult, Is.Null.Or.Empty);
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
					// SUT
					Assert.That(cop.ValidationResult, Is.Null.Or.Empty);
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
			}
		}

		[Test]
		public void HasFileHandlers_ValidCommit_Validates_DoesNothing()
		{
			using(var bob = new RepositorySetup("bob"))
			{
				bob.AddAndCheckinFile("test.chorusTest", "hello");
				using(var cop = new CommitCop(bob.Repository, ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(), bob.Progress))
				{
					bob.ChangeFile("test.chorusTest", "aloha");
					bob.AddAndCheckinFile("test2.chorusTest", "hi");
					Assert.That(cop.ValidationResult, Is.Null.Or.Empty);
				}
				bob.AssertHeadCount(1);
				bob.AssertLocalRevisionNumber(1);
				bob.AssertFileExistsInRepository("test2.chorusTest");
				bob.AssertFileContents("test.chorusTest", "aloha");
				bob.AssertFileContents("test2.chorusTest", "hi");
			}
		}

		[Test]
		public void InitialFileCommit_Invalid_BacksOut()
		{
			using(var bob = new RepositorySetup("bob"))
			{
				bob.AddAndCheckinFile("validfile.chorustest", "valid contents");
				bob.ChangeFile("test.chorusTest", ChorusTestFileHandler.GetInvalidContents());
				using(var cop = new CommitCop(bob.Repository, ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(), bob.Progress))
				{
					bob.Repository.AddAndCheckinFile("test.chorusTest");
					Assert.That(cop.ValidationResult, Does.Contain("Failed"));
				}
				Debug.WriteLine(bob.Repository.GetLog(-1));
				bob.AssertHeadCount(1);
				bob.AssertLocalRevisionNumber(2);
				bob.AssertFileDoesNotExistInRepository("test.chorusTest");
				bob.AssertFileExistsInRepository("validfile.chorustest");
			}
		}

		[Test]
		public void VeryFirstCommit_Invalid_Throws()
		{
			string validationResult = null;
			Assert.Throws<ApplicationException>(() =>
			{
				using(var bob = new RepositorySetup("bob"))
				{
					bob.ChangeFile("test.chorusTest", ChorusTestFileHandler.GetInvalidContents());
					using(var cop = new CommitCop(bob.Repository, ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(), bob.Progress))
					{
						bob.Repository.AddAndCheckinFile("test.chorusTest");
						// ReSharper disable once ReturnValueOfPureMethodIsNotUsed - SUT
						validationResult = cop.ValidationResult;
					}
				}
			});
			Assert.That(validationResult, Does.Contain("Failed"));
		}
	}
}
