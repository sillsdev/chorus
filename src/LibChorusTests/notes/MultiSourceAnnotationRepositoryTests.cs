using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Chorus.notes;
using NUnit.Framework;
using SIL.Progress;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class MultiSourceAnnotationRepositoryTests
	{
		private MockRepo _primary;
		private MockRepo _other1;
		private MockRepo _other2;
		private MultiSourceAnnotationRepository _msar;
		private Annotation _primaryAnnotation;
		private Annotation _other1Annotation;
		private Annotation _other2Annotation1;
		private Annotation _other2Annotation2;

		[SetUp]
		public void Setup()
		{
			_primary = new MockRepo();
			_other1 = new MockRepo();
			_other2 = new MockRepo();
			_primary.AddKey = "abc";

			_msar = new MultiSourceAnnotationRepository(_primary, new IAnnotationRepository[] { _other1, _other2 });

			_primaryAnnotation = new Annotation(XElement.Parse("<_primaryAnnotation class='foo' guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>"));
			_primary.Results["abc"] = new List<Annotation> { _primaryAnnotation };

			_other1Annotation = new Annotation(XElement.Parse("<_primaryAnnotation class='foo' guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>"));
			_other1.Results["abc"] = new List<Annotation> { _other1Annotation };
			_other2Annotation1 = new Annotation(XElement.Parse("<_primaryAnnotation class='foo' guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>"));
			_other2Annotation2 = new Annotation(XElement.Parse("<_primaryAnnotation class='foo' guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>"));
			_other2.Results["abc"] = new List<Annotation> { _other2Annotation1, _other2Annotation2 };

		}
		[Test]
		public void MsarGetsMatchesFromPrimaryAndSecondaries()
		{
			var results = _msar.GetMatchesByPrimaryRefKey("abc");
			Assert.That(results, Has.Member(_primaryAnnotation));
			Assert.That(results, Has.Member(_other1Annotation));
			Assert.That(results, Has.Member(_other2Annotation1));
			Assert.That(results, Has.Member(_other2Annotation2));
		}

		[Test]
		public void MsarAddsToPrimary()
		{
			var newAnnotation = new Annotation(XElement.Parse("<_primaryAnnotation class='foo' guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>"));

			_msar.AddAnnotation(newAnnotation);
			Assert.That(_primary.Results["abc"], Has.Member(newAnnotation));
			// In the event that it mistakenly tries to add to the others, it will crash because their AddKeys are not set.
		}

		[Test]
		public void MsarSaveNowSavesAll()
		{
			var progress = new NullProgress();
			_msar.SaveNowIfNeeded(progress);
			Assert.That(_primary.ProgressPassedToSaveNow, Is.EqualTo(progress), "SaveNow not forwarded properly to _primary");
			Assert.That(_other1.ProgressPassedToSaveNow, Is.EqualTo(progress), "SaveNow not forwarded properly to first other");
			Assert.That(_other2.ProgressPassedToSaveNow, Is.EqualTo(progress), "SaveNow not forwarded properly to second other");
		}

		[Test]
		public void MsarContainsItemInPrimary()
		{
			Assert.That(_msar.ContainsAnnotation(_primaryAnnotation));
		}

		[Test]
		public void MsarContainsItemInOthers()
		{
			Assert.That(_msar.ContainsAnnotation(_other1Annotation));
			Assert.That(_msar.ContainsAnnotation(_other2Annotation2));
		}

		[Test]
		public void MsarDoesNotContainUnknownItem()
		{
			var newAnnotation = new Annotation(XElement.Parse("<_primaryAnnotation class='foo' guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>"));
			Assert.That(_msar.ContainsAnnotation(newAnnotation), Is.False);
		}

		[Test]
		public void MsarCallsRemoveOnRepoContainingAnnotationOnly()
		{
			_msar.Remove(_primaryAnnotation);
			Assert.That(_primary.Removed, Is.EqualTo(_primaryAnnotation));
			Assert.That(_other1.Removed, Is.Null);

			_primary.Removed = null; // reset for new test
			_msar.Remove(_other2Annotation2);
			Assert.That(_primary.Removed, Is.Null);
			Assert.That(_other1.Removed, Is.Null);
			Assert.That(_other2.Removed, Is.EqualTo(_other2Annotation2));
		}

		[Test]
		public void MsarAddsObserverToAll()
		{
			var progress = new NullProgress();
			var observer = new MockObserver();
			_msar.AddObserver(observer, progress);

			Assert.That(_primary.Observer, Is.EqualTo(observer));
			Assert.That(_other1.Observer, Is.EqualTo(observer));
			Assert.That(_primary.ProgressPassedToAddObserver, Is.EqualTo(progress));
			Assert.That(_other2.ProgressPassedToAddObserver, Is.EqualTo(progress));
		}

	}

	class MockRepo : IAnnotationRepository
	{
		public Dictionary<string, List<Annotation>> Results = new Dictionary<string, List<Annotation>>();
		public string AddKey; // Key we use for AddAnnotation; real code deduces this from annotation. Must exist in dictionary.
		public IProgress ProgressPassedToSaveNow;
		public Annotation Removed;
		public IAnnotationRepositoryObserver Observer;
		public IProgress ProgressPassedToAddObserver;

		public IEnumerable<Annotation> GetMatchesByPrimaryRefKey(string key)
		{
			return Results[key];
		}

		public void AddAnnotation(Annotation annotation)
		{
			Results[AddKey].Add(annotation);
		}

		public void SaveNowIfNeeded(IProgress progress)
		{
			ProgressPassedToSaveNow = progress;
		}

		public bool ContainsAnnotation(Annotation annotation)
		{
			return Results.Any(kvp => kvp.Value.Contains(annotation));
		}

		public void Remove(Annotation annotation)
		{
			Removed = annotation;
			// Note that we don't actually remove it. This makes sure tests don't interfere with each other.
		}

		public void AddObserver(IAnnotationRepositoryObserver observer, IProgress progress)
		{
			Observer = observer;
			ProgressPassedToAddObserver = progress;
		}
	}

	class MockObserver : IAnnotationRepositoryObserver
	{
		public void Initialize(Func<IEnumerable<Annotation>> allAnnotationsFunction, IProgress progress)
		{
			throw new NotImplementedException();
		}

		public void NotifyOfAddition(Annotation annotation)
		{
			throw new NotImplementedException();
		}

		public void NotifyOfModification(Annotation annotation)
		{
			throw new NotImplementedException();
		}

		public void NotifyOfDeletion(Annotation annotation)
		{
			throw new NotImplementedException();
		}

		public void NotifyOfStaleList()
		{
			throw new NotImplementedException();
		}
	}
}
