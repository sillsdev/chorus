using System.IO;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Tests.merge;
using NUnit.Framework;

namespace Chorus.Tests.sync
{
	[TestFixture]
	public class ConflictFileSyncTests
	{
		[Test]
		public void ConflictFileIsCheckedIn()
		{
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob"))
			{
				using (RepositoryWithFilesSetup sally = new RepositoryWithFilesSetup("sally", bob))
				{
					bob.ReplaceSomething("bob");
					bob.Checkin();
					sally.ReplaceSomething("sally");
					sally.CheckinAndPullAndMerge(bob);

					string xmlConflictFile = XmlLogMergeEventListener.GetXmlConflictFilePath(sally._liftFile.Path);
					Assert.IsTrue(File.Exists(xmlConflictFile), "Conflict file should have been in working set");
					Assert.IsTrue(sally.Repo.GetFileExistsInRepo(xmlConflictFile),"Conflict file should have been in repository");

				}
			}
		}


		[Test]
		public void MergeConflictFiles_AncestorDidNotExist()
		{

			using (
				GroupOfConflictFiles group = new GroupOfConflictFiles("",
																	  "<conflicts><conflict guid='bobGuid'/></conflicts>",
																	  "<conflicts><conflict guid='sallyGuid'/></conflicts>")
				)
			{
				MergeOrder order = new MergeOrder(MergeOrder.ConflictHandlingModeChoices.WeWin, group.BobFile.Path,
												  string.Empty, group.SallyFile.Path, new NullMergeSituation());
				new ConflictFileTypeHandler().Do3WayMerge(order);

				XmlDocument doc = new XmlDocument();
				doc.Load(group.BobFile.Path);
				Assert.AreEqual(2, doc.SelectNodes("conflicts/conflict").Count);

			}
		}

		[Test]
		public void MergeConflictFiles_AncestorExistsButNoConflicts()
		{
			using (
				GroupOfConflictFiles group = new GroupOfConflictFiles("<conflicts/>",
																	  "<conflicts><conflict guid='bobGuid'/></conflicts>",
																	  "<conflicts><conflict guid='sallyGuid'/></conflicts>")
				)
			{
				MergeOrder order = new MergeOrder(MergeOrder.ConflictHandlingModeChoices.WeWin, group.BobFile.Path,
												  group.AncestorFile.Path, group.SallyFile.Path, new NullMergeSituation());
				new ConflictFileTypeHandler().Do3WayMerge(order);

				XmlDocument doc = new XmlDocument();
				doc.Load(group.BobFile.Path);
				Assert.AreEqual(2, doc.SafeSelectNodes("conflicts/conflict").Count);

			}
		}

		/// <summary>
		/// Review: maybe this belongs in a different test fixture... it is a refugee from a removed class
		/// </summary>
		[Test]
		public void NoConflictFileB4_ConflictsEncountered_HaveConflictFileAfter()
		{
			using (GroupOfConflictingLiftFiles group = new GroupOfConflictingLiftFiles())
			{
				var situation = new NullMergeSituation();
				MergeOrder order = new MergeOrder(MergeOrder.ConflictHandlingModeChoices.TheyWin, group.BobFile.Path,
												  group.AncestorFile.Path, group.SallyFile.Path, situation);
				new LiftFileHandler().Do3WayMerge(order);

				Assert.IsTrue(File.Exists(group.BobTextConflictsPath));
				Assert.AreNotEqual(string.Empty, File.ReadAllText(group.BobTextConflictsPath));
			}
		}
	}
}