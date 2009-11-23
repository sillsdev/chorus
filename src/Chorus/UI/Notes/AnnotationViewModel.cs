using System;
using System.Collections.Generic;
using System.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using Chorus.annotations;
using Message=Chorus.annotations.Message;

namespace Chorus.UI.Notes
{
	public class AnnotationViewModel
	{
		private readonly StyleSheet _styleSheet;
		private Annotation _currentAnnotation;
		private Message _currentFocussedMessage; //this is the part of the annotation in focus

		internal event EventHandler UpdateDisplay;

		public AnnotationViewModel(MessageSelectedEvent messageSelectedEventToSubscribeTo, StyleSheet styleSheet)
		{
			_styleSheet = styleSheet;
			messageSelectedEventToSubscribeTo.Subscribe((annotation, message) => SetAnnotationAndFocussedMessage(annotation,message));

		}

		private void SetAnnotationAndFocussedMessage(Annotation annotation, Message message)
		{
			_currentAnnotation = annotation;
			_currentFocussedMessage = message;
			if (UpdateDisplay != null)
			{
				UpdateDisplay.Invoke(this, null);
			}
		}

		public ListMessage CurrentAnnotation
		{
		   // get { return _currentAnnotation; }
			set
			{
				//it's fine if this null
			}
		}

		public bool AddButtonEnabled
		{
			get { return false; }
		}

		public string GetNewMessageHtml()
		{
			if (_currentAnnotation == null)
				return string.Empty;

			return "<html><body></body></html>";

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
				if (message.Guid == _currentFocussedMessage.Guid) //REVIEW: guid shouldn't be needed
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
					builder.AppendLine(message.HtmlText);

					if (message.Status != status)
					{
						if (status != string.Empty && message.Status.ToLower() != "open")//don't show the first status if it's just 'open'
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

		public bool IsResolved
		{
			get { return false; }
		}

		public bool ResolvedControlShouldBeVisible
		{
			get { return false; }
		}

		public string ClassLabel
		{
			get { return _currentAnnotation.ClassName; }
		}

		public string DetailsText
		{
			get { return string.Format("ref={0  } status={1}", _currentAnnotation.Ref, _currentAnnotation.Status); }
		}

		public Image GetAnnotationLogoImage()
		{
			return _currentAnnotation.GetImage(32);
		}
	}
}
