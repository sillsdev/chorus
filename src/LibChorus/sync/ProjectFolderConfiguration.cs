using System.Collections.Generic;

namespace Chorus.sync
{
	public class ProjectFolderConfiguration
	{
		public const string BareFolderReadmeFileName = "~~*.txt";
		private readonly List<string> _includePatterns = new List<string>(new[] { "**.ChorusNotes" });
		private readonly List<string> _excludePatterns = new List<string>(new[] { BareFolderReadmeFileName /* for bare folder readme file */, "**.NewChorusNotes" });
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

		public static void EnsureCommonPatternsArePresent(ProjectFolderConfiguration projectFolderConfiguration)
		{
			if (!projectFolderConfiguration._includePatterns.Contains("**.ChorusNotes"))
				projectFolderConfiguration._includePatterns.Add("**.ChorusNotes");
			if (!projectFolderConfiguration._excludePatterns.Contains(BareFolderReadmeFileName))
				projectFolderConfiguration._excludePatterns.Add(BareFolderReadmeFileName);
			if (!projectFolderConfiguration._excludePatterns.Contains("**.NewChorusNotes"))
				projectFolderConfiguration._excludePatterns.Add("**.NewChorusNotes");
		}

		public static void AddExcludedVideoExtensions(ProjectFolderConfiguration projectFolderConfiguration)
		{
			// Exclude these video extensions.
			// One can get a list of all sorts of extensions at: http://www.fileinfo.com/filetypes/video
			foreach (var videoExtension in VideoExtensions)
			{
				projectFolderConfiguration.ExcludePatterns.Add("**." + videoExtension);
			}
		}

		internal static IEnumerable<string> VideoExtensions
		{
			get
			{
				return new List<string>
							{
								"mpa",
								"mpe",
								"mpg",
								"mpeg",
								"mpv2",
								"mp2",
								"mp4",
								"mov",
								"wmv",
								"rm",
								"avi",
								"wvx",
								"m1v"
							};
			}
		}
	}
}