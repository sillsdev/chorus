using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.merge.xml.generic;

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

	public class NullEmbeddedContentLinkMaker : IEmbeddedMessageContentHandler
	{
		public string GetHyperLink(string cDataContent)
		{
			return string.Empty;
		}

		public bool CanHandleUrl(Uri uri)
		{
			throw new NotImplementedException();
		}

		public void HandleUrl(Uri uri, string annotationFilePath)
		{
			throw new NotImplementedException();
		}

		public bool CanHandleContent(string cDataContent)
		{
			throw new NotImplementedException();
		}
	}

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

	/// <summary>
	/// This class is a repository for finding a handler which can display additional information about a message
	/// based on some interpretation of a CDATA section in the message.
	/// The only current implementation (MergeConflictEmbeddedMessageContentHandler) uses WinForms and was therefore moved to Chorus,
	/// since it is a current goal that LibChorus should not reference WinForms.
	/// MergeConflictEmbeddedMessageContentHandler understands the CDATA that is embedded in MergeConflict notes,
	/// and creates a link that offers more details of the conflict.
	/// Configuring this repository to know about an implementation in an assembly which it does not reference is a problem.
	/// It may eventually done using MEF or some more dynamic way of finding available Embedded Messsage Content Handlers.
	/// Currently, it knows about DefaultEmbeddedMessageContentHandler, and that this should come last.
	/// It also finds any implementations in Chorus.exe, if that is found in the same directory.
	///
	/// Note that the handlers are tried in order until we find one which CanHandleUrl for a particular URL. Thus the order
	/// in which they are stored in _knownHandlers is potentially important. So far, all we need to know is that the Chorus
	/// one comes before the default one, which trivially handles anything. If at some point we have multiple ones in Chorus
	/// (or elsewhere) which could handle the same URLs, we will need to add an ordering mechanism...either by how we configure
	/// MEF, or perhaps by adding a "priority" key to IEmbeddedMessageContentHandler.
	/// </summary>
	public class EmbeddedMessageContentHandlerRepository
	{
		public EmbeddedMessageContentHandlerRepository()
		{
			_knownHandlers = new List<IEmbeddedMessageContentHandler>();
			var libChorusAssembly = Assembly.GetExecutingAssembly();
#if MONO
			var codeBase = libChorusAssembly.CodeBase.Substring(7);
#else
			var codeBase = libChorusAssembly.CodeBase.Substring(8);
#endif
			var dirname = Path.GetDirectoryName(codeBase);
			//var baseDir = new Uri(dirname).AbsolutePath; // NB: The Uri class in Windows and Mono are not the same.

			// We (sadly) know that the other handler is in Chorus.exe
			var chorusDirName = Path.Combine(dirname, @"Chorus.exe");
			if (File.Exists(chorusDirName))
			{
				var chorusAssembly = Assembly.LoadFrom(chorusDirName);
				var messageHandlerTypes =
					chorusAssembly.GetTypes().Where(typeof (IEmbeddedMessageContentHandler).IsAssignableFrom);
				foreach (var type in messageHandlerTypes)
					_knownHandlers.Add((IEmbeddedMessageContentHandler)Activator.CreateInstance(type));
			}
			// this one must always be last. It provides a fall-back implementation.
			_knownHandlers.Add(new DefaultEmbeddedMessageContentHandler());
		}

		readonly List<IEmbeddedMessageContentHandler> _knownHandlers;

		public IEmbeddedMessageContentHandler GetHandlerOrDefaultForCData(string cDataContent)
		{
			return _knownHandlers.FirstOrDefault(h => h.CanHandleContent(cDataContent));
		}

		public IEmbeddedMessageContentHandler GetHandlerOrDefaultForUrl(Uri uri)
		{
			return _knownHandlers.FirstOrDefault(h => h.CanHandleUrl(uri));
		}

		public IEnumerable<IEmbeddedMessageContentHandler> KnownHandlers
		{
			get { return _knownHandlers; }
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