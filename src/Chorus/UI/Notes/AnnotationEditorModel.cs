using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Chorus.notes;
using Chorus.UI.Notes.Html;
using Chorus.UI.Review;
using L10NSharp;
using Message=Chorus.notes.Message;

namespace Chorus.UI.Notes
{


	public class AnnotationEditorModel
	{
		public delegate AnnotationEditorModel Factory(Annotation annotation, bool showLabelAsHyperlink);//autofac uses this

		private readonly IChorusUser _user;
		private readonly StyleSheet _styleSheet;
		private readonly NavigateToRecordEvent _navigateToRecordEventToRaise;
		private readonly ChorusNotesDisplaySettings _displaySettings;
		private Message _currentFocusedMessage; //this is the part of the annotation in focus
		private bool _showLabelAsHyperLink=true;
		public MessageSelectedEvent EventToRaiseForChangedMessage { get; }

		internal event EventHandler UpdateContent;
		internal event EventHandler UpdateStates;



		//TODO: think about or merge these two constructors. this one is for when we're just
		//showing the control with a single annotation... it isn't tied to a list of messages.
		public AnnotationEditorModel(IChorusUser user,
		   StyleSheet styleSheet,
		   EmbeddedMessageContentHandlerRepository embeddedMessageContentHandlerRepository,
			Annotation annotation,
			NavigateToRecordEvent navigateToRecordEventToRaise,
			ChorusNotesDisplaySettings displaySettings,
			bool showLabelAsHyperlink)
		{
			_user = user;
			MessageContentHandlerRepository = embeddedMessageContentHandlerRepository;
			_styleSheet = styleSheet;
			Annotation = annotation;
			_navigateToRecordEventToRaise = navigateToRecordEventToRaise;
			_displaySettings = displaySettings;
			_showLabelAsHyperLink = showLabelAsHyperlink;
		}

		public AnnotationEditorModel(IChorusUser user,
							MessageSelectedEvent messageSelectedEventToSubscribeTo,
							StyleSheet styleSheet,
							EmbeddedMessageContentHandlerRepository embeddedMessageContentHandlerRepository,
							NavigateToRecordEvent navigateToRecordEventToRaise,
						ChorusNotesDisplaySettings displaySettings)
		{
			_user = user;
			MessageContentHandlerRepository = embeddedMessageContentHandlerRepository;
			_navigateToRecordEventToRaise = navigateToRecordEventToRaise;
			_styleSheet = styleSheet;
			_displaySettings = displaySettings;
			messageSelectedEventToSubscribeTo.Subscribe(SetAnnotationAndFocussedMessage);
			EventToRaiseForChangedMessage = messageSelectedEventToSubscribeTo;
		}

		public EmbeddedMessageContentHandlerRepository MessageContentHandlerRepository { get; }

		private void SetAnnotationAndFocussedMessage(Annotation annotation, Message message)
		{
			Annotation = annotation;
			_currentFocusedMessage = message;
			UpdateContentNow();
		}

		private void UpdateContentNow()
		{
			if (UpdateContent != null)
			{
				UpdateContent.Invoke(this, null);
			}
		}

		public Annotation Annotation { get; private set; }

		public string GetNewMessageHtml()
		{
			if (Annotation == null)
				return string.Empty;

			return @"<html><body></body></html>";

		}

		public IEnumerable<Message> Messages
		{
			get { return Annotation.Messages; }
		}

		public string GetExistingMessagesHtml()
		{
			if(Annotation == null)
				return string.Empty;

			var builder = new StringBuilder();
			builder.AppendLine(@"<html>");
			builder.AppendFormat(@"<head>{0}</head>", _styleSheet.TextForInsertingIntoHmtlHeadElement);
			builder.AppendLine(@"<body>");

			string status=string.Empty;
			foreach (var message in Annotation.Messages)
			{
				builder.AppendLine(@"<hr/>");
				if (_currentFocusedMessage!=null && message.Guid == _currentFocusedMessage.Guid) //REVIEW: guid shouldn't be needed
				{
					builder.AppendLine(@"<div class='selected message'>");
				}
				else
				{
					builder.AppendLine(@"<div class='message'>");
				}

				//add rounded borders CAN'T GET THIS STUFF TO WORK IN THE EMBEDDED BROWSER (BUT IT'S OK IN IE & FIREFOX)
//				builder.AppendLine(
//					"<div class='t'><div class='b'><div class='l'><div class='r'><div class='bl'><div class='br'><div class='tl'><div class='tr'>");


					builder.AppendFormat(@"<span class='sender'>{0}</span> <span class='when'> - {1}</span>", message.Author, message.Date.ToLongDateString());

					builder.AppendLine(@"<div class='messageContents'>");
					builder.AppendLine(message.GetHtmlText(MessageContentHandlerRepository));

					if (message.Status != status)
					{
						if (status != string.Empty || message.Status.ToLower() != Annotation.Open)//don't show the first status if it's just 'open'
						{
							// Enhance pH 2013.08: change this text to an image (check or red flag), similar to ParaTExt?
							builder.AppendFormat(
								@"<div class='statusChange'>" +
								LocalizationManager.GetString("Messages.MarkedNotAs", "{0} marked the note as {1}.") + @"</div>",
								message.Author,
								// add span tags *after* localization; change "closed" to "resolved" to match the button
								@"<span class='status'>" +
								(message.Status.ToLower() == Annotation.Closed ? "resolved" : message.Status) + @"</span>");
						}
						status = message.Status;
					}

					builder.AppendLine("</div>");
				//close off rounded borders
				//can't get it to work... builder.AppendLine("</div></div></div></div></div></div></div></div>");
				builder.AppendLine("</div>");

			}
			builder.AppendLine("</body>");
			builder.AppendLine("</html>");

			return builder.ToString();
		}

