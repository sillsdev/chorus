using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.Utilities;
using SIL.Progress;

namespace Chorus.notes
{
	/// <summary>
	/// Provides a way to speed-up queries which differ by a single string parameter.
	/// To use it, we make a concrete subclass, providing two parameters:
	///     1) where clause: predicate for the class/status of annotations ("filterForObjectsToTrack")
	///     2) select clause: a delegate which gives us a string to index on, the "key"
	///
	/// Then a client can get results in three ways:
	///     GetAll() will give all the annotations which pass the where clause
	///     GetMatchesByKey(key) will give all the annotation with that key
	///     GetMatches(predicate on the key) will give all the annotations with keys that match the predicate
	///
	/// While it could be used as-is, this is marked "abstract" in order to encourage wrapping
	/// in concrete indexes which are easier to code to and understand.
	/// </summary>
	public abstract class AnnotationIndex : IAnnotationRepositoryObserver
	{
		private MultiMap<string, Annotation> _keyToObjectsMap;
		private Func<Annotation, bool> _includeIndexPredicate = (a => true);
		private Func<Annotation, string> _keyMakingFunction = (a => a.RefStillEscaped);

		public AnnotationIndex(Func<Annotation, bool> includeIndexPredicate, Func<Annotation, string> keyMakingFunction)
		{
			_keyToObjectsMap = new MultiMap<string, Annotation>();
			_includeIndexPredicate = includeIndexPredicate;
			_keyMakingFunction = keyMakingFunction;
		}

		 public virtual void Initialize(Func<IEnumerable<Annotation>> allAnnotationsFunction, IProgress progress)
		{
			var annotationsThatBelongInIndex = from a in allAnnotationsFunction()
											   where _includeIndexPredicate(a)
											   select a;

			foreach (var annotation in annotationsThatBelongInIndex)
			{
				_keyToObjectsMap.Add(_keyMakingFunction(annotation), annotation);
			}

		}

		public void NotifyOfAddition(Annotation annotation)
		{
			if (_includeIndexPredicate(annotation))
			{
				_keyToObjectsMap.Add(_keyMakingFunction(annotation), annotation);
			}
		}

		public void NotifyOfModification(Annotation annotation)
		{
			bool belongsInIndex = _includeIndexPredicate(annotation);
			if (_keyToObjectsMap.ContainsKey(_keyMakingFunction(annotation)))
			{
				if (!belongsInIndex)
				{
					_keyToObjectsMap.RemoveAllItemsWithKey(_keyMakingFunction(annotation));
				}
			}
			else
			{
				if (belongsInIndex)
				{
					_keyToObjectsMap.Add(_keyMakingFunction(annotation), annotation);
				}
			}
		}

		public void NotifyOfDeletion(Annotation annotation)
		{
			var key = _keyMakingFunction(annotation);
			_keyToObjectsMap.RemoveKeyItemPair(key, annotation);
		}

		public void NotifyOfStaleList()
		{
			// Can we do anything useful here?
		}

		public IEnumerable<Annotation> GetMatches(Func<string, bool> predicateOnKey, IProgress progress)
		{
			foreach (var x in _keyToObjectsMap)
			{
				//note, we may be applying this predicate to the same key serveral
				//  times in a row, if that key is used by multiple target values
				if (predicateOnKey(x.Key))
					yield return x.Value;
			}
		}
		public IEnumerable<Annotation> GetMatchesByKey(string key)
		{
			return _keyToObjectsMap[key];
		}


		public IEnumerable<Annotation> GetAll()
		{
			foreach (KeyValuePair<string, Annotation> keyValuePair in _keyToObjectsMap)
			{
				yield return keyValuePair.Value;
			}
		}
	}
}