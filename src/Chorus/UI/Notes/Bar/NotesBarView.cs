using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.Properties;
using Chorus.Utilities;
using L10NSharp;
using Palaso.Progress;

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
		/// <param name="key">This is the value, like the id of the target.
		/// An annotation has a @ref attribute.  In that, there is a section of name/value pairs,
		/// e.g. somthing://blahblah?id=foo&offset=35
		/// Here, the key is foo, and the name, "id", must be what the AnnotationRepository was give
		/// as its key attribute, as well.</param>
		/// <returns></returns>
		public delegate NotesBarView Factory();//autofac uses this

		internal readonly NotesBarModel _model;
		private AnnotationEditorModel.Factory _annotationEditorModelFactory;

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

//        public NotesBarView(NotesBarModel model, AnnotationEditorModel.Factory annotationViewModelFactory)
//        {
//            _model = model;
//            _annotationEditorModelFactory = annotationViewModelFactory;
//            InitializeComponent();
//            _model.UpdateContent+=new EventHandler(OnUpdateContent);
//            ButtonHeight = 32;
//        }

	   // public int ButtonHeight { get; set; }

		//this is duplicated on the view so that clients don't have to know/think about
		//how this control is split into view and model
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

		private void AddNoteCreationControl()
		{
			Button b;
			b = new Button {Size = new Size(ButtonHeight, ButtonHeight)};
			b.Image = Resources.NewNote16x16;
			b.FlatStyle = FlatStyle.Flat;
			b.FlatAppearance.BorderSize = 0;
			toolTip1.SetToolTip(b, LocalizationManager.GetString("Messages.AddQuestion", "Add new question"));
			b.Click += new EventHandler(OnCreateNoteButtonClick);
			_buttonsPanel.Controls.Add(b);
		}

		private void OnCreateNoteButtonClick(object sender, EventArgs e)
		{
			try
			{
				var newguy = _model.CreateAnnotation();
				var btn = AddAnnotationButton(newguy);
				OnExistingAnnotationButtonClick(btn, null);
				var annotation = ((Annotation)btn.Tag);
				if (annotation.Messages.Count() == 0)
				{
					_model.RemoveAnnotation(annotation);
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
			var annotation = (Annotation) ((Button)sender).Tag;
			var dlg = new NoteDetailDialog(annotation, _annotationEditorModelFactory);
			dlg.ShowDialog();
			OnUpdateContent(null,null);
			_model.SaveNowIfNeeded(new NullProgress());
			Timer refreshTimer = new Timer() {Interval = 500, Enabled = true};
			refreshTimer.Tick += new EventHandler(OnRefreshTimer_Tick);
			components.Add(refreshTimer);
		}

		void OnRefreshTimer_Tick(object sender, EventArgs e)
		{
			_model.CheckIfWeNeedToReload();
		}

		private void NotesBarView_Load(object sender, EventArgs e)
		{
			OnUpdateContent(null,null);
		}


	  /* for now, let's just autosave
	   * public void SaveNowIfNeeded(IProgress progress)
		{
			_model.SaveNowIfNeeded(progress);
		}
	   */
	}
}
