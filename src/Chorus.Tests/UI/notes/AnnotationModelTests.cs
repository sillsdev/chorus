using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus.notes;
using Chorus.sync;
using Chorus.UI;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Html;
using Chorus.UI.Review;
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
			Assert.That(annotationModel.IsResolved, Is.False);
			Assert.That(annotation.IsClosed, Is.False);
			Assert.AreEqual(1, annotation.Messages.Count());
			annotationModel.AddMessage("hello");
			Assert.That(annotationModel.IsResolved, Is.False,"should not have changed status");
			Assert.AreEqual(2, annotation.Messages.Count());
			Assert.AreEqual("bob", annotation.Messages.Last().GetAuthor(""));
			Assert.AreEqual("hello", annotation.Messages.Last().Text);
			Assert.That(DateTime.Now.Subtract(annotation.Messages.Last().Date).Seconds, Is.LessThan(2));
		}

		[Test]
		public void CloseIssue_AnnotationGetsNewMessageWithNewStatus()
		{
			Annotation annotation = CreateAnnotation();
			var messageSelected = new MessageSelectedEvent();
			AnnotationEditorModel annotationModel = CreateAnnotationModel(messageSelected);
			messageSelected.Raise(annotation, annotation.Messages.First());
			Assert.That(annotationModel.IsResolved, Is.False);
			Assert.That(annotation.IsClosed, Is.False);
			annotationModel.IsResolved = true;
			Assert.That(annotationModel.IsResolved, Is.True);
			Assert.That(annotation.IsClosed, Is.True);
		}

		[Test]
		public void ResolveButtonClicked_NewMessageHasContents_ResolutionAndMessageAreOne()
		{
			Annotation annotation = CreateAnnotation();
			var messageSelected = new MessageSelectedEvent();
			AnnotationEditorModel annotationModel = CreateAnnotationModel(messageSelected);
			messageSelected.Raise(annotation, annotation.Messages.First());
			Assert.That(annotationModel.IsResolved, Is.False);
			Assert.That(annotation.IsClosed, Is.False);
			Assert.AreEqual(1, annotation.Messages.Count());
			annotationModel.UnResolveAndAddMessage("hello");
			Assert.That(annotationModel.IsResolved, Is.True, "should have changed status");
			Assert.AreEqual(2, annotation.Messages.Count());
			Assert.AreEqual("bob", annotation.Messages.Last().GetAuthor(""));
			Assert.AreEqual("hello", annotation.Messages.Last().Text);
			Assert.That(DateTime.Now.Subtract(annotation.Messages.Last().Date).Seconds, Is.LessThan(2));
		}

		private AnnotationEditorModel CreateAnnotationModel(MessageSelectedEvent messageSelected)
		{
			return new AnnotationEditorModel(new ChorusUser("bob"), messageSelected, StyleSheet.CreateFromDisk(), new EmbeddedMessageContentHandlerRepository(), new NavigateToRecordEvent(),
				new ChorusNotesDisplaySettings());
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
