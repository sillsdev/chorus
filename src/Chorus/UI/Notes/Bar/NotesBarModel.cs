using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.notes;
using Chorus.Utilities;
using SIL.Code;
using SIL.Progress;

namespace Chorus.UI.Notes.Bar
{
	public class NotesBarModel : IAnnotationRepositoryObserver
	{
		//use this one for apps that have just one file being edited, and thus on notes repository,
		//which would have been pushed into the container
		public delegate NotesBarModel Factory(IAnnotationRepository repository, NotesToRecordMapping mapping);//autofac uses this

		private readonly IAnnotationRepository _repository;
		private readonly NotesToRecordMapping _mapping;
		private object _targetObject;
		private bool _reloadPending;

		public void SetTargetObject(object target)
		{
			if (target != _targetObject)
			{
				_targetObject = target;
				UpdateContentNow();
			}
		}
		public bool TargetObjectIsNull
		{
			get { return _targetObject == null; }
		}

		internal event EventHandler UpdateContent;

		public NotesBarModel(IAnnotationRepository repository, NotesToRecordMapping mapping)
		{
			_repository = repository;
			_mapping = mapping;

			repository.AddObserver(this, new NullProgress());
		}
		internal NotesBarModel(IAnnotationRepository repository)
			: this(repository, NotesToRecordMapping.SimpleForTest())
		{
		}

		public void CheckIfWeNeedToReload()
		{
			if (_reloadPending)
				UpdateContentNow();
		}


		public IEnumerable<Annotation> GetAnnotationsToShow()
		{
			if (null == _targetObject)
				return new List<Annotation>();
			var targets =
				new [] {_mapping.FunctionToGoFromObjectToItsId(_targetObject)}
					.Concat(_mapping.FunctionToGoFromObjectToAdditionalIds(_targetObject));
			return targets.SelectMany(target => _repository.GetMatchesByPrimaryRefKey(target));

			 //todo: add controls for adding new notes, showing closed ones, etc.
		}

		private void UpdateContentNow()
		{
			if(UpdateContent!=null)
				UpdateContent.Invoke(this, null);

			_reloadPending = false;
		}

		/// <summary>
		/// Creates a new <c>Annotation</c>, but does not add it to the repository
		/// </summary>
		/// <returns>A new <c>Annotation</c></returns>
		public Annotation CreateAnnotation()
		{
			Guard.AgainstNull(_targetObject, "The program tried to create a note when TargetObject was empty.");
			var id = _mapping.FunctionToGoFromObjectToItsId(_targetObject);
			//nb: it's intentional and necessary to escape the id so it doesn't corrupt
			//the parsing of the url query string, even though the entire url will be
			//escaped again for xml purposes
			var escapedId = UrlHelper.GetEscapedUrl(id);
			var url = _mapping.FunctionToGetCurrentUrlForNewNotes(_targetObject, escapedId);
			var annotation = new Annotation("question", url, "doesntmakesense");
			return annotation;
		}

		public void SaveNowIfNeeded(IProgress progress)
		{
			_repository.SaveNowIfNeeded(progress);
		}

		/// <summary>
		/// Adds an <c>Annotation</c> to the repository.  Should be called only once per <c>Annotation</c>, and the
		/// <c>Annotation</c> should have been created by calling <c>CreateAnnotation</c>
		/// </summary>
		/// <param name="annotation"></param>
		public void AddAnnotation(Annotation annotation)
		{
			_repository.AddAnnotation(annotation);
			_repository.SaveNowIfNeeded(new NullProgress());
		}

		/// <summary>
		/// Removes an <c>Annotation</c> from the repository.  Unfortunately, this doesn't work at present.
		/// </summary>
		/// <param name="annotation"></param>
		public void RemoveAnnotation(Annotation annotation)
		{
			_repository.Remove(annotation);
			UpdateContentNow();
		}

		#region Implementation of IAnnotationRepositoryObserver

		public void Initialize(Func<IEnumerable<Annotation>> allAnnotationsFunction, IProgress progress)
		{
		}

		public void NotifyOfAddition(Annotation annotation)
		{
			_reloadPending = true;
		}

		public void NotifyOfModification(Annotation annotation)
		{
			_reloadPending = true;
		}

		public void NotifyOfDeletion(Annotation annotation)
		{
			_reloadPending = true;
		}

		public void NotifyOfStaleList()
		{
			_reloadPending = true;
		}

		#endregion
	}
}