using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Chorus.Utilities;
using Palaso.Progress;

namespace Chorus.notes
{
	/// <summary>
	/// This index is for finding open conflicts.  It makes an index of all the refs of them, and then
	/// provides a way to further query those with a predicate.
	/// </summary>
	public class IndexOfAllOpenConflicts : AnnotationIndex
	{
		public IndexOfAllOpenConflicts()
			: base( (a => a.ClassName == "conflict" && a.Status == "open"), // includeIndexPredicate
					(a => a.RefStillEscaped))                                           // keyMakingFunction
		{
		}

		public IEnumerable<Annotation> GetConflictsWithExactReference(string reference)
		{
			return GetMatchesByKey(reference);
		}

		public IEnumerable<Annotation> GetConflictsWhereReferenceContainsString(string searchString, IProgress progress)
		{
			return GetMatches(reference=> reference.Contains(searchString), progress);
		}
	}
}