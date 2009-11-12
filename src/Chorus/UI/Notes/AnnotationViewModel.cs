using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chorus.notes;
using Message=Chorus.notes.Message;

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

		public string GetHtml()
		{
			if(_currentAnnotation == null)
				return string.Empty;

			var builder = new StringBuilder();
			builder.AppendLine("<html>");
			builder.AppendFormat("<head>{0}</head>", _styleSheet.TextForInsertingIntoHmtlHeadElement);
			builder.AppendLine("<body>");
			builder.AppendFormat("<div class='AnnotationHeader'>Class={0} ref={1} status={2}</div>",_currentAnnotation.Class, _currentAnnotation.Ref, _currentAnnotation.Status);
			foreach (var message in _currentAnnotation.Messages)
			{
			   if (message.Guid == _currentFocussedMessage.Guid) //REVIEW: guid shouldn't be needed
				{
					builder.AppendLine("<div class='selected'>");
				}
				else
				{
					builder.AppendLine("<div>");
				}

				//add rounded borders CAN'T GET THIS STUFF TO WORK IN THE EMBEDDED BROWSER (BUT IT'S OK IN IE & FIREFOX)
				builder.AppendLine(
					"<div class='t'><div class='b'><div class='l'><div class='r'><div class='bl'><div class='br'><div class='tl'><div class='tr'>");


					builder.AppendFormat("<span class='sender'>{0}</span> <span class='when'>sent at {1}</span>", message.Author, message.Date.ToShortTimeString());
					builder.AppendLine("<div>");
					builder.AppendLine(message.HtmlText);
					builder.AppendLine("</div>");
				//close off rounded borders
				builder.AppendLine("</div></div></div></div></div></div></div></div>");
				builder.AppendLine("</div>");

			}
			builder.AppendLine("</body>");
			builder.AppendLine("</html>");

			return builder.ToString();
		}
	}
}
