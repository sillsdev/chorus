using System;
using System.Collections.Generic;
using Chorus.notes;
using Chorus.Utilities;
using Chorus.Utilities.code;

namespace Chorus.UI.Notes.Bar
{
	public class NotesBarModel
	{
		//use this one for apps that have just one file being edited, and thus on notes repository,
		//which would have been pushed into the container
		public delegate NotesBarModel Factory(AnnotationRepository repository, NotesToRecordMapping mapping);//autofac uses this

		private readonly AnnotationRepository _repository;
		private readonly NotesToRecordMapping _mapping;
		private readonly NotesUpdatedEvent _notesUpdatedEvent;
		private object _targetObject;

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

		public NotesBarModel(AnnotationRepository repository, NotesToRecordMapping mapping, NotesUpdatedEvent notesUpdatedEvent)
		{
			_repository = repository;
			_mapping = mapping;
			_notesUpdatedEvent = notesUpdatedEvent;
		}
		internal NotesBarModel(AnnotationRepository repository)
			: this(repository, NotesToRecordMapping.SimpleForTest(), null)
		{
		}



		public IEnumerable<Annotation> GetAnnotationsToShow()
		{
			if (null == _targetObject)
				return new List<Annotation>();

			return _repository.GetMatchesByPrimaryRefKey(_mapping.FunctionToGoFromObjectToItsId(_targetObject));
			//todo: add controls for adding new notes, showing closed ones, etc.
		}

		private void UpdateContentNow()
		{
			if(UpdateContent!=null)
				UpdateContent.Invoke(this, null);

			if(_notesUpdatedEvent !=null)   // tell the browser, if there is one, to update
				_notesUpdatedEvent.Raise(this);
		}

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
			_repository.AddAnnotation(annotation);

			_repository.SaveNowIfNeeded(new NullProgress());
			return annotation;
		}

		public void SaveNowIfNeeded(IProgress progress)
		{
			_repository.SaveNowIfNeeded(progress);
		}

		public void RemoveAnnotation(Annotation annotation)
		{
			_repository.Remove(annotation);
			UpdateContentNow();
		}
	}
}