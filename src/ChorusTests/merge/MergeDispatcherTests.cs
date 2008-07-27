using System.IO;
using System.Xml;
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
			using (GroupOfConflictingLiftFiles group = new GroupOfConflictingLiftFiles())
			{
				MergeOrder order = new MergeOrder(MergeOrder.ConflictHandlingMode.TheyWin, group.BobFile.Path, group.AncestorFile.Path, group.SallyFile.Path);
				MergeDispatcher.Go(order);

				Assert.IsTrue(File.Exists(group.BobTextConflictsPath));
				Assert.AreNotEqual(string.Empty, File.ReadAllText(group.BobTextConflictsPath));
			}
		}

		[Test, Ignore("not yet")]
		public void MergeConflictFiles_AncestorExistsButNoConflicts()
		{
			using (
				GroupOfConflictFiles group = new GroupOfConflictFiles("<conflicts/>",
																	  "<conflicts><conflict guid='bobGuid'/></conflicts>",
																	  "<conflicts><conflict guid='sallyGuid'/></conflicts>")
				)
			{
				MergeOrder order = new MergeOrder(MergeOrder.ConflictHandlingMode.WeWin, group.BobFile.Path,
												  group.AncestorFile.Path, group.SallyFile.Path);
				MergeDispatcher.Go(order);

				XmlDocument doc = new XmlDocument();
				doc.Load(group.BobFile.Path);
				Assert.AreEqual(2, doc.SelectNodes("conflicts/conflict").Count);

			}


		}

		[Test, Ignore("HOW TO TEST THIS?  WILL THIS EVEN WORK?")]
		public void MergeConflictFiles_AncestorDidNotExist()
		{


		}

			[Test, Ignore("not yet")]
		public void NoConflictFileB4_NoConflicts_HaveConflictFileAfter()
		{
		}
	}
}