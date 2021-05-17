using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus;
using Chorus.Utilities;
using ChorusMerge;
using NUnit.Framework;

namespace LibChorus.Tests.utilities
{
#if PartialIsAlive
	[TestFixture]
	public class ChorusDiff3Tests
	{
		[Test]
		public void Diff3IsAccessible()
		{
			Assert.That(TextMerger.GetVersion(), Does.Contain("Copyright"));
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
			Assert.That(m.OurPartial, Does.Contain("bob"));
			Assert.That(m.TheirPartial, Does.Contain("sally"));
		}

		private void AssertLeftNoMergeArtifacts(Merge m)
		{
			Assert.That(m.LeastCommonDenominator, Does.Not.Contain("<<<<"));
			Assert.That(m.LeastCommonDenominator, Does.Not.Contain(">>>>"));
			Assert.That(m.LeastCommonDenominator, Does.Not.Contain("===="));
			Assert.That(m.OurPartial, Does.Not.Contain("<<<<"));
			Assert.That(m.OurPartial, Does.Not.Contain("===="));
			Assert.That(m.OurPartial, Does.Not.Contain(">>>>"));
			Assert.That(m.TheirPartial, Does.Not.Contain("<<<<"));
			Assert.That(m.TheirPartial, Does.Not.Contain("===="));
			Assert.That(m.TheirPartial, Does.Not.Contain(">>>>"));
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

			Assert.That(m.LeastCommonDenominator, Does.Contain("two"));
			Assert.That(m.LeastCommonDenominator, Does.Contain("bob4"));
			Assert.That(m.LeastCommonDenominator, Does.Contain("sally6"));


			Assert.That(m.OurPartial, Does.Contain("bob2"));
			Assert.That(m.OurPartial, Does.Contain("bob4"));
			Assert.That(m.OurPartial, Does.Contain("sally6"));

			Assert.That(m.TheirPartial, Does.Contain("sally2"));
			Assert.That(m.TheirPartial, Does.Contain("bob4"));
			Assert.That(m.TheirPartial, Does.Contain("sally6"));
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
#endif
}