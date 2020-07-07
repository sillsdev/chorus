using Chorus.FileTypeHandlers.lift;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibChorus.Tests.merge.xml.lift
{
	[TestFixture]
	public class ValidationTests
	{
		[Test]
		public void CanValidateFile_AcceptsCorrectSet()
		{
			var handler = new LiftFileHandler();
			Assert.That(handler.CanValidateFile("foo.lift"), Is.True);
			Assert.That(handler.CanValidateFile("foo.LiFt"), Is.True);
			Assert.That(handler.CanValidateFile("foo.WeSayConfig"), Is.False);
			Assert.That(handler.CanValidateFile("foo.xml"), Is.False);
			Assert.That(handler.CanValidateFile("foo.abc"), Is.False);
		}

		[Test]
		public void ValidateFile_SimpleLift_ReturnsNull()
		{
			var handler = new LiftFileHandler();
			using(var file = new TempFile("<lift/>"))
			{
				var result = handler.ValidateFile(file.Path, new ConsoleProgress());
				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ValidateFile_IllFormedXml_ReturnsProblem()
		{
			var handler = new LiftFileHandler();
			using (var file = new TempFile("<lift>"))
			{
				var result = handler.ValidateFile(file.Path, new ConsoleProgress());
				Assert.That(result, Is.Not.Null);
			}
		}

		[Test, Ignore("Not yet")]
		public void ValidateFile_BadLift_ReturnsProblem()
		{
			var handler = new LiftFileHandler();
			using (var file = new TempFile("<lift><foo/></lift>"))
			{
				var result = handler.ValidateFile(file.Path, new ConsoleProgress());
				Assert.That(result, Is.Not.Null);
			}
		}
	}
}
