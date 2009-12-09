using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.annotations;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI.Notes.Browser
{
	public class NotesInProjectViewModel
	{
		public delegate NotesInProjectViewModel Factory(IProgress progress);//autofac uses this

		private readonly IChorusUser _user;
		private readonly MessageSelectedEvent _messageSelectedEvent;
		private List<AnnotationRepository> _repositories=new List<AnnotationRepository>();
		private string _searchText;

		public NotesInProjectViewModel( IChorusUser user, ProjectFolderConfiguration projectFolderConfiguration,
										MessageSelectedEvent messageSelectedEventToRaise, IProgress progress)
		{
			_user = user;
			_messageSelectedEvent = messageSelectedEventToRaise;
			foreach (var path in GetChorusNotesFilePaths(projectFolderConfiguration.FolderPath))
			{
				_repositories.Add(AnnotationRepository.FromFile(string.Empty, path, progress));
			}
		}

		private IEnumerable<string> GetChorusNotesFilePaths(string path)
		{
			return Directory.GetFiles(path, "*." + AnnotationRepository.FileExtension, SearchOption.AllDirectories);
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
						if (GetDoesMatch(annotation, message))
						{
							yield return new ListMessage(annotation, message);
						}
					}
				}
			}
		}



		private bool GetDoesMatch(Annotation annotation, Message message)
		{
			return string.IsNullOrEmpty(_searchText)
				   || annotation.LabelOfThingAnnotated.StartsWith(_searchText)
				   || annotation.ClassName.StartsWith(_searchText)
				   || message.Author.StartsWith(_searchText);
		}

		public void CloseAnnotation(ListMessage listMessage)
		{
			listMessage.ParentAnnotation.AddMessage(_user.Name, "closed", string.Empty);
		}

		public void SelectedMessageChanged(ListMessage listMessage)
		{
			if (_messageSelectedEvent != null)
				_messageSelectedEvent.Raise(listMessage.ParentAnnotation, listMessage.Message);
		}

		public void SearchTextChanged(string searchText)
		{
			int result;
			if(int.TryParse(searchText, out result))
			{
				throw new ApplicationException();
			}
			_searchText = searchText;
		}
	}
}