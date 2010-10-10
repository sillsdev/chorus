using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Notes.Bar;
using Chorus.Utilities;

namespace SampleApp
{
	public partial class DataEditor : UserControl
	{
		private readonly ChorusSystem _chorusSystem;
		private NotesBarView _notesBar;

		public DataEditor(ChorusSystem chorusSystem, string dataFilePath)
		{
			_chorusSystem = chorusSystem;
			InitializeComponent();

			var notesToRecordMapping = new NotesToRecordMapping()
										   {
											   FunctionToGetCurrentUrlForNewNotes = GetCurrentUrlForNewNotes,
											   FunctionToGoFromObjectToItsId = GetIdForObject
										   };

			_notesBar = _chorusSystem.WinForms.CreateNotesBar(dataFilePath, notesToRecordMapping, new NullProgress());
			_notesBar.Location = new Point(_syncButton.Right + 20, _syncButton.Top);
			this.Controls.Add(_notesBar);
		}

		private string GetIdForObject(object targetOfNote)
		{
			return (string)((TextBox)targetOfNote).Tag;
		}

		private string GetCurrentUrlForNewNotes(object dataItemInFocus, string escapedId)
		{
			return string.Format("SampleApp://box?id={0}", ((TextBox)dataItemInFocus).Tag);
		}

		private void OnSendReceiveClick(object sender, EventArgs e)
		{
			using (var dlg = _chorusSystem.WinForms.CreateSynchronizationDialog())
			{
				dlg.ShowDialog(this);
			}
		}

		private void _fruits_Enter(object sender, EventArgs e)
		{
			_notesBar.SetTargetObject(_fruits);
		}

		private void _vegetables_Enter(object sender, EventArgs e)
		{
			_notesBar.SetTargetObject(_vegetables);
		}

		private void DataEditor_Load(object sender, EventArgs e)
		{
			//doesn't work _fruits.Focus();
			_notesBar.SetTargetObject(_fruits);
		}
	}
}
