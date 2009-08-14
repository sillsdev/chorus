using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Builder;
using Chorus.FileTypeHanders;
using Chorus.retrieval;
using Chorus.Review;
using Chorus.Review.ChangedReport;
using Chorus.Review.ChangesInRevision;
using Chorus.Review.RevisionsInRepository;
using Chorus.Settings;
using Chorus.sync;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus
{
	public class BootStrapper :IDisposable
	{
		private readonly string _settingsPath;
		private IContainer _container;

		public BootStrapper(string settingsPath)
		{
			_settingsPath = settingsPath;
		}

		public Shell CreateShell(BrowseForRepositoryEvent browseForRepositoryEvent)
		{
			var builder = new Autofac.Builder.ContainerBuilder();

			builder.Register<ProjectFolderConfiguration>(
				c => new ProjectFolderConfiguration(_settingsPath));

			builder.Register<IProgress>(new NullProgress());
			builder.Register<Synchronizer>(c => Chorus.sync.Synchronizer.FromProjectConfiguration(
													c.Resolve<ProjectFolderConfiguration>(), new NullProgress()));
			builder.Register<HgRepository>(c => HgRepository.CreateOrLocate(_settingsPath, new NullProgress()));

			builder.Register<SyncUIFeatures>(SyncUIFeatures.Everything).SingletonScoped();

			builder.Register<BrowseForRepositoryEvent>(browseForRepositoryEvent).SingletonScoped();

			builder.Register(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());

			builder.Register<SyncPanel>();
			builder.Register<SyncControlModel>();

			RegisterSyncStuff(builder);
			RegisterReviewStuff(builder);
			RegisterSettingsStuff(builder);

			builder.Register<Shell>();

			_container = builder.Build();
			var shell= _container.Resolve<Shell>();

			shell.AddPage("Review", _container.Resolve<ReviewPage>());
			shell.AddPage("Send/Receive", _container.Resolve<SyncPanel>());
			shell.AddPage("Settings", _container.Resolve<SettingsView>());

			return shell;
		}



		private void RegisterSettingsStuff(ContainerBuilder builder)
		{
			builder.Register<SettingsModel>();
			builder.Register<SettingsView>();
		}

		private void RegisterSyncStuff(ContainerBuilder builder)
		{
			builder.Register<SyncControlModel>();
		}

		private void RegisterReviewStuff(ContainerBuilder builder)
		{
			builder.Register<RevisionInspector>();
			builder.Register<ChangesInRevisionModel>();
			builder.Register<ReviewPage>();
			builder.Register<ChangesInRevisionView>();
			builder.Register<ChangeReportView>();

			//review-related events
			builder.Register<RevisionSelectedEvent>();
			builder.Register<ChangedRecordSelectedEvent>();

			builder.Register<RevisionInRepositoryModel>();
			builder.Register<RevisionsInRepositoryView>();
		}

		public void Dispose()
		{
			_container.Dispose();
		}
	}
}