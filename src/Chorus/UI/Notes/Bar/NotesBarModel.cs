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

		public void SetIdOfCurrentAnnotatedObject(string key)
		{
			if (key != _idOfCurrentAnnotatedObject)
			{
				_idOfCurrentAnnotatedObject = key;
				UpdateContentNow();
			}
		}

		internal event EventHandler UpdateContent;

		public NotesBarModel(AnnotationRepository repository)
		{
			_repository = repository;
			UrlGenerater = ChorusNotesSystem.DefaultGenerator;
		}

		public ChorusNotesSystem.UrlGenerator UrlGenerater { get; set; }

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
			var url = UrlGenerater(escapedIdOfCurrentAnnotatedObject);
		  //  url = Uri.EscapeUriString(url);//change those pesky & inbetween parameters to &amp;, but leave the single quotes alone
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