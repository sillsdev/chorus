using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Notes.Browser;

namespace SampleApp
{
	public partial class Form1 : Form
	{
		private ChorusSystem _chorusSystem;

		public Form1(string dataDirectory, string dataFilePath)
		{
			InitializeComponent();

			_chorusSystem = new ChorusSystem(dataDirectory);

			var dataEditor = new DataEditor(_chorusSystem, dataFilePath);
			dataEditor.Dock = DockStyle.Fill;
			_frontPage.Controls.Add(dataEditor);

			var notesBrowserPage = _chorusSystem.WinForms.CreateNotesBrowser();
			notesBrowserPage.Dock = DockStyle.Fill;
			_notesPage.Controls.Add(notesBrowserPage);

			var historyControl = _chorusSystem.WinForms.CreateHistoryPage();
			historyControl.Dock = DockStyle.Fill;
			_historyPage.Controls.Add(historyControl);
		}


	}
}