		public bool IsResolved
		{
			get { return Annotation.Status == Annotation.Closed; }
			set
			{
				Annotation.SetStatus(_user.Name, value ? Annotation.Closed : Annotation.Open);
				UpdateContentNow();
			}
		}

		public bool ResolvedControlShouldBeVisible
		{
			get { return Annotation.CanResolve; }
		}

		public string ClassLabel
		{
			get { return Annotation.ClassName; }
		}

		public string DetailsText
		{
			get { return string.Format("ref={0} status={1}", Annotation.RefStillEscaped, Annotation.Status); }
		}

		public bool IsVisible
		{//wait for an annotation to be selected
			get { return Annotation != null; }
		}

		public string ResolveButtonText
		{
			get
			{
				if (IsResolved)
				{
					return LocalizationManager.GetString("AnnotationEditorView.UnresolveNote", "Un&resolve Note");
				}
				else
				{
					return LocalizationManager.GetString("AnnotationEditorView.ResolveNote", "&Resolve Note");
				}
			}
		}

		public string GetOKButtonText(bool closeButtonVisible)
		{
			if (closeButtonVisible)
			{
				return LocalizationManager.GetString("Common.OK", "&OK");
			}
			else
			{
				return LocalizationManager.GetString("AnnotationEditorView.AddMessage", "&Add Message");
			}
		}

		public string AnnotationLabel
		{
			get { return Annotation.LabelOfThingAnnotated; }
		}

		//In a dialog situation, we might not want to offer the hyperlink, if we don't plan to act on it.
		//Or, if we happen to know that noone is listenting...
		public bool ShowLabelAsHyperlink
		{
			get { return _showLabelAsHyperLink && _navigateToRecordEventToRaise.HasSubscribers; }
			set {_showLabelAsHyperLink = value;}
		}

		public Font FontForNewMessage
		{
			get { return new Font(_displaySettings.WritingSystemForNoteContent.FontName, _displaySettings.WritingSystemForNoteContent.FontSize); }
		}
		public Font FontForLabel
		{
			get { return new Font(_displaySettings.WritingSystemForNoteLabel.FontName, 14); }
		}

		/// <summary>
		/// Note that the icon used is independent of whether the annotation is resolved/closed or not.
		/// AnnotationEditorView._annotationLogo_Paint paints the check mark over the top if needed.
		/// (This is different from the 16x16 strategy, where we have fine-tuned distinct icons.)
		/// </summary>
		/// <returns></returns>
		public Image GetAnnotationLogoImage()
		{
			return Annotation.GetImage(32);
		}

		public string GetLongLabel()
		{
			return Annotation.GetLongLabel();
		}

		public void AddMessage(string newMessageText)
		{
			Annotation.AddMessage(_user.Name, null, newMessageText);
			UpdateContentNow();
		}

		/// <summary>
		/// Inverts whether the annotation is resolved or unresolved, adding the message text supplied
		/// </summary>
		/// <param name="newMessageText"></param>
		public void UnResolveAndAddMessage(string newMessageText)
		{
			Annotation.AddMessage(_user.Name,
				IsResolved ? Annotation.Open : Annotation.Closed, // Invert the status
				newMessageText);
			UpdateContentNow();
		}

		public string GetAllInfoForMessageBox()
		{
			return Annotation.GetDiagnosticDump();
		}

		public void HandleLinkClicked(Uri uri)
		{
			var handler = MessageContentHandlerRepository.GetHandlerOrDefaultForUrl(uri);
			if(handler!=null)
			{
				handler.HandleUrl(uri, Annotation.AnnotationFilePath);
			}
		}

		public void JumpToAnnotationTarget()
		{
			_navigateToRecordEventToRaise.Raise(Annotation.RefUnEscaped);
		}

		public void ActivateKeyboard()
		{
			_displaySettings.WritingSystemForNoteContent.ActivateKeyboard();
		}

		public IWritingSystem LabelWritingSystem
		{
			set { _displaySettings.WritingSystemForNoteLabel = value; }
		}

		public IWritingSystem MessageWritingSystem
		{
			set { _displaySettings.WritingSystemForNoteContent = value; }
		}
	}
}
