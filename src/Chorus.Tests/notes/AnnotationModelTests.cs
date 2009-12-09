using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus.annotations;
using Chorus.sync;
using Chorus.UI;
using Chorus.UI.Notes;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.notes
{
	[TestFixture]
	public class AnnotationModelTests
	{
		[Test]
		public void AddButtonClicked_NewMessageHasContents_NewMessageAppendedToAnnotation()
		{
			Annotation annotation = CreateAnnotation();
			var messageSelected = new MessageSelectedEvent();
			AnnotationEditorModel annotationModel = CreateAnnotationModel(messageSelected);
			messageSelected.Raise(annotation, annotation.Messages.First());
			Assert.IsFalse(annotationModel.IsClosed);
			Assert.IsFalse(annotation.IsClosed);
			Assert.AreEqual(1, annotation.Messages.Count());
			annotationModel.NewMessageText = "hello";
			annotationModel.AddButtonClicked();
			Assert.IsFalse(annotationModel.IsClosed,"should not have changed status");
			Assert.AreEqual(2,annotation.Messages.Count());
			Assert.AreEqual("bob", annotation.Messages.Last().GetAuthor(""));
			Assert.AreEqual("hello", annotation.Messages.Last().Text);
			Assert.IsTrue(DateTime.Now.Subtract(annotation.Messages.Last().Date).Seconds < 2);
		}

		[Test]
		public void CloseIssue_AnnotationGetsNewMessageWithNewStatus()
		{
			Annotation annotation = CreateAnnotation();
			var messageSelected = new MessageSelectedEvent();
			AnnotationEditorModel annotationModel = CreateAnnotationModel(messageSelected);
			messageSelected.Raise(annotation, annotation.Messages.First());
			Assert.IsFalse(annotationModel.IsClosed);
			Assert.IsFalse(annotation.IsClosed);
			annotationModel.IsClosed = true;
			Assert.IsTrue(annotationModel.IsClosed);
			Assert.IsTrue(annotation.IsClosed);
		}

		private AnnotationEditorModel CreateAnnotationModel(MessageSelectedEvent messageSelected)
		{
			return new AnnotationEditorModel(new ChorusUser("bob"), messageSelected, StyleSheet.CreateFromDisk(), new EmbeddedMessageContentHandlerFactory());
		}

		private Annotation CreateAnnotation()
		{
			string ann = @"<annotation  ref='lift://foo.lift?label=wantok' class='question'>
						<message guid='1234' author='joe' status='open' date='2009-09-28T11:11:11Z'>
							 What's up?
						</message>
						</annotation>";
			var element = XElement.Parse(ann);
			return new Annotation(element);
		}
	}


}
