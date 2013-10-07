using System.Collections.ObjectModel;
using Chorus.notes;
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
			var mapping = new NotesToRecordMapping();
			mapping.FunctionToGoFromObjectToItsId = (target) => "x" + target.ToString() + "x";
			mapping.FunctionToGetCurrentUrlForNewNotes = (unusedTarget, escapedId) => "foobar:" + escapedId;
			var model = new NotesBarModel(repo, mapping);
			model.SetTargetObject("foo3");
			model.CreateAnnotation();
			Assert.AreEqual(1, repo.GetAllAnnotations().Count());
			Assert.AreEqual("foobar:xfoo3x", repo.GetAllAnnotations().First().RefStillEscaped);
		}

		[Test]
		public void CreateAnnotation_KeyHasDangerousCharacters_ResultingUrlHasThemEscaped()
		{
			var repo = AnnotationRepository.FromString("id", "<notes version='0'/>");
		   var mapping = new NotesToRecordMapping();
		   mapping.FunctionToGetCurrentUrlForNewNotes = (unusedTarget, escapedId) => string.Format("lift://object?type=entry&id={0}&type=test", escapedId);
			mapping.FunctionToGoFromObjectToItsId = NotesToRecordMapping.DefaultIdGeneratorUsingObjectToStringAsId;

			//mapping.UrlGenerator = (target,key)=> string.Format("lift://object?type=entry&amp;id={0}&amp;type=test", key);
			 var model = new NotesBarModel(repo, mapping);
			model.SetTargetObject("two'<three&four");
			model.CreateAnnotation();
			Assert.IsTrue(repo.GetAllAnnotations().First().RefUnEscaped.Contains("two'<three&four"));
		}

		[Test]
		public void GetAnnotationsToShow_ShowsAnnotationsFromFunctionToGoFromObjectToItsId()
		{
			var repo = AnnotationRepository.FromString("guid",
@"<notes version='0'>
<annotation guid='123' ref='lift://FTeam.lift?type=entry&amp;guid=abc'><message guid='234'>hello</message></annotation>
<annotation guid='345' ref='lift://FTeam.lift?type=entry&amp;guid=def'><message guid='234'>hello</message></annotation>
</notes>");
			var mapping = new NotesToRecordMapping();
			mapping.FunctionToGoFromObjectToItsId = (x) => "def";
			var model = new NotesBarModel(repo, mapping);
			model.SetTargetObject("xyz");
			var annotationsToShow = model.GetAnnotationsToShow().ToList();
			Assert.That(annotationsToShow, Has.Count.EqualTo(1));
			Assert.That(annotationsToShow[0].Guid, Is.EqualTo("345"));
		}

		[Test]
		public void GetAnnotationsToShow_ShowsAnnotationsFromFunctionToGoFromObjectToAdditionalIds()
		{
			var repo = AnnotationRepository.FromString("guid",
@"<notes version='0'>
<annotation guid='123' ref='lift://FTeam.lift?type=entry&amp;guid=abc'><message guid='234'>hello</message></annotation>
<annotation guid='345' ref='lift://FTeam.lift?type=entry&amp;guid=def'><message guid='234'>hello</message></annotation>
<annotation guid='567' ref='lift://FTeam.lift?type=entry&amp;guid=ghi'><message guid='234'>hello</message></annotation>
<annotation guid='678' ref='lift://FTeam.lift?type=entry&amp;guid=klm'><message guid='234'>hello</message></annotation>
</notes>");
			var mapping = new NotesToRecordMapping();
			mapping.FunctionToGoFromObjectToItsId = (x) => "def";
			mapping.FunctionToGoFromObjectToAdditionalIds = (x) => new [] {"ghi", "abc"};
			var model = new NotesBarModel(repo, mapping);
			model.SetTargetObject("xyz");
			var annotationsToShow = model.GetAnnotationsToShow().ToList();
			Assert.That(annotationsToShow, Has.Count.EqualTo(3));
			Assert.That(annotationsToShow[0].Guid, Is.EqualTo("345"));
			Assert.That(annotationsToShow[1].Guid, Is.EqualTo("567"));
			Assert.That(annotationsToShow[2].Guid, Is.EqualTo("123"));
		}
	}


}
