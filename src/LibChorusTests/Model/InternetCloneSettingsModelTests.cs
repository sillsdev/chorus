using System.IO;
using NUnit.Framework;
using SIL.TestUtilities;
using Chorus.Model;
using Chorus.Utilities;

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
				model.InitFromUri("https://john:myPassword@hg-languageforge.org/tpi?localFolder=tokPisin");
				Assert.AreEqual("tokPisin", model.LocalFolderName);
				Assert.That(model.ReadyToDownload, Is.True);
				Assert.AreEqual("https://hg-languageforge.org/tpi", model.URL);
				Assert.AreEqual("john", model.Username);
				Assert.AreEqual("myPassword", model.Password);
			}
		}

		[Test]
		public void URL_AfterConstruction_GoodDefault()
		{
			using (var testFolder = new TemporaryFolder("cloneTest"))
			using (new ShortTermEnvironmentalVariable(ServerSettingsModel.ServerEnvVar, null))
			{
				var model = new InternetCloneSettingsModel(testFolder.Path);
				model.Username = "account";
				model.Password = "password";
				model.ProjectId = "id";
				Assert.AreEqual("https://hg-public.languageforge.org/id", model.URL.ToLower());
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
