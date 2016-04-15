// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Autofac;
using Chorus.notes;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Review;
using Chorus.UI.Sync;
using L10NSharp;
using Palaso.Progress;
using IContainer = Autofac.IContainer;

namespace Chorus
{
	/// <summary>
	/// A ChorusSystem object hides a lot of the complexity of Chorus from the client programmer.  It offers
	/// up the most common controls and services of Chorus. See the SampleApp for examples of using it.
	/// </summary>
	public class ChorusSystem: ChorusSystemSimple
	{
		/// <summary>
		/// Constructor. Need to Init after this
		/// </summary>
		public ChorusSystem()
		{
		}

		/// <summary>
		/// Constructor. Need to Init after this
		/// </summary>
		/// <param name="dataFolderPath">The root of the project</param>
		public ChorusSystem(string dataFolderPath): base(dataFolderPath)
		{
		}

		/// <summary>
		/// Inits the container builder.
		/// </summary>
		protected override ContainerBuilder InitContainerBuilder()
		{
			var builder = base.InitContainerBuilder();

			ChorusUIComponentsInjector.Inject(builder, _dataFolderPath);
			return builder;
		}

		/// <summary>
		/// Typically root directory of installed files is something like [application exe directory]/localizations.
		/// root directory of user modifiable tmx files has to be outside program files, something like
		/// GetXAppDataFolder()/localizations, where GetXAppDataFolder would typically return something like
		/// Company/Program (e.g. SIL/SayMore)
		/// </summary>
		/// <param name="desiredUiLangId"></param>
		/// <param name="rootDirectoryOfInstalledTmxFiles">The folder path of the original TMX files
		/// installed with the application.  The Chorus TMX files will be in a Chorus subdirectory of this directory.</param>
		/// <param name="relativeDirectoryOfUserModifiedTmxFiles">The path, relative to %appdata%, where your
		/// application stores user settings (e.g., "SIL\SayMore"). A folder named "Chorus\localizations" will be created there.</param>
		public static void SetUpLocalization(string desiredUiLangId, string rootDirectoryOfInstalledTmxFiles,
			string relativeDirectoryOfUserModifiedTmxFiles)
		{
			string directoryOfInstalledTmxFiles = Path.Combine(rootDirectoryOfInstalledTmxFiles, "Chorus");
			string directoryOfUserModifiedTmxFiles = Path.Combine(relativeDirectoryOfUserModifiedTmxFiles, "Chorus");

			// This is safer than Application.ProductVersion, which might contain words like 'alpha' or 'beta',
			// which (on the SECOND run of the program) fail when L10NSharp tries to make a Version object out of them.
			var versionObj = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			// We don't need to reload strings for every "revision" (that might be every time we build).
			var version = "" + versionObj.Major + "." + versionObj.Minor + "." + versionObj.Build;
			LocalizationManager.Create(desiredUiLangId, "Chorus", Application.ProductName,
						   version, directoryOfInstalledTmxFiles,
						   directoryOfUserModifiedTmxFiles,
						   Icon.FromHandle(Properties.Resources.chorus32x32.GetHicon()), // should call DestroyIcon, but when?
						   "issues@chorus.palaso.org", "Chorus");
		}

		/// <summary>
		/// Various factories for creating WinForms controls, already wired to the other parts of Chorus
		/// </summary>
		public WinFormsFactory WinForms
		{
			get { return new WinFormsFactory(this, _container); }
		}

		/// <summary>
		/// This class is exists only to organize all WindowForms UI component factories together,
		/// so the programmer can write, for example:
		/// _chorusSystem.WinForms.CreateSynchronizationDialog()
		/// </summary>
		public class WinFormsFactory
		{
			private readonly ChorusSystem _parent;
			private readonly IContainer _container;

			public WinFormsFactory(ChorusSystem parent, IContainer container)
			{
				_parent = parent;
				_container = container;
			}

			public Form CreateSynchronizationDialog()
			{
				return _container.Resolve<SyncDialog.Factory>()(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended);
			}

			public Form CreateSynchronizationDialog(SyncUIDialogBehaviors behavior, SyncUIFeatures uiFeaturesFlags)
			{
				return _container.Resolve<SyncDialog.Factory>()(behavior, uiFeaturesFlags);
			}

