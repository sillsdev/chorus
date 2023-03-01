using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Chorus.Properties;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;
using Newtonsoft.Json;
using SIL.Code;
using SIL.Progress;

namespace Chorus.Model
{
	public class ServerSettingsModel
	{
		#region static and constant
		private const string LanguageForge = "languageforge.org";
		internal const string ServerEnvVar = "LANGUAGEFORGESERVER";

		/// <remarks>
		/// The leading . (or -) is required because public.languageforge.org (production) must be replaceable by public-qa.languageforge.org (testing).
		/// We join subdomains with hyphens because a service we use (as of 2019) charges exorbitantly for subsubdomains.
		/// </remarks>
		public static string LanguageForgeServer
		{
			get
			{
				var lfServer = Environment.GetEnvironmentVariable(ServerEnvVar);
				return string.IsNullOrEmpty(lfServer) ? $".{LanguageForge}" : lfServer;
			}
		}

		[Obsolete("no known clients")]
		public static bool IsQaServer => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ServerEnvVar));

		public static bool IsPrivateServer { get; set; }

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
			Bandwidths = new[] {new BandwidthItem(BandwidthEnum.High), new BandwidthItem(BandwidthEnum.Low)};
		}
		#endregion static and constant

		private string _pathToRepo;

		public ServerSettingsModel()
		{
			Username = Properties.Settings.Default.LanguageForgeUser;
			Password = DecryptPassword(Properties.Settings.Default.LanguageForgePass);
			RememberPassword = !string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Username);
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
			var urlUsername = WebUtility.UrlDecode(UrlHelper.GetUserName(url));
			if (!string.IsNullOrEmpty(urlUsername))
			{
				Username = urlUsername;
				Password = WebUtility.UrlDecode(UrlHelper.GetPassword(url));
			}
			ProjectId = WebUtility.UrlDecode(UrlHelper.GetPathAfterHost(url));
			HasLoggedIn = !string.IsNullOrEmpty(ProjectId);
			Bandwidth = new BandwidthItem(RepositoryAddress.IsKnownResumableRepository(url) ? BandwidthEnum.Low : BandwidthEnum.High);

			const string languageDepot = "languagedepot.org";
			if (url.Contains(languageDepot))
			{
				url = url.Replace(languageDepot, LanguageForge).Replace("http://", "https://");
			}
			CustomUrl = UrlHelper.StripCredentialsAndQuery(url);
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

				return $"https://{Host}/{WebUtility.UrlEncode(ProjectId)}";
			}
		}

		protected internal string Host => IsCustomUrl
			? UrlHelper.GetHost(CustomUrl)
			: $"{(Bandwidth.Value == BandwidthEnum.Low ? "resumable" : IsPrivateServer ? "hg-private" : "hg-public")}{LanguageForgeServer}";


		public bool HaveGoodUrl
		{
			get
			{
				if (IsCustomUrl)
					return true;

				return !string.IsNullOrEmpty(ProjectId) &&
					   !string.IsNullOrEmpty(Username) &&
					   !string.IsNullOrEmpty(Password);
			}
		}

		public bool RememberPassword { get; set; }

		private string _password;
		public string Password
		{
			get { return _password; }
			set
			{
				_password = value;
				UsernameOrPasswordEdited = true;
			}
		}

		private string _username;
		public string Username
		{
			get { return _username; }
			set
			{
				_username = value;
				UsernameOrPasswordEdited = true;
			}
		}

		public bool IsCustomUrl { get; set; }
		public string CustomUrl { get; set; }
		public BandwidthItem Bandwidth { get; set; } = Bandwidths[0];
		public string ProjectId { get; set; }
		public List<string> AvailableProjects { get; private set; } = new List<string>();

		/// <summary>
		/// True if the user has logged in since this ServerSettingsModel was created, or has already connected this project to an internet server.
		/// </summary>
		private bool _hasLoggedIn = false;
		public bool HasLoggedIn
		{
			get { return _hasLoggedIn; }
			set
			{
				_hasLoggedIn = value;

				if (_hasLoggedIn)
					UsernameOrPasswordEdited = false;
			}
		}

		/// <summary>
		/// User can log in if:
		/// Username and Password are not empty
		/// AND
		/// User has not logged in OR they have logged in but have made addition
		/// edits to either the username or password.
		/// </summary>
		public bool CanLogIn
		{
			get
			{
				return !string.IsNullOrEmpty(Username) &&
					   !string.IsNullOrEmpty(Password) &&
						HasLoggedIn ? UsernameOrPasswordEdited : true;
			}
		}

		/// <summary>
		/// True if the username or password value has been edited since the
		/// last log in.
		/// </summary>
		private bool UsernameOrPasswordEdited { get; set; }

		/// <summary>
		/// Save the settings in the folder's .hg, creating the folder and settings if necessary.
		/// This is only available if you previously called InitFromProjectPath().  It isn't used
		/// in the GetCloneFromInternet scenario.
		/// </summary>
		public void SaveSettings()
		{
			if(string.IsNullOrEmpty(_pathToRepo))
			{
				throw new ArgumentException("SaveSettings() works only if you InitFromProjectPath()");
			}

			var repo = HgRepository.CreateOrUseExisting(_pathToRepo, new NullProgress());

			// Use safer SetTheOnlyAddressOfThisType method, as it won't clobber a shared network setting, if that was the clone source.
			repo.SetTheOnlyAddressOfThisType(CreateRepositoryAddress(AliasName));

			SaveUserSettings();
		}

		protected RepositoryAddress CreateRepositoryAddress(string name)
		{
			return new HttpRepositoryPath(name, URL, false);
		}

		public void SaveUserSettings()
		{
			Properties.Settings.Default.LanguageForgeUser = Username;
			Properties.Settings.Default.LanguageForgePass = RememberPassword ? EncryptPassword(Password) : null;
			PasswordForSession = Password;
			Properties.Settings.Default.Save();
		}

		public void LogIn(out string error)
		{
			try
			{
				var response = LogIn();
				var content = Encoding.UTF8.GetString(WebResponseHelper.ReadResponseContent(response, 300));
				HasLoggedIn = true;
				error = null;
				PasswordForSession = Password;
				// NB: I (Hasso) am tempted here to SaveUserSettings(); however,
				//  - if a user edits the settings for an already-received project, then clicks cancel,
				//		the user would expect all changes to be undone, and
				//  - if a user logs in to get a project from a colleague, the user is likely to click Download, which will save credentials.

				// Do this last so the user is "logged in" even if JSON parsing crashes
				PopulateAvailableProjects(content);
			}
			catch (WebException e)
			{
				HasLoggedIn = false;
				switch ((e.Response as HttpWebResponse)?.StatusCode)
				{
					case HttpStatusCode.NotFound:
					case HttpStatusCode.Forbidden:
						error = LocalizationManager.GetString("ServerSettings.LogIn.BadUserOrPass", "Incorrect username or password");
						if (IsPrivateServer)
						{
							HasLoggedIn = true;
							error = LocalizationManager.GetString("ServerSettings.PossibleErrorPrivateServer",
								"Your projects on the private server could not be listed, but you may still be able to download them.");
						}
						break;
					default:
						error = e.Message;
						break;
				}
			}
			catch (JsonReaderException)
			{
				error = LocalizationManager.GetString("ServerSettings.ErrorListingProjects",
					"Your username and password are correct, but there was an error listing your projects. You can still try to download your project.");
			}
		}

		private WebResponse LogIn()
		{
			var privateQuery = IsPrivateServer ? "?private=true" : string.Empty;
			var request = WebRequest.Create($"https://admin{LanguageForgeServer}/api/user/{Username}/projects{privateQuery}");
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

				return $"languageForge.org [{Bandwidth}]";
			}
		}

		internal static string EncryptPassword(string encryptMe)
		{
			if (string.IsNullOrEmpty(encryptMe))
			{
				return encryptMe;
			}

			// Password encryption/decryption not supported on .NET6 in Linux
			var encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(encryptMe),
				Encoding.Unicode.GetBytes(EntropyValue), DataProtectionScope.CurrentUser);
			return Convert.ToBase64String(encryptedData);
		}

		internal static string DecryptPassword(string decryptMe)
		{
			if (string.IsNullOrEmpty(decryptMe))
			{
				return decryptMe;
			}

			// Password encryption/decryption not supported on .NET6 in Linux
			var decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(decryptMe),
				Encoding.Unicode.GetBytes(EntropyValue), DataProtectionScope.CurrentUser);
			return Encoding.Unicode.GetString(decryptedData);
		}

		/// <summary>
		/// URL-encoded password to use for the current Send and Receive session. <see cref="PasswordForSession"/>
		/// </summary>
		/// <remarks>
		/// UrlEncode encodes spaces as "+" and "+" as "%2b". LanguageDepot fails to decode plus-encoded spaces. Encode spaces as "%20"
		/// </remarks>
		public static string EncodedPasswordForSession => WebUtility.UrlEncode(PasswordForSession)?.Replace("+", "%20");

		/// <summary>
		/// URL-encoded language forge username <see cref="Chorus.Properties.Settings.Default.LanguageForgeUser"/>
		/// </summary>
		public static string EncodedLanguageForgeUser => WebUtility.UrlEncode(Settings.Default.LanguageForgeUser);

		private static string _passwordForSession;

		/// <summary>
		/// The password to use for the current Send and Receive session. Default is the saved password, but it
		/// can be overridden for the current session by setting this property. For example, if the user chooses
		/// not to save the password, it should cached here so the user has to enter the correct password only once.
		/// See https://jira.sil.org/browse/LT-20549
		/// </summary>
		public static string PasswordForSession
		{
			internal get { return _passwordForSession ?? DecryptPassword(Properties.Settings.Default.LanguageForgePass); }
			set { _passwordForSession = value; }
		}

		/// <remarks>
		/// DO NOT USE. Internal for unit tests.
		/// </remarks>>
		internal const string PasswordAsterisks = "********";

		/// <summary>
		/// Removes the password from any URLs to be logged or otherwise displayed to the user, replacing it with asterisks.
		/// </summary>
		/// <param name="clearString">Any string containing a URL with the <see cref="PasswordForSession"/> in clear text.</param>
		internal static string RemovePasswordForLog(string clearString)
		{
			return clearString?.Replace($":{EncodedPasswordForSession}@", $":{PasswordAsterisks}@");
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