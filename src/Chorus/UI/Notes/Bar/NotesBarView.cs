using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.annotations;
using Chorus.Properties;
using Chorus.UI.Notes.Bar;
using Chorus.Utilities;

namespace Chorus.UI.Notes
{
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

		private readonly NotesBarModel _model;
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
		public void SetIdOfCurrentAnnotatedObject(string id)
		{
			_model.SetIdOfCurrentAnnotatedObject(id);
		}

		private void OnUpdateContent(object sender, EventArgs e)
		{
			SuspendLayout();
			_buttonsPanel.Controls.Clear();

			AddNoteCreationControl();

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
			b.Image = annotation.GetImage(ButtonImageHeight);
			b.Tag = annotation;
			b.FlatStyle = FlatStyle.Flat;
			b.FlatAppearance.BorderSize = 0;
			toolTip1.SetToolTip(b, annotation.GetTextForToolTip());

			b.Click += new EventHandler(OnExistingAnnotationButtonClick);
			b.Paint += new PaintEventHandler(OnPaintAnnotationButton);
			_buttonsPanel.Controls.Add(b);
			return b;
		}

		void OnPaintAnnotationButton(object sender, PaintEventArgs e)
		{
			Button b = (Button) sender;
			Annotation a = b.Tag as Annotation;
			if (a.IsClosed)
			{
				e.Graphics.DrawImage(Properties.Resources.check16x16, new Rectangle(9, 3, 14, 14));
			}
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
			toolTip1.SetToolTip(b, "Add new question");
			b.Click += new EventHandler(OnCreateNoteButtonClick);
			_buttonsPanel.Controls.Add(b);
		}

		private void OnCreateNoteButtonClick(object sender, EventArgs e)
		{
			var newguy = _model.CreateAnnotation();
			var btn = AddAnnotationButton(newguy);
			OnExistingAnnotationButtonClick(btn, null);
			var annotation = ((Annotation)btn.Tag);
			if(annotation.Messages.Count()==0)
			{
				_model.RemoveAnnotation(annotation);
			}
		}

		private void OnExistingAnnotationButtonClick(object sender, EventArgs e)
		{
			var annotation = (Annotation) ((Button)sender).Tag;
			var dlg = new NoteDetailDialog(annotation, _annotationEditorModelFactory);
			dlg.ShowDialog();
			OnUpdateContent(null,null);
			_model.SaveNowIfNeeded(new NullProgress());
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
