using System;
using System.IO;
using System.Linq;
using System.Threading;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.test;
using Chorus.VcsDrivers.Mercurial;
using Chorus.sync;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibChorus.Tests.FileHandlers
{
	/// <summary>
	/// Static class that tests (integration, not unit, since it involves so many units)
	/// to make sure a given file extension is, or is not, included in a repository,
	/// when committed by the Synchronizer class.
	///
	/// No filtering for large files is done at the repository commit level.
	/// </summary>
	public static class LargeFileIntegrationTestService
	{
		public static void TestThatALargeFileIsNotInRepository(string extension)
		{
			var pathToTestRoot = Path.Combine(Path.GetTempPath(), "LargeFileFilterTestFolder_" + extension + "_" + Guid.NewGuid());
			try
			{
				if (Directory.Exists(pathToTestRoot))
				{
					Thread.Sleep(2000);
					Directory.Delete(pathToTestRoot, true);
				}
				Directory.CreateDirectory(pathToTestRoot);

				var allHandlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.ToList();
				allHandlers.Add(new ChorusTestFileHandler());
				var handlerForExtension = allHandlers.FirstOrDefault(handler => handler.GetExtensionsOfKnownTextFileTypes().Contains(extension.ToLowerInvariant()))
					?? new DefaultFileTypeHandler();

				var goodFileName = "smallfry." + extension;
				var goodPathname = Path.Combine(pathToTestRoot, goodFileName);
				var goodFile = TempFile.WithFilename(goodPathname);
				File.WriteAllText(goodFile.Path, "Nice, short text.");

				var whopperFileName = "whopper." + extension;
				var whopperPathname = Path.Combine(pathToTestRoot, whopperFileName);
				var whopperFile = TempFile.WithFilename(whopperPathname);
				var whopperData = "whopperdata ";
				while (whopperData.Length < handlerForExtension.MaximumFileSize)
					whopperData += whopperData;
				File.WriteAllText(whopperFile.Path, whopperData);

				var progress = new NullProgress();
				var projectFolderConfiguration = new ProjectFolderConfiguration(pathToTestRoot);
				projectFolderConfiguration.IncludePatterns.Clear();
				projectFolderConfiguration.ExcludePatterns.Clear();
				projectFolderConfiguration.IncludePatterns.Add("*.*");
				RepositorySetup.MakeRepositoryForTest(pathToTestRoot, "Pesky", progress);
				var synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, progress);
				synchronizer.Repository.SetUserNameInIni("Pesky", progress);
				var syncOptions = new SyncOptions
					{
						// Basic commit. Nothing fancy.
						DoPullFromOthers = false,
						DoMergeWithOthers = false,
						DoSendToOthers = false,
						CheckinDescription = "Added"
					};

				var syncResults = synchronizer.SyncNow(syncOptions);
				Assert.That(syncResults.Succeeded, Is.True);

				projectFolderConfiguration.ExcludePatterns.Remove(ProjectFolderConfiguration.BareFolderReadmeFileName);
				Assert.That(projectFolderConfiguration.ExcludePatterns.Count, Is.EqualTo(2));
				Assert.That(projectFolderConfiguration.ExcludePatterns[0], Does.Contain(whopperFileName));

				var repo = new HgRepository(pathToTestRoot, progress);
				Assert.That(repo.GetFileExistsInRepo(goodFileName), Is.True, goodFileName);
				Assert.That(repo.GetFileExistsInRepo(whopperFileName), Is.False, whopperFileName);
			}
			finally
			{
				if (Directory.Exists(pathToTestRoot))
				{
					Thread.Sleep(2000);
					Directory.Delete(pathToTestRoot, true);
				}
			}
		}
	}
}