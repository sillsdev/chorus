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
	public class NotesBarModelTests
	{

		/* so far, not realing anything worth testing
		[Test]
		public void GetMessages_IdOfCurrentAnnotatedObjectChanges_TracksNewId()
		{
			using (var folder = new TempFolder("NotesModelTests"))
			using (var dataFile =new TempFile(folder, "one.xml",
				@"<notes version='0'>
					<annotation ref='somwhere://foo?id=one' class='question'/>
					<annotation ref='somwhere://foo?id=two' class='mergeconflict'/>
				</notes>"))
			{
				var repo = AnnotationRepository.FromFile("id", dataFile.Path, new ConsoleProgress());
				var model = new NotesBarModel(repo);
			   }
		}

		*/
	}


}
