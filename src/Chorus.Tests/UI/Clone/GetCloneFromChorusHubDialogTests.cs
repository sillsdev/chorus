using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.VcsDrivers.Mercurial;
using ChorusHub;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class GetCloneFromChorusHubDialogTests
	{
		[SetUp]
		public void Setup()
		{
			Application.EnableVisualStyles();//make progress bar work correctly
		}

		[Test, Ignore("By hand only")]
		public void Open()
		{
			Launch();
		}


		private void Launch()
		{
			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
			using (var chorusHubSourceFolder =  new TemporaryFolder(testRoot,"ChorusHub"))
			using (var server = new ChorusHubService(chorusHubSourceFolder.Path))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "repo2"))
			{
				server.Start(true);
				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());

				using (var dlg = new GetCloneFromChorusHubDialog(new GetCloneFromChorusHubModel(testRoot.Path)))
				{
					dlg.ShowDialog();
				}
			}
		}

	}
}