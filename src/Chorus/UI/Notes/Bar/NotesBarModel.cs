using System;
using System.Collections.Generic;
using Chorus.annotations;

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
		internal event EventHandler UpdateContent;

		public NotesBarModel(AnnotationRepository repository)
		{
			_repository = repository;
		}

		public IEnumerable<Annotation> GetAnnotationsToShow(string key)
		{
			return _repository.GetMatchesByPrimaryRefKey(key);
			//todo: add controls for adding new notes, showing closed ones, etc.
		}

//        private void UpdateContentNow()
//        {
//            if(UpdateContent!=null)
//                UpdateContent.Invoke(this, null);
//        }

	}
}