// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using NUnit.Framework;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace LibChorus.Tests.FileHandlers.xml
{
	[TestFixture]
	public class XmlDeletionChangeReportTests
	{
		[Test]
		public void XmlDeletionChangeReport_ReportsCorrectChangeWithoutCrashing()
		{
			// Setup
			var merger = new XmlMerger(new NullMergeSituation());

			// Exercise
			var result = merger.Merge("<r></r>", "<r><s><t>hello</t></s></r>", "<r><s><t>hello</t></s></r>");

			// Verify
			Assert.That(result.Changes.Select(x => x.GetType()), Is.EqualTo(new[] { typeof(XmlDeletionChangeReport) }));
			Assert.That(result.Changes[0].ToString(), Is.EqualTo("Deleted a <s>"));
			Assert.That(result.Changes[0].ActionLabel, Is.EqualTo("Deleted"));
		}

		[Test]
		public void XmlBothDeletionChangeReport_ReportsCorrectChangeWithoutCrashing()
		{
			// Setup
			var merger = new XmlMerger(new NullMergeSituation());

			// Exercise
			var result = merger.Merge("<r></r>", "<r></r>", "<r><s><t>hello</t></s></r>");

			// Verify
			Assert.That(result.Changes.Select(x => x.GetType()), Is.EqualTo(new[] { typeof(XmlBothDeletionChangeReport) }));
			Assert.That(result.Changes[0].ToString(), Is.EqualTo("Both deleted the <s>"));
			Assert.That(result.Changes[0].ActionLabel, Is.EqualTo("Deleted"));
		}
	}
}

