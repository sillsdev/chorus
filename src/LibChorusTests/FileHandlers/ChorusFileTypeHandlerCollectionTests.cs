// Copyright (c) 2015-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.audio;
using Chorus.FileTypeHandlers.test;
using Chorus.Utilities;

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
				var frameworkDir = Path.GetFileName(BaseDir);
				var samplePluginDllPath = Path.Combine(outputDir, "SamplePlugin", config, frameworkDir, "Tests-ChorusPlugin.dll");
				// Tests-ChorusPlugin is net462-only; reuse that build when exercising other TFMs.
				if (!File.Exists(samplePluginDllPath))
					samplePluginDllPath = Path.Combine(outputDir, "SamplePlugin", config, "net462", "Tests-ChorusPlugin.dll");
				return samplePluginDllPath;
			}
		}

		[OneTimeSetUp]
		public void RemoveLeftoverPluginsFromAppBase()
		{
			foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "*-ChorusPlugin.dll"))
			{
				try
				{
					File.Delete(file);
				}
				catch (IOException)
				{
					// Already loaded in this process; throw-when-none tests will Assume away.
				}
			}
		}

		private static void AssumeNoPluginsInAppBase()
		{
			Assume.That(!Directory.GetFiles(AppContext.BaseDirectory, "*-ChorusPlugin.dll").Any(),
				"Plugin already present in app base from a previous run");
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
			Assume.That(RuntimeInformation.FrameworkDescription.Contains(".NET Framework"), "Not running on .NET Framework");
			var handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers(
				new[] { SamplePluginPath }).Handlers;
			Assert.That(handlers.Select(x => x.GetType().Name), Has.Member("TestAFileTypeHandler"));
		}

		[Test]
		[Order(1)]
		public void CreateWithInstalledHandlers_RequirePlugins_ThrowsWhenNoneFound()
		{
			AssumeNoPluginsInAppBase();
			using (new ShortTermEnvironmentalVariable(
				ChorusFileTypeHandlerCollection.kRequirePluginsEnvVarName, null))
			{
				Assert.That(
					() => ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers(requirePlugins: true),
					Throws.TypeOf<InvalidOperationException>()
						.With.Message.Contains("*-ChorusPlugin.dll")
						.And.Message.Contains(AppContext.BaseDirectory));
			}
		}

		[Test]
		[Order(1)]
		public void CreateWithInstalledHandlers_RequirePlugins_ThrowsWhenEnvVarSet()
		{
			AssumeNoPluginsInAppBase();
			using (new ShortTermEnvironmentalVariable(
				ChorusFileTypeHandlerCollection.kRequirePluginsEnvVarName, "true"))
			{
				Assert.That(
					() => ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers(),
					Throws.TypeOf<InvalidOperationException>());
			}
		}

		[Test]
		[Order(1)]
		public void CreateWithInstalledHandlers_RequirePlugins_SucceedsWithAdditionalAssembly()
		{
			Assume.That(File.Exists(SamplePluginPath), $"Sample plugin not found at {SamplePluginPath}");
			using (new ShortTermEnvironmentalVariable(
				ChorusFileTypeHandlerCollection.kRequirePluginsEnvVarName, null))
			{
				Assert.That(
					() => ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers(
						new[] { SamplePluginPath }, requirePlugins: true),
					Throws.Nothing);
			}
		}

		[Test]
		[Order(2)]
		public void CreateWithInstalledHandlers_FindsPluginsWhenCwdDiffersFromAppBase()
		{
			Assume.That(File.Exists(SamplePluginPath), $"Sample plugin not found at {SamplePluginPath}");

			var samplePluginPathname = Path.Combine(AppContext.BaseDirectory, "Tests-ChorusPlugin.dll");
			if (!File.Exists(samplePluginPathname))
			{
				File.Copy(SamplePluginPath, samplePluginPathname, true);
			}

			var originalCwd = Directory.GetCurrentDirectory();
			var tempDir = Path.Combine(Path.GetTempPath(), "ChorusPluginDiscoveryCwdTest");
			Directory.CreateDirectory(tempDir);
			try
			{
				Directory.SetCurrentDirectory(tempDir);
				using (new ShortTermEnvironmentalVariable(
					ChorusFileTypeHandlerCollection.kRequirePluginsEnvVarName, null))
				{
					var handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers(
						requirePlugins: true).Handlers;
					Assert.That(handlers.Select(x => x.GetType().Name), Has.Member("TestAFileTypeHandler"));
				}
			}
			finally
			{
				Directory.SetCurrentDirectory(originalCwd);
			}
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
