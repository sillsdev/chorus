using Chorus.annotations;
using Chorus.UI.Notes.Bar;
using System.Linq;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class NotesBarModelTests
	{
		[Test]
		public void CreateAnnotation_CreatesNewAnotationUsingIdOfCurrentAnnotatedObject()
		{
			var repo = AnnotationRepository.FromString("id", "<notes version='0'/>");
			var model = new NotesBarModel(repo);
			model.SetTargetObject("foo3");
			model.CreateAnnotation();
			Assert.AreEqual(1, repo.GetAllAnnotations().Count());
			Assert.IsTrue(repo.GetAllAnnotations().First().RefStillEscaped.Contains("id="+"foo3"));
		}

		[Test]
		public void CreateAnnotation_HaveCustomUrlGenerator_UseIt()
		{
			var repo = AnnotationRepository.FromString("id", "<notes version='0'/>");
			var model = new NotesBarModel(repo);
			model.IdGenerator = (target) => "x"+ target.ToString()+"x";
			model.UrlGenerator = (escapedId) => "foobar:" + escapedId;
			model.SetTargetObject("foo3");
			model.CreateAnnotation();
			Assert.AreEqual(1, repo.GetAllAnnotations().Count());
			Assert.AreEqual("foobar:xfoo3x", repo.GetAllAnnotations().First().RefStillEscaped);
		}

		[Test]
		public void CreateAnnotation_KeyHasDangerousCharacters_ResultingUrlHasThemEscaped()
		{
			var repo = AnnotationRepository.FromString("id", "<notes version='0'/>");
			var model = new NotesBarModel(repo);
//            model.UrlGenerator = (target,key)=> string.Format("lift://object?type=entry&amp;id={0}&amp;type=test", key);
			model.UrlGenerator = (escapedId) => string.Format("lift://object?type=entry&id={0}&type=test", escapedId);
			model.SetTargetObject("two'<three&four");
			model.CreateAnnotation();
			Assert.IsTrue(repo.GetAllAnnotations().First().RefUnEscaped.Contains("two'<three&four"));
		}
	}


}
