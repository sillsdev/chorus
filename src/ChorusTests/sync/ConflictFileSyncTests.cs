using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace Chorus.Tests.merge
{
	[TestFixture]
	public class ConflictFileSyncTests
	{
		[Test]
		public void ConflictFileIsCheckedIn()
		{
			using (UserWithFiles bob = new UserWithFiles("bob"))
			{
				using (UserWithFiles sally = new UserWithFiles("sally", bob))
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
	}
}