using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.sync;

namespace Chorus.FileTypeHanders.lift
{
	/// <summary>
	/// since LIFT is really a folder format, and not just a file format, this class is a placeholder for
	/// future validation stuff. For now, it just has one useful static for describing what are the files
	/// known to be part of the LIFT file format.
	/// </summary>
	public class LiftFolder
	{
		public static void AddLiftFileInfoToFolderConfiguration(ProjectFolderConfiguration config)
		{
			config.ExcludePatterns.Add("**" + Path.DirectorySeparatorChar + "cache");
			config.ExcludePatterns.Add("**" + Path.DirectorySeparatorChar + "Cache");
			config.ExcludePatterns.Add("autoFonts.css");
			config.ExcludePatterns.Add("autoLayout.css");
			config.ExcludePatterns.Add("defaultDictionary.css");
			config.ExcludePatterns.Add("*.old");
			config.ExcludePatterns.Add("*.WeSayUserMemory");
			config.ExcludePatterns.Add("*.tmp");
			config.ExcludePatterns.Add("*.bak");
			config.ExcludePatterns.Add("export" + Path.DirectorySeparatorChar + "*.lift");
			config.ExcludePatterns.Add("*.plift");//normally in /export
			config.ExcludePatterns.Add("*.pdf");//normally in /export
			config.ExcludePatterns.Add("*.html");//normally in /export
			config.ExcludePatterns.Add("*.odt");//normally in /export
			config.ExcludePatterns.Add("*.ldml");

			config.IncludePatterns.Add("*.lift");
			config.IncludePatterns.Add(".lift-ranges");
			//config.IncludePatterns.Add(".ldml");
			config.IncludePatterns.Add("audio" + Path.DirectorySeparatorChar + "*.*");
			config.IncludePatterns.Add("pictures" + Path.DirectorySeparatorChar + "*.*");
			// Not yet config.IncludePatterns.Add("other" + Path.DirectorySeparatorChar + "*.*");
			config.IncludePatterns.Add("WritingSystems" + Path.DirectorySeparatorChar + "*.ldml");
			config.IncludePatterns.Add("export" + Path.DirectorySeparatorChar + "custom*.css"); //stylesheets
			config.IncludePatterns.Add("**.xml"); //hopefully the days of files ending in "xml" are numbered
			config.IncludePatterns.Add(".hgIgnore");

			config.IncludePatterns.Add("export" + Path.DirectorySeparatorChar + "*.lpconfig");//lexique pro

			//review (jh,jh): should these only be added when WeSay is the client?  Dunno.
			config.IncludePatterns.Add("**.WeSayConfig");
			config.IncludePatterns.Add("**.WeSayUserConfig");
		}
	}
}
