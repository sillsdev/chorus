using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.sync;
using Message=Chorus.notes.Message;

namespace Chorus.UI.Notes
{
	public class NotesInProjectModel
	{
		private readonly ChorusNotesUser _currentUser;
		private List<NotesRepository> _repositories=new List<NotesRepository>();

		public NotesInProjectModel(ChorusNotesUser currentUser, ProjectFolderConfiguration projectFolderConfiguration)
		{
			_currentUser = currentUser;
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
	}

	public class ListMessage
	{
		public Annotation ParentAnnotation { get; private set; }
		public Message Message { get; private set; }

		public ListMessage(Annotation parentAnnotation, Message message)
		{
			ParentAnnotation = parentAnnotation;
			Message = message;
		}

		public ListViewItem GetListViewItem()
		{
			var i = new ListViewItem(ParentAnnotation.Class);
			i.Tag = this;
			i.SubItems.Add(Message.Date.ToShortDateString());
			i.SubItems.Add(ParentAnnotation.GetLabel("unknown"));
			i.SubItems.Add(Message.GetAuthor("unknown"));
			return i;
		}
	}
}