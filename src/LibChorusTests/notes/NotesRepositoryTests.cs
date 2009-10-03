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
			using (var r = NotesRepository.FromString("<notes version='0'/>"))
			{
				Assert.AreEqual(0, r.GetAll().Count());
			}
		}

		[Test]
		public void GetAll_Has2_ReturnsBoth()
		{

			using (var r = NotesRepository.FromString(@"<notes version='0'>
	<annotation guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>
	<annotation guid='12D39999-E83D-41AD-BAB3-B7E46D8C13CE'/>
</notes>"))
			{
				Assert.AreEqual(2, r.GetAll().Count());
			}
		}

		[Test]
		public void GetByCurrentStatus_UsesTheLastMessage()
		{
			using (var r = NotesRepository.FromString(@"<notes version='0'>
	<annotation guid='123'><message status='open'/>
<message status='processing'/> <message status='closed'/>
</annotation>
</notes>"))
			{
				Assert.AreEqual(0, r.GetByCurrentStatus("open").Count());
				Assert.AreEqual(0, r.GetByCurrentStatus("processing").Count());
				Assert.AreEqual(1, r.GetByCurrentStatus("closed").Count());
			}
		}

		[Test]
		public void GetByCurrentStatus_NoMessages_ReturnsNone()
		{
			using (var r = NotesRepository.FromString(@"<notes version='0'>
	<annotation guid='123'/></notes>"))
			{
				Assert.AreEqual(0, r.GetByCurrentStatus("open").Count());
			}
		}
	}
}