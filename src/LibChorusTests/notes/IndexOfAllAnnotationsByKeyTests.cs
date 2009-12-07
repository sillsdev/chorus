using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chorus.annotations;
using Chorus.Utilities;
using NUnit.Framework;

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

				 var index = new IndexOfAllAnnotationsByKey("id");
				 r.ClearObservers();
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

				var index = new IndexOfAllAnnotationsByKey("id");
				r.ClearObservers();
				r.AddObserver(index, _progress);
				Assert.AreEqual(0, index.GetMatchesByKey("222").Count());
			}
		}
	}
}
