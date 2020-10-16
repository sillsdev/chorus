using System.IO;
using NUnit.Framework;
using SIL.TestUtilities;
using Chorus.Model;

namespace LibChorus.Tests.Model
{
	[TestFixture]
	public class InternetCloneSettingsModelTests
	{
		[Test]
		public void InitFromUri_GivenCompleteUri_AllPropertiesCorrect()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new InternetCloneSettingsModel(testFolder.Path);
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
				var model = new InternetCloneSettingsModel(testFolder.Path);
				model.Username = "account";
				model.Password = "password";
				model.ProjectId = "id";
				Assert.AreEqual("https://account:password@resumable.languagedepot.org/id", model.URL.ToLower());
			}
		}

		[Test]
		public void CleanUpAfterErrorOrCancel_DirectoryDeleted()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new InternetCloneSettingsModel(testFolder.Path);
				model.LocalFolderName = "SomeFolder";
				// REVIEW: Ideally would call model to start the clone - but that's in the dialog for now so fake it instead.
				Directory.CreateDirectory(model.TargetDestination);
				Assert.That(Directory.Exists(model.TargetDestination), Is.True);

				model.CleanUpAfterErrorOrCancel();
				Assert.That(Directory.Exists(model.TargetDestination), Is.False);
			}
		}
	}
}
