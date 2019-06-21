// Copyright (c) 2015 SIL International
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
		/// Gets the list of handlers
		/// </summary>
		[ImportMany]
		public IEnumerable<IChorusFileTypeHandler> Handlers { get; private set; }

		private ChorusFileTypeHandlerCollection(
			Expression<Func<ComposablePartDefinition, bool>> filter = null,
			string[] additionalAssemblies = null)
		{
			using (var aggregateCatalog = new AggregateCatalog())
			{
				aggregateCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
				aggregateCatalog.Catalogs.Add(new DirectoryCatalog(".", "*-ChorusPlugin.dll"));
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
                        System.Diagnostics.Debug.Fail("Loading exception!");
                        throw new AggregateException(ex.Message, loaderExceptions);
                    }
                }
			}
		}

		/// <summary/>
		public static ChorusFileTypeHandlerCollection CreateWithInstalledHandlers(
			string[] additionalAssemblies = null)
		{
			return new ChorusFileTypeHandlerCollection(additionalAssemblies: additionalAssemblies);
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
	}
}

