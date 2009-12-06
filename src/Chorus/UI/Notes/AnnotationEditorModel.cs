using System;
using System.Collections.Generic;
using System.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.annotations;
using Message=Chorus.annotations.Message;

namespace Chorus.UI.Notes
{


	public class AnnotationEditorModel
	{
		public delegate AnnotationEditorModel Factory(Annotation annotation);//autofac uses this

		private readonly ChorusNotesUser _user;
		private readonly StyleSheet _styleSheet;
		private Annotation _currentAnnotation;
		private Message _currentFocussedMessage; //this is the part of the annotation in focus
		private string _newMessageText;
		private EmbeddedMessageContentHandlerFactory _embeddedMessageContentHandlerFactory;

		internal event EventHandler UpdateContent;
		internal event EventHandler UpdateStates;

		public AnnotationEditorModel(ChorusNotesUser user,
			MessageSelectedEvent messageSelectedEventToSubscribeTo,
			StyleSheet styleSheet,
			EmbeddedMessageContentHandlerFactory embeddedMessageContentHandlerFactory)
		{
			_user = user;
			_embeddedMessageContentHandlerFactory = embeddedMessageContentHandlerFactory;
			_styleSheet = styleSheet;
			messageSelectedEventToSubscribeTo.Subscribe((annotation, message) => SetAnnotationAndFocussedMessage(annotation,message));
			NewMessageText = string.Empty;
		}

		//TODO: think about or merge these two constructors. this one is for when we're just
		//showing the control with a single annotation... it isn't tied to a list of messages.
		public AnnotationEditorModel(ChorusNotesUser user,
		   StyleSheet styleSheet,
		   EmbeddedMessageContentHandlerFactory embeddedMessageContentHandlerFactory,
			Annotation annotation)
		{
			_user = user;
			_embeddedMessageContentHandlerFactory = embeddedMessageContentHandlerFactory;
			_styleSheet = styleSheet;
			NewMessageText = string.Empty;
			_currentAnnotation = annotation;
		}


		private void SetAnnotationAndFocussedMessage(Annotation annotation, Message message)
		{
			_currentAnnotation = annotation;
			_currentFocussedMessage = message;
			UpdateContentNow();
		}

		private void UpdateContentNow()
		{
			if (UpdateContent != null)
			{
				UpdateContent.Invoke(this, null);
			}
		}

		private void UpdateStatesNow()
		{
			if (UpdateStates != null)
			{
				UpdateStates.Invoke(this, null);
			}
		}

		public Annotation CurrentAnnotation
		{
		   get { return _currentAnnotation; }

		}

		public bool AddButtonEnabled
		{
			get { return _currentAnnotation.Status!="closed" && NewMessageText.Length>0; }
		}

		public string GetNewMessageHtml()
		{
			if (_currentAnnotation == null)
				return string.Empty;

			return "<html><body></body></html>";

		}

		public IEnumerable<Message> Messages
		{
			get { return _currentAnnotation.Messages; }
		}

