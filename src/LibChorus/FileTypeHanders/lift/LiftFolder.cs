using System;
using System.Collections.Generic;
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
			config.ExcludePatterns.Add("autoFonts.css");
			config.ExcludePatterns.Add("autoLayout.css");
			config.ExcludePatterns.Add("defaultDictionary.css");
			config.ExcludePatterns.Add("*.old");
			config.ExcludePatterns.Add("*.WeSayUserMemory");
			config.ExcludePatterns.Add("*.tmp");
			config.ExcludePatterns.Add("*.bak");

			config.IncludePatterns.Add("**.lift");
			config.IncludePatterns.Add(".lift-ranges");
			config.IncludePatterns.Add(".ldml");
			config.IncludePatterns.Add("audio/*.*");
			config.IncludePatterns.Add("pictures/*.*");
			config.IncludePatterns.Add("**.css"); //stylesheets
			config.IncludePatterns.Add("**.xml"); //hopefully the days of files ending in "xml" are numbered
			config.IncludePatterns.Add(".hgIgnore");


			config.IncludePatterns.Add("export/*.lpconfig");//lexique pro

			//review (jh,jh): should these only be added when WeSay is the client?  Dunno.
			config.IncludePatterns.Add("**.WeSayConfig");
			config.IncludePatterns.Add("**.WeSayUserConfig");
		}
	}
}
