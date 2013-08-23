using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.Utilities;
using Palaso.Progress;


namespace Chorus.UI.Notes.Browser
{
	public partial class NotesBrowserPage : UserControl
	{
		public delegate NotesBrowserPage Factory(IEnumerable<AnnotationRepository> repositories);//autofac uses this

		internal NotesInProjectViewModel _notesInProjectModel;

		// TODO pH 2013.08: we should need only one of the following:
		internal AnnotationEditorView _annotationView;
		internal AnnotationEditorModel _annotationModel;

		//private Action<Annotation, Chorus.notes.Message> _checkBeforeUpdatingAnnotation;

		public NotesBrowserPage(NotesInProjectViewModel.Factory notesInProjectViewModelFactory,
			IEnumerable<AnnotationRepository> repositories, AnnotationEditorView annotationView)
		{
			InitializeComponent();
			this.Font = SystemFonts.MessageBoxFont;

			// TODO pH 2013.08: review necessity and placement:
			_annotationView = annotationView;
			//_annotationModel = annotationView.
			//_checkBeforeUpdatingAnnotation = _notesView_SelectionChanged;

			SuspendLayout();
			annotationView.ModalDialogMode = false;
			annotationView.Dock = DockStyle.Fill;
			annotationView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			splitContainer1.Panel2.Padding = new Padding(3,34,3,3);//drop it below the search box of the other pane

			_notesInProjectModel = notesInProjectViewModelFactory(repositories, new NullProgress());
			_notesInProjectModel.EventToRaiseForChangedMessage += _notesView_SelectionChanged;
			var notesInProjectView = new NotesInProjectView(_notesInProjectModel);
			notesInProjectView.Dock = DockStyle.Fill;

			splitContainer1.Panel1.Controls.Add(notesInProjectView);
			splitContainer1.Panel2.Controls.Add(annotationView);
			ResumeLayout();
		}

		public EmbeddedMessageContentHandlerRepository MessageContentHandlerRepository
		{
			get {
				var annotationView = splitContainer1.Panel2.Controls.OfType<AnnotationEditorView>().First();
				return annotationView.MesageContentHandlerRepository;
			}
		}

		/// <summary>
		/// Check that there is no unsaved message text in the AnnotationEditorView before loading a
		/// new annotation selected by the NotesInProjectView.
		/// </summary>
		/// <param name="annotation"></param><param name="message"></param>
//TODO		private void _notesView_SelectionChanged(Annotation annotation, Chorus.notes.Message message)
		//{
		//    // if no text has been entered or the user would like to discard entered text, perform the action
		//    if (!_annotationView.NewMessageTextEntered || DialogResult.Yes == MessageBox.Show(
		//        "You are about TODO pH 2103.08!!! - Continue without saving?", // TODO pH 2013.08: localize
		//        String.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
		//    {
		//        _annotationView.ClearNewMessageText();
		//        _annotationView.EventToRaiseForChangedMessage.Raise(annotation, message);
		//    }
		//    else // Else (the user has entered text and wants to continue editing), "undo" the action
		//    {
		//        // unsubscribe so we don't re-prompt the user
		//        _notesInProjectModel.EventToRaiseForChangedMessage.Unsubscribe(_notesView_SelectionChanged);
		//        // Undo the event (set index to "last" as stored in Model (TODO)
		//        _notesInProjectModel.EventToRaiseForChangedMessage.Subscribe(_notesView_SelectionChanged);
		//    }
		//}
		private void _notesView_SelectionChanged(object sender, CancelEventArgs e)
		{
			// if no text has been entered or the user would like to discard entered text, perform the action
			if (!_annotationView.NewMessageTextEntered || (sender == null && DialogResult.Yes == MessageBox.Show(
				"You are about TODO pH 2103.08!!! - Continue without saving?", // TODO pH 2013.08: localize
				String.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning)))
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
				//// unsubscribe so we don't re-prompt the user
				//_notesInProjectModel.EventToRaiseForChangedMessage = // TODO pH 2013.08: find a safer way to do this
				//    (CancelEventHandler)_notesInProjectModel.EventToRaiseForChangedMessage.Clone() - _notesView_SelectionChanged;
				//// Undo the event (set index to "last" as stored in Model (TODO)
				//_notesInProjectModel.EventToRaiseForChangedMessage += _notesView_SelectionChanged;
			}
		}

		////////
		/// The following methods are for a last-resort attempt to capture everything in the pane.
		/// TODO pH 2013.08: remove this
		////private void _panel1_anyChange(object sender, EventArgs e)
		////{
		////    // If there is any entered text in the , prompt and possibly prevent event
		////}

		////private void _panel1_previewKeyDown(object sender, PreviewKeyDownEventArgs e)
		////{
		////    if (e.KeyCode != Keys.Tab)
		////    {
		////        _panel1_anyChange(sender, e);
		////    }
		////}

		////protected bool CheckForUnsavedMessageText()
		////{
		////    _annotationView.OnUpdateContent(null, null);
		////    return true;
		////}

		/////// <returns>
		/////// true if the character was processed by the control; otherwise, false.
		/////// </returns>
		/////// <param name="msg">A <see cref="T:System.Windows.Forms.Message"/>,
		/////// passed by reference, that represents the window message to process. </param>
		/////// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process. </param>
		////protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
		////{
		////    if (keyData != Keys.Tab && CheckForUnsavedMessageText())
		////    {
		////        var myBool = true;
		////    }
		////    return base.ProcessCmdKey(ref msg, keyData);
		////}

		//////protected override bool // prevent mouse click, too
	}
}