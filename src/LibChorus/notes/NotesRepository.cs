using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using Chorus.merge.xml.generic;

namespace Chorus.notes
{
	public class NotesRepository : IDisposable
	{
		private XmlDocument _dom;
		private static int kCurrentVersion=0;

		public static NotesRepository FromFile(string path)
		{
			XmlDocument dom;
			dom = new XmlDocument();
			try
			{
				dom.Load(path);
			}
			catch (XmlException error)
			{
				throw new NotesFormatException(string.Empty,error);
			}

			ThrowIfVersionTooHigh(dom, path);

			return new NotesRepository(dom);
		}

		private static void ThrowIfVersionTooHigh(XmlDocument dom, string path)
		{
			var version = dom.FirstChild.GetStringAttribute("version");
			if (Int32.Parse(version) > kCurrentVersion)
			{
				throw new NotesFormatException(
					"The notes file {0} is of a newer version ({1}) than this version of the program supports ({2}).",
					path, version, kCurrentVersion.ToString());
			}
		}

		public static NotesRepository FromString(string contents)
		{
			XmlDocument dom;
			dom = new XmlDocument();
			try
			{
				dom.LoadXml(contents);
			}
			catch (XmlException error)
			{
				throw new NotesFormatException(string.Empty,error);
			}
			ThrowIfVersionTooHigh(dom, "unknown");
			return new NotesRepository(dom);
		}

		public NotesRepository(XmlDocument dom)
		{
			_dom = dom;

		}

		public void Dispose()
		{

		}

		public IEnumerable<Annotation> GetAll()
		{
			yield break;
		}
	}

	public class Annotation
	{
	}

	public class NotesFormatException : ApplicationException
	{
		public NotesFormatException(string message, Exception exception)
			: base(message, exception)
		{
		}
		public NotesFormatException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}

	}
}
