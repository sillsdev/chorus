using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Chorus.Utilities;

namespace Chorus.annotations
{
	public class AnnotationRepository : IDisposable
	{
		private XDocument _doc;
		private readonly string _annotationFilePath;
		private static int kCurrentVersion=0;
		public static string FileExtension = "ChorusNotes";
		private List<AnnotationIndex> _indices = new List<AnnotationIndex>();

		public static AnnotationRepository FromFile(string path)
		{
			try
			{
				var doc = XDocument.Load(path);
				ThrowIfVersionTooHigh(doc, path);
				return new AnnotationRepository(doc, path);
			}
			catch (XmlException error)
			{
				throw new AnnotationFormatException(string.Empty, error);
			}
		}


		public static AnnotationRepository FromString(string contents)
		{
			try
			{
				XDocument doc = XDocument.Parse(contents);
				ThrowIfVersionTooHigh(doc, "unknown");
				return new AnnotationRepository(doc, string.Empty);
			}
			catch (XmlException error)
			{
				throw new AnnotationFormatException(string.Empty,error);
			}
		}

		public AnnotationRepository(XDocument doc, string path)
		{
			_doc = doc;
			_annotationFilePath = path;


			foreach (var element in _doc.Root.Elements())
			{
				//nb: this is not going to catch a change to, say a message. The current model
				// is that the internals of a message never change.
				element.Changed+=new EventHandler<XObjectChangeEventArgs>(AnnotationElement_Changed);
			}
		}

		public void Dispose()
		{

		}

		public void AddIndex(AnnotationIndex index, IProgress progress)
		{
			if(_indices.Exists(i => i.Name == index.Name))
			{
				//harsh, but better to fail fast.
				throw new ApplicationException("And index with the name "+index.Name +" is already in the repository.");
			}
			_indices.Add(index);
			index.Initialize(this, progress);
		}


		public IEnumerable<Annotation> GetAllAnnotations()
		{
			return from a in _doc.Root.Elements() select new Annotation(a);
		}

		public IEnumerable<Annotation> GetByCurrentStatus(string status)
		{
			return from a in _doc.Root.Elements()
				   where Annotation.GetStatusOfLastMessage(a) == status
				   select new Annotation(a);
		}

		public void SaveAs(string path)
		{
			_doc.Save(path);
		}

		public void AddAnnotation(Annotation annotation)
		{
			_doc.Root.Add(annotation.Element);
			_indices.ForEach(index => index.NotifyOfAddition(annotation));
			annotation.Element.Changed += new EventHandler<XObjectChangeEventArgs>(AnnotationElement_Changed);
		}

		void AnnotationElement_Changed(object sender, XObjectChangeEventArgs e)
		{
			var element = sender as XElement;
			XElement annotationElement  =element.AncestorsAndSelf("annotation").First();
			_indices.ForEach(index => index.NotifyOfModification(new Annotation(annotationElement)));
		}

		private static void ThrowIfVersionTooHigh(XDocument doc, string path)
		{
			var version = doc.Element("notes").Attribute("version").Value;
			if (Int32.Parse(version) > kCurrentVersion)
			{
				throw new AnnotationFormatException(
					"The notes file {0} is of a newer version ({1}) than this version of the program supports ({2}).",
					path, version, kCurrentVersion.ToString());
			}
		}

		public IEnumerable<Annotation> GetMatches(Func<Annotation, string, bool> predicate, string parameter)
		{
			return from a in _doc.Root.Elements()
				   where predicate(new Annotation(a), parameter)    //enhance... very ineffienct making these constantly
				   select new Annotation(a);
		}


	}

	public class AnnotationFormatException : ApplicationException
	{
		public AnnotationFormatException(string message, Exception exception)
			: base(message, exception)
		{
		}
		public AnnotationFormatException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}

	}
}