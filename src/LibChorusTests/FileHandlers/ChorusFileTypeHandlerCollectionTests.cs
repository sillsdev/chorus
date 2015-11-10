using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHandlers;
using NUnit.Framework;
using Chorus.FileTypeHandlers.test;

namespace LibChorus.Tests.FileHandlers
{
	[TestFixture]
	public class ChorusFileTypeHandlerCollectionTests
	{
		[Test, Ignore("Run by hand only, since the dll can't be deleted, once it has been loaded.")]
		public void CreateWithInstalledHandlers_ContainsTestAFileTypeHandler()
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
		public void CreateWithInstalledHandlers_DefaulthandlerIsNotInMainCollection()
		{
			Assert.That(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
				.Select(x => x.GetType()), Has.No.Member(typeof(DefaultFileTypeHandler)));
		}

		[Test]
		public void CreateWithTestHandlerOnly_DefaulthandlerIsNotInTestCollection()
		{
			Assert.That(ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly().Handlers
				.Select(x => x.GetType()), Has.No.Member(typeof(DefaultFileTypeHandler)));
		}

		[Test]
		public void CreateWithTestHandlerOnly_OnlyOneHandlerIsInTestCollection()
		{
			var handlers = ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly().Handlers;
			Assert.That(handlers, Has.Count.EqualTo(1));
			Assert.That(handlers.Select(x => x.GetType()), Has.Member(typeof(ChorusTestFileHandler)));
		}
	}
}
