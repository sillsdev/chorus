using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.Properties;
using L10NSharp;
using SIL.Progress;

namespace Chorus.UI.Notes.Bar
{
	/// <summary>
	/// Normally, you should create this using your ChorusSystem object, not directly. As the user puts the cursor
	/// on different annotatable objects, call the SetTarget() method so that the bar can update appropriately.
	/// It will not show *anything* until you call SetTarget().
	///
	/// Note that you have to set up the NotesToRecordMapping.  See the summary on that
	/// class and SampleApp for more information.
	/// </summary>
	/// <example>    var notesToRecordMapping = new NotesToRecordMapping()
	///                                           {
	///                                               FunctionToGetCurrentUrlForNewNotes = GetCurrentUrlForNewNotes,
	///                                               FunctionToGoFromObjectToItsId = GetIdForObject
	///                                           };
	///             _notesBar = _chorusSystem.WinForms.CreateNotesBar(dataFilePath, notesToRecordMapping, new NullProgress());
	///             _notesBar.Location = new Point(10, 6);
	///             this.Controls.Add(_notesBar);
	///</example>
	public partial class NotesBarView : UserControl
	{
		/// <summary>
		/// Makes a view control. An instance of this Factory method is created by Autofac for us.
		/// </summary>
		public delegate NotesBarView Factory();//autofac uses this

		internal readonly NotesBarModel _model;
		private readonly AnnotationEditorModel.Factory _annotationEditorModelFactory;

		/// <summary>
		/// Normally, don't use this, use the autofac-generated factory instead.
		/// </summary>
		 public NotesBarView(NotesBarModel model, AnnotationEditorModel.Factory annotationViewModelFactory)
		{
			_model = model;
			_annotationEditorModelFactory = annotationViewModelFactory;
			InitializeComponent();
			_model.UpdateContent += new EventHandler(OnUpdateContent);
		   // ButtonHeight = 32;
			this.Height = 25;//nb: there is some confusion here.
		}

		/// <remarks>
		/// this is duplicated on the view so that clients don't have to know/think about
		/// how this control is split into view and model
		/// </remarks>
		public void SetTargetObject(object target)
		{
			_model.SetTargetObject(target);
		}

		private void OnUpdateContent(object sender, EventArgs e)
		{
			SuspendLayout();
			_buttonsPanel.Controls.Clear();
			if (!_model.TargetObjectIsNull)
			{
				AddNoteCreationControl();
			}

			foreach (var annotation in _model.GetAnnotationsToShow())
			{
				AddAnnotationButton(annotation);
			}
			ResumeLayout(false);
		}


		private Button AddAnnotationButton(Annotation annotation)
		{
			var b = new Button();

			b.Size = new Size(ButtonHeight, ButtonHeight);
			b.Image = annotation.GetOpenOrClosedImage(ButtonImageHeight);
			b.Tag = annotation;
			b.FlatStyle = FlatStyle.Flat;
			b.FlatAppearance.BorderSize = 0;
			toolTip1.SetToolTip(b, annotation.GetTextForToolTip());

			b.Click += new EventHandler(OnExistingAnnotationButtonClick);
			_buttonsPanel.Controls.Add(b);
			return b;
		}

		protected int ButtonHeight
		{
			get { return this.Height-8; }
		}

		protected int ButtonImageHeight
		{
			get
			{
				if (ButtonHeight > 35)
					return 32;
				else
				{
					return 16;
				}
			}
		}

		Button _createNoteBtn;

		/// <summary>
		/// Create the "Create Note" button, add it to the _buttonsPanel, and save it for future use.
		/// </summary>
		/// <remarks>
		/// Saving and reusing the button is needed to avoid a possible crash in WeSay on Linux.
		/// See https://jira.sil.org/browse/WS-211.  (You could view this as an optimization to
		/// avoid reallocating/recreating this button each time.)
		/// </remarks>
		private void AddNoteCreationControl()
		{
			if (_createNoteBtn == null || _createNoteBtn.IsDisposed)
			{
				_createNoteBtn = new Button {Size = new Size(ButtonHeight, ButtonHeight)};
				_createNoteBtn.Image = Resources.NewNote16x16;
				_createNoteBtn.FlatStyle = FlatStyle.Flat;
				_createNoteBtn.FlatAppearance.BorderSize = 0;
				toolTip1.SetToolTip(_createNoteBtn, LocalizationManager.GetString("Messages.AddQuestion", "Add new question"));
				_createNoteBtn.Click += new EventHandler(OnCreateNoteButtonClick);
			}
			// Note that _buttonsPanel.Controls.Clear() does not call Dispose() on the controls being cleared in OnUpdateContent().
			// See http://msdn.microsoft.com/en-us/library/system.windows.forms.control.controlcollection.clear%28v=vs.100%29.aspx.
			_buttonsPanel.Controls.Add(_createNoteBtn);
		}

		private void OnCreateNoteButtonClick(object sender, EventArgs e)
		{
			try
			{
				var newAnnotation = _model.CreateAnnotation();
				if (ShowNoteDetailDialog(newAnnotation) == DialogResult.OK && newAnnotation.Messages.Any())
				{
					_model.AddAnnotation(newAnnotation);
					AddAnnotationButton(newAnnotation);
				}
			}
			catch (Exception)
			{
				// nothing here is worth crashing over.  if/when we add palaso reporting, we could be more visible about it
#if DEBUG
				throw;
#endif
			}
		}

		private void OnExistingAnnotationButtonClick(object sender, EventArgs e)
		{
			ShowNoteDetailDialog((Annotation) ((Button) sender).Tag);
		}

		private DialogResult ShowNoteDetailDialog(Annotation annotation)
		{
			using (var dlg = new NoteDetailDialog(annotation, _annotationEditorModelFactory))
			{
				dlg.LabelWritingSystem = LabelWritingSystem;
				dlg.MessageWritingSystem = MessageWritingSystem;
				var result = dlg.ShowDialog();

				OnUpdateContent(null, null);
				_model.SaveNowIfNeeded(new NullProgress());
				Timer refreshTimer = new Timer() { Interval = 500, Enabled = true };
				refreshTimer.Tick += OnRefreshTimer_Tick;
				components.Add(refreshTimer);

				return result;
			}
		}

		void OnRefreshTimer_Tick(object sender, EventArgs e)
		{
			_model.CheckIfWeNeedToReload();
		}

		private void NotesBarView_Load(object sender, EventArgs e)
		{
			OnUpdateContent(null,null);
		}


		public IWritingSystem LabelWritingSystem
		{
			get;
			set;
		}

		public IWritingSystem MessageWritingSystem
		{
			get;
			set;
		}
	}
}
