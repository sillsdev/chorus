using System.IO;
using Chorus.merge;
using LibChorus.Tests.merge;
using Chorus.Utilities;
using NUnit.Framework;

namespace ChorusMerge.Tests
{
	/// <summary>
	/// The assembly under test doesn't actually do much, so the only tests
	/// needed here are to cover what little it does... most of the action is in libchorus
	/// </summary>
	[TestFixture]
	public class ChorusMergeTests
	{

		[Test]
		public void Main_NoConflictFileB4_ConflictsEncountered_HaveConflictFileAfter()
		{
			using (var group = new GroupOfConflictingLiftFiles())
			{
				Assert.AreEqual(0, DoMerge(group));
				Assert.IsTrue(File.Exists(group.BobTextConflictsPath));
				Assert.AreNotEqual(string.Empty, File.ReadAllText(group.BobTextConflictsPath));
			}
		}

		[Test]
		public void Main_UnhandledMergeFailure_Returns1()
		{
			using (var group = new GroupOfConflictingLiftFiles())
			{
				using (new FailureSimulator("LiftMerger.FindEntryById"))
				{
					Assert.AreEqual(1, DoMerge(group));
				}
			}
		}

		private int DoMerge(GroupOfConflictingLiftFiles group)
		{
			MergeSituation.PushRevisionsToEnvironmentVariables("bob", "-123", "sally", "-456");
			MergeOrder.PushToEnvironmentVariables(group.Folder.Path);
			return ChorusMerge.Program.Main(new string[] {group.BobFile.Path,group.AncestorFile.Path,group.SallyFile.Path});
		}
	}
}
