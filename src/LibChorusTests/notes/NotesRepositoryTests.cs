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
	public class NotesRepositoryTests
	{

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
					Assert.AreEqual("<p>hello", x.GetAllAnnotations().First().Messages.First().HtmlText);
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
					r.AddAnnotation("fooClass", "http://somewhere.org");
					r.SaveAs(t.Path);
				}
				using (var x = AnnotationRepository.FromFile(t.Path))
				{
					Assert.AreEqual(2, x.GetAllAnnotations().Count());
					Assert.AreEqual("<p>hello", x.GetAllAnnotations().First().Messages.First().HtmlText);
					Assert.AreEqual("fooClass", x.GetAllAnnotations().ToArray()[1].ClassName);
				}
			}
		}
	}
}