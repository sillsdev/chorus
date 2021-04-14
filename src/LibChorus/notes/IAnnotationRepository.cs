using System.Collections.Generic;
using SIL.Progress;

namespace Chorus.notes
{
	/// <summary>
	/// A subset of the functions of AnnotationRepository needed by NotesBarModel and also implemented by
	/// MultiSourceAnnotationRepository. These are the annotation repository functions needed by the notes bar.
	/// </summary>
	public interface IAnnotationRepository
	{
		IEnumerable<Annotation> GetMatchesByPrimaryRefKey(string key);
		void AddAnnotation(Annotation annotation);
		void SaveNowIfNeeded(IProgress progress);
		bool ContainsAnnotation(Annotation annotation); // not needed by NotesBarModel but needed to implement Remove properly
		void Remove(Annotation annotation);
		void AddObserver(IAnnotationRepositoryObserver observer, IProgress progress);
	}
}
