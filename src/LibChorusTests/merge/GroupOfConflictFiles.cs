using System;
using System.Collections.Generic;
using System.Text;
using Chorus.Utilities;

namespace LibChorus.Tests.merge
{
	internal class GroupOfConflictFiles : IDisposable
	{
		public TempFile BobFile;
		public TempFile SallyFile;
		public TempFile AncestorFile;
		public TempFolder Folder;

		public GroupOfConflictFiles(string ancestor, string bob, string sally)
		{
			Folder = new TempFolder("ChorusTest");
			AncestorFile =  TempFile.CreateXmlFileWithContents("ancestor.lift.conflicts", Folder, ancestor);
			BobFile =  TempFile.CreateXmlFileWithContents("bob.lift.conflicts", Folder, bob);
			SallyFile = TempFile.CreateXmlFileWithContents("sally.lift.conflicts", Folder, sally);
		}


		public void Dispose()
		{
			AncestorFile.Dispose();
			BobFile.Dispose();
			SallyFile.Dispose();
		}
	}
}
