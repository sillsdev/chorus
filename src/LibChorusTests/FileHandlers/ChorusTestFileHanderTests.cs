using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers
{
	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class ChorusTestFileHanderTests
	{
		[Test]
		public void ChorusFileTypeHandlerCollectionContainsTestAFileTypeHandler()
		{
			string samplePluginPathname = null;
			try
			{
				var assem = Assembly.GetExecutingAssembly();
				var baseDir = new Uri(Path.GetDirectoryName(assem.CodeBase)).AbsolutePath;
				var outputDir = Directory.GetParent(baseDir).FullName;
				var samplePluginDir = Path.Combine(outputDir, "SamplePlugin");
				var samplePluginDllPath = Path.Combine(samplePluginDir, "Tests-ChorusPlugin.dll");
				samplePluginPathname = Path.Combine(baseDir, "Tests-ChorusPlugin.dll");
				if (File.Exists(samplePluginDllPath))
					File.Copy(samplePluginDllPath, samplePluginPathname, true);

				Assert.IsNotNull((from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
								  where handler.GetType().Name == "TestAFileTypeHandler"
								  select handler).FirstOrDefault());
			}
			finally
			{
				if (!string.IsNullOrEmpty(samplePluginPathname) && File.Exists(samplePluginPathname))
					File.Delete(samplePluginPathname);
			}
		}

		[Test]
		public void MakeSureDefaulthandlerIsNotInMainCollection()
		{
			Assert.IsNull((from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
						   where handler.GetType().Name == "DefaultFileTypeHandler"
						   select handler).FirstOrDefault());
		}

		[Test]
		public void MakeSureDefaulthandlerIsNotInTestCollection()
		{
			Assert.IsNull((from handler in ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly().Handlers
						   where handler.GetType().Name == "DefaultFileTypeHandler"
						   select handler).FirstOrDefault());
		}

		[Test]
		public void MakeSureOnlyOneHandlerIsInTestCollection()
		{
			var handlers = ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly().Handlers;
			Assert.AreEqual(1, handlers.Count());
			Assert.IsNotNull((from handler in handlers
						   where handler.GetType().Name == "ChorusTestFileHandler"
						   select handler).FirstOrDefault());
		}
	}
}
