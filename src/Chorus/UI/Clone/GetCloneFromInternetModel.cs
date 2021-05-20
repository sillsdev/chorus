using System;
using System.Media;
using System.Threading;
using L10NSharp;
using SIL.Progress;
using SIL.Reporting;
using Chorus.Model;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Clone
{
	public class GetCloneFromInternetModel : InternetCloneSettingsModel
	{
		public GetCloneFromInternetModel() : base()
		{
			ShowCloneOnlyControls = false;
		}

		public GetCloneFromInternetModel(string parentDirectoryToPutCloneIn): base(parentDirectoryToPutCloneIn)
		{
			ShowCloneOnlyControls = true;
		}

		public bool ShowCloneOnlyControls { get; }

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

		///<returns>true if successful; false if failed</returns>
		public override bool SetRepositoryAddress()
		{
			try
			{
				return base.SetRepositoryAddress();
			}
			catch (Exception error)
			{
				_progress.WriteError(error.Message);
				return false;
			}
		}

		public override void DoClone()
		{
			try
			{
				base.DoClone();
				using (var player = new SoundPlayer(Properties.Resources.finishedSound))
				{
					player.PlaySync();
				}
			}
			catch (Exception error)
			{
				using (var player = new SoundPlayer(Properties.Resources.errorSound))
				{
					player.PlaySync();
				}
				if (error is RepositoryAuthorizationException)
				{
					_progress.WriteError(LocalizationManager.GetString("Messages.ServerRejectedLogon", "The server {0} did not accept the request of {1} to clone {2}."), Host, Username, ProjectId);
					ErrorReport.NotifyUserOfProblem(LocalizationManager.GetString("Messages.RejectedLogonDetails", "The server ({0}) rejected the project name ({1}), username ({2}), or password (sorry, it didn't tell us which one). Make sure that each of these is correct, and that '{2}' is a member of the '{1}' project, with permission to read data."),
						Host, ProjectId, Username);
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
	}
}
