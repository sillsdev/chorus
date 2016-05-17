using System;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.notes;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Xml;

namespace LibChorus.Tests.FileHandlers
{
	/// <summary>
	/// Test the ChorusNotesFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class ChorusNotesFileHandlerTests
	{
		private IChorusFileTypeHandler _chorusNotesFileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_chorusNotesFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
									  where handler.GetType().Name == "ChorusNotesFileHandler"
									  select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_chorusNotesFileHandler = null;
		}

		[Test]
		public void GetExtensionsOfKnownTextFileTypesIsChorusNotes()
		{
			var extensions = _chorusNotesFileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual(AnnotationRepository.FileExtension, extensions[0]);
		}

		[Test]
		public void CanValidate_IsFalse()
		{
			Assert.IsFalse(_chorusNotesFileHandler.CanValidateFile(null));
		}

		[Test]
		public void ValidateFile_Throws()
		{
			Assert.Throws<NotImplementedException>(() => _chorusNotesFileHandler.ValidateFile(null, null));
		}

		[Test]
		public void CanMergeAFile()
		{
			using (var tempFile = TempFile.WithExtension("." + AnnotationRepository.FileExtension))
			{
				File.WriteAllText(tempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<notes />");
				Assert.IsTrue(_chorusNotesFileHandler.CanMergeFile(tempFile.Path));
			}
		}

		[Test]
		public void EnsureMaximumFileSize_HasNoLimit()
		{
			Assert.AreEqual(UInt32.MaxValue, _chorusNotesFileHandler.MaximumFileSize);
		}

		/// <summary>
		/// The purpose of this test is to compare merge operations using the old and new merge approaches
		/// to make sure the CData in the 'message' elements is properly managed.
		/// I (RandyR) saw a merged data set where the CData material was not maintained,
		/// but this new test was not able to produce such results with the old or new merge approaches.
		/// I even checked to make sure the ChorusNotes listener handled them properly (it did),
		/// and the AnnotationRepository write mechanism worked right (it did).
		/// </summary>
		[Test]
		public void EnsureMergedCData_IsRetained()
		{
			using (var common = TempFile.WithFilename("common.ChorusNotes"))
			using (var ours = TempFile.WithFilename("ours.ChorusNotes"))
			using (var theirs = TempFile.WithFilename("theirs.ChorusNotes"))
			{
				const string commonData =
@"<?xml version='1.0' encoding='utf-8'?>
<notes
	version='0'>
	<annotation
		class='mergeConflict'
		ref='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
		guid='1cb66d60-90d5-4367-95b1-b7b41eb8986d'>
		<message
			author='merger'
			status='open'
			guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
			date='2012-07-20T14:18:35Z'>Entry 'pintu': user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.<![CDATA[<conflict
	typeGuid='3d9ba4ac-4a25-11df-9879-0800200c9a66'
	class='Chorus.merge.xml.generic.EditedVsRemovedElementConflict'
	relativeFilePath='Linguistics\Lexicon\Lexicon.lexdb'
	type='Removed Vs Edited Element Conflict'
	guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
	date='2012-07-20T14:18:35Z'
	whoWon='user57'
	htmlDetails='&lt;head&gt;&lt;style type='text/css'&gt;&lt;/style&gt;&lt;/head&gt;&lt;body&gt;&lt;div class='description'&gt;Entry &quot;pintu&quot;: user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.&lt;/div&gt;&lt;div class='alternative'&gt;user57's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 &lt;span style=&quot;text-decoration: line-through; color: red&quot;&gt;13:46:3.625&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;14:14:20.218&lt;/span&gt;&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;AStr&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Run&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;a&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;door&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Run&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/AStr&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='alternative'&gt;user57@tpad2's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='mergechoice'&gt;The merger kept the change made by user57&lt;/div&gt;&lt;/body&gt;'
	contextPath='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
	contextDataLabel='Entry &quot;pintu&quot;'>
	<MergeSituation
		alphaUserId='user57'
		betaUserId='user57@tpad2'
		alphaUserRevision='306520fcc148'
		betaUserRevision='5aa248710fbc'
		path='Linguistics\Lexicon\Lexicon.lexdb'
		conflictHandlingMode='WeWin' />
</conflict>]]></message>
		<message
			author='user57'
			status='closed'
			date='2012-07-20T22:49:03Z'
			guid='bf43783e-eca1-4b0f-bacd-6fe168d7d616'></message>
	</annotation>
</notes>";
				File.WriteAllText(common.Path, commonData);

				const string ourData =
@"<?xml version='1.0' encoding='utf-8'?>
<notes
	version='0'>
	<annotation
		class='mergeConflict'
		ref='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
		guid='1cb66d60-90d5-4367-95b1-b7b41eb8986d'>
		<message
			author='merger'
			status='open'
			guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
			date='2012-07-20T14:18:35Z'>Entry 'pintu': user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.<![CDATA[<conflict
	typeGuid='3d9ba4ac-4a25-11df-9879-0800200c9a66'
	class='Chorus.merge.xml.generic.EditedVsRemovedElementConflict'
	relativeFilePath='Linguistics\Lexicon\Lexicon.lexdb'
	type='Removed Vs Edited Element Conflict'
	guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
	date='2012-07-20T14:18:35Z'
	whoWon='user57'
	htmlDetails='&lt;head&gt;&lt;style type='text/css'&gt;&lt;/style&gt;&lt;/head&gt;&lt;body&gt;&lt;div class='description'&gt;Entry &quot;pintu&quot;: user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.&lt;/div&gt;&lt;div class='alternative'&gt;user57's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 &lt;span style=&quot;text-decoration: line-through; color: red&quot;&gt;13:46:3.625&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;14:14:20.218&lt;/span&gt;&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;AStr&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Run&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;a&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;door&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Run&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/AStr&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='alternative'&gt;user57@tpad2's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='mergechoice'&gt;The merger kept the change made by user57&lt;/div&gt;&lt;/body&gt;'
	contextPath='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
	contextDataLabel='Entry &quot;pintu&quot;'>
	<MergeSituation
		alphaUserId='user57'
		betaUserId='user57@tpad2'
		alphaUserRevision='306520fcc148'
		betaUserRevision='5aa248710fbc'
		path='Linguistics\Lexicon\Lexicon.lexdb'
		conflictHandlingMode='WeWin' />
</conflict>]]></message>
		<message
			author='user57'
			status='closed'
			date='2012-07-20T22:49:03Z'
			guid='bf43783e-eca1-4b0f-bacd-6fe168d7d616'></message>
		<message
			author='user57'
			status='open'
			date='2012-07-20T23:11:24Z'
			guid='524786f4-8e27-4ebf-b7ea-02846723c2d8'>Chorus seems to have chosen the better gloss anyway.</message>
	</annotation>
</notes>";
				File.WriteAllText(ours.Path, ourData);

				const string theirData =
@"<?xml version='1.0' encoding='utf-8'?>
<notes
	version='0'>
	<annotation
		class='mergeConflict'
		ref='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
		guid='1cb66d60-90d5-4367-95b1-b7b41eb8986d'>
		<message
			author='merger'
			status='open'
			guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
			date='2012-07-20T14:18:35Z'>Entry 'pintu': user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.<![CDATA[<conflict
	typeGuid='3d9ba4ac-4a25-11df-9879-0800200c9a66'
	class='Chorus.merge.xml.generic.EditedVsRemovedElementConflict'
	relativeFilePath='Linguistics\Lexicon\Lexicon.lexdb'
	type='Removed Vs Edited Element Conflict'
	guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
	date='2012-07-20T14:18:35Z'
	whoWon='user57'
	htmlDetails='&lt;head&gt;&lt;style type='text/css'&gt;&lt;/style&gt;&lt;/head&gt;&lt;body&gt;&lt;div class='description'&gt;Entry &quot;pintu&quot;: user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.&lt;/div&gt;&lt;div class='alternative'&gt;user57's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 &lt;span style=&quot;text-decoration: line-through; color: red&quot;&gt;13:46:3.625&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;14:14:20.218&lt;/span&gt;&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;AStr&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Run&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;a&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;door&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Run&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/AStr&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='alternative'&gt;user57@tpad2's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='mergechoice'&gt;The merger kept the change made by user57&lt;/div&gt;&lt;/body&gt;'
	contextPath='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
	contextDataLabel='Entry &quot;pintu&quot;'>
	<MergeSituation
		alphaUserId='user57'
		betaUserId='user57@tpad2'
		alphaUserRevision='306520fcc148'
		betaUserRevision='5aa248710fbc'
		path='Linguistics\Lexicon\Lexicon.lexdb'
		conflictHandlingMode='WeWin' />
</conflict>]]></message>
		<message
			author='user57'
			status='closed'
			date='2012-07-20T22:49:03Z'
			guid='bf43783e-eca1-4b0f-bacd-6fe168d7d616'></message>
		<message
			author='user57'
			status='closed'
			date='2012-07-20T23:11:27Z'
			guid='a0481907-3fff-45a2-bb1c-961c3198c86a'></message>
	</annotation>
</notes>";
				File.WriteAllText(theirs.Path, theirData);

				// Do it the new way.
				_chorusNotesFileHandler.Do3WayMerge(new MergeOrder(ours.Path, common.Path, theirs.Path, new NullMergeSituation()));
				var newWayResult = File.ReadAllText(ours.Path);
				CheckResults(false, ourData, newWayResult);

				// Do it the old way via XmlMerge.
				var mergeSit = new MergeSituation(ours.Path, "Me", "8", "You", "9", MergeOrder.ConflictHandlingModeChoices.WeWin);
				var merger = new XmlMerger(mergeSit);
				var listener = new ListenerForUnitTests();
				merger.MergeStrategies.SetStrategy("annotation", ElementStrategy.CreateForKeyedElement("guid", false));
				var messageStrategy = ElementStrategy.CreateForKeyedElement("guid", false);
				messageStrategy.IsImmutable = true;
				merger.MergeStrategies.SetStrategy("message", messageStrategy);
				merger.EventListener = listener;
				var ourDataNode = XmlUtilities.GetDocumentNodeFromRawXml(ourData.Replace("<?xml version='1.0' encoding='utf-8'?>", null).Trim(), new XmlDocument());
				var mergeResult = merger.Merge(
					ourDataNode.ParentNode, ourDataNode,
					XmlUtilities.GetDocumentNodeFromRawXml(theirData.Replace("<?xml version='1.0' encoding='utf-8'?>", null).Trim(), new XmlDocument()),
					XmlUtilities.GetDocumentNodeFromRawXml(commonData.Replace("<?xml version='1.0' encoding='utf-8'?>", null).Trim(), new XmlDocument()));
				var oldWayResult = mergeResult.MergedNode.OuterXml;
				CheckResults(false, ourData, oldWayResult);

				// Compare old and new results.
				CheckResults(true, newWayResult, oldWayResult);

				using (var log = new ChorusNotesMergeEventListener(ours.Path))
				{
					// The purpose here is to make sure that the listener works correctly regarding maintaining CData.
					// I (RandyR) saw a merged data set where the CData material was not maintained.
				}
				var result = File.ReadAllText(ours.Path);
				CheckResults(true, newWayResult, result);

				var doc = new XmlDocument();
				doc.Load(ours.Path);
				// This is how the AnnotationRepository class writes out an updated ChorusNotes file.
				using (var writer = XmlWriter.Create(ours.Path, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					doc.Save(writer);
				}
				CheckResults(true, result, File.ReadAllText(ours.Path));
			}
		}

		private static void CheckResults(bool expectedToMatch, string source, string target)
		{
			Assert.IsTrue(target.Contains("524786f4-8e27-4ebf-b7ea-02846723c2d8"));
			Assert.IsTrue(target.Contains("a0481907-3fff-45a2-bb1c-961c3198c86a"));
			Assert.IsTrue(target.Contains("<![CDATA[<conflict"));
			CompareResults(expectedToMatch, source, target);
		}

		private static void CompareResults(bool expectedToMatch, string source, string target)
		{
			Assert.AreEqual(
				expectedToMatch,
				XmlUtilities.AreXmlElementsEqual(
					RemoveDeclaration(source),
					RemoveDeclaration(target)));
		}

		private static string RemoveDeclaration(string data)
		{
			return data.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", null).Replace("<?xml version='1.0' encoding='utf-8'?>", null).Trim();
		}
	}
}
