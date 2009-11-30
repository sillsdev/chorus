using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.annotations;
using NUnit.Framework;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class AnnotationTests
	{
		[Test]
		public void Class_HasClass_ReturnsClass()
		{
			var a = new Annotation(XElement.Parse("<annotation class='foo' guid='12D388BD-E83D-41AD-BAB3-B7E46D8C13CE'/>"));
			Assert.AreEqual("foo", a.ClassName);
		}
		[Test]
		public void Guid_HasGuid_ReturnsGuid()
		{
			var guid = Guid.NewGuid();
			var a = new Annotation(XElement.Parse("<annotation class='foo' guid='"+guid+"'/>"));
			Assert.AreEqual(guid.ToString(), a.Guid);
		}
		[Test]
		public void Ref_HasUrl_ReturnsUrl()
		{
			var a = new Annotation(XElement.Parse("<annotation ref='pretend' class='foo' guid='123'/>"));
			Assert.AreEqual("pretend", a.Ref);
		}

		[Test]
		public void GetLabel_RefHasLabel_ReturnsLabel()
		{
			Annotation a = CreateAnnotation("<annotation ref='lift://somefile.lift?label=blah&somethingelse=3' class='note' guid='123'/>");
			Assert.AreEqual("blah", a.GetLabelFromRef("unknown"));
		}

		private Annotation CreateAnnotation(string contents)
		{
			contents = contents.Replace("&", "&amp;");
			return new Annotation(XElement.Parse(contents));
		}

		[Test]
		public void GetMessages()
		{
			var a = new Annotation(XElement.Parse(@"<annotation ref='pretend' class='foo' guid='123'>
			<message/><message/></annotation>"));
			Assert.AreEqual(2, a.Messages.Count());
		}
//
//        [Test]
//        public void Constructor_StatusIsOpen()
//        {
//
//        }


		[Test]
		public void AddMessage_Had0_Has1()
		{
			var a = new Annotation(XElement.Parse(@"<annotation ref='pretend' class='foo' guid='123'>
			</annotation>"));
			var m = a.AddMessage("joe", "closed", string.Empty);
			Assert.AreEqual(1, a.Messages.Count());
		}

		[Test]
		public void AddMessage_Had1_Has2InCorrectOrder()
		{
			var a = new Annotation(XElement.Parse(@"<annotation ref='pretend' class='foo' guid='123'>
			<message guid='123' status='open'/></annotation>"));

			var m  = a.AddMessage("joe", "closed", string.Empty);

			Assert.AreEqual(2, a.Messages.Count());
			Assert.AreEqual("open", a.Messages.First().Status);
			Assert.AreEqual("closed", a.Messages.ToArray()[1].Status );
		}


	}
}