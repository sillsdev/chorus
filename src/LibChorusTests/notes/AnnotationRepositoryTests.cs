using System;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.annotations;
using Chorus.Utilities;
using NUnit.Framework;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class AnnotationRepositoryTests
	{
		private IProgress _progress = new ConsoleProgress();

		[Test, ExpectedException(typeof(FileNotFoundException))]
		public void FromPath_PathNotFound_Throws()
		{
			AnnotationRepository.FromFile("bogus.xml");
		}

		[Test, ExpectedException(typeof(AnnotationFormatException))]
		public void FromString_FormatIsTooNew_Throws()
		{
			AnnotationRepository.FromString("<notes version='99'/>");
		}

		[Test, ExpectedException(typeof(AnnotationFormatException))]
		public void FromString_FormatIsBadXml_Throws()
		{
			AnnotationRepository.FromString("<notes version='99'>");
		}

		[Test]
		public void GetAll_EmptyDOM_OK()
		{
			using (var r = AnnotationRepository.FromString("<notes version='0'/>"))
			{
				Assert.AreEqual(0, r.GetAllAnnotations().Count());
			}
		}

		[Test]
		public void GetAll_Has2_ReturnsBoth()
		{

			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
	<annotation guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>
	<annotation guid='12D39999-E83D-41AD-BAB3-B7E46D8C13CE'/>
</notes>"))
			{
				Assert.AreEqual(2, r.GetAllAnnotations().Count());
			}
		}

		[Test]
		public void GetByCurrentStatus_UsesTheLastMessage()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
	<annotation guid='123'><message status='open'/>
<message status='processing'/> <message status='closed'/>
</annotation>
</notes>"))
			{
				Assert.AreEqual(0, r.GetByCurrentStatus("open").Count());
				Assert.AreEqual(0, r.GetByCurrentStatus("processing").Count());
				Assert.AreEqual(1, r.GetByCurrentStatus("closed").Count());
			}
		}

		[Test]
		public void GetByCurrentStatus_NoMessages_ReturnsNone()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'>
	<annotation guid='123'/></notes>"))
			{
				Assert.AreEqual(0, r.GetByCurrentStatus("open").Count());
			}
		}

		[Test]
		public void Save_AfterCreatingFromString_IsSaved()
		{
			using (var t = new TempFile())
			{
				File.Delete(t.Path);
				using (var r =AnnotationRepository.FromString(@"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
				{
					r.SaveAs(t.Path);
				}
				Assert.IsTrue(File.Exists(t.Path));
				using (var x = AnnotationRepository.FromFile(t.Path))
				{
					Assert.AreEqual(1, x.GetAllAnnotations().Count());
					Assert.AreEqual("<p>hello", x.GetAllAnnotations().First().Messages.First().GetSimpleHtmlText());
				}
			}
		}

		[Test]
		public void Save_AfterCreatingFromFile_IsSaved()
		{
			using (var t = new TempFile(@"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
			{
				using (var r = AnnotationRepository.FromFile(t.Path))
				{
					var an = new Annotation("fooClass", "http://somewhere.org", "somepath");
					r.AddAnnotation(an);
					r.SaveAs(t.Path);
				}
				using (var x = AnnotationRepository.FromFile(t.Path))
				{
					Assert.AreEqual(2, x.GetAllAnnotations().Count());
					Assert.AreEqual("<p>hello", x.GetAllAnnotations().First().Messages.First().GetSimpleHtmlText());
					Assert.AreEqual("fooClass", x.GetAllAnnotations().ToArray()[1].ClassName);
				}
			}
		}

		#region IndexHandlingTests

		[Test, ExpectedException(typeof(ApplicationException))]
		public void AddIndex_AddSamePredicateTwice_Throws()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'/>"))
			{
				var index1 = new AnnotationIndex("ILikeEverybody", (annotation, guid) => true);
				r.AddIndex(index1, _progress);
				var index2 = new AnnotationIndex("ILikeEverybody", (annotation, guid) => true);
				r.AddIndex(index2, _progress);
			}
		}

		[Test]
		public void AddAnnotation_NotifiesIndices()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'/>"))
			{
				var index = new TestAnnotationIndex("ILikeEverybody");
				r.AddIndex(index, _progress);

				r.AddAnnotation(new Annotation("question", "foo://blah.org?id=1", @"c:\pretendPath"));
				r.AddAnnotation(new Annotation("question", "foo://blah.org?id=1", @"c:\pretendPath"));

				Assert.AreEqual(2, index.Additions);
				Assert.AreEqual(0, index.Modification);
			}
		}

		[Test]
		public void CloseAnnotation_AnnotationWasAddedDynamically_RepositoryNotifiesIndices()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'/>"))
			{
				var index = new TestAnnotationIndex("ILikeEverybody");
				r.AddIndex(index, _progress);

				var annotation = new Annotation("question", "foo://blah.org?id=1", @"c:\pretendPath");
				r.AddAnnotation(annotation);

				Assert.AreEqual(0, index.Modification);
				annotation.SetStatus("joe", "closed");

				Assert.AreEqual(1, index.Modification);
			}
		}

		[Test]
		public void CloseAnnotation_AnnotationFromFile_RepositoryNotifiesIndices()
		{
			using (var r = AnnotationRepository.FromString(@"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
			{
				var index = new TestAnnotationIndex("ILikeEverybody");
				r.AddIndex(index, _progress);
				var annotation = r.GetAllAnnotations().First();
				annotation.SetStatus("joe", "closed");
				Assert.AreEqual(1, index.Modification);
			}
		}
		#endregion
	}

	public class TestAnnotationIndex : AnnotationIndex
	{
		public int Additions;
		public int Modification;

		public TestAnnotationIndex(string name)
			: base(name, (annotation, guid) => true)
		{
		}
		public override void NotifyOfAddition(Annotation annotation)
		{
			Additions++;
		}
		public override void NotifyOfModification(Annotation annotation)
		{
			Modification++;
		}
	}

}