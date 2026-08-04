// Copyright (c) 2015-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// Chorus file type handler collection.
	/// </summary>
	public class ChorusFileTypeHandlerCollection
	{
		/// <summary>
		/// When set to a truthy value (1, true, or yes),
		/// <see cref="CreateWithInstalledHandlers"/> throws if no external plugins are found.
		/// </summary>
		public const string kRequirePluginsEnvVarName = "ChorusRequirePlugins";

		private const string PluginSearchPattern = "*-ChorusPlugin.dll";

		/// <summary>
		/// Gets the list of handlers
		/// </summary>
		[ImportMany]
		public IEnumerable<IChorusFileTypeHandler> Handlers { get; private set; }

		private ChorusFileTypeHandlerCollection(
			Expression<Func<ComposablePartDefinition, bool>> filter = null,
			string[] additionalAssemblies = null,
			bool requirePlugins = false)
		{
			requirePlugins = requirePlugins ||
				IsTruthy(Environment.GetEnvironmentVariable(kRequirePluginsEnvVarName));

			using (var aggregateCatalog = new AggregateCatalog())
			{
				aggregateCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
				var pluginCatalog = new DirectoryCatalog(AppContext.BaseDirectory, PluginSearchPattern);
				aggregateCatalog.Catalogs.Add(pluginCatalog);
				if (additionalAssemblies != null)
				{
					foreach (var assemblyPath in additionalAssemblies)
						aggregateCatalog.Catalogs.Add(new AssemblyCatalog(assemblyPath));
				}

				ComposablePartCatalog catalog;
				if (filter != null)
					catalog = new FilteredCatalog(aggregateCatalog, filter);
				else
					catalog = aggregateCatalog;

				using (var container = new CompositionContainer(catalog))
				{
					try
					{
						container.ComposeParts(this);
					}
					catch (ReflectionTypeLoadException ex)
					{
						var loaderExceptions = ex.LoaderExceptions;
						System.Diagnostics.Debug.Fail(
							$"Loading exception: {ex.Message}\r\n{string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message))}");
						throw new AggregateException(ex.Message, loaderExceptions);
					}
				}

				if (requirePlugins &&
					!pluginCatalog.LoadedFiles.Any() &&
					(additionalAssemblies == null || additionalAssemblies.Length == 0))
				{
					throw new InvalidOperationException(
						$"No Chorus plugins matching '{PluginSearchPattern}' were found in '{AppContext.BaseDirectory}'. " +
						$"Set {kRequirePluginsEnvVarName}=0 or pass requirePlugins: false to allow running without plugins.");
				}
			}
		}

		/// <summary/>
		public static ChorusFileTypeHandlerCollection CreateWithInstalledHandlers(
			string[] additionalAssemblies = null,
			bool requirePlugins = false)
		{
			return new ChorusFileTypeHandlerCollection(
				additionalAssemblies: additionalAssemblies,
				requirePlugins: requirePlugins);
		}

		/// <summary/>
		public static ChorusFileTypeHandlerCollection CreateWithTestHandlerOnly()
		{
			return new ChorusFileTypeHandlerCollection(def => def.Metadata.ContainsKey("Scope") &&
				def.Metadata["Scope"].ToString() == "UnitTest");
		}

		/// <summary/>
		public IChorusFileTypeHandler GetHandlerForMerging(string path)
		{
			var handler = Handlers.FirstOrDefault(h => h.CanMergeFile(path));
			return handler ?? new DefaultFileTypeHandler();
		}
		/// <summary/>
		public IChorusFileTypeHandler GetHandlerForDiff(string path)
		{
			var handler = Handlers.FirstOrDefault(h => h.CanDiffFile(path));
			return handler ?? new DefaultFileTypeHandler();
		}
		/// <summary/>
		public IChorusFileTypeHandler GetHandlerForPresentation(string path)
		{
			var handler = Handlers.FirstOrDefault(h => h.CanPresentFile(path));
			return handler ?? new DefaultFileTypeHandler();
		}

		private static bool IsTruthy(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;
			return value.Equals("1", StringComparison.OrdinalIgnoreCase)
				|| value.Equals("true", StringComparison.OrdinalIgnoreCase)
				|| value.Equals("yes", StringComparison.OrdinalIgnoreCase);
		}
	}
}
