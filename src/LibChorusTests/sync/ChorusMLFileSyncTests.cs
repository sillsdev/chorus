using System;
using System.IO;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using LibChorus.Tests.merge;
using NUnit.Framework;
using SIL.Xml;

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

					string notesFile = ChorusNotesMergeEventListener.GetChorusNotesFilePath(sally.UserFile.Path);
					Console.WriteLine("notesFile '{0}'", notesFile);
					Assert.IsTrue(File.Exists(notesFile), "Conflict file should have been in working set");
					Assert.IsTrue(sally.Synchronizer.Repository.GetFileIsInRepositoryFromFullPath(notesFile),"Notes file should have been added to repository");

				}
			}
		}

		[Test]
		public void MergeConflictFiles_CheckIsMutableIsUsedToSkipMergingMessages()
		{
			//NB: we can't actualy unit test for this, since it is just a performance improvement, but
			//this test let me watch in the debugger to make sure it skipped trying to merge the message
			using (
				GroupOfConflictFiles group = new GroupOfConflictFiles("<notes version='0'><annotation guid='111'><message guid='123'>I am thirsty</message></annotation></notes>",
																	  "<notes version='0'><annotation guid='111'><message guid='123'>I am thirsty</message></annotation></notes>",
																	  "<notes version='0'><annotation guid='111'><message guid='123'>I am thirsty</message><message guid='222'>Me too.</message></annotation></notes>")
				)
			{
				MergeOrder order = new MergeOrder(  group.BobFile.Path,
													group.AncestorFile.Path,
													group.SallyFile.Path, new NullMergeSituation());
				new ChorusNotesFileHandler().Do3WayMerge(order);

				XmlDocument doc = new XmlDocument();
				doc.Load(group.BobFile.Path);
				Assert.AreEqual(1, doc.SelectNodes("notes/annotation").Count);
				Assert.AreEqual(2, doc.SelectNodes("notes/annotation/message").Count);
			}
		}

		[Test]
		public void MergeConflictFiles_AncestorDidNotExist()
		{
			using (GroupOfConflictFiles group = new GroupOfConflictFiles("",
																	  "<notes><annotation guid='bobGuid'/></notes>",
																	  "<notes><annotation guid='sallyGuid'/></notes>"))
			{
				MergeOrder order = new MergeOrder(group.BobFile.Path,
												  group.AncestorFile.Path, group.SallyFile.Path, new NullMergeSituation());
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
