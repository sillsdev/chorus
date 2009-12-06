using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Autofac;
using Chorus.annotations;
using Chorus.sync;
using Chorus.UI;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
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
			model.SetIdOfCurrentAnnotatedObject("foo3");
			model.CreateAnnotation();
			Assert.AreEqual(1, repo.GetAllAnnotations().Count());
			Assert.IsTrue(repo.GetAllAnnotations().First().RefStillEscaped.Contains("id="+"foo3"));
		}

		[Test]
		public void CreateAnnotation_HaveCustomUrlGenerator_UseIt()
		{
			var repo = AnnotationRepository.FromString("id", "<notes version='0'/>");
			var model = new NotesBarModel(repo);
			model.UrlGenerater = key => "foobar:"+key;
			model.SetIdOfCurrentAnnotatedObject("foo3");
			model.CreateAnnotation();
			Assert.AreEqual(1, repo.GetAllAnnotations().Count());
			Assert.AreEqual("foobar:foo3", repo.GetAllAnnotations().First().RefStillEscaped);
		}

		[Test]
		public void CreateAnnotation_KeyHasDangerousCharacters_ResultingUrlHasThemEscaped()
		{
			var repo = AnnotationRepository.FromString("id", "<notes version='0'/>");
			var model = new NotesBarModel(repo);
			model.SetIdOfCurrentAnnotatedObject("two'<three&four");
			model.CreateAnnotation();
			Assert.IsTrue(repo.GetAllAnnotations().First().RefUnEscaped.Contains("two'<three&four"));
		}
	}


}
