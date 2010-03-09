using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml
{
	[TestFixture]
	public sealed class FindByEqualityOfTreeTests
	{
		//a regression test: this cause a failure for ron,
		//as the xmldiff was give a simple string (here, "hello") and couldn't
		//parse it.
		[Test]
		public void GetNodeToMerge_TextOnlyRegression_DoesntThrow()
		{
			var doc1 = new XmlDocument();
			doc1.LoadXml(@"<dummy/>");

			var doc2 = new XmlDocument();
			doc2.LoadXml(@"<textHolder>hello</textHolder>");

			var node = doc2.SelectSingleNode("//textHolder");
			var finder = new FindByEqualityOfTree();
			finder.GetNodeToMerge(doc1, node);
		}

	}

}
