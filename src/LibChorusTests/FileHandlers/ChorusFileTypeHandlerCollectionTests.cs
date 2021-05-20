using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.audio;
using Chorus.FileTypeHandlers.test;
using SIL.PlatformUtilities;

namespace LibChorus.Tests.FileHandlers
{
	[TestFixture]
	public class ChorusFileTypeHandlerCollectionTests
	{
		private static string BaseDir
		{
			get
		{
				var assem = Assembly.GetExecutingAssembly();
				return Path.GetDirectoryName(assem.CodeBase.Substring(Platform.IsUnix ? 7 : 8));
			}
		}

		private static string SamplePluginPath
		{
			get
			{
				var outputDir = Directory.GetParent(BaseDir).FullName;
				var samplePluginDir = Path.Combine(outputDir, "SamplePlugin");
				var samplePluginDllPath = Path.Combine(samplePluginDir, "Tests-ChorusPlugin.dll");
				return samplePluginDllPath;
			}
		}

		[Test]
		[Ignore("Run by hand only, since the dll can't be deleted, once it has been loaded.")]
		public void CreateWithInstalledHandlers_ContainsTestAFileTypeHandler()
		{
			string samplePluginDllPath = SamplePluginPath;
			var samplePluginPathname = Path.Combine(BaseDir, "Tests-ChorusPlugin.dll");
				if (File.Exists(samplePluginDllPath))
					File.Copy(samplePluginDllPath, samplePluginPathname, true);

			var handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers;
			Assert.That(handlers.Select(x => x.GetType().Name), Has.Member("TestAFileTypeHandler"));
		}

		[Test]
		public void CreateWithInstalledHandlers_HandlersFromAdditionalAssembly()
		{
			var handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers(
				new[] { SamplePluginPath }).Handlers;
			Assert.That(handlers.Select(x => x.GetType().Name), Has.Member("TestAFileTypeHandler"));
		}

		[Test]
		public void CreateWithInstalledHandlers_DefaulthandlerIsNotInMainCollection()
		{
			Assert.That(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
				.Select(x => x.GetType()), Has.No.Member(typeof(DefaultFileTypeHandler)));
		}

		[Test]
		public void CreateWithInstalledHandlers_ContainsHandlers()
		{
			Assert.That(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
				.Select(x => x.GetType()), Has.Member(typeof(AudioFileTypeHandler)));
		}

		[Test]
		public void CreateWithTestHandlerOnly_DefaulthandlerIsNotInTestCollection()
		{
			Assert.That(ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly().Handlers
				.Select(x => x.GetType()), Has.No.Member(typeof(DefaultFileTypeHandler)));
		}

		[Test]
		public void CreateWithTestHandlerOnly_TestHandlerIsInTestCollection()
		{
			var handlers = ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly().Handlers;
			Assert.That(handlers.Count(), Is.EqualTo(1));
			Assert.That(handlers.Select(x => x.GetType()), Has.Member(typeof(ChorusTestFileHandler)));
		}
	}
}
