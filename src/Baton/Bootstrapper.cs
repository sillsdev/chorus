using System.IO;
using Baton.HistoryPanel;
using Baton.HistoryPanel.ChangedRecordControl;
using Baton.HistoryPanel.ChangedRecordsList;
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
			builder.Register<Shell>();

			builder.Register<ProjectFolderConfiguration>(
				c => new ProjectFolderConfiguration(Path.GetDirectoryName(_settingsPath)));
			builder.Register<SyncPanelModel>();

			builder.Register<ReviewPage>();
			builder.Register<ChangedRecordListView>();
			builder.Register<ChangedRecordView>();
			builder.Register<HistoryPanel.HistoryPanel>();
			builder.Register<RepositoryManager>(c=> Chorus.sync.RepositoryManager.FromRootOrChildFolder(
																c.Resolve<ProjectFolderConfiguration>()));

			builder.Register<SettingsPanel>();

			var container = builder.Build();
			var shell= container.Resolve<Shell>();

			shell.AddPage("Review", container.Resolve<ReviewPage>());
			shell.AddPage("Settings", container.Resolve<SettingsPanel>());

			return shell;
		}
	}
}
