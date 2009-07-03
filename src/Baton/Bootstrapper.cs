using System.IO;
using Autofac.Builder;
using Baton.HistoryPanel;
using Baton.HistoryPanel.ChangedRecordControl;
using Baton.HistoryPanel.ChangedRecordsList;
using Baton.Review;
using Baton.Settings;
using Chorus.sync;
using Chorus.UI;

namespace Baton
{
	public class BootStrapper
	{
		private readonly string _settingsPath;

		public BootStrapper(string settingsPath)
		{
			_settingsPath = settingsPath;
		}

		public Shell CreateShell()
		{
			var builder = new Autofac.Builder.ContainerBuilder();

			builder.Register<ProjectFolderConfiguration>(
				c => new ProjectFolderConfiguration(_settingsPath));

			builder.Register<RepositoryManager>(c => Chorus.sync.RepositoryManager.FromRootOrChildFolder(
																c.Resolve<ProjectFolderConfiguration>()));

			RegisterSyncStuff(builder);
			RegisterReviewStuff(builder);
			RegisterSettingsStuff(builder);

			builder.Register<Shell>();

			var container = builder.Build();
			var shell= container.Resolve<Shell>();

			shell.AddPage("Review", container.Resolve<ReviewPage>());
			shell.AddPage("Settings", container.Resolve<SettingsPanel>());

			return shell;
		}

		private void RegisterSettingsStuff(ContainerBuilder builder)
		{
			builder.Register<SettingsPanel>();
		}

		private void RegisterSyncStuff(ContainerBuilder builder)
		{
			builder.Register<SyncPanelModel>();
		}

		private void RegisterReviewStuff(ContainerBuilder builder)
		{
			builder.Register<ReviewPage>();
			builder.Register<ChangedRecordListView>();
			builder.Register<ChangedRecordView>();

			//review-related events
			builder.Register<Review.RevisionSelectedEvent>();
			builder.Register<Review.ChangedRecordSelectedEvent>();

			builder.Register<HistoryPanel.HistoryPanelModel>();
			builder.Register<HistoryPanel.HistoryPanel>();
		}
	}
}
