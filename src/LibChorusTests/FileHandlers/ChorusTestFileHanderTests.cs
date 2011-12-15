using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers
{
	[TestFixture]
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
	}
}
