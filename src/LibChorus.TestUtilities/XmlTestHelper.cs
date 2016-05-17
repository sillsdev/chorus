using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.TestUtilities
{
	public class XmlTestHelper
	{
		public static void AssertXPathMatchesExactlyOne(string xml, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			AssertXPathMatchesExactlyOneInner(doc, xpath);
		}
		public static void AssertXPathMatchesExactlyOne(XmlNode node, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(node.OuterXml);
			AssertXPathMatchesExactlyOneInner(doc, xpath);
		}

		private static void AssertXPathMatchesExactlyOneInner(XmlDocument doc, string xpath)
		{
			XmlNodeList nodes = doc.SelectNodes(xpath);
			if (nodes == null || nodes.Count != 1)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
				if (nodes != null && nodes.Count > 1)
				{
					Assert.Fail("Too Many matches for XPath: {0}", xpath);
				}
				else
				{
					Assert.Fail("No Match: XPath failed: {0}", xpath);
				}
			}
		}

		public static void CreateThreeNodes(string ourXml, string theirXml, string ancestorXml, out XmlNode ourNode, out XmlNode ourParent, out XmlNode theirNode, out XmlNode ancestorNode)
		{
			var doc = new XmlDocument();
			ourParent = XmlUtilities.GetDocumentNodeFromRawXml("<ParentNode>" + ourXml + "</ParentNode>", doc);
			ourNode = ourParent.FirstChild;
			theirNode = XmlUtilities.GetDocumentNodeFromRawXml("<ParentNode>" + theirXml + "</ParentNode>", doc).FirstChild;
			ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml("<ParentNode>" + ancestorXml + "</ParentNode>", doc).FirstChild;
		}

		public static void AssertXPathNotNull(string documentPath, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node == null)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNotNull(node);
		}

		public static void AssertXPathIsNull(string xml, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node != null)
			{
				XmlWriterSettings settings = new XmlWriterSettings
												{
													Indent = true,
													ConformanceLevel = ConformanceLevel.Fragment
												};
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNull(node);
		}

		public static void AssertXPathMatchesExactlyOne(string xml, string xpath, Dictionary<string, string> namespaces)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			var namespaceManager = new XmlNamespaceManager(doc.NameTable);
			foreach (var namespaceKvp in namespaces)
				namespaceManager.AddNamespace(namespaceKvp.Key, namespaceKvp.Value);

			var nodes = doc.SelectNodes(xpath, namespaceManager);
			if (nodes != null && nodes.Count == 1)
				return;

			var settings = new XmlWriterSettings { Indent = true, ConformanceLevel = ConformanceLevel.Fragment };
			var writer = XmlTextWriter.Create(Console.Out, settings);
			doc.WriteContentTo(writer);
			writer.Flush();
			if (nodes != null && nodes.Count > 1)
			{
				Assert.Fail("Too Many matches for XPath: {0}", xpath);
			}
			else
			{
				Assert.Fail("No Match: XPath failed: {0}", xpath);
			}
		}

		public static void AssertXPathIsNull(string xml, string xpath, Dictionary<string, string> namespaces)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			var namespaceManager = new XmlNamespaceManager(doc.NameTable);
			foreach (var namespaceKvp in namespaces)
				namespaceManager.AddNamespace(namespaceKvp.Key, namespaceKvp.Value);

			var node = doc.SelectSingleNode(xpath, namespaceManager);
			if (node != null)
			{
				var settings = new XmlWriterSettings
				{
					Indent = true,
					ConformanceLevel = ConformanceLevel.Fragment
				};
				var writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNull(node);
		}

		public static string DoMerge(
			MergeStrategies mergeStrategies,
			MergeSituation mergeSituation,
			string ancestorXml, string ourXml, string theirXml,
			IEnumerable<string> xpathQueriesThatMatchExactlyOneNode, IEnumerable<string> xpathQueriesThatReturnNull,
			int expectedConflictCount, List<Type> expectedConflictTypes,
			int expectedChangesCount, List<Type> expectedChangeTypes)
		{
			var doc = new XmlDocument();
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourXml, doc);
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirXml, doc);
			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(ancestorXml, doc);

			var eventListener = new ListenerForUnitTests();
			var merger = new XmlMerger(mergeSituation)
							{
								MergeStrategies = mergeStrategies
							};
			var retval = merger.Merge(eventListener, ourNode.ParentNode, ourNode, theirNode, ancestorNode).OuterXml;
			Assert.AreSame(eventListener, merger.EventListener); // Make sure it never changes it, while we aren't looking, since at least one Merge method does that very thing.

			CheckMergeResults(retval, eventListener,
				xpathQueriesThatMatchExactlyOneNode, xpathQueriesThatReturnNull,
				expectedConflictCount, expectedConflictTypes,
				expectedChangesCount, expectedChangeTypes);

			return retval;
		}

		public static void CheckMergeResults(string mergedResults, ListenerForUnitTests eventListener,
			IEnumerable<string> xpathQueriesThatMatchExactlyOneNode, IEnumerable<string> xpathQueriesThatReturnNull,
			int expectedConflictCount, List<Type> expectedConflictTypes,
			int expectedChangesCount, List<Type> expectedChangeTypes)
		{
			if (xpathQueriesThatMatchExactlyOneNode != null)
			{
				foreach (var query in xpathQueriesThatMatchExactlyOneNode)
					AssertXPathMatchesExactlyOne(mergedResults, query);
			}

			if (xpathQueriesThatReturnNull != null)
			{
				foreach (var query in xpathQueriesThatReturnNull)
					AssertXPathIsNull(mergedResults, query);
			}

			eventListener.AssertExpectedConflictCount(expectedConflictCount);
			expectedConflictTypes = expectedConflictTypes ?? new List<Type>();
			Assert.AreEqual(expectedConflictTypes.Count, eventListener.Conflicts.Count,
							"Expected conflict count and actual number found differ.");
			for (var idx = 0; idx < expectedConflictTypes.Count; ++idx)
				Assert.AreSame(expectedConflictTypes[idx], eventListener.Conflicts[idx].GetType());

			eventListener.AssertExpectedChangesCount(expectedChangesCount);
			expectedChangeTypes = expectedChangeTypes ?? new List<Type>();
			Assert.AreEqual(expectedChangeTypes.Count, eventListener.Changes.Count,
							"Expected change count and actual number found differ.");
			for (var idx = 0; idx < expectedChangeTypes.Count; ++idx)
				Assert.AreSame(expectedChangeTypes[idx], eventListener.Changes[idx].GetType());
		}

		public static string WriteConflictAnnotation(IConflict conflict)
		{
			var stringBuilder = new StringBuilder();

			using (var stringWriter = new StringWriter(stringBuilder))
			using (var textWriter = new XmlTextWriter(stringWriter))
			{
				conflict.WriteAsChorusNotesAnnotation(textWriter);
			}
			return stringBuilder.ToString();
		}

		public static string DoMerge(
			MergeStrategies mergeStrategies,
			string ancestorXml, string ourXml, string theirXml,
			IEnumerable<string> xpathQueriesThatMatchExactlyOneNode, IEnumerable<string> xpathQueriesThatReturnNull,
			int expectedConflictCount, List<Type> expectedConflictTypes,
			int expectedChangesCount, List<Type> expectedChangeTypes)
		{
			return DoMerge(
				mergeStrategies,
				new NullMergeSituation(),
				ancestorXml, ourXml, theirXml,
				xpathQueriesThatMatchExactlyOneNode, xpathQueriesThatReturnNull,
				expectedConflictCount, expectedConflictTypes,
				expectedChangesCount, expectedChangeTypes);
		}
	}
}