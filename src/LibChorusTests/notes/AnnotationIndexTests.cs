using System;
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
		public void GetMatches_QueryLikeEveryoneButNoMessages_ReturnsNone()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'/>"))
			{
				var index = new AnnotationIndex("test", (annotation, parameter) => true);
				r.AddIndex(index, _progress);
				Assert.AreEqual(0, index.GetMatches("blah", _progress).Count());
			}
		}
		[Test]
		public void GetMatches_QueryLikeEveryoneHasMessages_ReturnsThem()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'><annotation>
<message guid='123'/><message guid='234'/></annotation></notes>"))
			{
				var index = new AnnotationIndex("test", (annotation, parameter) => true);
				r.AddIndex(index, _progress);
				Assert.AreEqual(2, index.GetMatches("blah", _progress).Count());
			}
		}
		[Test]
		public void GetMatches_HasGuidPredicate_ReturnsMatchingItems()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'><annotation>
<message guid='123'/><message guid='234'/></annotation></notes>"))
			{
				var index = new AnnotationIndex("test", (annotation, guid) => annotation.Guid == guid);
				r.AddIndex(index, _progress);
				Assert.AreEqual(1, index.GetMatches("234", _progress).Count());
			}
		}


	}
}