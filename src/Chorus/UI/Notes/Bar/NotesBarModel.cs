using System;
using System.Collections.Generic;
using System.Web;
using Chorus.annotations;
using Chorus.Utilities;

namespace Chorus.UI.Notes.Bar
{
	public class NotesBarModel
	{
		//use this one for apps that have just one file being editted, and thus on notes repository,
		//which would have been pushed into the container
		public delegate NotesBarModel Factory(AnnotationRepository repository);//autofac uses this

		//use this one in apps that have multipel repositories
		//public delegate NotesBarModel FactoryWithExplicitRepository(AnnotationRepository repository, AnnotationIndex index);//autofac uses this

		private readonly AnnotationRepository _repository;
		private string _idOfCurrentAnnotatedObject;

		public delegate string UrlGenerator(string key);

		//set this if you want something other than a default, chorus-generated URL for your objects
		//note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		//your UrlGenerator do that.
		public UrlGenerator UrlGenerater { get; set; }

		public void SetIdOfCurrentAnnotatedObject(string key)
		{
			_idOfCurrentAnnotatedObject = key;
			UpdateContentNow();
		}

		internal event EventHandler UpdateContent;

		public NotesBarModel(AnnotationRepository repository)
		{
			_repository = repository;
			UrlGenerater = (key) => string.Format("chorus://object?id={0}", key);
		}

		public IEnumerable<Annotation> GetAnnotationsToShow()
		{
			if(string.IsNullOrEmpty(_idOfCurrentAnnotatedObject))
				return new List<Annotation>();

			return _repository.GetMatchesByPrimaryRefKey(_idOfCurrentAnnotatedObject);
			//todo: add controls for adding new notes, showing closed ones, etc.
		}

		private void UpdateContentNow()
		{
			if(UpdateContent!=null)
				UpdateContent.Invoke(this, null);
		}

		public Annotation CreateAnnotation()
		{
			var escapedIdOfCurrentAnnotatedObject = Annotation.GetEscapedString(_idOfCurrentAnnotatedObject);
			var annotation = new Annotation("question", UrlGenerater(escapedIdOfCurrentAnnotatedObject), "doesntmakesense");
			_repository.AddAnnotation(annotation);

			_repository.SaveNowIfNeeded(new NullProgress());
			return annotation;
		}

		public void SaveNowIfNeeded(IProgress progress)
		{
			_repository.SaveNowIfNeeded(progress);
		}
	}
}