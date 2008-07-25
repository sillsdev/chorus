using System;
using System.Collections.Generic;
using System.Text;
using Chorus.Utilities;

namespace Chorus.Tests.merge
{
	internal class GroupOfConflictingFiles : IDisposable
	{
		public TempLiftFile BobFile;
		public TempLiftFile SallyFile;
		public TempLiftFile AncestorFile;
		public TempFolder Folder;

		public GroupOfConflictingFiles()
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
			AncestorFile = new TempLiftFile(Folder, ancestor, "0.12");
			BobFile = new TempLiftFile(Folder, bob, "0.12");
			SallyFile = new TempLiftFile(Folder, sally, "0.12");
		}

		public string TextConflictsPath
		{
			get
			{
				return this.Folder.Combine("changeThis.lift.conflicts.txt");
			}
		}

		public string XmlConflictsPath
		{
			get {
				return this.Folder.Combine("changeThis.lift.conflicts.xml");
			}
		}

		public void Dispose()
		{
			AncestorFile.Dispose();
			BobFile.Dispose();
			SallyFile.Dispose();
		}
	}
}
