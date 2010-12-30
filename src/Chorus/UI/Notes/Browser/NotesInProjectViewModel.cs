using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.notes;
using Chorus.sync;
using Chorus.Utilities;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Notes.Browser
{
	public class NotesInProjectViewModel : IDisposable, IAnnotationRepositoryObserver
	{
		public delegate NotesInProjectViewModel Factory(IEnumerable<AnnotationRepository> repositories, IProgress progress);//autofac uses this
		internal event EventHandler ReloadMessages;

		private readonly IChorusUser _user;
		private readonly MessageSelectedEvent _messageSelectedEvent;
		private IEnumerable<AnnotationRepository> _repositories;
		private string _searchText;
		private bool _reloadPending=true;

		public NotesInProjectViewModel( IChorusUser user, IEnumerable<AnnotationRepository> repositories,
										MessageSelectedEvent messageSelectedEventToRaise, IProgress progress)
		{
			_user = user;
			_repositories = repositories;
			_messageSelectedEvent = messageSelectedEventToRaise;

			foreach (var repository in repositories)
			{
				repository.AddObserver(this, progress);
			}
		}


		private bool _showClosedNotes;
		public bool ShowClosedNotes
		{
			get { return _showClosedNotes; }
			set
			{
				_showClosedNotes = value;
				ReloadMessagesNow();
			}
		}

		public IEnumerable<ListMessage> GetMessages()
		{
			return GetMessagesUnsorted().OrderByDescending((msg) => msg.Date);
		}

		private IEnumerable<ListMessage> GetMessagesUnsorted()
		{
			foreach (var repository in _repositories)
			{
				IEnumerable<Annotation> annotations=  repository.GetAllAnnotations();
				if(!ShowClosedNotes)
				{
					annotations= annotations.Where(a=>a.Status!="closed");
				}

				foreach (var annotation in annotations)
				{
					foreach (var message in annotation.Messages)
					{
						if (GetShouldBeShown(annotation, message))
						{
							yield return new ListMessage(annotation, message);
						}
					}
				}
			}
		}

		private bool GetShouldBeShown(Annotation annotation, Message message)
		{
//            if (!ShowClosedNotes)
//            {
//                if (annotation.IsClosed)
//                    return false;
//            }

			if (string.IsNullOrEmpty(_searchText))
				return true;

			string t = _searchText.ToLowerInvariant();
			if(  annotation.LabelOfThingAnnotated.ToLowerInvariant().StartsWith(t)
				   || annotation.ClassName.ToLowerInvariant().StartsWith(t)
				   || message.Author.ToLowerInvariant().StartsWith(t))
				return true;

			if (t.Length > 2)//arbitrary, but don't want to search on ever last letter
			{
				return message.Text.ToLowerInvariant().Contains(t);
			}
			return false;
		}

		public void CloseAnnotation(ListMessage listMessage)
		{
			listMessage.ParentAnnotation.AddMessage(_user.Name, "closed", string.Empty);
		}

		public void SelectedMessageChanged(ListMessage listMessage)
		{
			if (_messageSelectedEvent != null)
			{
				if (listMessage == null) //nothing is selected now
				{
					_messageSelectedEvent.Raise(null, null);
				}
				else
				{
					_messageSelectedEvent.Raise(listMessage.ParentAnnotation, listMessage.Message);
				}
			}
		}

		public void SearchTextChanged(string searchText)
		{
			_searchText = searchText;
			ReloadMessagesNow();
		}

		private void ReloadMessagesNow()
		{
			if(ReloadMessages!=null)
				ReloadMessages(this,null);

			_reloadPending = false;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			foreach (var repository in _repositories)
			{
				repository.RemoveObserver(this);
			}
		}

		#endregion

		#region Implementation of IAnnotationRepositoryObserver

		public void Initialize(Func<IEnumerable<Annotation>> allAnnotationsFunction, IProgress progress)
		{
		}

		public void NotifyOfAddition(Annotation annotation)
		{
			_reloadPending=true;
			SaveChanges();
		}


		private void SaveChanges()
		{
			//this is a bit of a hack... seems like different clients have different times of saving;
			//not sure what the better answer would be.

			foreach (var repository in _repositories)
			{
				repository.SaveNowIfNeeded(new NullProgress());
			}
		}

		public void NotifyOfModification(Annotation annotation)
		{
			_reloadPending = true;
			SaveChanges();
		}

		public void NotifyOfDeletion(Annotation annotation)
		{
			_reloadPending = true;
			SaveChanges();
		}

		#endregion

		public void CheckIfWeNeedToReload()
		{
			if(_reloadPending)
				ReloadMessagesNow();
		}
	}
}