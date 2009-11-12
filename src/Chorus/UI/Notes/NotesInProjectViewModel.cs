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
	public class NotesInProjectViewModel
	{
		private readonly ChorusNotesUser _currentUser;
		private readonly MessageSelectedEvent _messageSelectedEvent;
		private List<NotesRepository> _repositories=new List<NotesRepository>();

		public NotesInProjectViewModel(ChorusNotesUser currentUser, ProjectFolderConfiguration projectFolderConfiguration, MessageSelectedEvent messageSelectedEventToRaise)
		{
			_currentUser = currentUser;
			_messageSelectedEvent = messageSelectedEventToRaise;
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

		public void SelectedMessageChanged(ListMessage listMessage)
		{
			if (_messageSelectedEvent != null)
				_messageSelectedEvent.Raise(listMessage.ParentAnnotation, listMessage.Message);
		}
	}
}