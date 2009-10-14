using System;
using Autofac.Builder;
using Chorus.FileTypeHanders;
using Chorus.retrieval;
using Chorus.sync;
using Chorus.UI.Review;
using Chorus.UI.Review.ChangedReport;
using Chorus.UI.Review.ChangesInRevision;
using Chorus.UI.Review.RevisionsInRepository;
using Chorus.UI.Settings;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus
{
	//Both the CHorus app and chorus clients can use this to inject chorus ui stuff into their local
	//autofac container, if that's what they user.
	public static class ChorusUIComponentsInjector
	{
		public static void Inject(ContainerBuilder builder, string projectPath, SyncUIFeatures syncDialogFeatures)
		{
			builder.Register<ProjectFolderConfiguration>(
			   c => new ProjectFolderConfiguration(projectPath));

			builder.Register<NavigateToRecordEvent>();

			builder.Register<IProgress>(new NullProgress());
			builder.Register<Synchronizer>(c => Chorus.sync.Synchronizer.FromProjectConfiguration(
													c.Resolve<ProjectFolderConfiguration>(), new NullProgress()));
			builder.Register<HgRepository>(c => HgRepository.CreateOrLocate(projectPath, new NullProgress()));


			//this is a sad hack... I don't know how to simly override the default using the container,
			//which I'd rather do, and just leave this to pushing in the "normal"
			builder.Register<SyncUIFeatures>(syncDialogFeatures).SingletonScoped();


			builder.Register(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());

			builder.Register<SyncPanel>();
			builder.Register<SyncControlModel>();
			builder.Register<Chorus.UI.Misc.TroubleshootingView>();

			RegisterSyncStuff(builder);
			RegisterReviewStuff(builder);
			RegisterSettingsStuff(builder);

			builder.Register<Chorus.UI.Notes.NotesInProjectModel>();
			builder.Register<Chorus.UI.Notes.NotesInProjectView>();
			builder.Register<Chorus.UI.Notes.AnnotationView>();
			builder.Register<Chorus.UI.Notes.NotesPage>();
		}

		public static void Inject(ContainerBuilder builder, string projectPath)
		{
			Inject(builder, projectPath, SyncUIFeatures.NormalRecommended);
		}


		private static void RegisterSettingsStuff(ContainerBuilder builder)
		{
			builder.Register<SettingsModel>();
			builder.Register<SettingsView>();
		}

		private static void RegisterSyncStuff(ContainerBuilder builder)
		{
			builder.Register<SyncControlModel>();
		}

		private static void RegisterReviewStuff(ContainerBuilder builder)
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


	}
}