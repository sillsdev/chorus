using System.Collections.Generic;
using System.Linq;
using SIL.Progress;

namespace Chorus.notes
{
	/// <summary>
	/// This class wraps multiple instances of IAnnotationRepository (including one identified as primary)
	/// and allows them to be treated as a single repository. Currently the functionality is limited to that
	/// needed by NotesBarModel, the only current client.
	/// </summary>
	public class MultiSourceAnnotationRepository : IAnnotationRepository
	{
		private IAnnotationRepository _primary;
		private IAnnotationRepository[] _others;

		// Construct one. The primary repo is the one where new notes will be created.
		// All will be searched (and saved, etc.)
		public MultiSourceAnnotationRepository(IAnnotationRepository primary, IEnumerable<IAnnotationRepository> others)
		{
			_primary = primary;
			_others = others.ToArray();
		}

		public IEnumerable<Annotation> GetMatchesByPrimaryRefKey(string key)
		{
			return _primary.GetMatchesByPrimaryRefKey(key).Concat(_others.SelectMany(o => o.GetMatchesByPrimaryRefKey(key)));
		}

		public void AddAnnotation(Annotation annotation)
		{
			_primary.AddAnnotation(annotation);
		}

		public void SaveNowIfNeeded(IProgress progress)
		{
			_primary.SaveNowIfNeeded(progress);
			foreach (var other in _others)
				other.SaveNowIfNeeded(progress);
		}

		public bool ContainsAnnotation(Annotation annotation)
		{
			return _primary.ContainsAnnotation(annotation) || _others.Any(o => o.ContainsAnnotation(annotation));
		}

		/// <summary>
		/// Remove the annotation (from the repo that claims to contain it, if any).
		/// </summary>
		/// <param name="annotation"></param>
		public void Remove(Annotation annotation)
		{
			if (_primary.ContainsAnnotation(annotation))
				_primary.Remove(annotation);
			else
			{
				foreach (var other in _others)
				{
					if (other.ContainsAnnotation(annotation))
					{
						other.Remove(annotation);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Add the observer to each repository.
		/// Note that if there are any _others, the same observer will be added to each,
		/// which currently results in its Initialize method being called with the GetAllAnnotations of each repo.
		/// Thus, in such cases, the observer's Initialize method must be capable of being called repeatedly
		/// and accumulating the results.
		/// </summary>
		/// <param name="observer"></param>
		/// <param name="progress"></param>
		public void AddObserver(IAnnotationRepositoryObserver observer, IProgress progress)
		{
			_primary.AddObserver(observer, progress);
			foreach (var other in _others)
				other.AddObserver(observer, progress);
		}
	}
}
