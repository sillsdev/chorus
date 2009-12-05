using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.annotations;
using Chorus.UI.Notes.Bar;

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

		private readonly string _key;
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
			ButtonHeight = 32;
		}

//        public NotesBarView(NotesBarModel model, AnnotationEditorModel.Factory annotationViewModelFactory)
//        {
//            _model = model;
//            _annotationEditorModelFactory = annotationViewModelFactory;
//            InitializeComponent();
//            _model.UpdateContent+=new EventHandler(OnUpdateContent);
//            ButtonHeight = 32;
//        }

		protected int ButtonHeight { get; set; }
		public string IdOfCurrentAnnotatedObject { get; set; }

		private void OnUpdateContent(object sender, EventArgs e)
		{
			_buttonsPanel.Controls.Clear();

			foreach (var annotation in _model.GetAnnotationsToShow(IdOfCurrentAnnotatedObject))
			{
				var b = new Button();

				b.Size = new Size(ButtonHeight + 14, ButtonHeight + 14);
				b.Image = annotation.GetImage(ButtonHeight);
				b.Tag = annotation;
				b.Click += new EventHandler(OnExistingAnnotationButtonClick);
				_buttonsPanel.Controls.Add(b);
			}
		}

		private void OnExistingAnnotationButtonClick(object sender, EventArgs e)
		{
			var annotation = (Annotation) ((Button)sender).Tag;
			var dlg = new NoteDetailDialog(annotation, _annotationEditorModelFactory);
			dlg.ShowDialog();
			OnUpdateContent(null,null);
		}

		private void NotesBarView_Load(object sender, EventArgs e)
		{
			OnUpdateContent(null,null);
		}


	}
}
