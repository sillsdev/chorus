using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus;
using Chorus.Utilities;
using ChorusMerge;
using NUnit.Framework;

namespace Chorus.Tests.utilities
{
	[TestFixture]
	public class ChorusDiff3Tests
	{
		[Test]
		public void Diff3IsAccessible()
		{
			Assert.IsTrue(TextMerger.GetVersion().Contains("Copyright"));
		}

		[Test]
		public void CommonIsEmpty()
		{
			Merge m = new Merge();
			m.CommonInput = new string[] {};
			m.OurInput = new string[] { "bob" };
			m.TheirInput = new string[] { "sally"};
			m.Go();
			AssertLeftNoMergeArtifacts(m);
			Assert.AreEqual(string.Empty, m.LeastCommonDenominator.Trim());
			Assert.IsTrue(m.OurPartial.Contains("bob"));
			Assert.IsTrue(m.TheirPartial.Contains("sally"));
		}

		private void AssertLeftNoMergeArtifacts(Merge m)
		{
			Assert.IsFalse(m.LeastCommonDenominator.Contains("<<<<"));
			Assert.IsFalse(m.LeastCommonDenominator.Contains(">>>>"));
			Assert.IsFalse(m.LeastCommonDenominator.Contains("===="));
			Assert.IsFalse(m.OurPartial.Contains("<<<<"));
			Assert.IsFalse(m.OurPartial.Contains("===="));
			Assert.IsFalse(m.OurPartial.Contains(">>>>"));
			Assert.IsFalse(m.TheirPartial.Contains("<<<<"));
			Assert.IsFalse(m.TheirPartial.Contains("===="));
			Assert.IsFalse(m.TheirPartial.Contains(">>>>"));
		}

		[Test]
		public void CanGetPartialMergeForUser()
		{
			Merge m = new Merge();
			m.CommonInput = new string[] {"one", "two", "three", "4", "five", "6", "seven"};
			m.OurInput = new string[] {"one", "bob2", "three", "bob4", "five", "6", "seven"};
			m.TheirInput = new string[] {"one", "sally2", "three", "4", "five", "sally6", "seven"};
			m.Go();
			AssertLeftNoMergeArtifacts(m);

			Assert.IsTrue(m.LeastCommonDenominator.Contains("two"));
			Assert.IsTrue(m.LeastCommonDenominator.Contains("bob4"));
			Assert.IsTrue(m.LeastCommonDenominator.Contains("sally6"));


			Assert.IsTrue(m.OurPartial.Contains("bob2"));
			Assert.IsTrue(m.OurPartial.Contains("bob4"));
			Assert.IsTrue(m.OurPartial.Contains("sally6"));

			Assert.IsTrue(m.TheirPartial.Contains("sally2"));
			Assert.IsTrue(m.TheirPartial.Contains("bob4"));
			Assert.IsTrue(m.TheirPartial.Contains("sally6"));
		}

		class Merge
		{
			public string[] CommonInput;
			public string[] OurInput;
			public string[] TheirInput;

			public string LeastCommonDenominator;
			public string OurPartial;
			public string TheirPartial;

			public void Go()
			{
				TempFile bob = new TempFile(OurInput);
				TempFile common = new TempFile(CommonInput);

				TempFile sally = new TempFile(TheirInput);
				TempFile bobPartial = new TempFile();
				TempFile sallyPartial = new TempFile();
				TempFile leastCommonDenominator = new TempFile();
				try
				{
					TextMerger.Merge(common.Path, bob.Path, sally.Path, leastCommonDenominator.Path, bobPartial.Path, sallyPartial.Path);
					LeastCommonDenominator = File.ReadAllText(leastCommonDenominator.Path);
					OurPartial = File.ReadAllText(bobPartial.Path);
					TheirPartial = File.ReadAllText(sallyPartial.Path);
				}
				finally
				{
					common.Dispose();
					bob.Dispose();
					sally.Dispose();
					bobPartial.Dispose();
					sallyPartial.Dispose();
					leastCommonDenominator.Dispose();
				}
			}
		}


	}
}