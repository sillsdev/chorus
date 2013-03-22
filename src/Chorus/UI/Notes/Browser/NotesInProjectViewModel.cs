using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.notes;
using Palaso.Progress;
using Palaso.Reporting;
using Palaso.Reporting;

namespace Chorus.UI.Notes.Browser
{
	public class NotesInProjectViewModel : IDisposable, IAnnotationRepositoryObserver
	{
		public ChorusNotesDisplaySettings DisplaySettings { get; set; }

		public delegate NotesInProjectViewModel Factory(IEnumerable<AnnotationRepository> repositories, IProgress progress);//autofac uses this
		internal event EventHandler ReloadMessages;

		private readonly IChorusUser _user;
		private MessageSelectedEvent _messageSelectedEvent;
		private IEnumerable<AnnotationRepository> _repositories;
		private string _searchText;
		private bool _reloadPending=true;

		public NotesInProjectViewModel( IChorusUser user, IEnumerable<AnnotationRepository> repositories,
										MessageSelectedEvent messageSelectedEventToRaise, ChorusNotesDisplaySettings displaySettings,
									IProgress progress)
		{
			DisplaySettings = displaySettings;
			_user = user;
			_repositories = repositories;
			_messageSelectedEvent = messageSelectedEventToRaise;

			foreach (var repository in repositories)
			{
				repository.AddObserver(this, progress);
			}
		}

		/// <summary>
		/// Where this AND the AnnotationEditorModel are both created by Autofac, they get created with different
		/// instances of MessageSelectedEvent. It's necessary to patch things up (currently in NotesBrowerPage constructor)
		/// so this one is given the instance that the AnnotationEditorModel is subscribed to.
		/// Don't know what we can do if we ever have other subscribers...
		/// </summary>
		public MessageSelectedEvent EventToRaiseForChangedMessage
		{
			get { return _messageSelectedEvent; }
			set { _messageSelectedEvent = value; }
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

		private bool _hideQuestions;
		public bool HideQuestions
		{
			get { return _hideQuestions; }
			set
			{
				_hideQuestions = value;
				ReloadMessagesNow();
			}
		}

		private bool _hideNotifications;
		/// <summary>
		/// Notifications are a type of Conflict considered to be lower priority.
		/// Typically where both users added something, we aren't quite sure of the order, but no actual data loss
		/// has occurred.
		/// </summary>
		public bool HideNotifications
		{
			get { return _hideNotifications; }
			set
			{
				_hideNotifications = value;
				ReloadMessagesNow();
			}
		}

		private bool _hideConflicts;
		public string FilterStateMessage
		{
			get
			{
				var items = new List<string>();
				if (!HideQuestions)
					items.Add("Questions");
				if (!HideCriticalConflicts)
					items.Add("Conflicts");
				if (!HideNotifications)
					items.Add("Notifications");
				if (ShowClosedNotes && items.Count > 0)
					items.Add("incl. Resolved");

				// NB: If we add another category, this number needs bumping.
				if (items.Count > 3)
					return "All";
				if (items.Count == 0)
					return "Nothing selected to display";
				return string.Join(", ", items.ToArray());
			}
		}
		/// <summary>
		/// This controls just the more serious conflicts (those that are not notifications).
		/// </summary>
		public bool HideCriticalConflicts
		{
			get { return _hideConflicts; }
			set
			{
				_hideConflicts = value;
				ReloadMessagesNow();
			}
		}

		public IEnumerable<ListMessage> GetMessages()
		{
			return GetMessagesUnsorted().OrderByDescending((msg) => msg.SortKey);
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
				if (HideQuestions)
				{
					annotations = annotations.Where(a => a.ClassName != "question");
				}

				if (HideCriticalConflicts)
				{
					if (HideNotifications)
					{
						// Hiding all conflicts, critical and otherwise
						annotations = annotations.Where(a => !a.IsConflict);
					}
					else
					{
						// Hiding critical conflicts only!
						annotations = annotations.Where(a => !a.IsCriticalConflict);
					}
				}
				else if (HideNotifications)
				{
					// Hiding non-critical conflicts (notifications) only
					annotations = annotations.Where(a => !a.IsNotification);
				}

				foreach (var annotation in annotations)
				{
					Message messageToShow = null;
					foreach (var message in annotation.Messages)
					{
						if (GetShouldBeShown(annotation, message))
						{
							if (messageToShow == null || messageToShow.Date < message.Date)
								messageToShow = message;
						}
					}
					if (messageToShow != null)
						yield return new ListMessage(annotation, messageToShow);
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
			GoodTimeToSave();
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
			//NB: this notification would come from the repository, not the view
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
			//NB: this notification would come from the repository, not the view
			_reloadPending = true;
			SaveChanges();
		}

		private void Save(Annotation annotation)
		{
			var owningRepo = _repositories.Where(r => r.ContainsAnnotation(annotation)).FirstOrDefault();
			if(owningRepo ==null)
			{
				ErrorReport.NotifyUserOfProblem(
					"A serious problem has occurred; Chorus cannot find the repository which owns this note, so it cannot be saved.");
				return;
			}

			owningRepo.SaveNowIfNeeded(new NullProgress());
		}

		public void NotifyOfDeletion(Annotation annotation)
		{
			//NB: this notification would come from the repository, not the view
			_reloadPending = true;
			SaveChanges();
		}

		#endregion

		public void CheckIfWeNeedToReload()
		{
			if(_reloadPending)
				ReloadMessagesNow();
		}

		public void GoodTimeToSave()
		{
			foreach (var repository in _repositories)
			{
				repository.SaveNowIfNeeded(new NullProgress());
			}
		}
	}
}