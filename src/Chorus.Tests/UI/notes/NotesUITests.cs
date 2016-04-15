using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.Review;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Notes.Html;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class NotesUITests
	{
		private IProgress _progress= new ConsoleProgress();

		[Test, Ignore("By Hand only")]
		public void ShowNotesBar()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			using (var dataFile = new TempFileFromFolder(folder, "one.txt", "just a pretend file"))
			using (new TempFileFromFolder(folder, "one.txt." + AnnotationRepository.FileExtension,
				@"<notes version='0'>
					<annotation ref='somwhere://foo?id=x' class='question'>
						<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>
							Suzie, is this ok?
						</message>
						<message guid='222' author='suzie' status='closed' date='2009-09-19T23:53:04Z'>
							It's fine.
						</message>
					</annotation>
					<annotation ref='lift://name%20with%20space.lift?id=x' class='mergeConflict'>
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

		[Test, RequiresSTA, Ignore("By Hand only")]
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

		[Test, Ignore("By Hand only"), RequiresSTA]
		public void ShowNotesBrowser_SmallNumber()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			using (new TempFileFromFolder(folder, "one." + AnnotationRepository.FileExtension,
				@"<notes version='0'>
					<annotation xref='somwhere://foo?label=korupsen' class='question'>
						<message guid='123' author='john' status='open' date='2009-07-18T23:53:04Z'>
							Suzie, is this ok?
						</message>
						<message guid='222' author='suzie' status='closed' date='2009-09-19T23:53:04Z'>
							It's fine.
						</message>
					</annotation>
					<annotation ref='lift://name%20with%20space.lift' class='note'>
						<message guid='342' author='john' status='closed' date='2009-07-18T23:53:04Z'>
							This is fun.
						</message>
					</annotation>
				</notes>"))
			using (new TempFileFromFolder(folder, "two." + AnnotationRepository.FileExtension,
				string.Format(@"<notes version='0'>
					<annotation ref='somwhere://foo?label=korupsen' class='mergeConflict'>
						<message guid='abc' author='merger' status='open' date='2009-07-18T23:53:04Z'>
							  <![CDATA[<someembedded>something</someembedded>]]>
						</message>
						<message guid='222' author='suzie' status='closed' date='2009-09-19T23:53:04Z'>
							It's fine.
						</message>
					</annotation>
				</notes>")))
			using (new TempFileFromFolder(folder, "three." + AnnotationRepository.FileExtension,
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
			//TODO (jh/jh): something here seems screwed up... we create a NotesInProjectViewModel here, and yet so does the NotesBrowserPage

			var messageSelected = new MessageSelectedEvent();
			var chorusNotesDisplaySettings = new ChorusNotesSettings()
			{
				WritingSystemForNoteLabel = new TestWritingSystem("Algerian"),
				WritingSystemForNoteContent = new TestWritingSystem("Bradley Hand ITC")
			};

			NotesInProjectViewModel notesInProjectModel = new NotesInProjectViewModel(new ChorusUser("Bob"), repositories, chorusNotesDisplaySettings, new ConsoleProgress());

			var annotationModel = new AnnotationEditorModel(new ChorusUser("bob"), messageSelected,
				StyleSheet.CreateFromDisk(), new EmbeddedMessageContentHandlerRepository(),
				new NavigateToRecordEvent(), chorusNotesDisplaySettings);
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

	internal class TestWritingSystem : IWritingSystem
	{
		private readonly string _fontName;

		public TestWritingSystem(string fontName)
		{
			_fontName = fontName;
		}

		public string Name
		{
			get { return "test"; }
		}

		public string Code
		{
			get { return "tst"; }
		}

		public string FontName
		{
			get { return _fontName; }
		}

		public int FontSize
		{
			get { return 24; }
		}

		public void ActivateKeyboard()
		{

		}
	}
}
