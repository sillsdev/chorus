using System.Drawing;
using System.Windows.Forms;
using Chorus.annotations;
using Chorus.UI.Notes;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests
{
	[TestFixture]
	public class ChorusNotesSystemTests
	{
		[Test]
		public void CanShowNotesBrowserPage()
		{
			using (var folder = new TempFolder("ChorusNotesSystemTests"))
			using (var dataFile = new TempFile(folder, "one.txt", "just a pretend file"))
			using ( new TempFile(folder, "one.txt" + AnnotationRepository.FileExtension,
						@"<notes version='0'>
					<annotation ref='somwhere://foo?id=x' class='mergeconflict'>
						<message guid='123' author='merger' status='open' date='2009-07-18T23:53:04Z'>
							some description of the conflict
						</message>
					</annotation>
				</notes>"))
			{
				var sys = new ChorusSystem(folder.Path, new ChorusUser("testguy"));
				var notes = sys.GetNotesSystem(dataFile.Path, new ConsoleProgress());
				var page = notes.CreateNotesBrowserPage();
				ShowWindowWithControlThenClose(page);
			}
		}

		private static void ShowWindowWithControlThenClose(Control control)
		{
			control.Dock = DockStyle.Fill;
			var form = new Form();
			form.Size = new Size(700, 600);
			form.Controls.Add(control);
			form.Shown+=((s,e)=>form.Close());
			Application.EnableVisualStyles();
			Application.Run(form);
		}
	}
}
