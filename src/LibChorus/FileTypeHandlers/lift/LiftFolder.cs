using System.IO;
using Chorus.sync;

namespace Chorus.FileTypeHandlers.lift
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
			config.ExcludePatterns.Add(Path.Combine("**", "cache"));
			config.ExcludePatterns.Add(Path.Combine("**", "Cache"));
			config.ExcludePatterns.Add("autoFonts.css");
			config.ExcludePatterns.Add("autoLayout.css");
			config.ExcludePatterns.Add("defaultDictionary.css");
			config.ExcludePatterns.Add("*.old");
			config.ExcludePatterns.Add("*.WeSayUserMemory");
			config.ExcludePatterns.Add("*.tmp");
			config.ExcludePatterns.Add("*.bak");
			config.ExcludePatterns.Add("**.log");
			config.ExcludePatterns.Add("*-ImportLog.htm");
			config.ExcludePatterns.Add(Path.Combine("export", "*.lift"));
			config.ExcludePatterns.Add(Path.Combine("export", "*.plift"));
			config.ExcludePatterns.Add(Path.Combine("export", "*.pdf"));
			config.ExcludePatterns.Add(Path.Combine("export", "*.html"));
			config.ExcludePatterns.Add(Path.Combine("export", "*.odt"));
			config.ExcludePatterns.Add(Path.Combine("export", "*.xml"));
			config.ExcludePatterns.Add("*.ldml"); // Supposed to be in 'WritingSystems' folder now.
			config.ExcludePatterns.Add("*.orig"); // Lift Bridge creates this backup, which ought to be excluded.

			ProjectFolderConfiguration.AddExcludedVideoExtensions(config); // For now at least.

			config.IncludePatterns.Add("*.lift");
			config.IncludePatterns.Add("*.lift-ranges");
			config.IncludePatterns.Add(Path.Combine("audio", "**.*")); // Including nested folders/files
			config.IncludePatterns.Add(Path.Combine("pictures", "**.*")); // Including nested folders/files
			config.IncludePatterns.Add(Path.Combine("others", "**.*")); // Including nested folders/files
			config.IncludePatterns.Add(Path.Combine("SharedSettings", "*.ulsx")); // Including user lexion settings
			config.IncludePatterns.Add(Path.Combine("SharedSettings", "*.plsx")); // Including project lexicon settings
			config.IncludePatterns.Add(Path.Combine("WritingSystems", "*.ldml"));
			config.IncludePatterns.Add("**.xml"); //hopefully the days of files ending in "xml" are numbered
			config.IncludePatterns.Add(".hgIgnore");

			config.IncludePatterns.Add(Path.Combine("export", "*.lpconfig"));//lexique pro
			config.IncludePatterns.Add(Path.Combine("export", "custom*.css")); //stylesheets
			config.IncludePatterns.Add(Path.Combine("export", "multigraphs.txt")); //list of multigraphs

			//review (jh,jh): should these only be added when WeSay is the client?  Dunno.
			config.IncludePatterns.Add("**.WeSayConfig");
			config.IncludePatterns.Add("**.WeSayUserConfig");
		}
	}
}
