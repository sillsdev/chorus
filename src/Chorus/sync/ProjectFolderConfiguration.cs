using System.Collections.Generic;

namespace Chorus.sync
{
	public class ProjectFolderConfiguration
	{
		private List<string> _includePatterns=new List<string>();
		private List<string> _excludePatterns=new List<string>();
		private string _folderPath;

		public ProjectFolderConfiguration(string folderPath)
		{
			FolderPath = folderPath;
		}

		/// <summary>
		/// File Patterns to Add to the repository, unless excluded by ExcludePatterns
		/// </summary>
		/// <example>"LP/*.*"  include all files under the lp directory</example>
		/// <example>"**.lift"  include all lift files, whereever they are found</example>
		public List<string> IncludePatterns
		{
			get { return _includePatterns; }
		}

		/// <summary>
		/// If includePatterns are also specified, these are applied after them.
		/// </summary>
		/// <example>"**/*.bak" </example>
		/// <example>"**/cache" any directory named 'cache'</example>
		public List<string> ExcludePatterns
		{
			get { return _excludePatterns; }
			set { _excludePatterns = value; }
		}

		public string FolderPath
		{
			get { return _folderPath; }
			set { _folderPath = value; }
		}
	}
}