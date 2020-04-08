using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chorus.notes;
using L10NSharp;
using SIL.Progress;


namespace Chorus.UI.Notes.Browser
{
	public partial class NotesBrowserPage : UserControl
	{
		public delegate NotesBrowserPage Factory(IEnumerable<AnnotationRepository> repositories);//autofac uses this

		internal NotesInProjectViewModel _notesInProjectModel;

		internal AnnotationEditorView _annotationView;

		public NotesBrowserPage(NotesInProjectViewModel.Factory notesInProjectViewModelFactory,
			IEnumerable<AnnotationRepository> repositories, AnnotationEditorView annotationView)
		{
			InitializeComponent();
			this.Font = SystemFonts.MessageBoxFont;

			_annotationView = annotationView;

			SuspendLayout();
			_annotationView.ModalDialogMode = false;
			_annotationView.Dock = DockStyle.Fill;
			_annotationView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			splitContainer1.Panel2.Padding = new Padding(3,34,3,3);//drop it below the search box of the other pane

			_notesInProjectModel = notesInProjectViewModelFactory(repositories, new NullProgress());
			_notesInProjectModel.EventToRaiseForChangedMessage += _notesView_SelectionChanged;
			var notesInProjectView = new NotesInProjectView(_notesInProjectModel);
			notesInProjectView.Dock = DockStyle.Fill;

			splitContainer1.Panel1.Controls.Add(notesInProjectView);
			splitContainer1.Panel2.Controls.Add(_annotationView);
			ResumeLayout();
		}

		public EmbeddedMessageContentHandlerRepository MessageContentHandlerRepository
		{
			get {
				var annotationView = splitContainer1.Panel2.Controls.OfType<AnnotationEditorView>().First();
				return annotationView.MesageContentHandlerRepository;
			}
		}

		private static bool ContinueWithoutSaving()
		{
			return DialogResult.Yes == MessageBox.Show(LocalizationManager.GetString("NotesBrowser.ContinueWithoutSaving",
				"You are about leave a Note with an unsaved message.  Continue without saving?"), // TODO pH 2013.08: better message?
				String.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
		}

		/// <summary>
		/// Check that there is no unsaved message text in the AnnotationEditorView before loading a
		/// new annotation selected by the NotesInProjectView.
		/// </summary>
		private void _notesView_SelectionChanged(object sender, CancelEventArgs e)
		{
			// if no text has been entered or the user would like to discard entered text, perform the action
			if (!_annotationView.NewMessageTextEntered || (sender == null && ContinueWithoutSaving()))
			{
				_annotationView.ClearNewMessageText();
				if (sender == null)
				{
					_annotationView.EventToRaiseForChangedMessage.Raise(null, null);
				}
				else
				{
					_annotationView.EventToRaiseForChangedMessage.Raise(((ListMessage) sender).ParentAnnotation,
																		((ListMessage) sender).Message);
				}
			}
			else // Else (the user has entered text and wants to continue editing), "undo" the action
			{
				e.Cancel = true;
			}
		}
	}
}