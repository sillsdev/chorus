using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web;
using Chorus.Utilities;

namespace Chorus.UI.Clone
{
	public class GetCloneFromInternetModel
	{
		public readonly Dictionary<string, string> Servers = new Dictionary<string, string>();

		public GetCloneFromInternetModel(string parentDirectoryToPutCloneIn)
		{
			ParentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;

			string languageDepotLabel = "languageDepot.org";
			Servers.Add(languageDepotLabel, "hg-public.languagedepot.org");
			Servers.Add("private.languageDepot.org", "hg-private.languagedepot.org");
			Servers.Add("Custom Location...", "");
			SelectedServerLabel = languageDepotLabel;
		}

		public string ParentDirectoryToPutCloneIn { get; set; }

		public string NameOfProjectOnRepository
		{
			get
			{
				if (!HaveNeededAccountInfo)
					return string.Empty;
				//                var uri = new Uri(URL);
				//                return uri.PathAndQuery.Trim('/').Replace('/', '-').Replace('?', '-').Replace('*', '-').Replace('\\', '-');
				return ProjectId;
			}
		}

		public string ProjectId { get; set; }


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


		public string URL
		{
			get
			{
				if (CustomUrlSelected)
				{
					return CustomUrl;
				}
				else
				{
					return "http://" +
						   HttpUtility.UrlEncode(AccountName) + ":" +
						   HttpUtility.UrlEncode(Password) + "@" + SelectedServerPath + "/" +
						   HttpUtility.UrlEncode(ProjectId);
				}
			}
		}

		public string CustomUrl { get; set; }

		public bool HaveNeededAccountInfo
		{
			get
			{
				if (!NeedProjectDetails)
				{
					return true;
				}
				else
				{
					try
					{
						//                    var uri = new Uri(_urlBox.Text);
						//                    return uri.Scheme =="http" &&
						//                           Uri.IsWellFormedUriString(_urlBox.Text, UriKind.Absolute) &&
						//                           !string.IsNullOrEmpty(uri.PathAndQuery.Trim('/'));
						return !string.IsNullOrEmpty(ProjectId) &&
							   !string.IsNullOrEmpty(AccountName) &&
							   !string.IsNullOrEmpty(Password);
					}
					catch (Exception)
					{
						return false;
					}
				}
			}
		}

		public string Password { get; set; }

		public string AccountName { get; set; }



		public bool TargetHasProblem
		{
			get {
				return !TargetLocationIsUnused || !HaveWellFormedTargetLocation;
			}
		}

		public bool HaveGoodUrl
		{
			get { return HaveNeededAccountInfo; }
		}


		public string SelectedServerPath
		{
			get
			{
				string path;
				if(Servers.TryGetValue(SelectedServerLabel, out path))
				{
					return path;
				}
				throw new ApplicationException("Somehow SelectedServerLabel was empty, when called from SelectedServerPath.");
			}
		}

		public string SelectedServerLabel
		{
			get; set;
		}

		public bool NeedProjectDetails
		{
			get { return !CustomUrlSelected; }
		}

		public bool CustomUrlSelected
		{
			get
			{
				string server;
				if (!Servers.TryGetValue(SelectedServerLabel, out server))
				{
					SelectedServerLabel = Servers.Keys.First();
				}
				return Servers[SelectedServerLabel] == string.Empty;
			}
			private set
			{
				if (value)
				{
					SelectedServerLabel = Servers.First((pair) => pair.Value == string.Empty).Key;
				}
			}
		}

		public void InitFromUri(string url)
		{
			LocalFolderName = UrlHelper.GetValueFromQueryStringOfRef(url, "localFolder", string.Empty);
			CustomUrl = UrlHelper.GetPathOnly(url);
			CustomUrlSelected = true;
		}
	}
}
