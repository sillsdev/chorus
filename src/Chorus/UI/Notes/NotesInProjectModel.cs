using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.notes;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Notes
{
	public class NotesInProjectModel
	{
		private readonly ChorusNotesUser _currentUser;
		private readonly AnnotationSelectedEvent _annotationSelectedEvent;
		private List<NotesRepository> _repositories=new List<NotesRepository>();

		public NotesInProjectModel(ChorusNotesUser currentUser, ProjectFolderConfiguration projectFolderConfiguration, AnnotationSelectedEvent annotationSelectedEventToRaise)
		{
			_currentUser = currentUser;
			_annotationSelectedEvent = annotationSelectedEventToRaise;
			foreach (var path in GetChorusNotesFilePaths(projectFolderConfiguration.FolderPath))
			{
				_repositories.Add(NotesRepository.FromFile(path));
			}
		}

		private IEnumerable<string> GetChorusNotesFilePaths(string path)
		{
			return Directory.GetFiles(path, "*." + NotesRepository.FileExtension, SearchOption.AllDirectories);
		}

		public bool ShowClosedNotes { get; set; }

		public IEnumerable<ListMessage> GetMessages()
		{
			foreach (var repository in _repositories)
			{
				IEnumerable<Annotation> annotations=  repository.GetAllAnnotations();
				if(ShowClosedNotes)
				{
					annotations= annotations.Where(a=>a.Status!="closed");
				}

				foreach (var annotation in annotations)
				{
					foreach (var message in annotation.Messages)
					{
						yield return new ListMessage(annotation, message);
					}
				}
			}
		}

		public void CloseAnnotation(ListMessage listMessage)
		{
			listMessage.ParentAnnotation.AddMessage(_currentUser.Name, "closed", string.Empty);
		}

		public void SelectedAnnotationChanged(ListMessage descriptor)
		{
			if (_annotationSelectedEvent != null)
				_annotationSelectedEvent.Raise(descriptor);
		}
	}
}