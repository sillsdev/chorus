using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.sync;
using Chorus.UI.Notes;
using Chorus.Utilities;
using LibChorus.Tests;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class NotesUITests
	{
		[Test]
		public void ShowPage()
		{
			using (var folder = new TempFolder("NotesModelTests"))
			using (new TempFile(folder, "one." + NotesRepository.FileExtension, "<notes version='0'><annotation><message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'/></annotation></notes>"))
			using (new TempFile(folder, "two." + NotesRepository.FileExtension, "<notes  version='0'><annotation><message guid='1234' author='pedro' status='closed' date='2009-09-28T11:11:11Z'/></annotation></notes>"))
			{
				ProjectFolderConfiguration projectConfig = new ProjectFolderConfiguration(folder.Path);
				NotesInProjectModel notesInProjectModel = new NotesInProjectModel(new ChorusNotesUser("Bob"), projectConfig);
				var notesInProjectView = new NotesInProjectView(notesInProjectModel);
				AnnotationView annotationView = new AnnotationView();
				var page = new NotesPage(notesInProjectView, annotationView);
				page.Dock = DockStyle.Fill;
				var form = new Form();
				form.Size = new Size(700,600);
				form.Controls.Add(page);

				Application.EnableVisualStyles();
				Application.Run(form);
			}
		}
	}
}
