using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.notes;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class AnnotationRepositoryTests
	{
		private IProgress _progress = new ConsoleProgress();

		[Test]
		public void FromPath_ParentDirectoryPathDoesntExist_Throws()
		{
			Assert.Throws<ArgumentException>(() =>

											 AnnotationRepository.FromFile("id", Path.Combine("blah", "bogus.xml"),
																		   new ConsoleProgress()));
		}

		[Test]
		public void FromPath_DoesntExistYet_Creates()
		{
			using(var f = new TempFile())
			{
				File.Delete(f.Path);
				var repo = AnnotationRepository.FromFile("id", f.Path, new ConsoleProgress());
				repo.Save(new ConsoleProgress());
				Assert.IsTrue(File.Exists(f.Path));
			}
		}

		[Test]
		public void Save_DoesntExistYet_CreatesAndSavesAsCanonicalXml()
		{
			string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
				+ "<notes\r\n"
				+ "\tversion=\"0\" />";
			using (var f = new TempFile())
			{
				File.Delete(f.Path);
				var repo = AnnotationRepository.FromFile("id", f.Path, new ConsoleProgress());
				repo.Save(new ConsoleProgress());
				string result = File.ReadAllText(f.Path);
				Assert.AreEqual(expected, result);
			}
		}

		[Test]
		public void FromString_FormatIsTooNew_Throws()
		{
			Assert.Throws<AnnotationFormatException>(() =>
													 AnnotationRepository.FromString("id", "<notes version='99'/>"));
		}

		[Test]
		public void FromString_FormatIsBadXml_Throws()
		{
			Assert.Throws<AnnotationFormatException>(() =>
				AnnotationRepository.FromString("id", "<notes version='99'>"));
		}

		[Test]
		public void GetAll_EmptyDOM_OK()
		{
			using (var r = AnnotationRepository.FromString("id", "<notes version='0'/>"))
			{
				Assert.AreEqual(0, r.GetAllAnnotations().Count());
			}
		}

		[Test]
		public void GetAll_Has2_ReturnsBoth()
		{

			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
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
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
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
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'>
	<annotation guid='123'/></notes>"))
			{
				Assert.AreEqual(0, r.GetByCurrentStatus("open").Count());
			}
		}

		[Test]
		public void Save_AfterCreatingFromString_Throws()
		{
			using (var r =AnnotationRepository.FromString("id", @"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
			{
				Assert.Throws<InvalidOperationException>(() =>
					r.Save(new ConsoleProgress()));
			}
		}

		[Test]
		public void Save_AfterCreatingFromFile_IsSaved()
		{
			using (var t = new TempFile(@"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
			{
				using (var r = AnnotationRepository.FromFile("id", t.Path, new ConsoleProgress()))
				{
					var an = new Annotation("fooClass", "http://somewhere.org", "somepath");
					r.AddAnnotation(an);
					r.Save(new ConsoleProgress());
				}
				using (var x = AnnotationRepository.FromFile("id", t.Path, new ConsoleProgress()))
				{
					Assert.AreEqual(2, x.GetAllAnnotations().Count());
					Assert.AreEqual("<p>hello", x.GetAllAnnotations().First().Messages.First().GetSimpleHtmlText());
					Assert.AreEqual("fooClass", x.GetAllAnnotations().ToArray()[1].ClassName);
				}
			}
		}

		#region IndexHandlingTests

		[Test]
		public void AddIndex_AddSameIndexTwice_Throws()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var index1 = new IndexOfAllOpenConflicts();
				r.AddObserver(index1, _progress);
				var index2 = new IndexOfAllOpenConflicts();
			  Assert.Throws<ApplicationException>(() => r.AddObserver(index2, _progress));
			}
		}

		[Test]
		public void AddIndex_CallInitializeOnIndex()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
			{
				var index = new TestAnnotationIndex();
				r.AddObserver(index, _progress);
				Assert.AreEqual(1, index.InitialItems);
			}
		}

		[Test]
		public void AddAnnotation_NotifiesIndices()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var index = new TestAnnotationIndex();
				r.AddObserver(index, _progress);

				r.AddAnnotation(new Annotation("question", "foo://blah.org?id=1", @"c:\pretendPath"));
				r.AddAnnotation(new Annotation("question", "foo://blah.org?id=1", @"c:\pretendPath"));

				Assert.AreEqual(2, index.Additions);
				Assert.AreEqual(0, index.Modification);
			}
		}

		[Test]
		public void CloseAnnotation_AnnotationWasAddedDynamically_RepositoryNotifiesIndices()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var index = new TestAnnotationIndex();
				r.AddObserver(index, _progress);

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
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
			{
				var index = new TestAnnotationIndex();
				r.AddObserver(index, _progress);
				var annotation = r.GetAllAnnotations().First();
				annotation.SetStatus("joe", "closed");
				Assert.AreEqual(1, index.Modification);
			}
		}


		[Test]
		public void Remove_AnnotationAddedDynamically_RemovesIt()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var a = new Annotation("question", "blah://blah/?id=foo", "");
				r.AddAnnotation(a);
				r.Remove(a);
				Assert.AreEqual(0, r.GetAllAnnotations().Count(), "should be none left");
			}
		}

		[Test]
		public void Remove_AnnotationWasAddedDynamically_RepositoryNotifiesIndices()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'/>"))
			{
				var index = new TestAnnotationIndex();
				r.AddObserver(index, _progress);

				var annotation = new Annotation("question", "foo://blah.org?id=1", @"c:\pretendPath");
				r.AddAnnotation(annotation);

				Assert.AreEqual(0, index.Deletions);
				r.Remove(annotation);

				Assert.AreEqual(1, index.Deletions);
			}
		}

				[Test]
		public void Remove_AnnotationFromFile_RepositoryNotifiesIndices()
		{
			using (var r = AnnotationRepository.FromString("id", @"<notes version='0'><annotation guid='123'>
<message guid='234'>&lt;p&gt;hello</message></annotation></notes>"))
			{
				var index = new TestAnnotationIndex();
				r.AddObserver(index, _progress);
				var annotation = r.GetAllAnnotations().First();
				r.Remove(annotation);
				Assert.AreEqual(1, index.Deletions);
			}
		}

		[Test]
		[Category("SkipOnBuildServer")]
		public void SaveAndLoad_10KRecords_CompletesQuickly()
		{
			using(var f = new TempFile("<notes version='0'/>"))
			{
				Console.WriteLine("Building File...");
				var r = AnnotationRepository.FromFile("id", f.Path, new NullProgress());
				for (int i = 0; i < 10000; i++)
				{
					var annotation = new Annotation("question", string.Format("nowhere://blah?id={0}", Guid.NewGuid().ToString()), f.Path);
					r.AddAnnotation(annotation);
					annotation.AddMessage("test", "open", "blah blah");
				}
				Console.WriteLine("Saving Large File...");
				var w = new System.Diagnostics.Stopwatch();
				w.Start();
				r.Save(new NullProgress());
				w.Stop();
				Console.WriteLine("Elapsed Time:"+w.ElapsedMilliseconds.ToString()+" milliseconds");
				Assert.IsTrue(w.ElapsedMilliseconds < 200); //it's around 70 on my laptop

				w.Reset();
				Console.WriteLine("Reading Large File...");
				w.Start();
				var rToRead = AnnotationRepository.FromFile("id", f.Path, new NullProgress());
				w.Stop();
				Console.WriteLine("Elapsed Time:"+w.ElapsedMilliseconds.ToString()+" milliseconds");
				Assert.IsTrue(w.ElapsedMilliseconds < 1000); //it's around 240 on my laptop
			}
		}


		#endregion
	}

	public class TestAnnotationIndex : IAnnotationRepositoryObserver
	{
		public int InitialItems;
		public int Additions;
		public int Modification;
		public int Deletions;

		public TestAnnotationIndex()
		{
		}

		public void Initialize(Func<IEnumerable<Annotation>> allAnnotationsFunction, IProgress progress)
		{
			InitialItems = allAnnotationsFunction().Count();
		}

		public void NotifyOfAddition(Annotation annotation)
		{
			Additions++;
		}
		public void NotifyOfModification(Annotation annotation)
		{
			Modification++;
		}

		public void NotifyOfDeletion(Annotation annotation)
		{
			Deletions++;
		}
	}

}