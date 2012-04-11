using System;
using System.Collections.Generic;

namespace Chorus.sync
{
	public class ProjectFolderConfiguration
	{
		private List<string> _includePatterns=new List<string>(new []{"*.chorusNotes"});
		private List<string> _excludePatterns = new List<string>(new[] { "~~*.txt" /* for bare folder readme file */});
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
		/// NB: These take precedence over any includePatterns.
		/// </summary>
		/// <example>"**/*.bak" </example>
		/// <example>"**/cache" any directory named 'cache'</example>
		public List<string> ExcludePatterns
		{
			get { return _excludePatterns; }
		 //   set { _excludePatterns = value; }
		}

		public string FolderPath
		{
			get { return _folderPath; }
			set { _folderPath = value; }
		}

		public ProjectFolderConfiguration Clone()
		{
			var clone = new ProjectFolderConfiguration(this._folderPath);
			clone.IncludePatterns.Clear();
			clone.IncludePatterns.AddRange(_includePatterns);
			clone.ExcludePatterns.Clear();
			clone.ExcludePatterns.AddRange(_excludePatterns);
			return clone;
		}
	}
}