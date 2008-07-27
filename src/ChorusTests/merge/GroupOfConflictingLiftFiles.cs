using System;
using System.Collections.Generic;
using System.Text;
using Chorus.Utilities;

namespace Chorus.Tests.merge
{
	internal class GroupOfConflictingLiftFiles : IDisposable
	{
		public TempLiftFile BobFile;
		public TempLiftFile SallyFile;
		public TempLiftFile AncestorFile;
		public TempFolder Folder;

		public GroupOfConflictingLiftFiles()
		{
			Folder = new TempFolder("ChorusTest");

			string ancestor = @"<entry id='one'>
						<lexical-unit>
							<form lang='a'>
								<text>original</text>
							</form>
						</lexical-unit>
					 </entry>";
			string bob = ancestor.Replace("original", "bob says");
			string sally = ancestor.Replace("original", "sally says");
			AncestorFile = new TempLiftFile("ancestor.lift", Folder, ancestor, "0.12");
			BobFile = new TempLiftFile("bob.lift", Folder, bob, "0.12");
			SallyFile = new TempLiftFile("sally.lift", Folder, sally, "0.12");
		}


		public string BobTextConflictsPath
		{
			get { return Folder.Combine("bob.lift.conflicts.txt"); }
		}

		public void Dispose()
		{
			AncestorFile.Dispose();
			BobFile.Dispose();
			SallyFile.Dispose();
		}
	}
}
