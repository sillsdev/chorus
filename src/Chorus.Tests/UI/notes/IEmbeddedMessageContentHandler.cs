using System;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus.notes;

namespace Chorus.Tests.notes
{
	public interface IEmbeddedMessageContentHandler
	{
		bool CanHandleContent(string contentXml);
		Control CreateWinFormsControl(string contentXml, Annotation parentAnnotation, ChorusUser user);
	}

	public class EmbeddedMessageContentTest : IEmbeddedMessageContentHandler
	{
		public static string SampleXml
		{
			get
			{
				return "<embeddedContentTest>hello</embeddedContentTest>";
			}
		}

		public bool CanHandleContent(string contentXml)
		{
			return contentXml.Contains("embeddedContentTest");
		}

		public Control CreateWinFormsControl(string contentXml, Annotation parentAnnotation, ChorusUser user)
		{
			var element = XElement.Parse(contentXml);
			var link = new LinkLabel();
			link.Tag = new object[] { parentAnnotation, user };
			link.Text = element.Value + ": when you click this, the annotation should close";
			link.Click += new EventHandler(OnLinkClicked);
			return link;
		}

		void OnLinkClicked(object sender, EventArgs e)
		{
			Annotation a = ((object[])((LinkLabel)sender).Tag)[0] as Annotation;
			ChorusUser user = ((object[])((LinkLabel)sender).Tag)[1] as ChorusUser;
			a.SetStatusToClosed(user.Name);
		}
	}
}