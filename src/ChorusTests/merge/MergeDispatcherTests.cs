using System.IO;
using Chorus.merge;
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
			using (GroupOfConflictingFiles group = new GroupOfConflictingFiles())
			{
				MergeOrder order = new MergeOrder(MergeOrder.ConflictHandlingMode.TheyWin, group.BobFile.Path, group.AncestorFile.Path, group.SallyFile.Path);
				MergeDispatcher.Go(order);

				Assert.IsTrue(File.Exists(group.BobTextConflictsPath));
				Assert.AreNotEqual(string.Empty, File.ReadAllText(group.BobTextConflictsPath));
			}
		}

		[Test, Ignore("not yet")]
		public void NoConflictFileB4_NoConflicts_HaveConflictFileAfter()
		{
		}
	}
}