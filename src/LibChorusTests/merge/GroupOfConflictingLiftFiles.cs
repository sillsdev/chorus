using System;
using SIL.TestUtilities;

namespace LibChorus.Tests.merge
{
	public class GroupOfConflictingLiftFiles : IDisposable
	{
		public TempLiftFile BobFile;
		public TempLiftFile SallyFile;
		public TempLiftFile AncestorFile;
		public TemporaryFolder Folder;

		public GroupOfConflictingLiftFiles()
		{
			Folder = new TemporaryFolder("ChorusTest");

			string ancestor = @"<entry id='one' guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
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
			get { return Folder.Combine("bob.lift.NewChorusNotes"); }
		}

		public void Dispose()
		{
			Folder.Dispose();
			// Folder Dispose took care of Bob. BobFile.Dispose();
			// Folder Dispose took care of Sally. SallyFile.Dispose();
			// Folder Dispose took care of Ancestor. AncestorFile.Dispose();
		}


	}
}