			/// <summary>
			/// Get a UI control designed to live near some data (e.g., a lexical entry);
			/// it provides buttons
			/// to let users see and open and existing notes attached to that data,
			/// or create new notes related to the data.
			/// </summary>
			public NotesBarView CreateNotesBar(string pathToAnnotatedFile, NotesToRecordMapping mapping, IProgress progress)
			{
				var model = CreateNotesBarModel(pathToAnnotatedFile, mapping, progress);
				return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
			}

			/// <summary>
			/// Get a UI control designed to live near some data (e.g., a lexical entry);
			/// it provides buttons
			/// to let users see and open and existing notes attached to that data,
			/// or create new notes related to the data.
			/// New annotations will be created in primaryAnnotationsFilePath.
			/// Annotations from all paths will be displayed.
			/// idAttrForOtherFiles specifies the attr in annotation urls that identifies the target of the annotation for those files (in primary, hard-coded to "id")
			/// </summary>
			public NotesBarView CreateNotesBar(string pathToPrimaryFile, IEnumerable<string> pathsToOtherFiles, string idAttrForOtherFiles, NotesToRecordMapping mapping, IProgress progress)
			{
				var model = CreateNotesBarModel(pathToPrimaryFile, pathsToOtherFiles, idAttrForOtherFiles, mapping, progress);
				return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
			}

			/// <summary>
			/// Get the model that would be needed if we go on to create a NotesBarView.
			/// FLEx (at least) needs this to help it figure out, before we go to create the actual NotesBar,
			/// whether there are any notes to show for the current entry.
			/// </summary>
			/// <param name="pathToAnnotatedFile"></param>
			/// <param name="mapping"></param>
			/// <param name="progress"></param>
			/// <returns></returns>
			public NotesBarModel CreateNotesBarModel(string pathToAnnotatedFile, NotesToRecordMapping mapping, IProgress progress)
			{
				var repo = _parent.GetNotesRepository(pathToAnnotatedFile, progress);
				var model = _container.Resolve<NotesBarModel.Factory>()(repo, mapping);
				return model;
			}

			/// <summary>
			/// Get the model that would be needed if we go on to create a NotesBarView.
			/// FLEx (at least) needs this to help it figure out, before we go to create the actual NotesBar,
			/// whether there are any notes to show for the current entry.
			/// New annotations will be created in primaryAnnotationsFilePath.
			/// Annotations from all paths will be displayed.
			/// </summary>
			/// <param name="pathToPrimaryFile"></param>
			/// <param name="pathsToOtherFiles"></param>
			/// <param name="idAttrForOtherFiles">Attr in url that identifies the target of the annotation.</param>
			/// <param name="mapping"></param>
			/// <param name="progress"></param>
			/// <returns></returns>
			public NotesBarModel CreateNotesBarModel(string pathToPrimaryFile, IEnumerable<string> pathsToOtherFiles, string idAttrForOtherFiles, NotesToRecordMapping mapping, IProgress progress)
			{
				var repo = _parent.GetNotesRepository(pathToPrimaryFile, pathsToOtherFiles, idAttrForOtherFiles, progress);
				var model = _container.Resolve<NotesBarModel.Factory>()(repo, mapping);
				return model;
			}

			/// <summary>
			/// Get a UI control which shows all notes in the project (including conflicts), and
			/// lets the user filter them and interact with them.
			/// </summary>
			public NotesBrowserPage CreateNotesBrowser()
			{
				_parent.EnsureAllNotesRepositoriesLoaded();
				return _container.Resolve<NotesBrowserPage.Factory>()(_parent._annotationRepositories.Values);
			}

			/// <summary>
			/// Get a UI control which shows all the revisions in the repository, and
			/// lets the user select one to see what changed.
			/// </summary>
			public HistoryPage CreateHistoryPage()
			{
				return _container.Resolve<HistoryPage.Factory>()(new HistoryPageOptions());
			}


			/// <summary>
			/// Get a UI control which shows all the revisions in the repository, and
			/// lets the user select one to see what changed.
			/// </summary>
			public HistoryPage CreateHistoryPage(HistoryPageOptions options)
			{
				return _container.Resolve<HistoryPage.Factory>()(options);
			}


		}
	}
}
