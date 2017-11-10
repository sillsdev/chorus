using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Chorus.notes;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.TestUtilities;

namespace Chorus.Tests
{
	[TestFixture, RequiresSTA]
	public class ChorusSystemTests
	{
		private TemporaryFolder _folder;
		private TempFile _targetFile1;
		private TempFile _existingNotesFile;
		private ChorusSystem _system;
		private IProgress _progress = new NullProgress();

		[SetUp]
		public void Setup()
		{
			_folder = new TemporaryFolder("ChorusSystemTests");
			_targetFile1 = new TempFileFromFolder(_folder,  "one.txt", "just a pretend file");
			_existingNotesFile = new TempFileFromFolder(_folder, "one.txt." + AnnotationRepository.FileExtension,
						@"<notes version='0'>
					<annotation ref='somwhere://foo?id=x' class='mergeConflict'>
						<message guid='123' author='merger' status='open' date='2009-07-18T23:53:04Z'>
							some description of the conflict
						</message>
					</annotation>
				</notes>");

			_system = new ChorusSystem(_folder.Path);
			_system.Init(string.Empty);
		}

		[TearDown]
		public void TearDown()
		{
			_system.Dispose();
			_targetFile1.Dispose();
			_existingNotesFile.Dispose();
			_folder.Dispose();
		}


		/// <summary>
		/// This is largely a test of the DI Container setup, since problems there aren't
		/// found at compile time
		/// </summary>
		[Test]
		[Category("KnownMonoIssue")]
		[Platform(Exclude="Mono")] //running CreateNotesBrowser twice in a mono test session causes a crash
		public void CanShowNotesBrowserPage()
		{
			using (var page = _system.WinForms.CreateNotesBrowser())
			{
				ShowWindowWithControlThenClose(page);
			}
		}

		/// <summary>
		/// This is largely a test of the DI Container setup, since problems there aren't
		/// found at compile time
		/// </summary>
		[Test]
		public void CanShowNotesBar()
		{
			using (var view =
				_system.WinForms.CreateNotesBar(_targetFile1.Path, new NotesToRecordMapping(), _progress))
			{
				ShowWindowWithControlThenClose(view);
			}
		}

		[Test]
		public void CanMakeNotesBarWithOtherFiles()
		{
			using (var otherFile = new TempFileFromFolder(_folder, "two.txt", "just a pretend file"))
			using (var otherNotesFile = new TempFileFromFolder(_folder,
				"two.txt." + AnnotationRepository.FileExtension,
				@"<notes version='0'>
					<annotation ref='somwhere://foo?guid=x' class='mergeConflict'>
						<message guid='123' author='merger' status='open' date='2009-07-18T23:53:04Z'>
							some description of the conflict
						</message>
					</annotation>
				</notes>"))
			{
				var mapping = new NotesToRecordMapping();
				mapping.FunctionToGoFromObjectToItsId =
					obj => "x"; // means it looks for "x" as the id in the one.txt urls and the guid in the two.txt urls.
				using (var view = _system.WinForms.CreateNotesBar(_targetFile1.Path,
					(new List<String> {otherFile.Path}), "guid", mapping, _progress))
				{
					view._model.SetTargetObject("myobj");
					var annotations = view._model.GetAnnotationsToShow().ToList();
					Assert.That(annotations, Has.Count.EqualTo(2),
						"should have obtained annotations from both files");
					ShowWindowWithControlThenClose(view);
				}
			}
		}

		/// <summary>
		/// Regression test. Once, the autofac container was generating new ProjectFolderConfiguration's with each call
		/// </summary>
		[Test]
		public void ProjectFolderConfiguration_IsNotNewEachMorning()
		{
			var originalCount = _system.ProjectFolderConfiguration.IncludePatterns.Count;
			_system.ProjectFolderConfiguration.IncludePatterns.Add("x");
			_system.ProjectFolderConfiguration.IncludePatterns.Add("y");
			Assert.AreEqual(originalCount + 2,_system.ProjectFolderConfiguration.IncludePatterns.Count);
		}

		private static void ShowWindowWithControlThenClose(Control control)
		{
			control.Dock = DockStyle.Fill;
			using (var form = new Form())
			{
				form.Size = new Size(700, 600);
				form.Controls.Add(control);
				Application.Idle += Application_Idle;
				Application.EnableVisualStyles();
				Application.Run(form);
			}
		}

		static void Application_Idle(object sender, EventArgs e)
		{
			Thread.Sleep(100);
			Application.Exit();
		}

		/// <summary>
		/// This tests that we're using the same repositories for all instances of Notes UI components
		/// </summary>
		[Test]
		[Category("KnownMonoIssue")]
		[Platform(Exclude="Mono")] //running CreateNotesBrowser twice in a mono test session causes a crash
		public void GetNotesBarAndBrowser_MakeNewAnnotationWithBar_BrowserSeesIt()
		{
			NotesToRecordMapping mapping =  NotesToRecordMapping.SimpleForTest();
			using (var bar = _system.WinForms.CreateNotesBar(_targetFile1.Path, mapping, _progress))
			using (var browser = _system.WinForms.CreateNotesBrowser())
			{
				Assert.AreEqual(1, browser._notesInProjectModel.GetMessages().Count());

				bar.SetTargetObject(this);
				var a = bar._model.CreateAnnotation();
				bar._model.AddAnnotation(a);
				a.AddMessage("test", "open", "hello");
				Assert.AreEqual(2, browser._notesInProjectModel.GetMessages().Count());
			}
		}

	}
}