		public string GetExistingMessagesHtml()
		{
			if(_currentAnnotation == null)
				return string.Empty;

			var builder = new StringBuilder();
			builder.AppendLine("<html>");
			builder.AppendFormat("<head>{0}</head>", _styleSheet.TextForInsertingIntoHmtlHeadElement);
			builder.AppendLine("<body>");

			string status=string.Empty;
			foreach (var message in _currentAnnotation.Messages)
			{
				builder.AppendLine("<hr/>");
				if (_currentFocussedMessage!=null && message.Guid == _currentFocussedMessage.Guid) //REVIEW: guid shouldn't be needed
				{
					builder.AppendLine("<div class='selected message'>");
				}
				else
				{
					builder.AppendLine("<div class='message'>");
				}

				//add rounded borders CAN'T GET THIS STUFF TO WORK IN THE EMBEDDED BROWSER (BUT IT'S OK IN IE & FIREFOX)
//                builder.AppendLine(
//                    "<div class='t'><div class='b'><div class='l'><div class='r'><div class='bl'><div class='br'><div class='tl'><div class='tr'>");


					builder.AppendFormat("<span class='sender'>{0}</span> <span class='when'> on {1}</span>", message.Author, message.Date.ToLongDateString());

					builder.AppendLine("<div class='messageContents'>");
					builder.AppendLine(message.GetHtmlText(_embeddedMessageContentHandlerFactory));
//                    if(message.HasEmbeddedData)
//                    {
//                        builder.AppendLine(message.HtmlText);
//                    }

				if (message.Status != status)
					{
						if (status != string.Empty || message.Status.ToLower() != "open")//don't show the first status if it's just 'open'
						{
							builder.AppendFormat(
								"<div class='statusChange'>{0} marked the note as <span class='status'>{1}</span>.</div>",
								message.Author, message.Status);
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

		public bool IsClosed
		{
			get { return _currentAnnotation.Status == "closed";}
			set
			{
				_currentAnnotation.SetStatus(_user.Name, value? "closed":"open");
				UpdateContentNow();
			}
		}

		public bool ResolvedControlShouldBeVisible
		{
			get { return _currentAnnotation.CanResolve; }
		}

		public string ClassLabel
		{
			get { return _currentAnnotation.ClassName; }
		}

		public string DetailsText
		{
			get { return string.Format("ref={0  } status={1}", _currentAnnotation.RefStillEscaped, _currentAnnotation.Status); }
		}


		public bool ShowNewMessageControls
		{
			get { return _currentAnnotation.Status != "closed"; }
		}

		public string NewMessageText
		{
			get {
				return _newMessageText;
			}
			set {
				_newMessageText = value;
				UpdateStatesNow();
			}
		}

		public bool IsVisible
		{//wait for an annotation to be selected
			get { return _currentAnnotation != null; }
		}

		public string CloseButtonText
		{
			get
			{
				if (_newMessageText.Length > 0)
				{
					return "Add && &Close";
				}
				else
				{
				   return "&Close";
				}
			}
		}

		public Image GetAnnotationLogoImage()
		{
			return _currentAnnotation.GetImage(32);
		}

		public void AddButtonClicked()
		{
			_currentAnnotation.AddMessage(_user.Name, null, NewMessageText);
			NewMessageText = string.Empty;
			UpdateContentNow();
		}

		public string GetAllInfoForMessageBox()
		{
			return _currentAnnotation.GetDiagnosticDump();
		}
//
//        public Control GetControlForMessage(Message message)
//        {
//            var browser = new WebBrowser();
//            browser.DocumentText = GetHtmlForMessage(message);
//            return browser;
//        }
//        private string GetHtmlForMessage(Message message)
//        {
//            var builder = new StringBuilder();
//            builder.AppendLine("<html>");
//            builder.AppendFormat("<head>{0}</head>", _styleSheet.TextForInsertingIntoHmtlHeadElement);
//            builder.AppendLine("<body>");
//
//            string status = string.Empty;
//                builder.AppendLine("<hr/>");
//                if (message.Guid == _currentFocussedMessage.Guid) //REVIEW: guid shouldn't be needed
//                {
//                    builder.AppendLine("<div class='selected message'>");
//                }
//                else
//                {
//                    builder.AppendLine("<div class='message'>");
//                }
//
//                //add rounded borders CAN'T GET THIS STUFF TO WORK IN THE EMBEDDED BROWSER (BUT IT'S OK IN IE & FIREFOX)
//                //                builder.AppendLine(
//                //                    "<div class='t'><div class='b'><div class='l'><div class='r'><div class='bl'><div class='br'><div class='tl'><div class='tr'>");
//
//
//                builder.AppendFormat("<span class='sender'>{0}</span> <span class='when'> on {1}</span>", message.Author, message.Date.ToLongDateString());
//
//                builder.AppendLine("<div class='messageContents'>");
//                builder.AppendLine(message.HtmlText);
//
//                if (message.Status != status)
//                {
//                    if (status != string.Empty || message.Status.ToLower() != "open")//don't show the first status if it's just 'open'
//                    {
//                        builder.AppendFormat(
//                            "<div class='statusChange'>{0} marked the note as <span class='status'>{1}</span>.</div>",
//                            message.Author, message.Status);
//                    }
//                    status = message.Status;
//                }
//
//                builder.AppendLine("</div>");
//                //close off rounded borders
//                //can't get it to work... builder.AppendLine("</div></div></div></div></div></div></div></div>");
//                builder.AppendLine("</div>");
//
//
//            builder.AppendLine("</body>");
//            builder.AppendLine("</html>");
//
//            return builder.ToString();
//        }
		public void HandleLinkClicked(Uri uri)
		{
			var handler = _embeddedMessageContentHandlerFactory.GetHandlerOrDefaultForUrl(uri);
			if(handler!=null)
			{
				handler.HandleUrl(uri);
			}
		}
	}
}
