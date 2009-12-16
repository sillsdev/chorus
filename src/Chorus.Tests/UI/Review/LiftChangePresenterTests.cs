using System.Diagnostics;
using System.Xml;
using Chorus.FileTypeHanders.lift;
using NUnit.Framework;

namespace Chorus.Tests.UI.Review
{
	[TestFixture]
	public class LiftChangePresenterTests
	{
		[Test]
		public void TESTNAME()
		{
			var r = GetHtml(@"
  <entry id='grisim_8bbee099-1a7e-44a2-99fc-fbbc6ad20894' dateCreated='2009-07-09T12:45:33Z' dateModified='2009-07-18T06:42:07Z' guid='8bbee099-1a7e-44a2-99fc-fbbc6ad20894'>
	<lexical-unit>
	  <form lang='chorus'>
		<text>grisim</text>
	  </form>
	</lexical-unit>
	<sense id='1c342088-547b-4632-b2cc-38a01f2a0c10'>
	  <grammatical-info value='verb' />
	  <definition>
		<form lang='en'>
		  <text>to talk in a convincing way, mollifyA24</text>
		</form>
	  </definition>
	  <example>
		<form lang='chorus'>
		  <text>Em i no bai hamamas, tasol, i olrait, mi bai grisim em.</text>
		</form>
		<translation>
		  <form lang='en'>
			<text>He won't be happy, but it's all right, I'll talk him into it.</text>
		  </form>
		</translation>
	  </example>
	  <example>
		<form lang='chorus'>
		  <text>Bai em i no hamamas, tasol, i olrait, bai mi grisim em!</text>
		</form>
		<translation>
		  <form lang='en'>
			<text>He won't be happy, but it's all right, I'll talk him into it!</text>
		  </form>
		</translation>
	  </example>
	</sense>
  </entry>
");
			Debug.WriteLine(r);
		}

		private string GetHtml(string entryXml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(entryXml);
			return LiftChangePresenter.GetHtmlForEntry(doc.FirstChild);
		}

	}
}