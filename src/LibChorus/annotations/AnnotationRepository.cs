using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace Chorus.annotations
{
	public class AnnotationRepository : IDisposable
	{
		private XDocument _doc;
		private readonly string _annotationFilePath;
		private static int kCurrentVersion=0;
		public static string FileExtension = "ChorusNotes";


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
		}

		public void Dispose()
		{

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

		public Annotation AddAnnotation(string annotationClss, string refUrl)
		{
			var annotation = new Annotation(annotationClss, refUrl, _annotationFilePath);
			_doc.Root.Add(annotation.Element);
			return annotation;
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