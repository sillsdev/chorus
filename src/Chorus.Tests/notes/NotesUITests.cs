using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chorus.annotations;
using Chorus.sync;
using Chorus.UI;
using Chorus.UI.Notes;
using Chorus.Utilities;
using LibChorus.Tests;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class NotesUITests
	{
		[Test, Ignore("By Hand only")]
		public void ShowPage()
		{
			using (var folder = new TempFolder("NotesModelTests"))
			using (new TempFile(folder, "one." + AnnotationRepository.FileExtension,
				@"<notes version='0'>
					<annotation ref='somwhere://foo?label=korupsen' class='question'>
						<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>
							Suzie, is this ok?
						</message>
						<message guid='222' author='suzie' status='closed' date='2009-09-19T23:53:04Z'>
							It's fine.
						</message>
					</annotation>
					<annotation ref='somwhere://foo2' class='note'>
						<message guid='342' author='john' status='open' date='2009-07-18T23:53:04Z'>
							This is fun.
						</message>
					</annotation>
				</notes>"))
			using (new TempFile(folder, "two." + AnnotationRepository.FileExtension, "<notes  version='0'><annotation  ref='lift://foo.lift?label=wantok' class='mergeConflict'><message guid='1234' author='merger' status='open' date='2009-09-28T11:11:11Z'>Some description of hte conflict</message></annotation></notes>"))
			{
				var messageSelected = new MessageSelectedEvent();
				ProjectFolderConfiguration projectConfig = new ProjectFolderConfiguration(folder.Path);
				NotesInProjectViewModel notesInProjectViewModel = new NotesInProjectViewModel(new ChorusNotesUser("Bob"), projectConfig, messageSelected);
				var notesInProjectView = new NotesInProjectView(notesInProjectViewModel);

				var annotationModel = new AnnotationViewModel(new ChorusNotesUser("bob"), messageSelected, StyleSheet.CreateFromDisk());
				AnnotationView annotationView = new AnnotationView(annotationModel);
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
