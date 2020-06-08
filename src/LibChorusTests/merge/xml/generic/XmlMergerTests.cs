using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Various notes on merging. Note that because of the way CheckOneWay is implemented, for all tests using it,
	/// elements a, b, c, and d are special: they expect the attribute 'key' to identify matching elements
	/// in other branches.
	/// </summary>
	[TestFixture]
	public class XmlMergerTests
	{
		[Test]
		public void OneAddedNewChildElement()
		{
			string red = @"<a/>";
			string ancestor = red;

			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";

			CheckBothWaysNoConflicts(red, blue, ancestor, "a/b[@key='one']/c[text()='first']");
		}

		[Test]
		public void PreMergeCalledBeforeMerging()
		{
			string red = @"<a/>";
			string ancestor = red;
			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";
			var specialMergeStrategies = new Dictionary<string, ElementStrategy>();
			var elementStrat = new ElementStrategy(true)
				{
					Premerger = new SillyPremerger()
				};
			specialMergeStrategies["a"] = elementStrat;
			CheckOneWay(red, blue, ancestor, new NullMergeSituation(), specialMergeStrategies, (Action<string, ElementStrategy>)null,
				"a[@silly='nonsense']/b[@key='one']/c[text()='first']");
		}

		private class SillyPremerger : IPremerger
		{
			public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
			{
				((XmlElement)ours).SetAttribute("silly", "nonsense");
				((XmlElement)theirs).SetAttribute("silly", "nonsense");
				((XmlElement)ancestor).SetAttribute("silly", "nonsense");
			}
		}

		private void CheckBothWaysNoConflicts(string red, string blue, string ancestor, params string[] xpaths)
		{
			ChangeAndConflictAccumulator r = CheckOneWay(red, blue, ancestor, xpaths);
			AssertNoConflicts(r);

			r = CheckOneWay(blue, red, ancestor, xpaths);
			AssertNoConflicts(r);
		}

		private static void AssertNoConflicts(ChangeAndConflictAccumulator r)
		{
			if (r.Conflicts.Count > 0)
			{
				foreach (IConflict conflict in r.Conflicts)
				{
					Console.WriteLine("*Unexpected: "+ conflict.GetFullHumanReadableDescription());
				}
			}
			Assert.AreEqual(0, r.Conflicts.Count, "There were unexpected conflicts.");
		}

		private ChangeAndConflictAccumulator CheckOneWay(string ours, string theirs, string ancestor, params string[] xpaths)
		{
			return CheckOneWay(ours, theirs, ancestor, new NullMergeSituation(), null, xpaths);
		}

		private ChangeAndConflictAccumulator CheckOneWay(string ours, string theirs, string ancestor,
			MergeSituation situation,
			Dictionary<string, ElementStrategy> specialMergeStrategies,
			params string[] xpaths)
		{
			return CheckOneWay(ours, theirs, ancestor, situation, specialMergeStrategies, null, xpaths);
		}

		private ChangeAndConflictAccumulator CheckOneWay(string ours, string theirs, string ancestor, MergeSituation situation,
			Dictionary<string, ElementStrategy> specialMergeStrategies,
			Action<string, ElementStrategy> adjustStrategy,
			params string[] xpaths)
		{
			var m = new XmlMerger(situation);
			AddMergeStrategies(m);
			if (specialMergeStrategies != null)
			{
				foreach (var kvp in specialMergeStrategies)
					m.MergeStrategies.ElementStrategies[kvp.Key] = kvp.Value; // don't use add, some may replace standard ones.
			}
			if (adjustStrategy != null)
			{
				foreach (var kvp in m.MergeStrategies.ElementStrategies)
					adjustStrategy(kvp.Key, kvp.Value);
			}

			var result = m.Merge(ours, theirs, ancestor);
			foreach (string xpath in xpaths)
			{
				XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, xpath);
			}
			return result;
		}

		protected virtual void AddMergeStrategies(XmlMerger m)
		{
			m.MergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateForKeyedElement("key", true));
			m.MergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateForKeyedElement("key", true));
			m.MergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElement("key", true));
			m.MergeStrategies.ElementStrategies.Add("d", ElementStrategy.CreateForKeyedElement("key", true));
		}

		[Test]
		public void DefaultHtmlDetails_ContainsDiffsOfversions()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='two'>data</c>
								</b>
							</a>";
			string red = @"<a>
								<b key='one'>
									<c key='two'>change1</c>
								</b>
							</a>";

			string blue = @"<a>
								<b key='one'>
									<c key='two'>change2</c>
								</b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a/b[@key='one']/c[@key='two' and text()='change2']");
			Assert.AreEqual(typeof(XmlTextBothEditedTextConflict), r.Conflicts[0].GetType());
			// red wins
			var mergeSituation = new MergeSituation("somepath", "red", "some rev", "blue", "another rev",
				MergeOrder.ConflictHandlingModeChoices.WeWin);
			r = CheckOneWay(red, blue, ancestor, mergeSituation, null,
										"a/b[@key='one']/c[@key='two' and text()='change1']");
			Assert.AreEqual(typeof(XmlTextBothEditedTextConflict), r.Conflicts[0].GetType());

			var c = r.Conflicts[0];
			Assert.That(c.HtmlDetails.StartsWith("<head>"));
			Assert.That(c.HtmlDetails, Contains.Substring(c.GetFullHumanReadableDescription()));
			var ancestorHml = XmlUtilities.GetXmlForShowingInHtml("<c key='two'>data</c>");
			// For now decided that with diffs we don't need the ancestor
			//Assert.That(c.HtmlDetails, Contains.Substring(ancestorHml));
			Assert.That(c.HtmlDetails.EndsWith("</body>"));
			var oursHtml = XmlUtilities.GetXmlForShowingInHtml("<c key='two'>change1</c>");
			var m = new Rainbow.HtmlDiffEngine.Merger(ancestorHml, oursHtml);
			Assert.That(c.HtmlDetails, Contains.Substring(m.merge()));

			var theirsHtml = XmlUtilities.GetXmlForShowingInHtml("<c key='two'>change2</c>");
			m = new Rainbow.HtmlDiffEngine.Merger(ancestorHml, theirsHtml);
			Assert.That(c.HtmlDetails, Contains.Substring(m.merge()));

			Assert.That(c.HtmlDetails, Contains.Substring("kept the change made by red"));
			AssertDivsMatch(c.HtmlDetails);
		}

		[Test]
		public void DefaultHtmlDetails_ReportsOneDeleted()
		{
			string ancestor = @"<a key='one'>
								<b key='one'>
									<c key='two'>data</c>
								</b>
							</a>";
			string red = @"<a key='one'>
								<b key='one'>
									<c key='two'>change1</c>
								</b>
							</a>";

			string blue = @"<a key='one'>
							</a>";

			// blue would normally win, but this is a delete vs edit.
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[@key='one']/b[@key='one']/c[@key='two' and text()='change1']");
			Assert.AreEqual(typeof(RemovedVsEditedElementConflict), r.Conflicts[0].GetType());
			// red wins
			var mergeSituation = new MergeSituation("somepath", "red", "some rev", "blue", "another rev",
				MergeOrder.ConflictHandlingModeChoices.WeWin);
			r = CheckOneWay(red, blue, ancestor, mergeSituation, null,
										"a[@key='one']/b[@key='one']/c[@key='two' and text()='change1']");
			Assert.AreEqual(typeof(EditedVsRemovedElementConflict), r.Conflicts[0].GetType());

			var c = r.Conflicts[0];
			Assert.That(c.HtmlDetails.StartsWith("<head>"));
			Assert.That(c.HtmlDetails, Contains.Substring(c.GetFullHumanReadableDescription()));
			var ancestorHml = XmlUtilities.GetXmlForShowingInHtml("<c key='two'>data</c>");
			// For now decided that with diffs we don't need the ancestor
			//Assert.That(c.HtmlDetails, Contains.Substring(ancestorHml));
			Assert.That(c.HtmlDetails.EndsWith("</body>"));
			var oursHtml = XmlUtilities.GetXmlForShowingInHtml("<c key='two'>change1</c>");
			var m = new Rainbow.HtmlDiffEngine.Merger(ancestorHml, oursHtml);
			Assert.That(c.HtmlDetails, Contains.Substring(m.merge()));

			Assert.That(c.HtmlDetails, Contains.Substring("kept the change made by red"));
			AssertDivsMatch(c.HtmlDetails);
		}

		private void AssertDivsMatch(string input)
		{
			var reDiv = new Regex("<div[^>]*>");
			var reEndDiv = new Regex("</div>");
			Assert.That(reDiv.Matches(input).Count, Is.EqualTo(reEndDiv.Matches(input).Count), "<div>s should match </div>s");
		}

		[Test]
		public void ParentOrderIsRelevant_TrumpsChildOrderIsRelevant()
		{
			string ancestor = @"<a key='one'>
								<b key='one'>
									<c key='two'>data</c>
									<c key='three'>data</c>
								</b>
							</a>";
			string red = @"<a key='one'>
								<b key='one'>
									<c key='two'>data</c>
									<c key='four'>data</c>
									<c key='three'>data</c>
								</b>
							</a>";
			string blue = @"<a key='one'>
								<b key='one'>
									<c key='two'>data</c>
									<c key='five'>data</c>
									<c key='three'>data</c>
								</b>
							</a>";
			// With no override, order is significant for c, so we get a conflict.
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a/b[@key='one']/c[@key='four' and text()='data']", "a/b[@key='one']/c[@key='five' and text()='data']");
			var conflicts = r.Conflicts;
			Assert.That(conflicts[0], Is.InstanceOf<AmbiguousInsertConflict>());

			var strategyForB = ElementStrategy.CreateForKeyedElement("key", true);
			var childOrderNotRelevantStrategy = new MockChildOrderer() {Answer = ChildOrder.NotSignificant};
			strategyForB.ChildOrderPolicy = childOrderNotRelevantStrategy;
			var specialMergeStrategies = new Dictionary<string, ElementStrategy>();
			specialMergeStrategies["b"] = strategyForB;

			// Overriding so that order is not significant for children of B, we should not get an ambiguous insert conflict.
			r = CheckOneWay(blue, red, ancestor, new NullMergeSituation(), specialMergeStrategies,
										"a/b[@key='one']/c[@key='four' and text()='data']", "a/b[@key='one']/c[@key='five' and text()='data']");
			Assert.That(r.Conflicts.Count, Is.EqualTo(0), "order is significant should have been overridden by the parent policy");
			Assert.That(childOrderNotRelevantStrategy.Parent.Name, Is.EqualTo("b"));

			// Overriding so that order is not significant for C's should also prevent it.
			specialMergeStrategies.Remove("b");
			var strategyForC = ElementStrategy.CreateForKeyedElement("key", false);
			specialMergeStrategies["c"] = strategyForC;
			r = CheckOneWay(blue, red, ancestor, new NullMergeSituation(), specialMergeStrategies,
										"a/b[@key='one']/c[@key='four' and text()='data']", "a/b[@key='one']/c[@key='five' and text()='data']");
			Assert.That(r.Conflicts.Count, Is.EqualTo(0), "order is significant should have been overridden by the parent policy");
			Assert.That(childOrderNotRelevantStrategy.Parent.Name, Is.EqualTo("b"));

			// But we should get it back by overriding B the other way.
			childOrderNotRelevantStrategy.Answer = ChildOrder.Significant;
			specialMergeStrategies["b"] = strategyForB;
			r = CheckOneWay(blue, red, ancestor, new NullMergeSituation(), specialMergeStrategies,
										"a/b[@key='one']/c[@key='four' and text()='data']", "a/b[@key='one']/c[@key='five' and text()='data']");
			Assert.That(r.Conflicts.Count, Is.EqualTo(1), "order is not significant should have been overridden by the parent policy");
			Assert.That(childOrderNotRelevantStrategy.Parent.Name, Is.EqualTo("b"));
		}

		class MockChildOrderer : IChildOrderPolicy
		{
			public ChildOrder Answer;
			public XmlNode Parent;
			public ChildOrder OrderSignificance(XmlNode parent)
			{
				Parent = parent;
				return Answer;
			}
		}

		[Test]
		public void DefaultHtmlDetails_UsesClientHtmlGenerator()
		{
			string ancestor = @"<a key='one'>
								<b key='one'>
									<c key='two'>data</c>
								</b>
							</a>";
			string red = @"<a key='one'>
								<b key='one'>
									<c key='two'>change1</c>
								</b>
							</a>";

			string blue = @"<a key='one'>
								<b key='one'>
									<c key='two'>change2</c>
								</b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a/b[@key='one']/c[@key='two' and text()='change2']");
			Assert.AreEqual(typeof(XmlTextBothEditedTextConflict), r.Conflicts[0].GetType());
			// red wins
			var mergeSituation = new MergeSituation("somepath", "red", "some rev", "blue", "another rev",
				MergeOrder.ConflictHandlingModeChoices.WeWin);
			var specialStrategies = new Dictionary<string, ElementStrategy>();
			var strategy = ElementStrategy.CreateForKeyedElement("key", true);
			strategy.ContextDescriptorGenerator = new MockContextGenerator2();
			specialStrategies.Add("c", strategy);
			r = CheckOneWay(red, blue, ancestor, mergeSituation, specialStrategies,
										"a/b[@key='one']/c[@key='two' and text()='change1']");
			Assert.AreEqual(typeof(XmlTextBothEditedTextConflict), r.Conflicts[0].GetType());

			var c = r.Conflicts[0];
			Assert.That(c.HtmlDetails.StartsWith("<head><style type='text/css'>div.myStyle {margin-left:  0.2in}</style>"));
			Assert.That(c.HtmlDetails, Contains.Substring(c.GetFullHumanReadableDescription()));
			var ancestorHml = "<div class='test'>" + XmlUtilities.GetXmlForShowingInHtml("<c key='two'>data</c>") + "</div>";
			// For now decided that with diffs we don't need the ancestor
			//Assert.That(c.HtmlDetails, Contains.Substring(ancestorHml));
			Assert.That(c.HtmlDetails.EndsWith("</body>"));
			var oursHtml = "<div class='test'>" + XmlUtilities.GetXmlForShowingInHtml("<c key='two'>change1</c>") + "</div>";
			var m = new Rainbow.HtmlDiffEngine.Merger(ancestorHml, oursHtml);
			Assert.That(c.HtmlDetails, Contains.Substring(m.merge()));

			var theirsHtml = "<div class='test'>" + XmlUtilities.GetXmlForShowingInHtml("<c key='two'>change2</c>") + "</div>";
			m = new Rainbow.HtmlDiffEngine.Merger(ancestorHml, theirsHtml);
			Assert.That(c.HtmlDetails, Contains.Substring(m.merge()));

			Assert.That(c.HtmlDetails, Contains.Substring("kept the change made by red"));
			AssertDivsMatch(c.HtmlDetails);
		}

		class MockContextGenerator2 : IGenerateContextDescriptor, IGenerateHtmlContext
		{
			public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
			{
				return new ContextDescriptor("dummy context", filePath);
			}

			public string HtmlContext(XmlNode mergeElement)
			{
				return "<div class='test'>" + XmlUtilities.GetXmlForShowingInHtml(mergeElement.OuterXml) + "</div>";
			}

			public string HtmlContextStyles(XmlNode mergeElement)
			{
				return "div.myStyle {margin-left:  0.2in}";
			}
		}
		[Test]
		public void OneEditedDeepChildOfElementOtherDeleted()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='two'>
										<d key='three'>
											<e>data</e>
										</d>
									</c>
								</b>
							</a>";
			string red = @"<a>
								<b key='one'>
									<c key='two'>
										<d key='three'>
											<e>changed</e>
										</d>
									</c>
								</b>
							</a>";

			string blue = @"<a>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor, new NullMergeSituation(), null, AddContextGenForB,
										"a/b[@key='one']/c[@key='two']/d[@key='three']/e[text()='changed']");
			Assert.AreEqual(typeof(RemovedVsEditedElementConflict), r.Conflicts[0].GetType());
			CheckEditVsDeleteDetails(r.Conflicts[0].HtmlDetails);
			var context = r.Conflicts[0].Context;
			Assert.That(context, Is.Not.Null);
			// red wins
			r = CheckOneWay(red, blue, ancestor, new NullMergeSituation(), null, AddContextGenForB,
										"a/b[@key='one']/c[@key='two']/d[@key='three']/e[text()='changed']");
			Assert.AreEqual(typeof(EditedVsRemovedElementConflict), r.Conflicts[0].GetType());
			CheckEditVsDeleteDetails(r.Conflicts[0].HtmlDetails);
		}

		/// <summary>
		/// Put in a special context generator for 'b', the element that is deleted.
		/// This should be used to generate the HTML.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="strategy"></param>
		private void AddContextGenForB(string key, ElementStrategy strategy)
		{
			if (key != "b")
				return;
			strategy.ContextDescriptorGenerator = new EditDeleteContextGenerator();
		}

		class EditDeleteContextGenerator : IGenerateContextDescriptor, IGenerateHtmlContext
		{
			public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
			{
				return new ContextDescriptor("my label", "my url");
			}

			public string HtmlContext(XmlNode mergeElement)
			{
				return "my html";
			}

			public string HtmlContextStyles(XmlNode mergeElement)
			{
				return "";
			}
		}

		private static void CheckEditVsDeleteDetails(string html)
		{
			Assert.That(html, Does.Contain("changes:"));
			Assert.That(html, Does.Not.Contain("version"),
				"This appears if we try to do HTML diffs on the representation of <a></a>");
			Assert.That(html.IndexOf("changes", StringComparison.InvariantCulture),
				Is.EqualTo(html.LastIndexOf("changes", StringComparison.InvariantCulture)),
				"On EditVsDelete, we should display only one set of changes in details");
			Assert.That(html, Does.Contain("my html"), "Should have used the custom HTML generator we configured for B");
		}

		[Test]
		public void TextElement_OneAdded_NoConflicts()
		{
			CheckBothWaysNoConflicts("<r><t>hello</t></r>", "<r/>", "<r/>",
									 "r[count(t)=1]",
									 "r/t[text()='hello']");

			CheckBothWaysNoConflicts("<r><t>hello</t></r>", "<r><t/></r>", "<r/>",
									 "r[count(t)=1]",
									 "r/t[text()='hello']");

			CheckBothWaysNoConflicts("<r><t>hello</t></r>", "<r><t/></r>", "<r><t/></r>",
									 "r[count(t)=1]",
									 "r/t[text()='hello']");
		}

		[Test]
		public void TextElement_BothDeleted_NoConflicts()
		{
			CheckBothWaysNoConflicts("<r><t/></r>", "<r><t></t></r>", "<r><t>hello</t></r>",
									 "r/t[not(text())]",
									 "r[count(t)=1]");
		}

		[Test]
		public void TextElement_EachEdited_OursKept_ConflictRegistered()
		{
			string ancestor = @"<t>original</t>";
			string ours = @"<t>mine</t>";
			string theirs = @"<t>theirs</t>";

			XmlMerger m = new XmlMerger(new MergeSituation("pretendPath", "userX", "XRev","userY", "YRev", MergeOrder.ConflictHandlingModeChoices.WeWin));
			var result = m.Merge(ours, theirs, ancestor);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, "t[text()='mine']");
			Assert.AreEqual("pretendPath", result.Conflicts[0].RelativeFilePath);

			Assert.AreEqual(typeof(XmlTextBothEditedTextConflict), result.Conflicts[0].GetType());
		}

		[Test]
		public void TextElement_WeEditedTheyDeleted_OursKept_ConflictRegistered()
		{
			string ancestor = @"<t>original</t>";
			string ours = @"<t>mine</t>";
			string theirs = @"<t></t>";

			XmlMerger m = new XmlMerger(new NullMergeSituation());
			var result = m.Merge(ours, theirs, ancestor);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, "t[text()='mine']");

			Assert.AreEqual(typeof(XmlTextEditVsRemovedConflict), result.Conflicts[0].GetType());
		}

		[Test]
		public void TextElement_TheyEditedWeDeleted_EditedIsKept_ConflictRegistered()
		{
			string ancestor = @"<t>original</t>";
			string ours = @"<t></t>";
			string theirs = @"<t>change</t>";

			XmlMerger m = new XmlMerger(new NullMergeSituation());
			var result = m.Merge(ours, theirs, ancestor);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, "t[text()='change']");

			Assert.AreEqual(typeof(XmlTextRemovedVsEditConflict), result.Conflicts[0].GetType());
		}

		[Test]
		public void EachAddedDifferentSyblings_GetBoth()
		{

			string ancestor = @"<a/>";
			string red = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";

			string blue = @"<a>
								<b key='two'>
									<c>second</c>
								</b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a/b[@key='one']/c[text()='first']",
										"a/b[@key='two']/c[text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r.Conflicts[0].GetType());
			// red wins (they will be in a different order, but we don't care which)
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a/b[@key='one']/c[text()='first']",
										 "a/b[@key='two']/c[text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r2.Conflicts[0].GetType());
		}

		[Test]
		public void OneAddedASyblingElement_GetBoth()
		{
			string red = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
							</a>";

			string ancestor = red;

			string blue = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='two'>
									<c key='two'>second</c>
								</b>
							</a>";

			CheckBothWaysNoConflicts(red, blue, ancestor,
									 "a/b[@key='one']/c[text()='first']",
									 "a/b[@key='two']/c[text()='second']");
		}

		[Test]
		public void OneAddedSomethingDeep()
		{
			string red = @"<a>
								<b key='one'/>
							</a>";

			string ancestor = red;

			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";

			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(red, blue));

			CheckBothWaysNoConflicts(red, blue, ancestor,
									 "a/b[@key='one']/c[text()='first']");
		}

		// JohnT: changed <c> elements to <t>. Otherwise, since c expects a key, <c/> does not 'correspond'
		// to <c>first</c>, and the merger tries to insert both, and we get an ambiguous insert exception.
		// Review JohnH(JohnT): should the FindByKeyAttribute also allow a secondary match on a keyless
		// empty element?
		[Test]
		public void OnePutTextContentInPreviouslyElement()
		{
			string red = @"<a>
								<b key='one'><t/></b>
							</a>";

			string ancestor = red;


			string blue = @"<a>
								<b key='one'>
									<t>first</t>
								</b>
							</a>";

			CheckBothWaysNoConflicts(red, blue, ancestor,
									 "a/b[@key='one']/t[text()='first']");
		}

		[Test]
		public void WeDeletedAnElement_Removed()
		{
			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";
			string ancestor = blue;

			string red = @"<a></a>";

			CheckOneWay(blue, red, ancestor, "a[ not(b)]");
		}

		[Test]
		public void TheyDeleteAnElement_Removed()
		{
			string red = @"<a></a>";
			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";
			string ancestor = blue;

			CheckOneWay(blue, red, ancestor, "a[ not(b)]");
		}

		[Test]
		public void BothAddedAnUnkeyableNephewElement()
		{
			string ancestor = @"<a>
								<b key='one'>
									<cx>first</cx>
								</b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<cx>first</cx>
									<cx>second</cx>
								</b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<cx>first</cx>
									<cx>third</cx>
								</b>
							</a>";

			CheckOneWay(red, blue, ancestor,
						"a[count(b)='1']",
						"a/b[count(cx)='3']",
						"a/b[@key='one']/cx[text()='first']",
						"a/b[@key='one']/cx[text()='second']",
						"a/b[@key='one']/cx[text()='third']");

			CheckOneWay(blue, red, ancestor,
						"a[count(b)='1']",
						"a/b[count(cx)='3']",
						"a/b[@key='one']/cx[text()='first']",
						"a/b[@key='one']/cx[text()='second']",
						"a/b[@key='one']/cx[text()='third']");

		}


		[Test]
		public void BothAddedANephewElementWithKeyAttr()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
								</b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
								</b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='z'>third</c>
								</b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='3']",
										"a/b[@key='one']/c[text()='first']",
										"a/b[@key='one']/c[text()='second']",
										"a/b[@key='one']/c[text()='third']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r.Conflicts[0].GetType());
			// red wins (they will be in a different order, but we don't care which)
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='3']",
										 "a/b[@key='one']/c[text()='first']",
										 "a/b[@key='one']/c[text()='second']",
										 "a/b[@key='one']/c[text()='third']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r2.Conflicts[0].GetType());
		}

		[Test]
		public void InsertInMiddleInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = ancestor;


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='z'>third</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='x' and text()='first']",
									 "a/b[@key='one']/c[2][@key='z' and text()='third']",
									 "a/b[@key='one']/c[3][@key='y' and text()='second']");
		}
		[Test]
		public void InsertAtStartInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = ancestor;


			string blue = @"<a>
								<b key='one'>
									<c key='z'>third</c>
								   <c key='x'>first</c>
									 <c key='y'>second</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='z' and text()='third']",
									 "a/b[@key='one']/c[2][@key='x' and text()='first']",
									 "a/b[@key='one']/c[3][@key='y' and text()='second']");

		}

		[Test]
		public void InsertAtEndInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = ancestor;


			string blue = @"<a>
								<b key='one'>
								   <c key='x'>first</c>
									 <c key='y'>second</c>
									<c key='z'>third</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='x' and text()='first']",
									 "a/b[@key='one']/c[2][@key='y' and text()='second']",
									 "a/b[@key='one']/c[3][@key='z' and text()='third']");
		}

		[Test]
		public void HattonTempCheck()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='z'>extra</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='a' and text()='first']",
									 "a/b[@key='one']/c[2][@key='z' and text()='extra']",
									 "a/b[@key='one']/c[3][@key='d' and text()='fourth']");
		}
		/// <summary>
		/// Red inserted at the start, blue at the end. Both should be in the right place.
		/// </summary>
		[Test]
		public void BothInsertedStartEndInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='r'>red</c>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='bl'>blue</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='5']",
									 "a/b[@key='one']/c[1][@key='r' and text()='red']",
									 "a/b[@key='one']/c[2][@key='a' and text()='first']",
									 "a/b[@key='one']/c[3][@key='b' and text()='second']",
									 "a/b[@key='one']/c[4][@key='c' and text()='third']",
									 "a/b[@key='one']/c[5][@key='bl' and text()='blue']");
		}
		/// <summary>
		/// Red moved two items and changed one of them, and blue inserted something between them.
		/// </summary>
		[Test]
		public void ReorderModifyAndInsert()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>red</c>
									<c key='e'>fifth</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='z'>extra</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='6']",
									 "a/b[@key='one']/c[1][@key='a' and text()='first']",
									 "a/b[@key='one']/c[2][@key='d' and text()='red']",
									 "a/b[@key='one']/c[3][@key='z' and text()='extra']",
									 "a/b[@key='one']/c[4][@key='e' and text()='fifth']",
									 "a/b[@key='one']/c[5][@key='b' and text()='second']",
									 "a/b[@key='one']/c[6][@key='c' and text()='third']");
		}

		/// <summary>
		/// Red deleted an item, and blue edited it. Regardless of who initiated the merge,
		/// we should keep the edit.
		/// </summary>
		[Test]
		public void ElementDeleteAndModifyConflict()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='x'>first</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>blue</c>
							   </b>
							</a>";

			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='2']",
										"a/b[@key='one']/c[1][@key='x' and text()='first']",
										"a/b[@key='one']/c[2][@key='y' and text()='blue']");
			Assert.AreEqual(typeof(XmlTextEditVsRemovedConflict), r.Conflicts[0].GetType());

			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='2']",
										"a/b[@key='one']/c[1][@key='x' and text()='first']",
										"a/b[@key='one']/c[2][@key='y' and text()='blue']");
			Assert.AreEqual(typeof(XmlTextRemovedVsEditConflict), r2.Conflicts[0].GetType());
		}

		[Test]
		public void BothInsertedSameInDifferentPlaces()
		{
			string ancestor = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
							</a>";
			string red = @"<a key='one'>
							  <b key='two'>
									<c key='two'>second</c>
								</b>
							   <b key='one'>
									<c key='one'>first</c>
								</b>
							</a>";

			string blue = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='two'>
									<c key='two'>second</c>
								</b>
						  </a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										 "a/b[@key='one']/c[text()='first']",
				"a/b[@key='one']/following-sibling::b/c[text()='second']");
			Assert.AreEqual(typeof(BothInsertedAtDifferentPlaceConflict), r.Conflicts[0].GetType());
			// red wins
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a/b[@key='two']/c[text()='second']",
				"a/b[@key='two']/following-sibling::b/c[text()='first']");
			Assert.AreEqual(typeof(BothInsertedAtDifferentPlaceConflict), r.Conflicts[0].GetType());
		}

		/// <summary>
		/// Both re-orderd things differently.
		/// </summary>
		[Test]
		public void ConflictingReorder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='e'>fifth</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='e'>fifth</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='5']",
										"a/b[@key='one']/c[1][@key='a' and text()='first']",
										"a/b[@key='one']/c[2][@key='b' and text()='second']",
										"a/b[@key='one']/c[3][@key='c' and text()='third']",
										"a/b[@key='one']/c[4][@key='e' and text()='fifth']",
										"a/b[@key='one']/c[5][@key='d' and text()='fourth']");
			Assert.AreEqual(typeof(BothReorderedElementConflict), r.Conflicts[0].GetType());
			// red wins
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='5']",
										 "a/b[@key='one']/c[1][@key='a' and text()='first']",
										 "a/b[@key='one']/c[2][@key='e' and text()='fifth']",
										 "a/b[@key='one']/c[3][@key='b' and text()='second']",
										 "a/b[@key='one']/c[4][@key='c' and text()='third']",
										 "a/b[@key='one']/c[5][@key='d' and text()='fourth']");
			Assert.AreEqual(typeof(BothReorderedElementConflict), r2.Conflicts[0].GetType());
		}

		/// <summary>
		/// Both re-ordered things the same way; blue also edited.
		/// </summary>
		[Test]
		public void BothSameReorder()
		{
			string ancestor =
				@"<a>
						<b key='one'>
							<c key='a'>first</c>
							<c key='b'>second</c>
							<c key='c'>third</c>
							<c key='d'>fourth</c>
							<c key='e'>fifth</c>
					   </b>
					</a>";

			string red =
				@"<a>
						<b key='one'>
							<c key='a'>first</c>
							<c key='e'>fifth</c>
							<c key='b'>second</c>
							<c key='c'>third</c>
							<c key='d'>fourth</c>
					   </b>
					</a>";


			string blue =
				@"<a>
						<b key='one'>
							<c key='a'>first</c>
							<c key='e'>blue</c>
							<c key='b'>second</c>
							<c key='c'>third</c>
							<c key='d'>fourth</c>
					   </b>
					</a>";
			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='5']",
									 "a/b[@key='one']/c[1][@key='a' and text()='first']",
									 "a/b[@key='one']/c[2][@key='e' and text()='blue']",
									 "a/b[@key='one']/c[3][@key='b' and text()='second']",
									 "a/b[@key='one']/c[4][@key='c' and text()='third']",
									 "a/b[@key='one']/c[5][@key='d' and text()='fourth']");
		}

		/// <summary>
		/// Test inserting different things at the same spot. Arguably this is a less serious conflict
		/// than many others, as all of both inserts happens; but we don't know what the order should be.
		///
		/// It is therefore somewhat arbitrary 'their' insert comes before 'ours'. I just made the test
		/// conform to the current implementation plan.
		/// </summary>
		[Test]
		public void DifferentInsertsAtSamePlaceConflict()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='r'>red</c>
									<c key='y'>second</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='b'>blue</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='4']",
										"a/b[@key='one']/c[1][@key='x' and text()='first']",
										"a/b[@key='one']/c[2][@key='r' and text()='red']",
										"a/b[@key='one']/c[3][@key='b' and text()='blue']",
										"a/b[@key='one']/c[4][@key='y' and text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r.Conflicts[0].GetType());
			// red wins
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='4']",
										 "a/b[@key='one']/c[1][@key='x' and text()='first']",
										 "a/b[@key='one']/c[2][@key='b' and text()='blue']",
										 "a/b[@key='one']/c[3][@key='r' and text()='red']",
										 "a/b[@key='one']/c[4][@key='y' and text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r2.Conflicts[0].GetType());
		}

		/// <summary>
		/// This one is subtle. Red re-ordered fourth (d) before second. Blue inserted something (z) after fourth.
		/// Since only red re-ordered things, red's order basically wins. But we don't really know whether
		/// to insert 'z' after 'd' or before 'e', since those are no longer the same position.
		/// Arbitrarily, after 'd' currently wins out.
		/// </summary>
		[Test]
		public void AmbiguousInsertWhenOtherReordered()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>fourth</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='z'>blue</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='6']",
										"a/b[@key='one']/c[1][@key='a' and text()='first']",
										"a/b[@key='one']/c[2][@key='d' and text()='fourth']",
										"a/b[@key='one']/c[3][@key='z' and text()='blue']",
										"a/b[@key='one']/c[4][@key='b' and text()='second']",
										"a/b[@key='one']/c[5][@key='c' and text()='third']",
										"a/b[@key='one']/c[6][@key='e' and text()='fifth']"
				);
			Assert.AreEqual(typeof(AmbiguousInsertReorderConflict), r.Conflicts[0].GetType());
			// red wins
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='6']",
										 "a/b[@key='one']/c[1][@key='a' and text()='first']",
										 "a/b[@key='one']/c[2][@key='d' and text()='fourth']",
										 "a/b[@key='one']/c[3][@key='z' and text()='blue']",
										 "a/b[@key='one']/c[4][@key='b' and text()='second']",
										 "a/b[@key='one']/c[5][@key='c' and text()='third']",
										 "a/b[@key='one']/c[6][@key='e' and text()='fifth']"
				);
			Assert.AreEqual(typeof(AmbiguousInsertReorderConflict), r2.Conflicts[0].GetType());
		}

		[Test]
		public void MergeChildren_UsesNodeToGenerateContextDescriptorIfPossible()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
							   </b>
							</a>";

			string red = ancestor;


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
								</b>
							</a>";
			XmlMerger m = new XmlMerger(new NullMergeSituation());
			m.MergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateForKeyedElementInList("key"));
			var strategy = ElementStrategy.CreateForKeyedElementInList("key");
			var contextGenerator = new MockContextGenerator();
			strategy.ContextDescriptorGenerator = contextGenerator;
			m.MergeStrategies.ElementStrategies.Add("b", strategy);
			m.MergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElementInList("key"));
			m.MergeStrategies.ElementStrategies.Add("d", ElementStrategy.CreateForKeyedElementInList("key"));
			m.Merge(red, blue, ancestor);
			Assert.That(contextGenerator.InputNode, Is.Not.Null);
			Assert.That(contextGenerator.InputNode.Name, Is.EqualTo("b"));
		}

		[Test]
		public void AllFilesMissingThrows()
		{
			var merger = new XmlMerger(new NullMergeSituation());
			Assert.Throws<InvalidOperationException>(() => merger.MergeFiles(null, null, null));
		}

		[Test]
		public void AncestorIsNotXmlThrows()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, "Not xml stuff.");
				File.WriteAllText(ours.Path, data);
				File.WriteAllText(theirs.Path, data);
				var merger = new XmlMerger(new NullMergeSituation());
				Assert.Throws<XmlException>(() => merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path));
			}
		}

		[Test]
		public void OursIsNotXmlThrows()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, data);
				File.WriteAllText(ours.Path, "Not xml stuff.");
				File.WriteAllText(theirs.Path, data);
				var merger = new XmlMerger(new NullMergeSituation());
				Assert.Throws<XmlException>(() => merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path));
			}
		}

		[Test]
		public void TheirsIsNotXmlThrows()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, data);
				File.WriteAllText(ours.Path, data);
				File.WriteAllText(theirs.Path, "Not xml stuff.");
				var merger = new XmlMerger(new NullMergeSituation());
				Assert.Throws<XmlException>(() => merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path));
			}
		}

		[Test]
		public void AncestorIsEmptyFileAndBothAddedSameThingHasOneChangeReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, "");
				File.WriteAllText(ours.Path, data);
				File.WriteAllText(theirs.Path, data);
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
					{
						EventListener = listener
					};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlBothAddedSameChangeReport>();
				listener.AssertExpectedConflictCount(0);
			}
		}

		[Test]
		public void AncestorAndOursAreEmptyFileAndTheyAddedHasOneChangeReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, "");
				File.WriteAllText(ours.Path, "");
				File.WriteAllText(theirs.Path, data);
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlAdditionChangeReport>();
				listener.AssertExpectedConflictCount(0);
			}
		}

		[Test]
		public void AncestorAndTheirsAreEmptyFileAndWeAddedHasOneChangeReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, "");
				File.WriteAllText(ours.Path, data);
				File.WriteAllText(theirs.Path, "");
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlAdditionChangeReport>();
				listener.AssertExpectedConflictCount(0);
			}
		}

		[Test]
		public void AncestorIsEmptyFileAndWeBothAddedDifferentContentHasOneConflictReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, "");
				File.WriteAllText(ours.Path, data.Replace("<data />", "<data>our addition</data>"));
				File.WriteAllText(theirs.Path, data.Replace("<data />", "<data>their addition</data>"));
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(1);
				listener.AssertFirstConflictType<BothAddedMainElementButWithDifferentContentConflict>();
				Assert.IsTrue(result.MergedNode.OuterXml.Contains("our addition"));
			}
		}

		[Test]
		public void AncestorIsEmptyFileAndWeBothAddedDifferentContentWithTheyWinHasOneConflictReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, "");
				File.WriteAllText(ours.Path, data.Replace("<data />", "<data>our addition</data>"));
				File.WriteAllText(theirs.Path, data.Replace("<data />", "<data>their addition</data>"));
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituationTheyWin())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(1);
				listener.AssertFirstConflictType<BothAddedMainElementButWithDifferentContentConflict>();
				Assert.IsTrue(result.MergedNode.OuterXml.Contains("their addition"));
			}
		}

		[Test]
		public void AncestorHasDataAndWeBothDeletedFileHasOneChangeReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, data);
				File.WriteAllText(ours.Path, "");
				File.WriteAllText(theirs.Path, "");
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlBothDeletionChangeReport>();
				listener.AssertExpectedConflictCount(0);
				Assert.IsNull(result.MergedNode);
			}
		}

		[Test]
		public void WeDeletedFileTheyDidNothingHasOneChangeReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, data);
				File.WriteAllText(ours.Path, "");
				File.WriteAllText(theirs.Path, data);
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
				listener.AssertExpectedConflictCount(0);
				Assert.IsNull(result.MergedNode);
			}
		}

		[Test]
		public void TheyChangedWeDeletedFileHasOneConflictReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, data);
				File.WriteAllText(ours.Path, "");
				File.WriteAllText(theirs.Path, data.Replace("<data />", "<data>their change</data>"));
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(1);
				listener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
				Assert.IsTrue(result.MergedNode.OuterXml.Contains("their change"));
			}
		}

		[Test]
		public void TheyDeletedFileWeDidNothingHasOneChangeReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, data);
				File.WriteAllText(ours.Path, data);
				File.WriteAllText(theirs.Path, "");
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
				listener.AssertExpectedConflictCount(0);
				Assert.IsNull(result.MergedNode);
			}
		}

		[Test]
		public void WeChangedTheyDeletedFileHasOneConflictReport()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<data />";
			using (var ancestor = new TempFile())
			using (var ours = new TempFile())
			using (var theirs = new TempFile())
			{
				File.WriteAllText(ancestor.Path, data);
				File.WriteAllText(ours.Path, data.Replace("<data />", "<data>our change</data>"));
				File.WriteAllText(theirs.Path, "");
				var listener = new ListenerForUnitTests();
				var merger = new XmlMerger(new NullMergeSituation())
				{
					EventListener = listener
				};
				var result = merger.MergeFiles(ours.Path, theirs.Path, ancestor.Path);
				Assert.IsNotNull(result);
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(1);
				listener.AssertFirstConflictType<EditedVsRemovedElementConflict>();
				Assert.IsTrue(result.MergedNode.OuterXml.Contains("our change"));
			}
		}
	}

	internal class MockContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode
	{
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			Assert.Fail("should not call the string version when an XmlNode version is available");
			return null; // to satisfy compiler
		}

		public XmlNode InputNode;

		public ContextDescriptor GenerateContextDescriptor(XmlNode mergeElement, string filePath)
		{
			InputNode = mergeElement;
			return null;
		}
	}
}