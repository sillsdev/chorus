using System;
using System.Linq;
using System.Xml.Linq;
using Chorus.notes;
using Chorus.Review;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Html;
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
			Assert.IsFalse(annotationModel.IsResolved);
			Assert.IsFalse(annotation.IsClosed);
			Assert.AreEqual(1, annotation.Messages.Count());
			annotationModel.AddMessage("hello");
			Assert.IsFalse(annotationModel.IsResolved,"should not have changed status");
			Assert.AreEqual(2, annotation.Messages.Count());
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
			Assert.IsFalse(annotationModel.IsResolved);
			Assert.IsFalse(annotation.IsClosed);
			annotationModel.IsResolved = true;
			Assert.IsTrue(annotationModel.IsResolved);
			Assert.IsTrue(annotation.IsClosed);
		}

		[Test]
		public void ResolveButtonClicked_NewMessageHasContents_ResolutionAndMessageAreOne()
		{
			Annotation annotation = CreateAnnotation();
			var messageSelected = new MessageSelectedEvent();
			AnnotationEditorModel annotationModel = CreateAnnotationModel(messageSelected);
			messageSelected.Raise(annotation, annotation.Messages.First());
			Assert.IsFalse(annotationModel.IsResolved);
			Assert.IsFalse(annotation.IsClosed);
			Assert.AreEqual(1, annotation.Messages.Count());
			annotationModel.UnResolveAndAddMessage("hello");
			Assert.IsTrue(annotationModel.IsResolved, "should have changed status");
			Assert.AreEqual(2, annotation.Messages.Count());
			Assert.AreEqual("bob", annotation.Messages.Last().GetAuthor(""));
			Assert.AreEqual("hello", annotation.Messages.Last().Text);
			Assert.IsTrue(DateTime.Now.Subtract(annotation.Messages.Last().Date).Seconds < 2);
		}

		private AnnotationEditorModel CreateAnnotationModel(MessageSelectedEvent messageSelected)
		{
			return new AnnotationEditorModel(new ChorusUser("bob"), messageSelected,
				StyleSheet.CreateFromDisk(), new EmbeddedMessageContentHandlerRepository(),
				new NavigateToRecordEvent(),
				new ChorusNotesSettings());
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
