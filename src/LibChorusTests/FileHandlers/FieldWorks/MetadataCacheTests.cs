using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.Utilities;
using NUnit.Framework;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the FieldWorks MetadataCache class.
	/// </summary>
	[TestFixture]
	public class MetadataCacheTests
	{
		private MetadataCache _mdc;

		/// <summary>
		/// Set up the test fixture class.
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = new MetadataCache();
		}

		/// <summary></summary>
		[Test]
		public void Access_Class_Info_With_Null_ClassName_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => _mdc.GetClassInfo(null));
		}

		/// <summary></summary>
		[Test]
		public void Access_Class_Info_With_Empty_String_For_ClassName_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => _mdc.GetClassInfo(""));
		}

		/// <summary></summary>
		[Test]
		public void Access_Class_Info_With_Bogus_ClassName_Throws()
		{
			Assert.Throws<KeyNotFoundException>(() => _mdc.GetClassInfo("Bogus"));
		}

		/// <summary></summary>
		[Test]
		public void CmObject_Has_No_Properties()
		{
			Assert.IsTrue(_mdc.GetClassInfo("CmObject").AllProperties.Count() == 0);
		}

		/// <summary></summary>
		[Test]
		public void Can_Add_Custom_Property()
		{
			var wordformInfo = _mdc.GetClassInfo("WfiWordform");
			Assert.IsNull((from propInfo in wordformInfo.AllProperties
							  where propInfo.PropertyName == "Certified"
							  select propInfo).FirstOrDefault());

			_mdc.AddCustomPropInfo("WfiWordform", new FdoPropertyInfo("Certified", DataType.Boolean));

			Assert.IsNotNull((from propInfo in wordformInfo.AllProperties
									 where propInfo.PropertyName == "Certified"
									 select propInfo).FirstOrDefault());

		}

		/// <summary></summary>
		[Test]
		public void LexDb_Has_Collection_Properties()
		{
			Assert.IsTrue(_mdc.GetClassInfo("LexDb").AllCollectionProperties.Count() > 0);
		}

		/// <summary></summary>
		[Test]
		public void Segment_Has_No_Collection_Properties()
		{
			Assert.IsTrue(_mdc.GetClassInfo("Segment").AllCollectionProperties.Count() == 0);
		}
	}
}