using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.merge;
using LibChorus.Tests.merge.xml;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	internal static class FieldWorksTestServices
	{
		internal static string DoMerge(
			IChorusFileTypeHandler chorusFileHandler,
			string commonAncestor, string ourContent, string theirContent,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, List<Type> conflictTypes,
			int expectedChangesCount, List<Type> changeTypes)
		{
			string result;
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(commonAncestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);
				var eventListener = new ListenerForUnitTests();
				mergeOrder.EventListener = eventListener;

				chorusFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
				if (matchesExactlyOne != null)
				{
					foreach (var query in matchesExactlyOne)
						XmlTestHelper.AssertXPathMatchesExactlyOne(result, query);
				}
				if (isNull != null)
				{
					foreach (var query in isNull)
						XmlTestHelper.AssertXPathIsNull(result, query);
				}
				eventListener.AssertExpectedConflictCount(expectedConflictCount);
				Assert.AreEqual(conflictTypes.Count, eventListener.Conflicts.Count);
				for (var idx = 0; idx < conflictTypes.Count; ++idx)
					Assert.AreSame(conflictTypes[idx], eventListener.Conflicts[idx].GetType());
				eventListener.AssertExpectedChangesCount(expectedChangesCount);
				Assert.AreEqual(changeTypes.Count, eventListener.Changes.Count);
				for (var idx = 0; idx < changeTypes.Count; ++idx)
					Assert.AreSame(changeTypes[idx], eventListener.Changes[idx].GetType());
			}
			return result;
		}

		internal static XmlNode CreateNodes(string commonAncestor, string ourContent, string theirContent, out XmlNode theirNode, out XmlNode ancestorNode)
		{
			var ancestorDoc = new XmlDocument();
			ancestorDoc.LoadXml(commonAncestor);
			ancestorNode = ancestorDoc.DocumentElement.FirstChild;

			var ourDoc = new XmlDocument();
			ourDoc.LoadXml(ourContent);
			var ourNode = ourDoc.DocumentElement.FirstChild;

			var theirDoc = new XmlDocument();
			theirDoc.LoadXml(theirContent);
			theirNode = theirDoc.DocumentElement.FirstChild;
			return ourNode;
		}
	}
}