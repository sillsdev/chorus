using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class ElementStrategyTests
	{
		[Test]
		public void DefaultNumberOfChildren_Is_NumberOfChildrenAllowed_ZeroOrMore()
		{
			Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, new ElementStrategy(false).NumberOfChildren);
		}

		[Test]
		public void DefaultIsImmutable_Is_False()
		{
			Assert.IsFalse(new ElementStrategy(false).IsImmutable);
		}

		[Test]
		public void DefaultIsAtomic_Is_False()
		{
			Assert.IsFalse(new ElementStrategy(false).IsAtomic);
		}

		[Test]
		public void ElementStrategy_CreateForKeyedElement_Has_FindByKeyAttribute_Finder()
		{
			var elementStrategy = ElementStrategy.CreateForKeyedElement("myKey", false);
			Assert.IsInstanceOf<FindByKeyAttribute>(elementStrategy.MergePartnerFinder);
		}

		[Test]
		public void ElementStrategy_CreateForKeyedElementInList_Has_FindByKeyAttributeInList_Finder()
		{
			var elementStrategy = ElementStrategy.CreateForKeyedElementInList("myKey");
			Assert.IsInstanceOf<FindByKeyAttributeInList>(elementStrategy.MergePartnerFinder);
		}

		[Test]
		public void ElementStrategy_CreateSingletonElement_Has_FindFirstElementWithSameName_Finder()
		{
			var elementStrategy = ElementStrategy.CreateSingletonElement();
			Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
		}
	}
}