using System;
using System.IO;
using System.Text;
using Chorus.UI.Misc;
using Chorus.Utilities;

namespace Chorus.UI.Clone
{
	public class GetCloneFromInternetModel : ServerSettingsModel
	{
		private bool _showCloneSpecificSettings;

		public GetCloneFromInternetModel(string parentDirectoryToPutCloneIn)
		{
			_showCloneSpecificSettings = true;
			ParentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
		}
		public GetCloneFromInternetModel()
		{
			_showCloneSpecificSettings = false;
		}

		public string ParentDirectoryToPutCloneIn { get; set; }


		public override void InitFromUri(string url)
		{
			base.InitFromUri(url);
			LocalFolderName = UrlHelper.GetValueFromQueryStringOfRef(url, "localFolder", string.Empty);
		}

		public bool ReadyToDownload
		{
			get
			{
				return HaveNeededAccountInfo && TargetLocationIsUnused && HaveWellFormedTargetLocation;
			}
		}
		public bool TargetLocationIsUnused
		{
			get
			{
				try
				{
					// the target location is "unused" if either the Target Destination doesn't exist OR
					//  if it has nothing in it (I tried Clone once and it failed because the repo spec
					//  was wrong, but since it had created the Target Destination folder, it wouldn't
					//  try again-rde)
					return Directory.Exists(ParentDirectoryToPutCloneIn) &&
						   (!Directory.Exists(TargetDestination)
						   || (Directory.GetFiles(TargetDestination, "*.*", SearchOption.AllDirectories).Length == 0));
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public bool HaveWellFormedTargetLocation
		{
			get
			{
				return (!string.IsNullOrEmpty(LocalFolderName) && LocalFolderName.LastIndexOfAny(Path.GetInvalidFileNameChars()) == -1);
			}
		}

		public string LocalFolderName { get; set; }

		public string TargetDestination
		{
			get
			{
				if(string.IsNullOrEmpty(LocalFolderName))
				{
					return "";
				}
				return Path.Combine(ParentDirectoryToPutCloneIn, LocalFolderName);
			}
		}


		public bool TargetHasProblem
		{
			get {
				return !TargetLocationIsUnused || !HaveWellFormedTargetLocation;
			}
		}


		public bool ShowCloneOnlyControls
		{
			get { return _showCloneSpecificSettings; }
		}
	}
}
