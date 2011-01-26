using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Chorus.Utilities;
using NUnit.Framework;

namespace LibChorus.Tests.utilities
{
	/// <summary>
	/// Tests for the FastXmlElementSplitter class.
	/// </summary>
	[TestFixture]
	public class FastXmlElementSplitterTests
	{
		[Test]
		public void Null_Pathname_Throws()
		{
			Assert.Throws<ArgumentException>(() => new FastXmlElementSplitter(null));
		}

		[Test]
		public void Empty_String_Pathname_Throws()
		{
			Assert.Throws<ArgumentException>(() => new FastXmlElementSplitter(null));
		}

		[Test]
		public void File_Not_Found_Throws()
		{
			Assert.Throws<FileNotFoundException>(() => new FastXmlElementSplitter("Non-existant-file.xml"));
		}

		[Test]
		public void Null_Parameter_Throws()
		{
			// review: I (CP) don't know that this is a sufficiently good method for determining the file - even in windows.
			// In mono I had to make this the absolute path. 2011-01
#if MONO
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file://", null)))
#else
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", null)))
#endif
			{
				Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes(null));
			}
		}

		[Test]
		public void Empty_String_Parameter_Throws()
		{
			// review: I (CP) don't know that this is a sufficiently good method for determining the file - even in windows.
			// In mono I had to make this the absolute path. 2011-01
#if MONO
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file://", null)))
#else
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", null)))
#endif
			{
				Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes(""));
			}
		}

		[Test]
		public void No_Records_With_Children_Is_Fine()
		{
			const string noRecordsInput =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
</languageproject>";
			var goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".fwdata");
			try
			{
				File.WriteAllText(goodXmlPathname, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(goodXmlPathname))
				{
					Assert.AreEqual(0, reader.GetSecondLevelElementBytes("rt").Count());
				}
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void No_Records_Without_Children_Is_Fine()
		{
			const string noRecordsInput =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016' />";

			var goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".fwdata");
			try
			{
				File.WriteAllText(goodXmlPathname, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(goodXmlPathname))
				{
					Assert.AreEqual(0, reader.GetSecondLevelElementBytes("rt").Count());
				}
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void Not_Xml_Throws()
		{
			const string noRecordsInput = "Some random text file.";
			var goodPathname = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
			try
			{
				File.WriteAllText(goodPathname, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(goodPathname))
				{
					Assert.Throws<InvalidOperationException>(() => reader.GetSecondLevelElementBytes("rt"));
				}
			}
			finally
			{
				File.Delete(goodPathname);
			}
		}

		[Test]
		public void Can_Find_Good_FieldWorks_Records()
		{
			const string hasRecordsInput =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='emptyElement1'/>
<rt guid='normalElement'>
	<randomElement />
</rt>
<rt
	guid='atterOnNextLine'>
</rt>
<rt		guid='tabAfterOpenTag'>
</rt>
<rt guid='emptyElement2' />
</languageproject>";

			CheckGoodFile(hasRecordsInput, 5, "AdditionalFields", "rt");
			CheckGoodFile(hasRecordsInput, 5, "AdditionalFields", "<rt");
		}

		[Test]
		public void Can_Find_Custom_FieldWorks_Element()
		{
			const string hasRecordsInput =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>
<rt guid='emptyElement1'/>
<rt guid='normalElement'>
	<randomElement />
</rt>
<rt
	guid='atterOnNextLine'>
</rt>
<rt		guid='tabAfterOpenTag'>
</rt>
<rt guid='emptyElement2' />
</languageproject>";

			CheckGoodFile(hasRecordsInput, 6, "AdditionalFields", "rt");
		}

		[Test]
		public void Can_Find_Good_Lift_Records()
		{
			const string hasRecordsInput =
@"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
					   <entry id='sameInBoth'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='themOnly'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUs'/>

						<entry
							id='brewingConflict'>
							<sense>
								 <gloss lang='a'>
									<text>them</text>
								 </gloss>
							 </sense>
						</entry>

					</lift>";

			CheckGoodFile(hasRecordsInput, 4, "header", "entry");
		}

		private static void CheckGoodFile(string hasRecordsInput, int expectedCount, string firstElementMarker, string recordMarker)
		{
			var goodPathname = Path.GetTempFileName();
			try
			{
				var enc = Encoding.UTF8;
				File.WriteAllText(goodPathname, hasRecordsInput, enc);
				using (var reader = new FastXmlElementSplitter(goodPathname))
				{
					bool foundOptionalFirstElement;
					var elementBytes = reader.GetSecondLevelElementBytes(firstElementMarker, recordMarker, out foundOptionalFirstElement).ToList();
					Assert.AreEqual(expectedCount, elementBytes.Count);
					var elementStrings = reader.GetSecondLevelElementStrings(firstElementMarker, recordMarker, out foundOptionalFirstElement).ToList();
					Assert.AreEqual(expectedCount, elementStrings.Count);
					for (var i = 0; i < elementStrings.Count; ++i)
					{
						Assert.AreEqual(
							elementStrings[i],
							enc.GetString(elementBytes[i]));
					}
				}
			}
			finally
			{
				File.Delete(goodPathname);
			}
		}
	}
}
