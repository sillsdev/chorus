using System.IO;
using Chorus.merge.xml.generic;
using Chorus.Properties;
using LibChorus.Tests.merge;
using NUnit.Framework;

namespace LibChorus.Tests.Integration
{
	/// <summary>
	/// This class tests a complete series of operations over several units including:
	/// merging and syncing, Some tests may also include conflicts and the respective ChorusNotes file.
	/// </summary>
	[TestFixture]
	public class MergeIntegrationTests
	{
		[Test]
		public void EnsureRightPersonMadeChanges()
		{
			const string commonAncestor =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt
	class='LexSense'
	guid='a99e8509-a0eb-49fe-bd4b-ae337951e423'
	ownerguid='a17a7cff-59c8-4fdf-bb5c-3ec12b1f7b11'>
	<Custom
		name='Paradigm'>
		<AStr
			ws='qaa-x-ezpi'>
			<Run
				ws='qaa-x-ezpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
		</AStr>
	</Custom>
</rt>
</classdata>";
			const string sue =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt
	class='LexSense'
	guid='a99e8509-a0eb-49fe-bd4b-ae337951e423'
	ownerguid='a17a7cff-59c8-4fdf-bb5c-3ec12b1f7b11'>
	<Custom
		name='Paradigm'>
		<AStr
			ws='qaa-x-ezpi'>
			<Run
				ws='qaa-x-ezpi'>saglo, yzaglo, rzaglo, wzaglo, nzaglo, -</Run>
		</AStr>
	</Custom>
</rt>
</classdata>";
			const string randy =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt
	class='LexSense'
	guid='a99e8509-a0eb-49fe-bd4b-ae337951e423'
	ownerguid='a17a7cff-59c8-4fdf-bb5c-3ec12b1f7b11'>
	<Custom
		name='Paradigm'>
		<AStr
			ws='zpi'>
			<Run
				ws='zpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
		</AStr>
	</Custom>
</rt>
</classdata>";

			const string customPropData =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
	<CustomField
		class='LexEntry'
		destclass='7'
		key='LexEntryTone'
		listRoot='53241fd4-72ae-4082-af55-6b659657083c'
		name='Tone'
		type='ReferenceCollection' />
	<CustomField
		class='LexSense'
		key='LexSenseParadigm'
		name='Paradigm'
		type='String'
		wsSelector='-2' />
	<CustomField
		class='WfiWordform'
		key='WfiWordformCertified'
		name='Certified'
		type='Boolean' />
</AdditionalFields>";

			using (var sueRepo = new RepositoryWithFilesSetup("Sue", "LexSense.ClassData", commonAncestor))
			{
				var sueProjPath = sueRepo.ProjectFolder.Path;
				// Add model version number file.
				var modelVersionPathname = Path.Combine(sueProjPath, "ZPI.ModelVersion");
				File.WriteAllText(modelVersionPathname, AnnotationImages.kModelVersion);
				sueRepo.Repository.AddSansCommit(modelVersionPathname);
				// Add custom property data file.
				var customPropsPathname = Path.Combine(sueProjPath, "ZPI.CustomProperties");
				File.WriteAllText(customPropsPathname, customPropData);
				sueRepo.Repository.AddSansCommit(customPropsPathname);
				sueRepo.AddAndCheckIn();

				using (var randyRepo = RepositoryWithFilesSetup.CreateByCloning("Randy", sueRepo))
				{
					// By doing the clone first, we get the common starting state in both repos.
					sueRepo.WriteNewContentsToTestFile(sue);
					sueRepo.AddAndCheckIn();

					var mergeConflictsNotesFile = ChorusNotesMergeEventListener.GetChorusNotesFilePath(randyRepo.UserFile.Path);
					Assert.IsFalse(File.Exists(mergeConflictsNotesFile), "ChorusNotes file should NOT have been in working set.");
					randyRepo.WriteNewContentsToTestFile(randy);
					randyRepo.CheckinAndPullAndMerge(sueRepo);
					Assert.IsTrue(File.Exists(mergeConflictsNotesFile), "ChorusNotes file should have been in working set.");
					var notesContents = File.ReadAllText(mergeConflictsNotesFile);
					Assert.IsNotNullOrEmpty(notesContents);
					Assert.IsTrue(notesContents.Contains("Removed Vs Edited Element Conflict"));
					Assert.IsTrue(notesContents.Contains("Randy deleted this element"));
					Assert.IsTrue(notesContents.Contains("Sue edited it"));
					Assert.IsTrue(notesContents.Contains("The automated merger kept the change made by Sue."));
					Assert.IsTrue(notesContents.Contains("whoWon=\"Sue\""));
					Assert.IsTrue(notesContents.Contains("alphaUserId=\"Randy\""));
					Assert.IsTrue(notesContents.Contains("betaUserId=\"Sue\""));
				}
			}
		}
	}
}
