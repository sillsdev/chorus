using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Newtonsoft.Json;
using SIL.Code;
using SIL.Network;
using SIL.Progress;

namespace Chorus.Model
{
	public class ServerSettingsModel
	{
		#region static and constant
		private const string EntropyValue = "LAMED videte si est dolor sicut dolor meus";

		internal class Project
		{
			public string Identifier { get; set; }
			public string Name { get; set; }
			public string Repository { get; set; }
			public string Role { get; set; }
		}

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

			public override bool Equals(object other)
			{
				return (other as BandwidthItem)?.Value == Value;
			}

			public override int GetHashCode()
			{
				return (int) Value;
			}
		}

		public static readonly BandwidthItem[] Bandwidths;

		static ServerSettingsModel()
		{
			Bandwidths = new[] {new BandwidthItem(BandwidthEnum.Low), new BandwidthItem(BandwidthEnum.High)};
		}
		#endregion static and constant

		private string _pathToRepo;

		public ServerSettingsModel()
		{
			Username = Properties.Settings.Default.LanguageForgeUser;
			Password = DecryptPassword(Properties.Settings.Default.LanguageForgePass);
		}

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
			var urlUsername = HttpUtilityFromMono.UrlDecode(UrlHelper.GetUserName(url));
			if (!string.IsNullOrEmpty(urlUsername))
			{
				Username = urlUsername;
				Password = HttpUtilityFromMono.UrlDecode(UrlHelper.GetPassword(url));
			}
			ProjectId = HttpUtilityFromMono.UrlDecode(UrlHelper.GetPathAfterHost(url));
			Bandwidth = new BandwidthItem(RepositoryAddress.IsKnownResumableRepository(url) ? BandwidthEnum.Low : BandwidthEnum.High);
			CustomUrl = UrlHelper.GetPathOnly(url);
			IsCustomUrl = !UrlHelper.GetHost(url).Equals(Host);
		}

		public string URL
		{
			get
			{
				if (IsCustomUrl)
				{
					return CustomUrl;
				}

				return $"https://{Host}/{HttpUtilityFromMono.UrlEncode(ProjectId)}";
			}
		}

		protected internal string Host => IsCustomUrl
			? UrlHelper.GetHost(CustomUrl)
			: $"{(Bandwidth.Value == BandwidthEnum.Low ? "resumable" : "hg-public")}.languageforge.org";


		public bool HaveGoodUrl => true; // TODO
		//{
		//	get
		//	{
		//		if (IsCustomUrl)
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
		public bool IsCustomUrl { get; set; }
		public string CustomUrl { get; set; }
		public BandwidthItem Bandwidth { get; set; } = Bandwidths[0];
		public string ProjectId { get; set; }
		public List<string> AvailableProjects { get; private set; } = new List<string>();

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
			repo.SetTheOnlyAddressOfThisType(CreateRepositoryAddress(AliasName));

			SaveUserSettings();
		}

		protected RepositoryAddress CreateRepositoryAddress(string name)
		{
			return new HttpRepositoryPath(name, URL, false, Username, Password, Bandwidth.Value == BandwidthEnum.Low);
		}

		private void SaveUserSettings()
		{
			Properties.Settings.Default.LanguageForgeUser = Username;
			Properties.Settings.Default.LanguageForgePass = EncryptPassword(Password);
			Properties.Settings.Default.Save();
		}

		public void LogIn(out string error)
		{
			try
			{
				var response = LogIn();
				var content = Encoding.UTF8.GetString(WebResponseHelper.ReadResponseContent(response));
				HasLoggedIn = true;
				error = null;
				SaveUserSettings();

				// Do this last so, if the JSON is bad, the credentials are still saved
				PopulateAvailableProjects(content);
			}
			catch (WebException e)
			{
				HasLoggedIn = false;
				error = e.Message;
			}
			catch (JsonReaderException)
			{
				error = "Your username and password are correct, but there was an error listing your projects. You can still try to download your project.";
			}
		}

		private WebResponse LogIn()
		{
			var request = WebRequest.Create($"https://admin.languageforge.org/api/user/{Username}/projects");
			request.Method = "POST";
			var passwordBytes = Encoding.UTF8.GetBytes($"password={Password}");
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = passwordBytes.Length;
			var passwordStream = request.GetRequestStream();
			passwordStream.Write(passwordBytes, 0, passwordBytes.Length);
			passwordStream.Close();
			return request.GetResponse();
		}

		internal void PopulateAvailableProjects(string projectsJSON)
		{
			var projects = JsonConvert.DeserializeObject<List<Project>>(projectsJSON) ?? new List<Project>();
			AvailableProjects = projects.Select(p => p.Identifier).ToList();
			AvailableProjects.Sort();
		}

		public string AliasName
		{
			get
			{
				if (IsCustomUrl)
				{
					Uri uri;
					if (Uri.TryCreate(URL, UriKind.Absolute, out uri) && !string.IsNullOrEmpty(uri.Host))
						return uri.Host;
					return "custom";
				}

				return Host;
			}
		}

		internal static string EncryptPassword(string encryptMe)
		{
			if (string.IsNullOrEmpty(encryptMe))
			{
				return encryptMe;
			}
			byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(encryptMe), Encoding.Unicode.GetBytes(EntropyValue), DataProtectionScope.CurrentUser);
			return Convert.ToBase64String(encryptedData);
		}

		internal static string DecryptPassword(string decryptMe)
		{
			if (string.IsNullOrEmpty(decryptMe))
			{
				return decryptMe;
			}
			byte[] decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(decryptMe), Encoding.Unicode.GetBytes(EntropyValue), DataProtectionScope.CurrentUser);
			return Encoding.Unicode.GetString(decryptedData);
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