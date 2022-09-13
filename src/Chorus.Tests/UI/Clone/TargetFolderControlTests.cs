using System.IO;
using Chorus.UI.Clone;
using NUnit.Framework;
using SIL.TestUtilities;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	[Category("RequiresUI")]
	public class TargetFolderControlTests
	{
		///--------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that the attempt to display to the user that they entered bad path info does not crash while trying
		/// to use that bad info to construct the error message.
		/// </summary>
		///--------------------------------------------------------------------------------------
		[Test]
		public void UpdateDisplay_BadModelDoesNotThrow()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.Username = "account";
				model.Password = "password";
				model.ProjectId = "id";
				model.LocalFolderName = "Some<Folder";
				var ctrl = new TargetFolderControl(model);
				Assert.DoesNotThrow(() => { ctrl._localFolderName.Text = "Some<Folders"; });
			}
		}

		[Test]
		public void LocalFolderName_WontAcceptSpacesBeforeName()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.Username = "account";
				model.Password = "password";
				model.ProjectId = "id";
				model.LocalFolderName = "";
				var ctrl = new TargetFolderControl(model);
				ctrl._localFolderName.Text = "  Bob";
				Assert.That(model.TargetHasProblem, Is.False);
				Assert.AreEqual(Path.Combine(testFolder.Path, "Bob"), model.TargetDestination);
			}
		}

		[Test]
		public void LocalFolderName_WontAcceptSpacesAfterName()
		{
			using (var testFolder = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.Username = "account";
				model.Password = "password";
				model.ProjectId = "id";
				model.LocalFolderName = "";
				var ctrl = new TargetFolderControl(model);
				ctrl._localFolderName.Text = "Billy ";
				Assert.That(model.TargetHasProblem, Is.True);
				Assert.AreEqual(Path.Combine(testFolder.Path, "Billy"), model.TargetDestination);
			}
		}
	}
}
