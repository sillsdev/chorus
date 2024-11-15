// Copyright (c) 2015-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.audio;
using Chorus.FileTypeHandlers.test;

namespace LibChorus.Tests.FileHandlers
{
	[TestFixture]
	public class ChorusFileTypeHandlerCollectionTests
	{
		private static string BaseDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		private static string SamplePluginPath
		{
			get
			{
				var configOutputDir = Directory.GetParent(BaseDir);
				var outputDir = configOutputDir.Parent.FullName;
				var config = configOutputDir.Name;
				var samplePluginDllPath = Path.Combine(outputDir, "SamplePlugin", config, "net462", "Tests-ChorusPlugin.dll");
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
		public void CreateWithInstalledHandlers_DefaultHandlerIsNotInMainCollection()
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
		public void CreateWithTestHandlerOnly_DefaultHandlerIsNotInTestCollection()
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
