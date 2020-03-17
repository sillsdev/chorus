using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Chorus.notes;
using L10NSharp;
using SIL.Progress;
using SIL.Reporting;

namespace Chorus.UI.Notes.Browser
{
	public class NotesInProjectViewModel : IDisposable, IAnnotationRepositoryObserver
	{
		public ChorusNotesDisplaySettings DisplaySettings { get; set; }

		public delegate NotesInProjectViewModel Factory(IEnumerable<AnnotationRepository> repositories, IProgress progress);//autofac uses this
		internal event EventHandler ReloadMessages;

		private readonly IChorusUser _user;
		private IEnumerable<AnnotationRepository> _repositories;
		private bool _reloadPending=true;

		private ListMessage _selectedMessage;
		/// <summary>
		/// The GUID of the Message that *should* be selected;
		/// </summary>
		public string SelectedMessageGuid
		{
			get { return _selectedMessage == null ? null : _selectedMessage.ParentAnnotation.Guid; }
		}

		public NotesInProjectViewModel( IChorusUser user, IEnumerable<AnnotationRepository> repositories,
										ChorusNotesDisplaySettings displaySettings, IProgress progress)
		{
			DisplaySettings = displaySettings;
			_user = user;
			_repositories = repositories;

			_showQuestions = _showConflicts = _showNotifications = true;

			foreach (var repository in repositories)
			{
				repository.AddObserver(this, progress);
			}
		}

		private event CancelEventHandler _messageSelectedEvent;
		/// <summary>
		/// Param [sender] is the newly-selected ListMessage
		/// </summary>
		public CancelEventHandler EventToRaiseForChangedMessage
		{
			get { return _messageSelectedEvent; }
			set { _messageSelectedEvent = value; }
		}

		private bool _showClosedNotes;
		public bool ShowClosedNotes
		{
			get { return _showClosedNotes; }
			set { FilterChanged(value, null, null, null, null); }
		}

		private bool _showQuestions;
		public bool ShowQuestions
		{
			get { return _showQuestions; }
			set { FilterChanged(null, value, null, null, null); }
		}

		private bool _showConflicts;
		/// <summary>
		/// This controls only critical conflicts (those that are not notifications).
		/// </summary>
		public bool ShowConflicts
		{
			get { return _showConflicts; }
			set { FilterChanged(null, null, value, null, null); }
		}

		private bool _showNotifications;
		/// <summary>
		/// Notifications are a type of Conflict considered to be lower priority.
		/// Typically where both users added something, we aren't quite sure of the order, but no actual data loss
		/// has occurred.
		/// </summary>
		public bool ShowNotifications
		{
			get { return _showNotifications; }
			set { FilterChanged(null, null, null, value, null); }
		}

		private string _searchText;
		public string SearchText
		{
			get { return _searchText; }
			set { FilterChanged(null, null, null, null, value); }
		}

		public string FilterStateMessage
		{
			get
			{
				var items = new List<string>();
				if (ShowQuestions)
					items.Add(LocalizationManager.GetString("NotesInProjectView.Questions", "Questions",
						"Combined in list to show filter status (keep short!)"));
				if (ShowConflicts)
					items.Add(LocalizationManager.GetString("NotesInProjectView.Conflicts", "Conflicts",
						"Combined in list to show filter status (keep short!)"));
				if (ShowNotifications)
					items.Add(LocalizationManager.GetString("NotesInProjectView.Notifications", "Notifications",
						"Combined in list to show filter status (keep short!)"));
				if (ShowClosedNotes && items.Count > 0)
					items.Add(LocalizationManager.GetString("NotesInProjectView.IncludeResolved", "incl. Resolved",
						"Combined in list to show filter status (keep short!)"));

				// NB: If we add another category, this number needs bumping.
				if (items.Count == 4)
					return LocalizationManager.GetString("NotesInProjectView.All", "All",
						"Used in place of list for filter status");
				if (items.Count == 0)
					return LocalizationManager.GetString("NotesInProjectView.Nothing", "Nothing selected to display",
						"Used in place of list for filter status");
				return string.Join(", ", items.ToArray());
			}
		}

		public IEnumerable<ListMessage> GetMessages()
		{
			return GetMessagesUnsorted().OrderByDescending(msg => msg.SortKey);
		}

