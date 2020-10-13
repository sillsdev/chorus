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
using SIL.ObjectModel;
using SIL.Progress;

namespace Chorus.Model
{
	public class ServerSettingsModel
	{
		internal enum BandwidthEnum
		{
			Low, High
		}

		public class BandwidthItem
		{
			internal BandwidthEnum Value { get; }

			internal BandwidthItem(BandwidthEnum value)
			{
				Value = value;
			}

			public override string ToString()
			{
				return $"{Value} bandwidth";
			}

			// TODO (Hasso) 2020.10: override Equals and GetHashCode?
		}

		private string _pathToRepo;

		public static readonly BandwidthItem[] Bandwidths;

		static ServerSettingsModel()
		{
			Bandwidths = new[] {new BandwidthItem(BandwidthEnum.Low), new BandwidthItem(BandwidthEnum.High)};
		}

		//	Servers.Add("LanguageDepot.org []", new ServerModel("resumable.languagedepot.org"));
		//	Servers.Add("LanguageDepot.org [Safe Mode]", new ServerModel("hg-public.languagedepot.org"));
		//	Servers.Add("LanguageDepot.org [Private Secure + Safe Mode]", new ServerModel("hg-private.languagedepot.org"));
		//	Servers.Add("LanguageForge.org [test server]", new ServerModel("hg.languageforge.org"));

		///<summary>
		/// Show settings for an existing project. The project doesn't need to have any
		/// previous chorus activity (e.g. no .hg folder is needed).
		///</summary>
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
			Password = HttpUtilityFromMono.UrlDecode(UrlHelper.GetPassword(url));
			Username = HttpUtilityFromMono.UrlDecode(UrlHelper.GetUserName(url));
			ProjectId = HttpUtilityFromMono.UrlDecode(UrlHelper.GetPathAfterHost(url));
			CustomUrl = UrlHelper.GetPathOnly(url);
		}

		public string URL
		{
			get
			{
				if (CustomUrlSelected)
				{
					return CustomUrl;
				}

				return "https://" +
					HttpUtilityFromMono.UrlEncode(Username) + ":" +
					HttpUtilityFromMono.UrlEncode(Password) + "@" +
					//"{0}.LanguageForge.org/" +
					"resumable.languagedepot.org/" +
					HttpUtilityFromMono.UrlEncode(ProjectId);
				}
			}


		public bool HaveGoodUrl => true; // TODO
		//{
		//	get
		//	{
		//		if (CustomUrlSelected)
		//			return true;

		//		try
		//		{
		//			return !string.IsNullOrEmpty(ProjectId) &&
		//				   !string.IsNullOrEmpty(Username) &&
		//				   !string.IsNullOrEmpty(Password);
		//		}
		//		catch (Exception)
		//		{
		//			return false;
		//		}
		//	}
		//}

		public string Password { get; set; }
		public string Username { get; set; }
		public bool CustomUrlSelected { get; set; }
		public string CustomUrl { get; set; }
		public BandwidthItem Bandwidth { get; set; } = Bandwidths[0];
		public string ProjectId { get; set; }

		/// <summary>
		/// True if the user has logged in since this ServerSettingsModel was created, or has already connected this project to an internet server.
		/// </summary>
		public bool HasLoggedIn { get; set; }

		public bool CanLogIn => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

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
			get { throw new NotImplementedException(); }
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