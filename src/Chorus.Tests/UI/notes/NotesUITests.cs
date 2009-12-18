using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.sync;
using Chorus.UI;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Review;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class NotesUITests
	{
		private IProgress _progress= new ConsoleProgress();


		[Test, Ignore("By Hand only")]
		public void ShowNotesBar()
		{
			using (var folder = new TempFolder("NotesModelTests"))
			using (var dataFile = new TempFile(folder, "one.txt", "just a pretend file"))
			using (new TempFile(folder, "one.txt." + AnnotationRepository.FileExtension,
				@"<notes version='0'>
					<annotation ref='somwhere://foo?id=x' class='question'>
						<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>
							Suzie, is this ok?
						</message>
						<message guid='222' author='suzie' status='closed' date='2009-09-19T23:53:04Z'>
							It's fine.
						</message>
					</annotation>
					<annotation ref='somwhere://foo?id=x' class='mergeConflict'>
						<message guid='123' author='merger' status='open' date='2009-07-18T23:53:04Z'>
							some description of the conflict
						</message>
					</annotation>
					<annotation ref='somwhere://foo2?id=y' class='note'/>
				</notes>"))
			{
				var chorus = new ChorusSystem(folder.Path);
				var view = chorus.WinForms.CreateNotesBar(dataFile.Path, NotesToRecordMapping.SimpleForTest(), _progress);
				view.Height = 32;
				view.SetTargetObject("x");

				TextBox b = new TextBox();
				b.Location = new Point(0, 50);
				b.Text = "x";
				b.TextChanged += new EventHandler((s,e)=>view.SetTargetObject(b.Text));
				var form = new Form();
				form.Size = new Size(700, 600);
				form.Controls.Add(view);
				form.Controls.Add(b);

				Application.EnableVisualStyles();
				Application.Run(form);
			}
		}

		[Test, Ignore("By Hand only")]
		public void ShowNotesBrowser_LargeNumber()
		{
			using (var f = new TempFile("<notes version='0'/>"))
			{
				var r = AnnotationRepository.FromFile("id", f.Path, new NullProgress());
				for (int i = 0; i < 10000; i++)
				{
					var annotation = new Annotation("question",
													string.Format("nowhere://blah?id={0}&label={1}", Guid.NewGuid().ToString(), i.ToString()),
													f.Path);
					r.AddAnnotation(annotation);
					annotation.AddMessage("test", "open", "blah blah");
				}

				ShowBrowser(new List<AnnotationRepository> {r});
			}
		}

		[Test, Ignore("By Hand only")]
		public void ShowNotesBrowser_SmallNumber()
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
						<message guid='342' author='john' status='closed' date='2009-07-18T23:53:04Z'>
							This is fun.
						</message>
					</annotation>
				</notes>"))
			using (new TempFile(folder, "two." + AnnotationRepository.FileExtension,
				string.Format(@"<notes version='0'>
					<annotation ref='somwhere://foo?label=korupsen' class='mergeConflict'>
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
						<message guid='1234' author='merger' status='open' date='2009-02-28T11:11:11Z'>
							 Some description of hte conflict
							  <![CDATA[<conflict>something</conflict>]]>
						</message>
						</annotation>
					</notes>"))
			{
				var repositories = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
				ShowBrowser(repositories);
			}
		}

		private void ShowBrowser(IEnumerable<AnnotationRepository> repositories)
		{
			var messageSelected = new MessageSelectedEvent();
			NotesInProjectViewModel notesInProjectModel = new NotesInProjectViewModel(new ChorusUser("Bob"), repositories, messageSelected, new ConsoleProgress());

			var writingSystems= new List<IWritingSystem>(new []{new EnglishWritingSystem()});
			var annotationModel = new AnnotationEditorModel(new ChorusUser("bob"), messageSelected, StyleSheet.CreateFromDisk(),
				new EmbeddedMessageContentHandlerFactory(), new NavigateToRecordEvent(), writingSystems);
			AnnotationEditorView annotationView = new AnnotationEditorView(annotationModel);
			annotationView.ModalDialogMode=false;
			var page = new NotesBrowserPage((unusedRepos,progress)=>notesInProjectModel, repositories, annotationView);
			page.Dock = DockStyle.Fill;
			var form = new Form();
			form.Size = new Size(700,600);
			form.Controls.Add(page);

			Application.EnableVisualStyles();
			Application.Run(form);
		}
	}


}
