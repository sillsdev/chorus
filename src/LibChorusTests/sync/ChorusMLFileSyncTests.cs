using System;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge;
using NUnit.Framework;

namespace LibChorus.Tests.sync
{
	[TestFixture]
	[Category("Sync")]
	public class ChorusNotesFileSyncTests
	{
		[Test]
		public void ConflictFileIsCheckedIn()
		{
			using (RepositoryWithFilesSetup bob =  RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			{
				using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
				{
					bob.ReplaceSomething("bob");
					bob.AddAndCheckIn();
					sally.ReplaceSomething("sally");
					sally.CheckinAndPullAndMerge(bob);

					string xmlConflictFile = ChorusNotesMergeEventListener.GetXmlConflictFilePath(sally.UserFile.Path);
					Console.WriteLine("xmlConflictFile '{0}'", xmlConflictFile);
					Assert.IsTrue(File.Exists(xmlConflictFile), "Conflict file should have been in working set");
					Assert.IsTrue(sally.Synchronizer.Repository.GetFileIsInRepositoryFromFullPath(xmlConflictFile),"Conflict file should have been in repository");

				}
			}
		}


		[Test]
		public void MergeConflictFiles_AncestorDidNotExist()
		{

			using (
				GroupOfConflictFiles group = new GroupOfConflictFiles("",
																	  "<notes><annotation guid='bobGuid'/></notes>",
																	  "<notes><annotation guid='sallyGuid'/></notes>")
				)
			{
				MergeOrder order = new MergeOrder(group.BobFile.Path,
												  string.Empty, group.SallyFile.Path, new NullMergeSituation());
				new ChorusNotesFileHandler().Do3WayMerge(order);

				XmlDocument doc = new XmlDocument();
				doc.Load(group.BobFile.Path);
				Assert.AreEqual(2, doc.SelectNodes("notes/annotation").Count);

			}
		}

		[Test]
		public void MergeConflictFiles_AncestorExistsButNoConflicts()
		{
			using (
				GroupOfConflictFiles group = new GroupOfConflictFiles("<notes/>",
																	  "<notes><annotation guid='bobGuid'/></notes>",
																	  "<notes><annotation guid='sallyGuid'/></notes>")
				)
			{
				MergeOrder order = new MergeOrder( group.BobFile.Path,
												  group.AncestorFile.Path, group.SallyFile.Path, new NullMergeSituation());
				new ChorusNotesFileHandler().Do3WayMerge(order);

				XmlDocument doc = new XmlDocument();
				doc.Load(group.BobFile.Path);
				Assert.AreEqual(2, doc.SafeSelectNodes("notes/annotation").Count);

			}
		}


	}
}
