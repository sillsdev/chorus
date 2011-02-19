using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chorus.notes;
using Chorus.Utilities;
using NUnit.Framework;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.notes
{

	[TestFixture]
	public class IndexOfAllAnnotationsByKeyTests
	{
		private IProgress _progress = new ConsoleProgress();

		[Test]
		public void GetMatchesByKey_HasTwoMatches_Found()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'></notes>"))
			{
				r.AddAnnotation(new Annotation("foobar", "lift://blah.lift?id=fooid", "somepath"));
				r.AddAnnotation(new Annotation("question", "lift://blah.lift?id=fooid", "somepath"));

				 var index = new SubClassForTest("id");
				 r.AddObserver(index, _progress);
				Assert.AreEqual(2, index.GetMatchesByKey("fooid").Count());
				Assert.AreEqual(0, index.GetMatchesByKey("222").Count());
			}
		}


		[Test]
		public void GetMatchesByKey_HasAnnotationWithoutRef_DoesntCrash()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'></notes>"))
			{
				r.AddAnnotation(new Annotation("question", "lift://blah.lift", "somepath"));

				var index = new SubClassForTest("id");
				r.AddObserver(index, _progress);
				Assert.AreEqual(0, index.GetMatchesByKey("222").Count());
			}
		}


		[Test]
		public void Remove_FromConstructorOnlyAnnotationsOfKey_RemovesIt()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var a = new Annotation("note", "blah://blah?id=blue", "");
				r.AddAnnotation(a);
				var blues = r.GetMatchesByPrimaryRefKey("blue");
				Assert.AreEqual(a, blues.First());
				r.Remove(a);
				blues = r.GetMatchesByPrimaryRefKey("blue");
				Assert.AreEqual(0, blues.Count(), "should be none left");
			}
		}


		[Test]
		public void Remove_FromXmlOnlyAnnotationsOfKey_RemovesIt()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='note' ref='blah://blah?id=blue'/>
</notes>"))
			{
				var blues = r.GetMatchesByPrimaryRefKey("blue");
				r.Remove(blues.First());
				blues = r.GetMatchesByPrimaryRefKey("blue");
				Assert.AreEqual(0, blues.Count(), "should be none left");
			}
		}

		[Test]
		public void Remove_IndexAddedAfterRepoConstruction_RemovesIt()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='note' ref='blah://blah?id=blue'/>
</notes>"))
			{
				var index = new SubClassForTest("id");
				r.AddObserver(index, _progress);
				var blues = index.GetMatchesByKey("blue");
				r.Remove(blues.First());
				blues = index.GetMatchesByKey("blue");
				Assert.AreEqual(0, blues.Count(), "should be none left");
			}
		}

		[Test]
		public void Remove_2AnnotationsWithSameTarget_OnlyRemoves1FromIndex()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='question' ref='blah://blah?id=blue'/>
<annotation class='note' ref='blah://blah?id=blue'/>
</notes>"))
			{
				var index = new SubClassForTest("id");
				r.AddObserver(index, _progress);
				var blues = index.GetMatchesByKey("blue");
				r.Remove(blues.First());
				blues = index.GetMatchesByKey("blue");
				Assert.AreEqual(1, blues.Count(), "should be one left");
			}
		}

		class SubClassForTest: IndexOfAllAnnotationsByKey
		{
			public SubClassForTest(string nameOfParameterInRefToIndex) : base(nameOfParameterInRefToIndex)
			{
			}
		}
	}


}
