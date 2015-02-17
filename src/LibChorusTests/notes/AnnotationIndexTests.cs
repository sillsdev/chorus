using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.notes;
using Chorus.Utilities;
using NUnit.Framework;
using SIL.Progress;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class AnnotationIndexTests
	{
		private IProgress _progress = new ConsoleProgress();

		[Test]
		public void GetAll_NoneInIndex_Returns0()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(0, index.GetAll().Count());
			}
		}

		[Test]
		public void GetAll_0OutOf1MatchFilter_Returns0()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='note' ref='file://black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(0, index.GetAll().Count());
			}
		}

		[Test]
		public void GetAll_2OutOf3MatchFilter_Returns2()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='question' ref='file://red'/>
<annotation class='question' ref='file://blue'/>
<annotation class='note' ref='file://black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(2, index.GetAll().Count());
			}
		}

		[Test]
		public void GetMatchesOfKey_ReturnsMatchingItems()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='question' ref='file://red'/>
<annotation class='question' ref='file://blue'/>
<annotation class='question' ref='file://blue'/>
<annotation class='note' ref='file://black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(2, index.GetMatchesByKey("file://blue").Count());
			}
		}

		[Test]
		public void GetMatchesOfKey_Has0Matches_ReturnsNone()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='question' ref='file://red'/>
<annotation class='note' ref='file://black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(0, index.GetMatchesByKey("blue").Count());
			}
		}


		[Test]
		public void GetMatches_Has0Matches_ReturnsNone()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(0, index.GetMatches(key=>key.Contains("b"), _progress).Count());
			}
		}


		[Test]
		public void GetMatches_PredicateGivesNullForOne_ReturnsIt()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='question'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(1, index.GetMatches(key => string.IsNullOrEmpty(key), _progress).Count());
			}
		}

		[Test]
		public void GetMatches_Has2Matches_Returns2()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
<annotation class='question' ref='file://red'/>
<annotation class='question' ref='file://blue'/>
<annotation class='question' ref='file://blue'/>
<annotation class='note' ref='file://black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddObserver(index, _progress);
				Assert.AreEqual(2, index.GetMatches(key => key.Contains("b"), _progress).Count());
			}
		}


	}

	class IndexOfRefsOfQuestionAnnotations : AnnotationIndex
	{
		public IndexOfRefsOfQuestionAnnotations() : base(a => a.ClassName == "question", a => a.RefStillEscaped)
		{
		}

	}
}