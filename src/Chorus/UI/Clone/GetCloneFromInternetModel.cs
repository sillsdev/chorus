using System;
using System.IO;
using System.Media;
using System.Threading;
using Chorus.UI.Misc;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using Palaso.Reporting;

namespace Chorus.UI.Clone
{
	public class GetCloneFromInternetModel : ServerSettingsModel
	{
		private bool _showCloneSpecificSettings;
		private MultiProgress _progress;


		public GetCloneFromInternetModel(string parentDirectoryToPutCloneIn)
		{
			_showCloneSpecificSettings = true;
			ParentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
			_progress = new MultiProgress();
		}
		public GetCloneFromInternetModel()
		{
			_showCloneSpecificSettings = false;
			_progress = new MultiProgress();
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
				return HaveNeededAccountInfo && HaveWellFormedTargetLocation && TargetLocationIsUnused;
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
				return (!string.IsNullOrEmpty(LocalFolderName) && LocalFolderName.LastIndexOfAny(Path.GetInvalidPathChars()) == -1);
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
				return !HaveWellFormedTargetLocation || !TargetLocationIsUnused;
			}
		}

		public bool ShowCloneOnlyControls
		{
			get { return _showCloneSpecificSettings; }
		}

		public bool CancelRequested
		{
			get { return _progress.CancelRequested; }
			set { _progress.CancelRequested = value; }
		}

		public IProgressIndicator ProgressIndicator
		{
			get { return _progress.ProgressIndicator; }
			set { _progress.ProgressIndicator = value; }
		}

		public SynchronizationContext UIContext
		{
			get { return _progress.SyncContext; }
			set { _progress.SyncContext = value; }
		}

		///<summary>
		///</summary>
		///<returns>true of successful; false if failed</returns>
		public bool SetRepositoryAddress()
		{
			try
			{
				var repo = new HgRepository(TargetDestination, _progress);
				var name = new Uri(URL).Host;
				if (String.IsNullOrEmpty(name)) //This happens for repos on the local machine
				{
					name = "LocalRepository";
				}
				if (name.ToLower().Contains("languagedepot"))
					name = "LanguageDepot";

				var address = RepositoryAddress.Create(name, URL);

				//this will also remove the "default" one that hg puts in, which we don't really want.
				repo.SetKnownRepositoryAddresses(new[] { address });
				repo.SetIsOneDefaultSyncAddresses(address, true);
				return true;
			}
			catch (Exception error)
			{
				_progress.WriteError(error.Message);
				return false;
			}
		}

		public void DoClone()
		{
			try
			{
				//review: do we need to get these out of the DoWorkEventArgs instead?
				var actualCloneLocation = HgRepository.Clone(new HttpRepositoryPath(URL, URL, false), TargetDestination, _progress);
				LocalFolderName = Path.GetFileName(actualCloneLocation.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
				using (SoundPlayer player = new SoundPlayer(Properties.Resources.finishedSound))
				{
					player.PlaySync();
				}

			}

			catch (Exception error)
			{
				using (SoundPlayer player = new SoundPlayer(Properties.Resources.errorSound))
				{
					player.PlaySync();
				}
				if (error is RepositoryAuthorizationException)
				{
					_progress.WriteError("The server {0} did not accept the reqest of {1} to clone from {2} using password {3}.", SelectedServerPath, AccountName, ProjectId, Password);
					ErrorReport.NotifyUserOfProblem("The server ({0}) rejected the project name ({1}), user name ({2}), or password ({3}) (sorry, it didn't tell us which one). Make sure that each of these is correct, and that '{2}' is a member of the '{1}' project, with permission to read data.",
						SelectedServerPath, ProjectId, AccountName, Password);
				}

				else if (error is HgCommonException)
				{
					_progress.WriteError(error.Message);
					ErrorReport.NotifyUserOfProblem(error.Message);
				}
				else if (error is UserCancelledException)
				{
					_progress.WriteMessage(error.Message);
				}
				else
				{
					_progress.WriteError(error.Message);
				}
			}
		}

		public void Click_FixSettingsButton()
		{
			_progress.CancelRequested = false;
		}

		public void CleanUpAfterErrorOrCancel()
		{
			if (Directory.Exists(TargetDestination))
			{
				Directory.Delete(TargetDestination, true);
			}
		}

		public void AddMessageProgress(IProgress p)
		{
			_progress.AddMessageProgress(p);
		}

		public void AddStatusProgress(IProgress p)
		{
			_progress.AddStatusProgress(p);
		}

		public void AddProgress(IProgress p)
		{
			_progress.Add(p);
		}
	}
}
