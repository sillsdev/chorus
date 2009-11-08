using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.UI.Notes;
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
			using (var setup = new RepositorySetup("john"))
			{
				ChorusNotesUser user = new ChorusNotesUser("john");
				NotesInProjectModel notesInProjectModel = new NotesInProjectModel(user, setup.ProjectFolderConfig);
				var notesInProjectView = new NotesInProjectView(notesInProjectModel);
				AnnotationView annotationView = new AnnotationView();
				var page = new NotesPage(notesInProjectView, annotationView);
				page.Dock = DockStyle.Fill;
				var form = new Form();
				form.Size = new Size(500,500);
				form.Controls.Add(page);

				Application.EnableVisualStyles();
				Application.Run(form);
			}
		}
	}
}
