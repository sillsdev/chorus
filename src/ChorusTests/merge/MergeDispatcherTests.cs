using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.sync;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.merge
{
	[TestFixture]
	public class MergeDispatcherTests
	{
		[Test]
		public void NoConflictFileB4_ConflictsEncountered_HaveConflictFileAfter()
		{
			using (TempFolder folder = new TempFolder("ChorusTest"))
			{
				string ancestor = @"<entry id='one'>
						<lexical-unit>
							<form lang='a'>
								<text>original</text>
							</form>
						</lexical-unit>
					 </entry>";
				string bob = ancestor.Replace("original", "bob says");
				string sally = ancestor.Replace("original", "sally says");
				using (TempLiftFile ancestorFile = new TempLiftFile(folder, ancestor, "0.12"))
				using (TempLiftFile bobFile = new TempLiftFile(folder, bob, "0.12"))
				using (TempLiftFile sallyFile = new TempLiftFile(folder, sally, "0.12"))
				{
					MergeOrder order = new MergeOrder(MergeOrder.ConflictHandlingMode.TheyWin, bobFile.Path,ancestorFile.Path,sallyFile.Path);
					order.conflictHandlingMode = MergeOrder.ConflictHandlingMode.WeWin;
					MergeDispatcher.Go(order);

					string textConflictsPath = folder.Combine("changeThis.lift.conflicts.txt");
					string xmlConflictsPath = folder.Combine("changeThis.lift.conflicts.xml");

					Assert.IsTrue(File.Exists(textConflictsPath));
					Assert.IsTrue(File.Exists(xmlConflictsPath));
					Assert.AreNotEqual(string.Empty,File.ReadAllText(textConflictsPath));
					Assert.AreNotEqual(string.Empty,File.ReadAllText(xmlConflictsPath));
				}
			}
		}

		[Test]
		public void NoConflictFileB4_NoConflicts_HaveConflictFileAfter()
		{
			Assert.Fail("not yet");
		}
	}
}