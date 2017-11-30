using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;
using SIL.Code;
using SIL.Network;
using SIL.Progress;

namespace Chorus.Model
{
	public class ServerModel
	{
		public string DomainName { get; set; }
		public string Protocol { get; set; }

		public ServerModel(string domainName, bool isSecureProtocol = true)
		{
			DomainName = domainName;
			Protocol = isSecureProtocol ? "https" : "http";
		}
	}

	public class ServerSettingsModel
	{
		public readonly Dictionary<string, ServerModel> Servers = new Dictionary<string, ServerModel>();
		private string _pathToRepo;


		public ServerSettingsModel()
		{
			const string languageDepotLabel = "LanguageDepot.org";
			Servers.Add(languageDepotLabel, new ServerModel("resumable.languagedepot.org", false));
			Servers.Add(languageDepotLabel + " [Secure]", new ServerModel("resumable.languagedepot.org"));
			Servers.Add("LanguageDepot.org [Safe Mode]", new ServerModel("hg-public.languagedepot.org", false));
			Servers.Add("LanguageDepot.org [Secure + Safe Mode]", new ServerModel("hg-public.languagedepot.org"));
			Servers.Add("LanguageDepot.org [Private Safe Mode]", new ServerModel("hg-private.languagedepot.org", false));
			Servers.Add("LanguageDepot.org [Private Secure + Safe Mode]", new ServerModel("hg-private.languagedepot.org"));
			Servers.Add("LanguageDepot.org [test server]", new ServerModel("hg.languageforge.org"));

			Servers.Add(LocalizationManager.GetString("Messages.CustomLocation", "Custom Location..."), new ServerModel(""));
			SelectedServerLabel = languageDepotLabel;
		}


		///<summary>
		/// Show settings for an existing project. The project doesn't need to have any
		/// previous chorus activity (e.g. no .hg folder is needed).
		///</summary>
		///<param name="path"></param>
		public virtual void InitFromProjectPath(string path)
		{
			RequireThat.Directory(path).Exists();

			var repo = HgRepository.CreateOrUseExisting(path, new NullProgress());
			_pathToRepo = repo.PathToRepo;

			var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
			if (address != null)
			{
				InitFromUri(address.URI);
			}

			//otherwise, just leave everything in the default state
		}

		public virtual void InitFromUri(string url)
		{
			SetServerLabelFromUrl(url);
			Password = HttpUtilityFromMono.UrlDecode(UrlHelper.GetPassword(url));
			AccountName = HttpUtilityFromMono.UrlDecode(UrlHelper.GetUserName(url));
			ProjectId = HttpUtilityFromMono.UrlDecode(UrlHelper.GetPathAfterHost(url));
			CustomUrl = UrlHelper.GetPathOnly(url);
			//CustomUrlSelected = true;
		}

		private void SetServerLabelFromUrl(string url)
		{
			var host = UrlHelper.GetHost(url).ToLower();
			var pair = Servers.FirstOrDefault((p) => p.Value.DomainName.ToLower() == host);
			if (pair.Key == null)
			{
				SelectedServerLabel = Servers.Last().Key;
			}
			else
			{
				SelectedServerLabel = pair.Key;
			}
		}

		public string NameOfProjectOnRepository
		{
			get
			{
				if (!HaveNeededAccountInfo)
					return string.Empty;
				return ProjectId;
			}
		}

		public string ProjectId { get; set; }

		public string URL
		{
			get
			{
				if (CustomUrlSelected)
				{
					return CustomUrl;
				}

				return SelectedServerModel.Protocol + "://" +
					HttpUtilityFromMono.UrlEncode((string)AccountName) + ":" +
					HttpUtilityFromMono.UrlEncode((string)Password) + "@" + SelectedServerModel.DomainName + "/" +
					HttpUtilityFromMono.UrlEncode(ProjectId);
				}
			}

		public string CustomUrl { get; set; }

		public bool HaveNeededAccountInfo
		{
			get
			{
				if (!NeedProjectDetails)
					return true;

					try
					{
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

		public string Password { get; set; }
		public string AccountName { get; set; }

		public bool HaveGoodUrl
		{
			get { return HaveNeededAccountInfo; }
		}

		public ServerModel SelectedServerModel
		{
			get
			{
				ServerModel serverModel;
				if (Servers.TryGetValue(SelectedServerLabel, out serverModel))
				{
					return serverModel;
				}
				throw new ApplicationException("Somehow SelectedServerLabel was empty, when called from SelectedServerModel.");
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
				ServerModel server;
				if (!Servers.TryGetValue(SelectedServerLabel, out server))
				{
					SelectedServerLabel = Servers.Keys.First();
				}
				return Servers[SelectedServerLabel].DomainName == string.Empty;
			}
		}

		/// <summary>
		/// Save the settings in the folder's .hg, creating the folder and settings if necessary.
		/// This is only available if you previously called InitFromProjectPath().  It isn't used
		/// in the GetCloneFromInternet scenario.
		/// </summary>
		public void SaveSettings()
		{
			if(string.IsNullOrEmpty(_pathToRepo))
			{
				throw new ArgumentException("SaveSettings() only works if you InitFromProjectPath()");
			}

			var repo = HgRepository.CreateOrUseExisting(_pathToRepo, new NullProgress());

			// Use safer SetTheOnlyAddressOfThisType method, as it won't clobber a shared network setting, if that was the clone source.
			repo.SetTheOnlyAddressOfThisType(new HttpRepositoryPath(AliasName, URL, false));
		}

		public string AliasName
		{
			get
			{
				if (CustomUrlSelected)
				{
					Uri uri;
					if (Uri.TryCreate(URL, UriKind.Absolute, out uri) && !String.IsNullOrEmpty(uri.Host))
						return uri.Host;
						return "custom";
					}

					return SelectedServerLabel.Replace(" ","");
				}
			}

		/// <summary>
		/// Use this to make use of, say, the contents of the clipboard (if it looks like a url)
		/// </summary>
		public void SetUrlToUseIfSettingsAreEmpty(string url)
		{
			if (!HaveGoodUrl)
			{
				InitFromUri(url);
			}
		}
	}
}