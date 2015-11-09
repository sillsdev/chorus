using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHandlers;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers
{
	[TestFixture]
	public class ChorusTestFileHanderTests
	{
		[Test, Ignore("Run by hand only, since the dll can't be deleted, once it has been loaded.")]
		public void ChorusFileTypeHandlerCollectionContainsTestAFileTypeHandler()
		{
			string samplePluginPathname = null;
			//try
			//{
				var assem = Assembly.GetExecutingAssembly();
#if MONO
			var codeBase = assem.CodeBase.Substring(7);
#else
				var codeBase = assem.CodeBase.Substring(8);
#endif
				//Debug.WriteLine("codeBase: " + codeBase);
				var dirname = Path.GetDirectoryName(codeBase);
				//Debug.WriteLine("dirname: " + dirname);
				//var baseDir = new Uri(dirname).AbsolutePath; // NB: The Uri class in Windows and Mono are not the same.
				var baseDir = dirname;
				//var baseDir = new Uri(Path.GetDirectoryName(assem.CodeBase)).AbsolutePath;
				var outputDir = Directory.GetParent(baseDir).FullName;
				var samplePluginDir = Path.Combine(outputDir, "SamplePlugin");
				var samplePluginDllPath = Path.Combine(samplePluginDir, "Tests-ChorusPlugin.dll");
				samplePluginPathname = Path.Combine(baseDir, "Tests-ChorusPlugin.dll");
				if (File.Exists(samplePluginDllPath))
					File.Copy(samplePluginDllPath, samplePluginPathname, true);

				Assert.IsNotNull((from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
								  where handler.GetType().Name == "TestAFileTypeHandler"
								  select handler).FirstOrDefault());
			//}
			//finally
			//{
			//	//if (!string.IsNullOrEmpty(samplePluginPathname) && File.Exists(samplePluginPathname))
			//	//	File.Delete(samplePluginPathname);
			//}
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
