using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.UI.Clone;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class GetSharedProjectModelTests
	{
		[Test]
		public void HasNoExtantRepositories()
		{
			using (var hasProject = new TemporaryFolder("hasRepo"))
			{
				var newFile = Path.Combine(hasProject.Path, "test.txt");
				File.WriteAllText(newFile, "some stuff");
				Assert.AreEqual(0, GetSharedProjectModel.ExtantRepoIdentifiers(hasProject.Path, null).Count);
			}
		}

		[Test]
		public void HasExtantRepositories()
		{
			using (var parentFolder = new TemporaryFolder("parentFolder"))
			using (var childFolder = new TemporaryFolder(parentFolder, "childFolder"))
			{
				var newFile = Path.Combine(childFolder.Path, "test.txt");
				File.WriteAllText(newFile, "some stuff");
				var repo = new HgRepository(childFolder.Path, new NullProgress());
				repo.Init();
				repo.AddAndCheckinFile(newFile);

				var extantRepoIdentifiers = GetSharedProjectModel.ExtantRepoIdentifiers(parentFolder.Path, null);
				Assert.AreEqual(1, extantRepoIdentifiers.Count);
				Assert.IsTrue(extantRepoIdentifiers.ContainsKey(repo.Identifier));
				Assert.That(extantRepoIdentifiers[repo.Identifier], Is.EqualTo("childFolder"));
			}
		}

		[Test]
		public void ExtantRepositories_FindsRepoInOtherRepoLocation()
		{
			using (var parentFolder = new TemporaryFolder("parentFolder"))
			using (var childFolder = new TemporaryFolder(parentFolder, "childFolder"))
			using (var otherFolder = new TemporaryFolder(childFolder, "otherRepositories"))
			using (var repoFolder = new TemporaryFolder(otherFolder, "LIFT"))
			{
				var newFile = Path.Combine(repoFolder.Path, "test.txt");
				File.WriteAllText(newFile, "some stuff");
				var repo = new HgRepository(childFolder.Path, new NullProgress());
				repo.Init();
				repo.AddAndCheckinFile(newFile);

				var extantRepoIdentifiers = GetSharedProjectModel.ExtantRepoIdentifiers(parentFolder.Path, "otherRepositories");
				Assert.AreEqual(1, extantRepoIdentifiers.Count);
				Assert.IsTrue(extantRepoIdentifiers.ContainsKey(repo.Identifier));
				Assert.That(extantRepoIdentifiers[repo.Identifier], Is.EqualTo("childFolder"));
			}
		}
	}
}
