using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.annotations;
using Chorus.Utilities;
using NUnit.Framework;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class AnnotationIndexTests
	{
		private IProgress _progress = new ConsoleProgress();

		[Test]
		public void GetAll_NoneInIndex_Returns0()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'/>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddIndex(index, _progress);
				Assert.AreEqual(0, index.GetAll().Count());
			}
		}

		[Test]
		public void GetAll_0OutOf1MatchFilter_Returns0()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
<annotation class='note' ref='black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddIndex(index, _progress);
				Assert.AreEqual(0, index.GetAll().Count());
			}
		}

		[Test]
		public void GetAll_2OutOf3MatchFilter_Returns2()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
<annotation class='question' ref='red'/>
<annotation class='question' ref='blue'/>
<annotation class='note' ref='black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddIndex(index, _progress);
				Assert.AreEqual(2, index.GetAll().Count());
			}
		}

		[Test]
		public void GetMatchesOfKey_ReturnsMatchingItems()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
<annotation class='question' ref='red'/>
<annotation class='question' ref='blue'/>
<annotation class='question' ref='blue'/>
<annotation class='note' ref='black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddIndex(index, _progress);
				Assert.AreEqual(2, index.GetMatchesOfKey("blue").Count());
			}
		}

		[Test]
		public void GetMatchesOfKey_Has0Matches_ReturnsNone()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
<annotation class='question' ref='red'/>
<annotation class='note' ref='black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddIndex(index, _progress);
				Assert.AreEqual(0, index.GetMatchesOfKey("blue").Count());
			}
		}


		[Test]
		public void GetMatches_Has0Matches_ReturnsNone()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'/>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddIndex(index, _progress);
				Assert.AreEqual(0, index.GetMatches(key=>key.Contains("b"), _progress).Count());
			}
		}


		[Test]
		public void GetMatches_Has2Matches_Returns2()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
<annotation class='question' ref='red'/>
<annotation class='question' ref='blue'/>
<annotation class='question' ref='blue'/>
<annotation class='note' ref='black'/>
</notes>"))
			{
				var index = new IndexOfRefsOfQuestionAnnotations();
				r.AddIndex(index, _progress);
				Assert.AreEqual(2, index.GetMatches(key => key.Contains("b"), _progress).Count());
			}
		}


	}

	class IndexOfRefsOfQuestionAnnotations : AnnotationIndex
	{
		public IndexOfRefsOfQuestionAnnotations() : base(a => a.ClassName == "question", a => a.Ref)
		{
		}

	}
}