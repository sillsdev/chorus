using System;
using System.IO;
using SIL.IO;
using SIL.TestUtilities;

namespace LibChorus.Tests.merge
{
	public class GroupOfConflictFiles : IDisposable
	{
		public TempFile BobFile;
		public TempFile SallyFile;
		public TempFile AncestorFile;
		public TemporaryFolder Folder;

		public GroupOfConflictFiles(string ancestor, string bob, string sally)
		{
			Folder = new TemporaryFolder("ChorusTest");
			// NB: ChorusNotesFileSyncTests::MergeConflictFiles_AncestorDidNotExist feeds an empty string for ancestor,
			// which is not valid in the Palaso CreateXmlFileWithContents method now.
			// So, we have to use the CreateAt method instead.
			AncestorFile = string.IsNullOrEmpty(ancestor)
				? TempFileFromFolder.CreateAt(Path.Combine(Folder.Path, "ancestor.lift.ChorusNotes"), ancestor)
				: TempFileFromFolder.CreateXmlFileWithContents("ancestor.lift.ChorusNotes", Folder, ancestor);
			BobFile = TempFileFromFolder.CreateXmlFileWithContents("bob.lift.ChorusNotes", Folder, bob);
			SallyFile = TempFileFromFolder.CreateXmlFileWithContents("sally.lift.ChorusNotes", Folder, sally);
		}

		public void Dispose()
		{
			Folder.Dispose();
			//AncestorFile.Dispose();
			//BobFile.Dispose();
			//SallyFile.Dispose();
		}
	}
}
