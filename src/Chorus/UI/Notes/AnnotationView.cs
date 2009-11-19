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
		}

		void OnModelUpdateDisplay(object sender, EventArgs e)
		{
			this.webBrowser1.DocumentText = _model.GetHtml();
		}
	}
}