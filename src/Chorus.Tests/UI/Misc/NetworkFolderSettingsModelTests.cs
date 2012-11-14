using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Misc;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace Chorus.Tests.UI.Misc
{
	[TestFixture]
	class NetworkFolderSettingsModelTests
	{
		/// <summary>
		/// If a user picks an folder with a different name then the project name we notify them that a repository will be created in
		/// that folder.
		/// </summary>
		[Test]
		public void NoExistingProjectFolderGivesRepositoryCreated()
		{
			var model = new NetworkFolderSettingsModel {MessageBoxService = new MessageBoxService()};
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			using (var repoFolder = new TemporaryFolder("OrigRepoFolder"))
			{
				model.InitFromProjectPath(repoFolder.Path);
				model.SharedFolder = folder.Path;
				model.SaveSettings();
				Assert.True(((MessageBoxService)model.MessageBoxService).LastMessage.StartsWith("The repository will be created in"));
				var resultModel = new NetworkFolderSettingsModel {MessageBoxService = null};
			}
		}

		[Test]
		public void ExistingProjectSubFolderGivesNoPrompt()
		{
			var model = new NetworkFolderSettingsModel { MessageBoxService = new MessageBoxService() };
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			using (var cloneFolder = new TemporaryFolder(Path.Combine("ServerSettingsModel", "OrigRepoFolder")))
			using (var repoFolder = new TemporaryFolder("OrigRepoFolder"))
			{
				model.InitFromProjectPath(repoFolder.Path);
				model.SharedFolder = folder.Path;
				model.SaveSettings();
				Assert.Null(((MessageBoxService)model.MessageBoxService).LastMessage,
							"Unexpected message {0} when selecting parent folder with empty project subfolder",
							((MessageBoxService)model.MessageBoxService).LastMessage);
			}
		}

		[Test]
		public void ExistingProjectFolderGivesNoPrompt()
		{
			var model = new NetworkFolderSettingsModel { MessageBoxService = new MessageBoxService() };
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			using (var cloneFolder = new TemporaryFolder(Path.Combine("ServerSettingsModel", "OrigRepoFolder")))
			using (var repoFolder = new TemporaryFolder("OrigRepoFolder"))
			{
				model.InitFromProjectPath(repoFolder.Path);
				model.SharedFolder = cloneFolder.Path;
				model.SaveSettings();
				Assert.Null(((MessageBoxService)model.MessageBoxService).LastMessage,
							"Unexpected message {0} when selecting parent folder with empty project subfolder",
							((MessageBoxService)model.MessageBoxService).LastMessage);
				// test that the correct data was set in the repository ini. Assert.AreEqual(model.);
			}
		}

		/// <summary>
		/// If a user picks an folder that does not exist we ask the user and then create the folder if they say ok.
		/// </summary>
		[Test]
		public void NoFolderPromptsCreateFolder()
		{
			var model = new NetworkFolderSettingsModel { MessageBoxService = new MessageBoxService() };
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			using (var repoFolder = new TemporaryFolder("OrigRepoFolder"))
			{
				model.InitFromProjectPath(repoFolder.Path);
				model.SharedFolder = Path.Combine(folder.Path, "NoExisty");
				model.SaveSettings();
				Assert.True(((MessageBoxService)model.MessageBoxService).LastMessage.StartsWith("Create the folder and make a new repository?"));
			}
		}

		[Test]
		public void ExistingSameRepoGivesNoMessage()
		{
			var model = new NetworkFolderSettingsModel { MessageBoxService = new MessageBoxService() };
			var firstRepo = new RepositorySetup("bert", "OrigRepoFolder");
			firstRepo.AddAndCheckinFile("dummyFile", "makeid");
			model.InitFromProjectPath(firstRepo.ProjectFolder.Path);
			//clone repository files into other directory.
			var secondRepo = new RepositorySetup("ernie", firstRepo);
			model.SharedFolder = secondRepo.ProjectFolder.Path;
			model.SaveSettings();
			Assert.Null(((MessageBoxService)model.MessageBoxService).LastMessage,
						"Unexpected message {0} when selecting a folder containing the same repo",
						((MessageBoxService)model.MessageBoxService).LastMessage);
		}

		[Test]
//		[Category("KnownMonoIssue")] I think this was an intermittant failure due to Hg sometimes coming up with the same hash for these two repositories
//									 since their contents was identical. If it has failed on Mono again I was wrong.
		public void ExistingWrongRepoGivesWrongRepoPrompt()
		{

			var model = new NetworkFolderSettingsModel { MessageBoxService = new MessageBoxService() };
				var firstRepo = new RepositorySetup("bob", "OrigRepoFolder");
				firstRepo.AddAndCheckinFile("this.file", "filetext");
				model.InitFromProjectPath(firstRepo.ProjectFolder.Path);
				var secondRepo = new RepositorySetup("bob", "OtherRepoFolder");
				secondRepo.AddAndCheckinFile("that.file", "othertext");
				model.SharedFolder = secondRepo.ProjectFolder.Path;
				model.SaveSettings();
				Assert.True(((MessageBoxService)model.MessageBoxService).LastMessage != null &&
						   ((MessageBoxService)model.MessageBoxService).LastMessage.StartsWith("You selected a repository for a different project."),
						   "Selecting an unrelated repository did not complain.");
		}

		[Test]
		public void SelectedHgFolderTriesParent()
		{
			var model = new NetworkFolderSettingsModel { MessageBoxService = new MessageBoxService() };
			var firstRepo = new RepositorySetup("bert", "OrigRepoFolder");
			firstRepo.AddAndCheckinFile("dummyFile", "makeid");
			model.InitFromProjectPath(firstRepo.ProjectFolder.Path);
			//clone repository files into other directory.
			model.SharedFolder = Path.Combine(firstRepo.ProjectFolder.Path, ".hg");
			model.SaveSettings();
			Assert.Null(((MessageBoxService)model.MessageBoxService).LastMessage,
						"Unexpected message {0} when selecting a folder containing the same repo",
						((MessageBoxService)model.MessageBoxService).LastMessage);
		}

		private class MessageBoxService : NetworkFolderSettingsModel.IMessageBoxService
		{
			public string LastMessage { get; set; }

			public DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
			{
				LastMessage = message;
				return DialogResult.OK;
			}
		}
	}
}
