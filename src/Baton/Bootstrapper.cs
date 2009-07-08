using System.Collections.Generic;
using System.IO;
using Autofac.Builder;
using Baton.HistoryPanel;
using Baton.Review.ChangedReport;
using Baton.Review.RevisionChanges;
using Baton.Review.RevisionsInRepository;
using Baton.Settings;
using Chorus.FileTypeHanders;
using Chorus.retrieval;
using Chorus.sync;
using Chorus.UI;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

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

			builder.Register<IProgress>(new NullProgress());
			builder.Register<RepositoryManager>(c => Chorus.sync.RepositoryManager.FromRootOrChildFolder(
																c.Resolve<ProjectFolderConfiguration>()));
			builder.Register<HgRepository>(c=> c.Resolve<RepositoryManager>().GetRepository(c.Resolve<IProgress>()));


			builder.Register(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());

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
			builder.Register<RevisionInspector>();
			builder.Register<ChangesInRevisionModel>();
			builder.Register<ReviewPage>();
			builder.Register<ChangesInRevisionView>();
			builder.Register<ChangeReportView>();

			//review-related events
			builder.Register<Review.RevisionSelectedEvent>();
			builder.Register<Review.ChangedRecordSelectedEvent>();

			builder.Register<RevisionInRepositoryModel>();
			builder.Register<RevisionsInRepositoryView>();
		}
	}
}
