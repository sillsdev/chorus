using System;
using System.Collections.Generic;
using Chorus.Utilities;

namespace Chorus.notes
{
	public interface IAnnotationRepositoryObserver
	{
		void Initialize(Func<IEnumerable<Annotation>> allAnnotationsFunction, IProgress progress);
		void NotifyOfAddition(Annotation annotation);
		void NotifyOfModification(Annotation annotation);
		void NotifyOfDeletion(Annotation annotation);
	}
}