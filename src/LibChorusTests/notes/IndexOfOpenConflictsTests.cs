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
	public class IndexOfOpenConflictsTests
	{
		private IProgress _progress = new ConsoleProgress();

		[Test]
		public void GetMatches_AddedAfterIndexInitialization_FoundViaPredicate()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'></notes>"))
			{
				var index = new IndexOfAllOpenConflicts();
				r.AddIndex(index, _progress);
				Assert.AreEqual(0, index.GetMatches(rf => rf.Contains("rid=12345"), _progress).Count());
				var ann = new Annotation("conflict", "blah://blah?rid=12345", "somepath");
				ann.AddMessage("merger", "open", string.Empty);

				r.AddAnnotation(ann);
				Assert.AreEqual(1, index.GetMatches(rf => rf.Contains("rid=12345"), _progress).Count());
				Assert.AreEqual(0, index.GetMatches(rf => rf.Contains("rid=333"), _progress).Count());
				ann.SetStatusToClosed("testman");
				Assert.AreEqual(0, index.GetMatches(rf => rf.Contains("rid=12345"), _progress).Count());
			}
		}

		[Test]
		public void GetMatches_AddedBeforeIndexInitialization_FoundViaPredicate()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'></notes>"))
			{
				var ann = new Annotation("conflict", "blah://blah?rid=12345", "somepath");
				ann.AddMessage("merger", "open", string.Empty);
				 r.AddAnnotation(ann);

				var index = new IndexOfAllOpenConflicts();
				r.AddIndex(index, _progress);
				Assert.AreEqual(1, index.GetMatches(rf => rf.Contains("rid=12345"), _progress).Count());
				Assert.AreEqual(0, index.GetMatches(rf => rf.Contains("rid=333"), _progress).Count());
				ann.SetStatusToClosed("testman");
				Assert.AreEqual(0, index.GetMatches(rf => rf.Contains("rid=12345"), _progress).Count());
			}
		}
	}
}
