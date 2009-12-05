using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Autofac;
using Chorus.annotations;
using Chorus.sync;
using Chorus.UI;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class NotesUITests
	{
		[Test, Ignore("By Hand only")]
		public void ShowNotesBar()
		{
			using (var folder = new TempFolder("NotesModelTests"))
			using (var dataFile = new TempFile(folder, "one.txt", "just a pretend file"))
			using (new TempFile(folder, "one.txt" + AnnotationRepository.FileExtension,
				@"<notes version='0'>
					<annotation ref='somwhere://foo?id=korupsen' class='question'>
						<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>
							Suzie, is this ok?
						</message>
						<message guid='222' author='suzie' status='closed' date='2009-09-19T23:53:04Z'>
							It's fine.
						</message>
					</annotation>
					<annotation ref='somwhere://foo?id=korupsen' class='mergeconflict'>
						<message guid='123' author='merger' status='open' date='2009-07-18T23:53:04Z'>
							some description of the conflict
						</message>
					</annotation>
					<annotation ref='somwhere://foo2' class='note'/>
				</notes>"))
			{
				var chorus = new ChorusSystem(folder.Path);
				var notesSystem = chorus.GetNotesSystem(dataFile.Path, new ConsoleProgress());
				var view = notesSystem.CreateNotesBarView();

				view.IdOfCurrentAnnotatedObject="korupsen";

				var form = new Form();
				form.Size = new Size(700, 600);
				form.Controls.Add(view);

				Application.EnableVisualStyles();
				Application.Run(form);
			}
		}

		[Test, Ignore("By Hand only")]
		public void ShowNotesPage()
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
			using (new TempFile(folder, "two." + AnnotationRepository.FileExtension,
				string.Format(@"<notes version='0'>
					<annotation ref='somwhere://foo?label=korupsen' class='conflict'>
						<message guid='abc' author='merger' status='open' date='2009-07-18T23:53:04Z'>
							  <![CDATA[<someembedded>something</someembedded>]]>
						</message>
						<message guid='222' author='suzie' status='closed' date='2009-09-19T23:53:04Z'>
							It's fine.
						</message>
					</annotation>
				</notes>", EmbeddedMessageContentTest.SampleXml)))
			using (new TempFile(folder, "three." + AnnotationRepository.FileExtension,
				@"<notes  version='0'>
					 <annotation  ref='lift://foo.lift?label=wantok' class='mergeConflict'>
						<message guid='1234' author='merger' status='open' date='2009-09-28T11:11:11Z'>
							 Some description of hte conflict
							  <![CDATA[<conflict>something</conflict>]]>
						</message>
						</annotation>
					</notes>"))
			{
				var messageSelected = new MessageSelectedEvent();
				ProjectFolderConfiguration projectConfig = new ProjectFolderConfiguration(folder.Path);
				NotesInProjectViewModel notesInProjectViewModel = new NotesInProjectViewModel("id",new ChorusNotesUser("Bob"), projectConfig, messageSelected,new ConsoleProgress());
				var notesInProjectView = new NotesInProjectView(notesInProjectViewModel);

				var annotationModel = new AnnotationEditorModel(new ChorusNotesUser("bob"), messageSelected, StyleSheet.CreateFromDisk(), new EmbeddedMessageContentHandlerFactory());
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
