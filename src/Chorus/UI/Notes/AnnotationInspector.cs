using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Chorus.notes;

namespace Chorus.UI.Notes
{
	public partial class AnnotationInspector : Form
	{
		private Annotation _annotation;

		public AnnotationInspector(Annotation annotation)
		{
			InitializeComponent();
			_annotation = annotation;
		}

		private void AnnotationInspector_Load(object sender, EventArgs e)
		{
			this._pathLabel.Text = _annotation.AnnotationFilePath;
			string htmlText = Render(_annotation.Element);
#if MONO
			webBrowser1.LoadHtml(htmlText);
#else
			webBrowser1.DocumentText = htmlText;
#endif
		}

		internal static string Render(XElement element)
		{
			try
			{
				var xslCompiledTransform = new XslCompiledTransform(true);
				System.IO.StringReader stringReader = new System.IO.StringReader(Properties.Resources.XmlToHtml10Basic);
				XmlReader xmlReader = XmlReader.Create(stringReader);
				XsltSettings xsltSettings = new XsltSettings(true, true);
				xslCompiledTransform.Load(xmlReader, xsltSettings, new XmlUrlResolver());

				XsltArgumentList a = new XsltArgumentList();
				// Need to pass the xml string as an input parameter so
				// we can do some parsing for extra bits that XSLT won't do.
				a.AddParam("xmlinput", string.Empty, element.ToString());
				var stringBuilder = new StringBuilder();
				XmlWriter xmlWriter = XmlWriter.Create(stringBuilder); // Target not a chorus file so Palaso.Xml.CanonicalXmlSettings not needed here.
				xslCompiledTransform.Transform(element.CreateReader(), a, xmlWriter);
				return stringBuilder.ToString();
			}
			catch (Exception e)
			{
				return e.Message;
			}

		}

	}
}
