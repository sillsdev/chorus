// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using SIL.PlatformUtilities;

namespace Chorus.notes
{
	/// <summary>
	/// This class is a repository for finding a handler which can display additional information
	/// about a message based on some interpretation of a CDATA section in the message.
	/// The only current implementation (MergeConflictEmbeddedMessageContentHandler) uses WinForms
	/// and was therefore moved to Chorus, since it is a current goal that LibChorus should not
	/// reference WinForms.
	/// MergeConflictEmbeddedMessageContentHandler understands the CDATA that is embedded in
	/// MergeConflict notes, and creates a link that offers more details of the conflict.
	/// We use MEF to add an implementation in an assembly which it does not reference.
	/// It also knows about DefaultEmbeddedMessageContentHandler which should come last.
	/// It also finds any implementations in Chorus.exe, if that is found in the same directory.
	///
	/// Note that the handlers are tried in order until we find one which CanHandleUrl for a
	/// particular URL. Thus the order in which they are stored in KnownHandlers is potentially
	/// important. So far, all we need to know is that the Chorus one comes before the default one,
	/// which trivially handles anything. If at some point we have multiple ones in Chorus
	/// (or elsewhere) which could handle the same URLs, we will need to add an ordering
	/// mechanism...either by how we configure MEF, or perhaps by adding a "priority" key to
	/// IEmbeddedMessageContentHandler.
	/// </summary>
	public class EmbeddedMessageContentHandlerRepository
	{
		public EmbeddedMessageContentHandlerRepository()
		{
			var libChorusAssembly = Assembly.GetExecutingAssembly();

			//Set the codebase variable appropriately depending on the OS
			var codeBase = libChorusAssembly.CodeBase.Substring(Platform.IsUnix ? 7 : 8);
			var baseDir = Path.GetDirectoryName(codeBase);

			// REVIEW: for some reason using *.* or *.dll didn't work - creating the catalogs in
			// unit tests (on Linux) failed with FileNotFoundException - don't know why.
			using (var aggregateCatalog = new AggregateCatalog(new DirectoryCatalog(baseDir, "Chorus.exe"),
				new AssemblyCatalog(libChorusAssembly)))
			using (var catalog = new FilteredCatalog(aggregateCatalog,
				def => !def.Metadata.ContainsKey("IsDefault")))
			using (var defaultExportProvider = new CatalogExportProvider(new TypeCatalog(typeof(IEmbeddedMessageContentHandler))))
			{
				using (var container = new CompositionContainer(catalog, defaultExportProvider))
				{
					defaultExportProvider.SourceProvider = container;
					container.ComposeParts(this);
				}
			}
		}

		public IEmbeddedMessageContentHandler GetHandlerOrDefaultForCData(string cDataContent)
		{
			return KnownHandlers.FirstOrDefault(h => h.CanHandleContent(cDataContent));
		}

		public IEmbeddedMessageContentHandler GetHandlerOrDefaultForUrl(Uri uri)
		{
			return KnownHandlers.FirstOrDefault(h => h.CanHandleUrl(uri));
		}

		[ImportMany]
		public IEnumerable<IEmbeddedMessageContentHandler> KnownHandlers { get; private set; }
	}
}

