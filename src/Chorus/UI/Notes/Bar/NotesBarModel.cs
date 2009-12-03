using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.annotations;

namespace Chorus.UI.Notes.Bar
{
	public class NotesBarModel
	{
		private readonly AnnotationRepository _repository;
		private readonly AnnotationIndex _keyToAnnotationIndex;
		private readonly string _key;
		internal event EventHandler UpdateContent;

		public NotesBarModel(AnnotationRepository repository, AnnotationIndex keyToAnnotationIndex, string key)
		{
			_repository = repository;
			_keyToAnnotationIndex = keyToAnnotationIndex;
			_key = key;
		}

		public IEnumerable<Annotation> GetAnnotationsToShow()
		{
			return _keyToAnnotationIndex.GetMatchesByKey(_key);
			//todo: add controls for adding new notes, showing closed ones, etc.
		}



		private void UpdateContentNow()
		{
			if(UpdateContent!=null)
				UpdateContent.Invoke(this, null);
		}

	}
}