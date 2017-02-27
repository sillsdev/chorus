using System;
using System.ComponentModel.Composition;

namespace Chorus.notes
{
	public interface IEmbeddedMessageContentHandler
	{
		bool CanHandleContent(string cDataContent);
	   // Control CreateWinFormsControl(string contentXml, Annotation parentAnnotation, ChorusUser user);
		string GetHyperLink(string cDataContent);
		bool CanHandleUrl(Uri uri);
		void HandleUrl(Uri uri, string annotationFilePath);
	}

	[Export(typeof(IEmbeddedMessageContentHandler))]
	public class NullEmbeddedContentLinkMaker : IEmbeddedMessageContentHandler
	{
		public string GetHyperLink(string cDataContent)
		{
			return string.Empty;
		}

		public bool CanHandleUrl(Uri uri)
		{
			return false;
		}

		public void HandleUrl(Uri uri, string annotationFilePath)
		{
			throw new NotImplementedException();
		}

		public bool CanHandleContent(string cDataContent)
		{
			return false;
		}
	}

	[Export(typeof(IEmbeddedMessageContentHandler))]
	[PartMetadata("Default", true)]
	public class DefaultEmbeddedMessageContentHandler : IEmbeddedMessageContentHandler
	{
		public virtual string GetHyperLink(string cDataContent)
		{
			return string.Format("<a href={0}>{1}</a>", "test", "Details");
		}

		public bool CanHandleUrl(Uri uri)
		{
			return true;
		}

		public void HandleUrl(Uri uri, string annotationFilePath)
		{

		}

		public bool CanHandleContent(string cDataContent)
		{
			return true;
		}
	}

//    public class DummyEmbeddedMessageContentHandler : IEmbeddedMessageContentHandler
//    {
//        public static string SampleXml
//        {
//            get
//            {
//                return "<dummy>hello</dummy>";
//            }
//        }
//
//        public bool CanHandleContent(string contentXml)
//        {
//            return contentXml.Contains("dummy");
//        }
//
//        public Control CreateWinFormsControl(string contentXml, Annotation parentAnnotation, ChorusUser user)
//        {
//            var element = XElement.Parse(contentXml);
//            var link = new LinkLabel();
//            link.Tag = new object[] { parentAnnotation, user };
//            link.Text = element.Value + ": when you click this, the annotation should close";
//            link.Click += new EventHandler(OnLinkClicked);
//            return link;
//        }
//
//        void OnLinkClicked(object sender, EventArgs e)
//        {
//            Annotation a = ((object[])((LinkLabel)sender).Tag)[0] as Annotation;
//            ChorusUser user = ((object[])((LinkLabel)sender).Tag)[1] as ChorusUser;
//            a.SetStatusToClosed(user.Name);
//        }
//    }
//
//    public class DefaultEmbeddedMessageContentHandler : IEmbeddedMessageContentHandler
//    {
//        public bool CanHandleContent(string contentXml)
//        {
//            return true;
//        }
//
//        public Control CreateWinFormsControl(string contentXml, Annotation parentAnnotation, ChorusUser user)
//        {
//            var box = new TextBox();
//            box.Multiline = true;
//            box.Text = "The message has embedded content which the current system does not know how to display:\r\n" +
//                       contentXml;
//            return box;
//        }
//    }
}