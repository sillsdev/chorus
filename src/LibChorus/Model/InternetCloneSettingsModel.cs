using System;
using System.IO;
using SIL.Progress;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.Model
{
	public class InternetCloneSettingsModel : ServerSettingsModel
	{
		protected MultiProgress _progress;

		public InternetCloneSettingsModel()
		{
			_progress = new MultiProgress();
		}

		public InternetCloneSettingsModel(string parentDirectoryToPutCloneIn)
		{
			ParentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
			_progress = new MultiProgress();
		}

		public string ParentDirectoryToPutCloneIn { get; set; }

		public override void InitFromUri(string url)
		{
			base.InitFromUri(url);
			LocalFolderName = UrlHelper.GetValueFromQueryStringOfRef(url, @"localFolder", string.Empty);
		}

		public bool ReadyToDownload => HaveGoodUrl && HaveWellFormedTargetLocation && TargetLocationIsUnused;

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
						(!Directory.Exists(TargetDestination) ||
							(Directory.GetFiles(TargetDestination, "*.*", SearchOption.AllDirectories).Length == 0));
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
				return !string.IsNullOrEmpty(LocalFolderName) &&
				       LocalFolderName.LastIndexOfAny(Path.GetInvalidPathChars()) == -1 &&
				       LocalFolderName == LocalFolderName.TrimEnd(); // LT-19858 don't accept spaces at end
			}
		}

		public string LocalFolderName { get; set; }

		public string TargetDestination
		{
			get
			{
				return string.IsNullOrWhiteSpace(LocalFolderName?.TrimEnd()) ?
					string.Empty :
					Path.Combine(ParentDirectoryToPutCloneIn, LocalFolderName.TrimEnd());
			}
		}


		public bool TargetHasProblem
		{
			get {
				return !HaveWellFormedTargetLocation || !TargetLocationIsUnused;
			}
		}

		///<returns>true if successful; false if failed</returns>
		public virtual bool SetRepositoryAddress()
		{
			var repo = new HgRepository(TargetDestination, _progress);
			var name = new Uri(URL).Host;
			if (String.IsNullOrEmpty(name)) //This happens for repos on the local machine
			{
				name = @"LocalRepository";
			}
			if (name.ToLower().Contains(@"languagedepot"))
				name = @"LanguageDepot";

			var address = RepositoryAddress.Create(name, URL);

			//this will also remove the "default" one that hg puts in, which we don't really want.
			repo.SetKnownRepositoryAddresses(new[] { address });
			repo.SetIsOneDefaultSyncAddresses(address, true);
			return true;
		}

		public virtual void DoClone()
		{
			//review: do we need to get these out of the DoWorkEventArgs instead?
			var actualCloneLocation = HgRepository.Clone(CreateRepositoryAddress(URL), TargetDestination, _progress);
			LocalFolderName = Path.GetFileName(actualCloneLocation.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
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
