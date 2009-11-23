using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chorus.UI.Notes
{
	public partial class AnnotationView : UserControl
	{
		private readonly AnnotationViewModel _model;

		public AnnotationView(AnnotationViewModel model)
		{
			_model = model;
			_model.UpdateDisplay += OnModelUpdateDisplay;
			InitializeComponent();
			this.Visible = false;//wait for an annotation to be selected
		}

		void OnModelUpdateDisplay(object sender, EventArgs e)
		{
			_annotationLogo.Image = _model.GetAnnotationLogoImage();
			_annotationDetailsLabel.Text = _model.DetailsText;
			_annotationClassLabel.Text = _model.ClassLabel;

			this._existingMessagesHtmlView.DocumentText = _model.GetExistingMessagesHtml();
//            _newMessageHtmlView.DocumentText = _model.GetNewMessageHtml();
//            var doc = _newMessageHtmlView.Document.DomDocument as NativeMethods.IHTMLDocument2;
//            doc.designMode = "On";

			_resolvedCheckBox.Checked = _model.IsResolved;
			_resolvedCheckBox.Visible = _model.ResolvedControlShouldBeVisible;
			_addButton.Enabled = _model.AddButtonEnabled;
			Visible = true;
		}

		private void AnnotationView_Load(object sender, EventArgs e)
		{

		}

		private void _existingMessagesHtmlView_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			_existingMessagesHtmlView.Document.BackColor = this.BackColor;
		}
	}
}