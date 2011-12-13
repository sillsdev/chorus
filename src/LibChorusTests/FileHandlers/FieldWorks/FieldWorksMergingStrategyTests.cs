using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the merge override capabilities of the FieldWorksMergingStrategy implementation of the IMergeStrategy interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksMergingStrategyTests
	{
		private ListenerForUnitTests _eventListener;
		private FieldWorksMergingStrategy _fwMergeStrategy;
		private MetadataCache _mdc;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = new MetadataCache();
			_mdc.AddCustomPropInfo("LexSense", new FdoPropertyInfo("Paradigm", DataType.MultiString, true));
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_mdc = null;
		}

		[SetUp]
		public void TestSetup()
		{
			_eventListener = new ListenerForUnitTests();
			_fwMergeStrategy = new FieldWorksMergingStrategy(new NullMergeSituation(), _mdc);
		}

		[TearDown]
		public void TestTeardown()
		{
			_eventListener = null;
			_fwMergeStrategy = null;
		}

		[Test]
		public void NewerTimestampInOurWins()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='LexEntry' guid='oldie' >
<DateModified val='2000-1-1 23:59:59.000' />
</rt>
</classdata>";
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("2002-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void NewerTimestampInTheirsWins()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='LexEntry' guid='oldie' >
<DateModified val='2000-1-1 23:59:59.000' />
</rt>
</classdata>";
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("2002-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Checksum_Conflict_Merges_To_Zero()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='WfiWordform' guid='someguid' >
<Checksum val='1' />
</rt>
</classdata>";
			var ourContent = commonAncestor.Replace("val='1'", "val='2'");
			var theirContent = commonAncestor.Replace("val='1'", "val='3'");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("val='0'") || result.Contains("val=\"0\""));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Checksum_Missing_From_Ours_Merges_To_Zero()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='WfiWordform' guid='someguid' >
<Checksum val='1' />
</rt>
</classdata>";
			var ourContent = commonAncestor.Replace("<Checksum val='1' />", null);
			var theirContent = commonAncestor.Replace("val='1'", "val='3'");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("val='0'") || result.Contains("val=\"0\""));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Checksum_Missing_From_Theirs_Merges_To_Zero()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='WfiWordform' guid='someguid' >
<Checksum val='1' />
</rt>
</classdata>";
			var ourContent = commonAncestor.Replace("val='1'", "val='2'");
			var theirContent = commonAncestor.Replace("<Checksum val='1' />", null);

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("val='0'") || result.Contains("val=\"0\""));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void MultiStrCustomPropertyMergesRight()
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

			XmlNode sueNode;
			XmlNode ancestorNode;
			var randyNode = FieldWorksTestServices.CreateNodes(commonAncestor, randy, sue, out sueNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, randyNode, sueNode, ancestorNode);
			var resElement = XElement.Parse(result);
			Assert.IsTrue(resElement.Elements("Custom").Count() == 1);
			var aStrNodes = resElement.Element("Custom").Elements("AStr");
			Assert.IsTrue(aStrNodes.Count() == 2);
			var aStrNode = aStrNodes.ElementAt(0);
			Assert.IsTrue(aStrNode.Attribute("ws").Value == "qaa-x-ezpi");
			Assert.AreEqual("saglo, yzaglo, rzaglo, wzaglo, nzaglo, -", aStrNode.Element("Run").Value);
			aStrNode = aStrNodes.ElementAt(1);
			Assert.IsTrue(aStrNode.Attribute("ws").Value == "zpi");
			Assert.AreEqual("saklo, yzaklo, rzaklo, wzaklo, nzaklo, -", aStrNode.Element("Run").Value);

			_eventListener.AssertExpectedConflictCount(1);
			_eventListener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
			_eventListener.AssertExpectedChangesCount(1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void EnsureReferenceCollectionDoesNotConflictOnMerge()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='three' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
<objsur guid='weAdded' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='theyAdded' t='r' />
<objsur guid='three' t='r' />
<objsur guid='four' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);
			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);

			_eventListener.AssertExpectedConflictCount(0);

			var resElement = XElement.Parse(result);
			var refTargets = resElement.Descendants("objsur");
			Assert.AreEqual(8, refTargets.Count());
			// Make sure they are the correct six.
			Assert.IsNotNull((from target in refTargets
								  where target.Attribute("guid").Value == "one"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "two"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "three"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "four"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "five"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "six"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "theyAdded"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "weAdded"
							  select target).FirstOrDefault());
		}

		[Test]
		public void EnsureReferenceCollectionDoesNotConflictOnMergeWhenBothMadeTheSameChanges()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='three' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
<objsur guid='bothAdded' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='bothAdded' t='r' />
<objsur guid='five' t='r' />
<objsur guid='four' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);
			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);

			_eventListener.AssertExpectedConflictCount(0);

			var resElement = XElement.Parse(result);
			var refTargets = resElement.Descendants("objsur");
			Assert.AreEqual(6, refTargets.Count());
			// Make sure they are the correct six.
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "one"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "two"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "four"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "six"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "five"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "bothAdded"
							  select target).FirstOrDefault());
		}
	}
}