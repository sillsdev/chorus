using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.Utilities;
using Chorus.Utilities.code;

namespace Chorus.annotations
{
	/// <summary>
	/// Provides a way to speed-up queries which differ by a single string parameter.
	/// </summary>
	public class AnnotationIndex
	{
		private readonly Func<Annotation, string, bool> _predicate;
		protected AnnotationRepository _parent;

		public AnnotationIndex(string name, Func<Annotation, string, bool> predicate)
		{
			_predicate = predicate;
			Name = name;
		}
		public AnnotationIndex(Func<Annotation, string, bool> predicate)
		{
			_predicate = predicate;
			Name = GetType().ToString();
		}

		public string Name { get; private set; }

		public virtual void Initialize(AnnotationRepository parent, IProgress progress)
		{
			_parent = parent;
			//TODO: the whole point of htis index is to eventually keep an index, or a cache or something
			// but this isn't implemented yet.  It would be done here.
		}

		public virtual void NotifyOfAddition(Annotation annotation)
		{
		}

		public virtual void NotifyOfModification(Annotation annotation)
		{
		}

		public virtual IEnumerable<Annotation> GetMatches(string parameter, IProgress progress)
		{
			Guard.AgainstNull(_parent, "ParentRepository (not initialized yet?)");

			//TODO: the whole point of this class is to eventually just consult our index.
			return _parent.GetMatches(_predicate, parameter);
		}
	}


	/// <summary>
	/// The idea for this one is to keep an index of refs which have open conflicts annotations refering to them
	/// </summary>
	public class IndexOfAllOpenConflicts : AnnotationIndex
	{
		private MultiMap <string, Annotation> _annotationsByRef;
		private Func<Annotation, bool> _includeIndexPredicate = (a => a.ClassName == "conflict" && a.Status == "open");
		private Func<Annotation, string> _keyMakingFunction = (a => a.Ref);

		public IndexOfAllOpenConflicts() : base((a,unused)=>a.IsClosed)
		{
			_annotationsByRef = new MultiMap<string, Annotation>();
		}

		public override void Initialize(AnnotationRepository parent, IProgress progress)
		{
			base.Initialize(parent, progress);


			//enhance.... this is working by 2 levels of predicates...
			//1) get open conflicts
			//2) the actual query over these
			//In fact, it's going to be faster to remove the expressiveness of (2) and
			//keep an actual index from some parameter to 0+ annotations
			//So we'd take (in the generalized situation) something like the where... select clause.
			//1) where clause: predicate for the class/status of annotations
			//2) select clause: a delegate which gives us a string to index on
			var annotationsThatBelongInIndex = from a in _parent.GetAllAnnotations()
										  where _includeIndexPredicate(a)
										  select a;

			foreach (var annotation in annotationsThatBelongInIndex)
			{
				_annotationsByRef.Add(_keyMakingFunction(annotation), annotation);
			}

		}

		public override void NotifyOfAddition(Annotation annotation)
		{
			if(_includeIndexPredicate(annotation))
			{
				_annotationsByRef.Add(annotation.Ref, annotation);
			}
		}

		public override void NotifyOfModification(Annotation annotation)
		{
			bool belongsInIndex = _includeIndexPredicate(annotation);
			if (_annotationsByRef.ContainsKey(_keyMakingFunction(annotation)))
			{
				if(!belongsInIndex)
				{
					_annotationsByRef.Remove(_keyMakingFunction(annotation));
				}
			}
			else
			{
				if(belongsInIndex)
				{
					_annotationsByRef.Add(_keyMakingFunction(annotation), annotation);
				}
			}
		}

		public IEnumerable<Annotation> GetMatches(Func<string, bool> predicateOnKey, IProgress progress)
		{
			foreach (var x in _annotationsByRef)
			{
				//note, we may be applying this predicate to the same key serveral
				//  times in a row, if that key is used by multiple target values
				if(predicateOnKey(x.Key))
					yield return x.Value;
			}
	   }
		public override IEnumerable<Annotation> GetMatches(string key, IProgress progress)
		{
			return _annotationsByRef[key];
		}
	}
}