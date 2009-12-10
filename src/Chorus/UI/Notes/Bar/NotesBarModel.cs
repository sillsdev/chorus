using System;
using System.Collections.Generic;
using Chorus.annotations;
using Chorus.Utilities;

namespace Chorus.UI.Notes.Bar
{
	public class NotesBarModel
	{
		//use this one for apps that have just one file being editted, and thus on notes repository,
		//which would have been pushed into the container
		public delegate NotesBarModel Factory(AnnotationRepository repository, ChorusNotesSystem.UrlGeneratorFunction functionToMakeUrlForAnnotation);//autofac uses this

		//use this one in apps that have multipel repositories
		//public delegate NotesBarModel FactoryWithExplicitRepository(AnnotationRepository repository, AnnotationIndex index);//autofac uses this

		private readonly AnnotationRepository _repository;
	  //  private string _idOfCurrentAnnotatedObject;
		private object _targetObject;

		public void SetTargetObject(object target)
		{
			if (target != _targetObject)
			{
				_targetObject = target;
				UpdateContentNow();
			}
		}

		internal event EventHandler UpdateContent;

		public NotesBarModel(AnnotationRepository repository, ChorusNotesSystem.UrlGeneratorFunction functionToMakeUrlForAnnotation)
		{
			_repository = repository;
			UrlGenerator = functionToMakeUrlForAnnotation;
			IdGenerator = ChorusNotesSystem.DefaultIdGeneratorUsingObjectToStringAsId;
		}
		public NotesBarModel(AnnotationRepository repository)
			: this(repository, ChorusNotesSystem.DefaultUrlGenerator)
		{
		}

		/// <summary>
		/// Used to make new annotations with a url refelctign the current object/insertion-point/whatever
		/// </summary>
		public ChorusNotesSystem.UrlGeneratorFunction UrlGenerator { get; set; }

		/// <summary>
		/// Used to figure out which existing notes to show
		/// </summary>
		public ChorusNotesSystem.IdGeneratorFunction IdGenerator { get; set; }

		public IEnumerable<Annotation> GetAnnotationsToShow()
		{
			if (null == _targetObject)
				return new List<Annotation>();

			return _repository.GetMatchesByPrimaryRefKey(IdGenerator(_targetObject));
			//todo: add controls for adding new notes, showing closed ones, etc.
		}

		private void UpdateContentNow()
		{
			if(UpdateContent!=null)
				UpdateContent.Invoke(this, null);
		}

		public Annotation CreateAnnotation()
		{
			var id = IdGenerator(_targetObject);
			//nb: it's intentional and necessary to escape the id so it doesn't corrupt
			//the parsing of the url query string, even though the entire url will be
			//escaped again for xml purposes
			var escapedId = Annotation.GetEscapedUrl(id);
			var url = UrlGenerator(escapedId);
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