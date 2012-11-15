using System.Collections.Generic;
using Chorus.Utilities;
using NUnit.Framework;

namespace LibChorus.Tests.utilities
{
	[TestFixture]
	public class MultiMapTests
	{
		[Test]
		public void Enumerate_Empty_Ok()
		{
			MultiMap<string, string> m = new MultiMap<string, string>();
			foreach (var map in m)
			{
				Assert.Fail();
			}
		}

		[Test]
		public void Enumerate_KeyHasMultiple_ReturnsBoth()
		{
			MultiMap<string, string> m = new MultiMap<string, string>();
			m.Add("fruit","apple");
			m.Add("vegetable","broccoli");
			m.Add("fruit", "orange");
			var returned = new List<KeyValuePair<string, string>>();
			foreach (var pair in m)
			{
				returned.Add(pair);
			}
			Assert.AreEqual(3, returned.Count);
			Assert.AreEqual("apple", returned[0].Value);
			Assert.AreEqual("orange", returned[1].Value);
			Assert.AreEqual("broccoli", returned[2].Value);
			Assert.AreEqual("fruit", returned[0].Key);
			Assert.AreEqual("fruit", returned[1].Key);
			Assert.AreEqual("vegetable", returned[2].Key);
		}
	}
}
