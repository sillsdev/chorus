using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.sync;
using Chorus.UI.Notes;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class NotesInProjectModelTests
	{

		[SetUp]
		public void Setup()
		{
			TheUser = new ChorusNotesUser("joe");
		}

		[Test]
		public void GetMessages_NoNotesFiles()
		{
			using (var folder = new TempFolder("NotesModelTests"))
			{
				ProjectFolderConfiguration project = new ProjectFolderConfiguration(folder.Path);
				var m = new NotesInProjectModel(TheUser, project);
				Assert.AreEqual(0, m.GetMessages().Count());
			}
		}

		protected ChorusNotesUser TheUser
		{
			get; set;
		}

		[Test]
		public void GetMessages_FilesInSubDirs_GetsThemAll()
		{
			using (var folder = new TempFolder("NotesModelTests"))
			using (var subfolder = new TempFolder(folder,"Sub"))
			using (new TempFile(folder,"one."+NotesRepository.FileExtension, "<notes version='0'><annotation><message/></annotation></notes>"))
			using (new TempFile(subfolder, "two." + NotesRepository.FileExtension, "<notes  version='0'><annotation><message/></annotation></notes>"))
			{
				ProjectFolderConfiguration project = new ProjectFolderConfiguration(folder.Path);
				var m = new NotesInProjectModel(TheUser, project);
				Assert.AreEqual(2, m.GetMessages().Count());
			}
		}

	}
}