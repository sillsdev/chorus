using System.IO;
using Chorus.UI.Clone;
using NUnit.Framework;
using SIL.TestUtilities;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class GetCloneFromInternetModelTest
	{
		[Test]
		public void InitFromUri_GivenCompleteUri_AllPropertiesCorrect()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.InitFromUri("http://john:myPassword@hg-languagedepot.org/tpi?localFolder=tokPisin");
				Assert.AreEqual("tokPisin", model.LocalFolderName);
				Assert.IsTrue(model.ReadyToDownload);
				Assert.AreEqual("http://john:myPassword@hg-languagedepot.org/tpi",model.URL);
			}
		}

		[Test]
		public void URL_AfterConstruction_GoodDefault()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.AccountName = "account";
				model.Password = "password";
				model.ProjectId = "id";
				Assert.AreEqual("http://account:password@resumable.languagedepot.org/id", model.URL.ToLower());
			}
		}

		[Test]
		public void CleanUpAfterErrorOrCancel_DirectoryDeleted()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.LocalFolderName = "SomeFolder";
				// Ideally would call model to start the clone - but that's in the dialog for now so fake it instead.
				Directory.CreateDirectory(model.TargetDestination);
				Assert.That(Directory.Exists(model.TargetDestination), Is.True);

				model.CleanUpAfterErrorOrCancel();
				Assert.That(Directory.Exists(model.TargetDestination), Is.False);
			}
		}
	}
}