		private IEnumerable<ListMessage> GetMessagesUnsorted()
		{
			foreach (var repository in _repositories)
			{
				IEnumerable<Annotation> annotations = repository.GetAllAnnotations();

				annotations = annotations.Where(a => MatchesFilterFlags(
					a, _showClosedNotes, _showQuestions, ShowConflicts, ShowNotifications));

				foreach (var annotation in annotations)
				{
					Message messageToShow = null;
					foreach (var message in annotation.Messages)
					{
						if (MatchesSearchText(annotation, message))
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

		private static bool MatchesFilterFlags(Annotation annotation, bool showClosedNotes,
			bool showQuestions, bool showConflicts, bool showNotifications)
		{
			return !((!showClosedNotes && annotation.IsClosed)
					 || (!showQuestions && annotation.ClassName == "question")
					 || (!showConflicts && annotation.IsCriticalConflict)
					 || (!showNotifications && annotation.IsNotification));
		}

		private bool MatchesSearchText(Annotation annotation, Message message)
		{
			if (string.IsNullOrEmpty(_searchText))
				return true;

			string t = _searchText.ToLowerInvariant();
			if (annotation.LabelOfThingAnnotated.ToLowerInvariant().StartsWith(t)
				   || annotation.ClassName.ToLowerInvariant().StartsWith(t)
				   || message.Author.ToLowerInvariant().StartsWith(t))
				return true;

			if (t.Length > 2)//arbitrary, but don't want to search on ever last letter
				return message.Text.ToLowerInvariant().Contains(t);

			return false;
		}

		private static bool MatchesSearchText(Annotation annotation, string searchText)
		{
			if (string.IsNullOrEmpty(searchText))
				return true;

			string t = searchText.ToLowerInvariant();
			if(annotation.LabelOfThingAnnotated.ToLowerInvariant().StartsWith(t)
				   || annotation.ClassName.ToLowerInvariant().StartsWith(t)
				   || annotation.Messages.Any(m => m.Author.ToLowerInvariant().StartsWith(t)))
				return true;

			if (t.Length > 2) // arbitrary, but don't want to search on ever last letter
				return annotation.Messages.Any(m => m.Text.ToLowerInvariant().Contains(t));

			return false;
		}

		private bool UserCanceledSelectedMessageChange(ListMessage listMessage)
		{
			var e = new CancelEventArgs();
			_messageSelectedEvent.Invoke(listMessage, e);
			return e.Cancel;
		}

		public void SelectedMessageChanged(ListMessage listMessage)
		{
			if (_messageSelectedEvent != null)
			{
				if (UserCanceledSelectedMessageChange(listMessage))
				{
					if ((listMessage == null ? null : listMessage.ParentAnnotation.Guid) != SelectedMessageGuid)
					{
						// if GUID's are different, reload messages to switch back,
						// but wait _after_ the canceled selection change event completes.
						// The View's Timer will pick this up
						_reloadPending = true;

						// Enhance pH 2013.08: figure out how to keep the list view
						// (or at least the canceled selection) from blinking during this refresh
					}
				}
				else
				{
					_selectedMessage = listMessage;
				}
			}

			GoodTimeToSave();
		}

		private void FilterChanged(bool? showClosedNotes,
			bool? showQuestions, bool? showConflicts, bool? showNotifications, string searchText)
		{
			// populate fields (we expect all but one to be null)
			showClosedNotes = showClosedNotes ?? ShowClosedNotes;
			showQuestions = showQuestions ?? ShowQuestions;
			showConflicts = showConflicts ?? ShowConflicts;
			showNotifications = showNotifications ?? ShowNotifications;
			searchText = searchText ?? SearchText;

			// Verify the user will not lose work:
			if (_selectedMessage != null // There is an annotation selected
				// The selected annotation would be hidden by the new filter
				&& (!MatchesFilterFlags(_selectedMessage.ParentAnnotation, showClosedNotes.Value,
										showQuestions.Value, showConflicts.Value, showNotifications.Value)
					|| !MatchesSearchText(_selectedMessage.ParentAnnotation, searchText))
				// The user has typed a new Message and wants to continue working
				&& UserCanceledSelectedMessageChange(null))
				// Do nothing; the filter will not be changed, and the view should reset itself
				return;

			// update the filter and reload the message list
			_showClosedNotes = showClosedNotes.Value;
			_showQuestions = showQuestions.Value;
			_showConflicts = showConflicts.Value;
			_showNotifications = showNotifications.Value;
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

		public void NotifyOfStaleList()
		{
			_reloadPending = true;
			ReloadMessagesNow();
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
					LocalizationManager.GetString("Messages.CannotFindRepo", "A serious problem has occurred; Chorus cannot find the repository which owns this note, so it cannot be saved."));
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