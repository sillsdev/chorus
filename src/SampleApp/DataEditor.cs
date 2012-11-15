using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Chorus;
using Chorus.UI.Notes.Bar;
using Chorus.Utilities;
using Palaso.Progress;
using Palaso.Xml;

namespace SampleApp
{
	/// <summary>
	/// This class just lets you edit two objects, and notes to them.
	/// Here we show how to add a "NotesBar" to your form, and how to help it map between urls and data objects
	/// you want people to be able to add notes to.
	 /// </summary>
	public partial class DataEditor : UserControl
	{
		private readonly ChorusSystem _chorusSystem;
		private readonly string _dataFilePath;
		private NotesBarView _notesBar;

		public DataEditor(ChorusSystem chorusSystem, string dataFilePath)
		{
			_chorusSystem = chorusSystem;
			_dataFilePath = dataFilePath;
			InitializeComponent();

			var notesToRecordMapping = new NotesToRecordMapping()
										   {
											   FunctionToGetCurrentUrlForNewNotes = GetCurrentUrlForNewNotes,
											   FunctionToGoFromObjectToItsId = GetIdForObject
										   };

			_notesBar = _chorusSystem.WinForms.CreateNotesBar(dataFilePath, notesToRecordMapping, new NullProgress());
			_notesBar.Location = new Point(10, 6);
			this.Controls.Add(_notesBar);

			XmlDocument doc = new XmlDocument();
			doc.Load(dataFilePath);

			var areas = doc.SelectNodes("//area");

			_area1Text.Tag = _area1Label.Text = areas[0].Attributes["id"].Value;
			_area1Text.Text = areas[0].InnerText.Trim();

			_area2Text.Tag = _area2Label.Text = areas[1].Attributes["id"].Value;
			_area2Text.Text = areas[1].InnerText.Trim();

		}

		private string GetIdForObject(object targetOfNote)
		{
			return (string)((TextBox)targetOfNote).Tag;
		}

		private string GetCurrentUrlForNewNotes(object dataItemInFocus, string escapedId)
		{
			return string.Format("ShoppingList://area?id={0}&label={0}", ((TextBox)dataItemInFocus).Tag);
		}


		public void SaveNow()
		{
			using(var writer = XmlWriter.Create(_dataFilePath, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartElement("shopping");
				WriteArea(writer, _area1Label.Text, _area1Text.Text);
				WriteArea(writer, _area2Label.Text, _area2Text.Text);
				writer.WriteEndElement();
			}
		}

		private void WriteArea(XmlWriter writer, string id, string value)
		{
			writer.WriteStartElement("area");
			writer.WriteAttributeString("id", id);
			writer.WriteString(value);
			writer.WriteEndElement();
		}

		private void OnEnterBox(object sender, EventArgs e)
		{
			//this is a bit weird because we're using the textbox itself as the target object; in a real
			//app, you'd have some data object instead.
			_notesBar.SetTargetObject(sender);
		}

		private void DataEditor_Load(object sender, EventArgs e)
		{
			_notesBar.SetTargetObject(_area1Text);
		}
	}
}
