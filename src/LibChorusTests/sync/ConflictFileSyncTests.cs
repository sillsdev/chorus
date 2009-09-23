using System;
﻿using System.IO;
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
	public class ConflictFileSyncTests
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

					string xmlConflictFile = XmlLogMergeEventListener.GetXmlConflictFilePath(sally.UserFile.Path);
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
																	  "<conflicts><conflict guid='bobGuid'/></conflicts>",
																	  "<conflicts><conflict guid='sallyGuid'/></conflicts>")
				)
			{
				MergeOrder order = new MergeOrder(group.BobFile.Path,
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
				MergeOrder order = new MergeOrder( group.BobFile.Path,
												  group.AncestorFile.Path, group.SallyFile.Path, new NullMergeSituation());
				new ConflictFileTypeHandler().Do3WayMerge(order);

				XmlDocument doc = new XmlDocument();
				doc.Load(group.BobFile.Path);
				Assert.AreEqual(2, doc.SafeSelectNodes("conflicts/conflict").Count);

			}
		}


	}
}