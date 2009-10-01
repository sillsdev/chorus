using System;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.notes;
using NUnit.Framework;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class NotesRepositoryTests
	{

		[Test, ExpectedException(typeof(FileNotFoundException))]
		public void FromPath_PathNotFound_Throws()
		{
			NotesRepository.FromFile("bogus.xml");
		}

		[Test, ExpectedException(typeof(NotesFormatException))]
		public void FromString_FormatIsTooNew_Throws()
		{
			NotesRepository.FromString("<notes version='99'/>");
		}

		[Test, ExpectedException(typeof(NotesFormatException))]
		public void FromString_FormatIsBadXml_Throws()
		{
			NotesRepository.FromString("<notes version='99'>");
		}

		[Test]
		public void GetAll_EmptyDOM_OK()
		{
			using (var r = NotesRepository.FromString("<notes version='0'>"))
			{
				Assert.AreEqual(0, r.GetAll().Count());
			}

		}


	}


}