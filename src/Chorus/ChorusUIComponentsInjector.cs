using System;
using Autofac;
using Autofac.Builder;
using Chorus.notes;
using Chorus.FileTypeHanders;
using Chorus.retrieval;
using Chorus.sync;
using Chorus.UI;
using Chorus.UI.Misc;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Notes.Html;
using Chorus.UI.Review;
using Chorus.UI.Review.ChangedReport;
using Chorus.UI.Review.ChangesInRevision;
using Chorus.UI.Review.RevisionsInRepository;
using Chorus.UI.Settings;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus
{
	//Both the CHorus app and chorus clients can use this to inject chorus ui stuff into their local
	//autofac container, if that's what they use.
	public static class ChorusUIComponentsInjector
	{
		public static void Inject(ContainerBuilder builder, string projectPath, SyncUIFeatures syncDialogFeatures)
		{
			//TODO: shouldn't we have people provide the whole project configuration? Otherwise, we have an empty set of
			//include/exlcude patterns, so new files aren't going to get added.  Maybe if we're going to do that, it
			//doesn't make sense for this to do the injecting at all... maybe the client should do it.  Similar issue
			//below, with SyncUIFeatures

			builder.Register<ProjectFolderConfiguration>(
			   c => new ProjectFolderConfiguration(projectPath));

			builder.RegisterType<NavigateToRecordEvent>();

			builder.RegisterInstance(new NullProgress()).As<IProgress>();
			builder.Register<Synchronizer>(c => Chorus.sync.Synchronizer.FromProjectConfiguration(
													c.Resolve<ProjectFolderConfiguration>(), new NullProgress()));
			builder.Register<HgRepository>(c => HgRepository.CreateOrLocate(projectPath, new NullProgress()));


			//this is a sad hack... I don't know how to simly override the default using the container,
			//which I'd rather do, and just leave this to pushing in the "normal"
			builder.Register<SyncUIFeatures>(c => syncDialogFeatures).As<SyncUIFeatures>().SingleInstance();

			builder.RegisterInstance(new EmbeddedMessageContentHandlerFactory());

			builder.RegisterInstance(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());

			builder.RegisterType<SyncPanel>();
			builder.RegisterType<SyncControlModel>();
			builder.RegisterType<SyncDialog>().InstancePerDependency();
			builder.RegisterGeneratedFactory<SyncDialog.Factory>();
			builder.RegisterType<Chorus.UI.Misc.TroubleshootingView>();

			RegisterSyncStuff(builder);
			RegisterReviewStuff(builder);
			RegisterSettingsStuff(builder);

			InjectNotesUI(builder);
		}

		/// <summary>
		/// Only call this directly if you're not using the synching stuff (e.g., testing the notes UI)
		/// </summary>
		/// <param name="builder"></param>
		public static void InjectNotesUI(ContainerBuilder builder)
		{
			builder.RegisterType<MessageSelectedEvent>();
			builder.RegisterType<Chorus.notes.EmbeddedMessageContentHandlerFactory>();
			builder.RegisterType<NotesInProjectViewModel>();
			builder.RegisterType<NotesInProjectView>();
			builder.RegisterType<Chorus.UI.Notes.AnnotationEditorView>();
			builder.RegisterType<Chorus.UI.Notes.AnnotationEditorModel>().InstancePerDependency();
			builder.RegisterType<NotesBrowserPage>();
			builder.Register<StyleSheet>(c =>  StyleSheet.CreateFromDisk());
			builder.RegisterGeneratedFactory<AnnotationEditorModel.Factory>();
			builder.RegisterType<NotesBarModel>();
			builder.RegisterGeneratedFactory<NotesBarModel.Factory>();
			builder.RegisterType<NotesBarView>();
			builder.RegisterGeneratedFactory<NotesBarView.Factory>();

			builder.RegisterGeneratedFactory<NotesInProjectView.Factory>().InstancePerLifetimeScope();
			builder.RegisterGeneratedFactory<NotesInProjectViewModel.Factory>().InstancePerLifetimeScope();
			builder.RegisterGeneratedFactory<NotesBrowserPage.Factory>().InstancePerLifetimeScope();

		}

		public static void Inject(ContainerBuilder builder, string projectPath)
		{
			Inject(builder, projectPath, SyncUIFeatures.NormalRecommended);
		}


		private static void RegisterSettingsStuff(ContainerBuilder builder)
		{
			builder.RegisterType<SettingsModel>();
			builder.RegisterType<SettingsView>();
		}

		private static void RegisterSyncStuff(ContainerBuilder builder)
		{
			builder.RegisterType<SyncControlModel>();
		}

		internal static void RegisterReviewStuff(ContainerBuilder builder)
		{
			builder.RegisterInstance(new ConsoleProgress( )).As<IProgress>();
			builder.RegisterType<RevisionInspector>();
			builder.RegisterType<ChangesInRevisionModel>();
			builder.RegisterType<HistoryPage>();
			builder.RegisterGeneratedFactory<HistoryPage.Factory>();

			builder.RegisterType<ChangesInRevisionView>();
			builder.RegisterType<ChangeReportView>();

			//review-related events
			builder.RegisterType<RevisionSelectedEvent>();
			builder.RegisterType<ChangedRecordSelectedEvent>();

			builder.RegisterType<RevisionInRepositoryModel>();
			builder.RegisterGeneratedFactory<RevisionInRepositoryModel.Factory>();
			builder.RegisterType<RevisionsInRepositoryView>();

		}

		private static Shell CreateShell(string projectPath, Autofac.ContainerBuilder builder)
		{
			builder.RegisterType<Shell>();
			builder.RegisterType<HistoryPage>();
			builder.RegisterType<RevisionsInRepositoryView>();
			builder.RegisterType<RevisionInRepositoryModel>();
			builder.RegisterType<ChangesInRevisionModel>();
			builder.RegisterType<ChangesInRevisionView>();
			builder.RegisterType<ChangeReportView>();
			builder.RegisterType<RevisionInspector>();

			builder.Register(c => HgRepository.CreateOrLocate(projectPath, c.Resolve<IProgress>()));

			var container = builder.Build();
			var shell = container.Resolve<Shell>();

			shell.AddPage("Review", container.Resolve<HistoryPage>());
			shell.AddPage("Send/Receive", container.Resolve<SyncPanel>());
			shell.AddPage("Settings", container.Resolve<SettingsView>());
			shell.AddPage("Troubleshooting", container.Resolve<TroubleshootingView>());

			return shell;
		}


	}
}