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
			Assert.IsTrue(handler.CanValidateFile("foo.lift"));
			Assert.IsTrue(handler.CanValidateFile("foo.LiFt"));
			Assert.IsFalse(handler.CanValidateFile("foo.WeSayConfig"));
			Assert.IsFalse(handler.CanValidateFile("foo.xml"));
			Assert.IsFalse(handler.CanValidateFile("foo.abc"));
		}

		[Test]
		public void ValidateFile_SimpleLift_ReturnsNull()
		{
			var handler = new LiftFileHandler();
			using(var file = new TempFile("<lift/>"))
			{
				var result = handler.ValidateFile(file.Path, new ConsoleProgress());
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ValidateFile_IllFormedXml_ReturnsProblem()
		{
			var handler = new LiftFileHandler();
			using (var file = new TempFile("<lift>"))
			{
				var result = handler.ValidateFile(file.Path, new ConsoleProgress());
				Assert.IsNotNull(result);
			}
		}

		[Test, Ignore("Not yet")]
		public void ValidateFile_BadLift_ReturnsProblem()
		{
			var handler = new LiftFileHandler();
			using (var file = new TempFile("<lift><foo/></lift>"))
			{
				var result = handler.ValidateFile(file.Path, new ConsoleProgress());
				Assert.IsNotNull(result);
			}
		}
	}
}
