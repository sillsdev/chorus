using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using Chorus.Utilities;
using Chorus.merge;
using Chorus.Model;
using Chorus.Properties;
using L10NSharp;
using Nini.Ini;
using SIL.Code;
using SIL.IO;
using SIL.Network;
using SIL.PlatformUtilities;
using SIL.Progress;

namespace Chorus.VcsDrivers.Mercurial
{

	public class HgRepository : IRetrieveFileVersionsFromRepository
	{
		private readonly string _pathToRepository;
		private string _userName;
		private IProgress _progress;
		private Dictionary<string, ExecutionResult> _hgLogCache;
		public int SecondsBeforeTimeoutOnLocalOperation = 15 * 60;
		public int SecondsBeforeTimeoutOnMergeOperation = 15 * 60;
		public const int SecondsBeforeTimeoutOnRemoteOperation = 40 * 60;
		private bool _haveLookedIntoProxySituation;
		private string _proxyCongfigParameterString = string.Empty;
		private bool _hgrcUpdateNeeded;
		private static bool _alreadyCheckedMercurialIni;
		internal const string EmptyRepoIdentifier = "0000000000000000000000000000000000000000";
		/// <summary>
		/// Template to produce a consistent and parseable revision log entry
		/// </summary>
		private const string DetailedRevisionTemplate = "--template \"changeset:{rev}:{node|short}\nbranch:{branches}\nuser:{author}\ndate:{date|rfc822date}\ntag:{tags}\nsummary:{desc}\n\"";
		private bool _mercurialTwoCompatible;
		private HgModelVersionBranch _branchHelper;

		public static string GetEnvironmentReadinessMessage(string messageLanguageId)
		{
			try
			{
				HgRunner.Run("hg version", Environment.CurrentDirectory, 5, new NullProgress());
			}
			catch (Exception)
			{
				return "Chorus requires the Mercurial version control system.  It must be installed and part of the PATH environment variable.";
			}
			return null;
		}

		public HgModelVersionBranch BranchingHelper
		{
			get { return _branchHelper ?? (_branchHelper = new HgModelVersionBranch(this, _progress)); }
		}

		/// <exception cref="Exception">This will throw when the hgrc is locked</exception>
		public RepositoryAddress GetDefaultNetworkAddress<T>() where T : RepositoryAddress
		{
			//the first one found in the default list that is of the requisite type and is NOT a 'default'
			// path (inserted by the Hg Clone process). See https://trello.com/card/send-receive-dialog-displays-default-as-a-configured-local-network-location-for-newly-obtained-projects/4f3a90277ae2b69b010988ac/37
			// This could be a problem if there was some way for the user to create a 'default' path, but the paths we want
			// to find here are currently always named with an adaptation of the path. I don't think that process can produce 'default'.
			var paths = GetRepositoryPathsInHgrc();
			var networkPaths = paths.Where(p => p is T && p.Name != "default").ToArray();

			//none found in the hgrc
			if (!networkPaths.Any()) //nb: because of lazy eval, the hgrc lock exception can happen here
				return null;


			var defaultAliases = GetDefaultSyncAliases();

			foreach (var path in networkPaths)
			{
				//avoid "access to modified closure"
				var pathName = path.Name;
				if (defaultAliases.Any(a => a == pathName))
					return path;
			}
			return networkPaths.First();
		}

		/// <summary>
		/// Given a file path or directory path, create (or use existing) repository at this location.
		/// </summary>
		/// <returns></returns>
		public static HgRepository CreateOrUseExisting(string startingPointForPathSearch, IProgress progress)
		{
			Guard.AgainstNullOrEmptyString(startingPointForPathSearch, "startingPointForPathSearch");
			Guard.Against(!Directory.Exists(startingPointForPathSearch) && !File.Exists(startingPointForPathSearch), "File or directory wasn't found");

				/*
				 I'm leaning away from this intervention at the moment.
					string newRepositoryPath = AskUserForNewRepositoryPath(startingPath);

				 Let's see how far we can get by just silently creating it, and leave it to the future
				 or user documentation/training to know to set up a repository at the level they want.
				*/
			var newRepositoryPath = startingPointForPathSearch;
			if (File.Exists(startingPointForPathSearch))
				newRepositoryPath = Path.GetDirectoryName(startingPointForPathSearch);

			if (Directory.Exists(Path.Combine(newRepositoryPath, ".hg")))
				return new HgRepository(newRepositoryPath, progress);

			var hg = CreateRepositoryInExistingDir(newRepositoryPath, progress);

			//review: Machine name would be more accurate, but most people have, like "Compaq" as their machine name
			//but in any case, this is just a default until they set the name explicity
			hg.SetUserNameInIni(Environment.UserName, progress);
			return hg;
		}

		public HgRepository(string pathToRepository, IProgress progress) : this(pathToRepository, true, progress)
		{
		}

		public HgRepository(string pathToRepository, bool updateHgrc, IProgress progress)
		{
			Guard.AgainstNull(progress, "progress");
			_pathToRepository = pathToRepository;
			_hgLogCache = new Dictionary<string, ExecutionResult>();

			// make sure it exists
			if (GetIsLocalUri(_pathToRepository) && !Directory.Exists(_pathToRepository))
				Directory.CreateDirectory(_pathToRepository);

			_progress = progress;

			_userName = GetUserIdInUse();

			_mercurialTwoCompatible = true;
			_hgrcUpdateNeeded = updateHgrc;
			var timeoutOverride = Environment.GetEnvironmentVariable("CHORUS_LOCAL_TIMEOUT_SECONDS");
			int timeoutValue;
			if (int.TryParse(timeoutOverride, out timeoutValue))
			{
				SecondsBeforeTimeoutOnLocalOperation = timeoutValue;
			}
		}

		/// <summary>
		/// put anything in the hgrc that chorus requires
		/// Note: Maybe we could ship the mercurial.ini separately, some how.... there is some value in modifying the hgrc itself, since that way technians doing
		/// hg stuff by hand will get the right extensions in play.
		/// </summary>
		internal void CheckAndUpdateHgrc()
		{
			CheckMercurialIni();
			if (!_hgrcUpdateNeeded)
				return;

			try
			{
				lock(_pathToRepository) // Avoid crash if two threads try to Sync simultaneously
				{
					EnsureChorusMergeAddedToHgrc();
					EnsureCacertsIsSet();
					var extensions = HgExtensions;
					EnsureTheseExtensionsAndFormatSet(extensions);
					_hgrcUpdateNeeded = false;
				}
			}
			catch (Exception error)
			{
				throw new ApplicationException($"Failed to set up extensions for the repository: {error.Message}", error);
			}
		}

		internal static Dictionary<string, string> HgExtensions
		{
			get
			{
				/*
					fixutf8 makes it possible to have unicode characters in path names. Note that it is prone to break with new versions of mercurial.
					We currently have one tweaked to work with all tested versions of Mercurial 3
					When updating you will likely have to modify our version since people who need this use git now.
					Note too that to make use of this in a cmd window, first set font to consolas (more characters)
					and change the codepage to utf with "chcp 65001"
				*/

				//NB: this is REQUIRED because we are now, in the hgrunner, saying that we will be getting utf8 output. If we made this extension optional, we'd have to know to not say that.

				var extensions = new Dictionary<string, string>();
				extensions.Add("eol", ""); //for converting line endings
				extensions.Add("hgext.graphlog", ""); //for more easily readable diagnostic logs
				extensions.Add("convert", ""); //for catastrophic repair in case of repo corruption
				string fixUtfFolder = FileLocationUtilities.GetDirectoryDistributedWithApplication(false, "MercurialExtensions", "fixutf8");
				if(!string.IsNullOrEmpty(fixUtfFolder))
					extensions.Add("fixutf8", Path.Combine(fixUtfFolder, "fixutf8.py"));
				return extensions;
			}
		}

		private static void CheckMercurialIni()
		{
			if(_alreadyCheckedMercurialIni)
				return;

			try
			{
				var extensions = HgExtensions;
				var doc = GetMercurialConfigInMercurialFolder();
				if(!CheckExtensions(doc, extensions))
				{
					// Maybe we are running in a test environment, so attempt to write a correct
					// mercurial.ini file for this environment.
					// Note that we would not succeed in a installed environment, the installer
					// should have already set this correctly, so that the check above would pass.
					// review: Is there a better way to do this, so that this test only code is not
					// included in the main code? CP 2012-04
					SetExtensions(doc, HgExtensions);
					try
					{
						doc.Save();
					}
					// ReSharper disable EmptyGeneralCatchClause
					catch(Exception)
					{
					}
					// ReSharper restore EmptyGeneralCatchClause
					doc = GetMercurialConfigInMercurialFolder();
					if(!CheckExtensions(doc, extensions))
					{
						throw new ApplicationException(
							"The mercurial.ini file shipped with this application does not have the fixutf8 extension enabled."
							);
					}
				}
				_alreadyCheckedMercurialIni = true;
			}
			catch(Exception error)
			{
				throw new ApplicationException(string.Format("Failed to set up extensions: {0}", error.Message),
					error);
			}
		}

		private void EnsureChorusMergeAddedToHgrc()
		{
			var mergetoolname = "chorusmerge";
			var doc = GetMercurialConfigForRepository();
			var chorusMergeLoc = SurroundWithQuotes(ExecutionEnvironment.ChorusMergeFilePath());
			var uiSection = doc.Sections.GetOrCreate("ui");

			uiSection.Set("merge", mergetoolname);
			var mergeToolsSection = doc.Sections.GetOrCreate("merge-tools");
			// If the premerge is allowed to happen Mercurial will occasionally think it did a good enough job and not
			// call our mergetool. This has data corrupting results for us so we tell mercurial to skip it.
			mergeToolsSection.Set(string.Format("{0}.premerge", mergetoolname), "False");
			mergeToolsSection.Set(string.Format("{0}.executable", mergetoolname), chorusMergeLoc);
			doc.SaveAndThrowIfCannot();
		}

		public bool GetFileIsInRepositoryFromFullPath(string fullPath)
		{
			if (fullPath.IndexOf(_pathToRepository) < 0)
			{
				throw new ArgumentException(
					string.Format("GetFileIsInRepositoryFromFullPath() requies the argument {0} be a child of the root {1}",
					fullPath,
					_pathToRepository));

			}
			string subPath = fullPath.Replace(_pathToRepository, "");
			if (subPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
			{
				subPath = subPath.Remove(0, 1);
			}
			return GetFileExistsInRepo(subPath);
		}

		/// <returns>true if changes were received</returns>
		public bool Pull(RepositoryAddress source, string targetUri)
		{
			_progress.WriteMessage("Receiving any changes from {0}", source.Name);
			_progress.WriteVerbose("({0} is {1})", source.Name, ServerSettingsModel.RemovePasswordForLog(targetUri));
				CheckAndUpdateHgrc();

			bool result;
			var transport = CreateTransportBetween(source, targetUri);
			result = transport.Pull();
			return result;
		}

		public void PushToTarget(string targetLabel, string targetUri)
		{
			try
			{
				CheckAndUpdateHgrc();
				Execute(false, _mercurialTwoCompatible,SecondsBeforeTimeoutOnRemoteOperation, "push --new-branch " + GetProxyConfigParameterString(targetUri), SurroundWithQuotes(targetUri));
			}
			catch (Exception err)
			{
				_progress.WriteMessageWithColor("OrangeRed",
					$"Could not send to {ServerSettingsModel.RemovePasswordForLog(targetUri)}{Environment.NewLine}{err.Message}");
			}

			if (GetIsLocalUri(targetUri))
			{
				try
				{
					Execute(SecondsBeforeTimeoutOnLocalOperation, "update", "-C"); // for usb keys and other local repositories
				}
				catch (Exception err)
				{
					_progress.WriteMessageWithColor("OrangeRed",
						$"Could not update the actual files after a pushing to {ServerSettingsModel.RemovePasswordForLog(targetUri)}{Environment.NewLine}{err.Message}");
				}
			}
		}

		public bool PullFromTarget(string targetLabel, string targetUri)
		{
				CheckAndUpdateHgrc();
			try
			{
				var tip = GetTip();
				Execute(SecondsBeforeTimeoutOnRemoteOperation, "pull" + GetProxyConfigParameterString(targetUri), SurroundWithQuotes(targetUri));

				var newTip = GetTip();
				if (tip == null)
					return newTip != null;
				return tip.Number.Hash != newTip.Number.Hash;
				//review... I believe you can't pull without getting a new tip
			}
			catch (Exception error)
			{
				var targetUriForLog = ServerSettingsModel.RemovePasswordForLog(targetUri);
				_progress.WriteWarning("Could not receive from " + targetLabel);
				var specificError = error;
				if (UriProblemException.ErrorMatches(error))
				{
					specificError = new UriProblemException(targetUriForLog);
				}
				else if (ProjectLabelErrorException.ErrorMatches(error))
				{
					specificError = new ProjectLabelErrorException(targetUriForLog);
				}
				else if(UnrelatedRepositoryErrorException.ErrorMatches(error))
				{
					specificError = new UnrelatedRepositoryErrorException(targetUriForLog);
				}
				else if (FirewallProblemSuspectedException.ErrorMatches(error))
				{
					specificError = new FirewallProblemSuspectedException();
				}
				else if (ServerErrorException.ErrorMatches(error))
				{
					specificError = new ServerErrorException();
				}
				else if (PortProblemException.ErrorMatches(error))
				{
					specificError = new PortProblemException(targetUriForLog);
				}
				else if (RepositoryAuthorizationException.ErrorMatches(error))
				{
					specificError = new RepositoryAuthorizationException();
				}
				throw specificError;
			}
		}

		private IHgTransport CreateTransportBetween(RepositoryAddress source, string targetUri)
		{
			if (source.IsResumable)
			{
				_progress.WriteVerbose("Initiating Resumable Transport");
				return new HgResumeTransport(this, source.Name, new HgResumeRestApiServer(targetUri), _progress);
			}
			_progress.WriteVerbose("Initiating Normal Transport");
			return new HgNormalTransport(this, source.Name, targetUri, _progress);
		}

		/// <summary>
		/// Gives an id string which is unique to this repository, but shared across all clones of it.  Can be used to identify relatives in crowd.
		/// </summary>
		public string Identifier
		{
			get
			{
				// Or: id -i -r0 for short id
				var results = Execute(SecondsBeforeTimeoutOnLocalOperation, "log -r0 --template " + SurroundWithQuotes("{node}"));
				// NB: This may end with a new line (&#xA; entity in xml).
				// It could possibly have multiple lines, in which case, we want the last one.
				// Earlier ones may be coming from some other version of Hg that complains about deprecated extensions Chorus uses.
				var id = results.StandardOutput;
				if (string.IsNullOrEmpty(id) || id.Trim().Equals(EmptyRepoIdentifier))
					return null;
				var split = id.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
				id = split.Length == 0 ? null : split[split.Length - 1]; // Get last one.

				return id;
			}
		}

		public void Push(RepositoryAddress source, string targetUri)
		{
			_progress.WriteMessage("Sending changes to {0}", source.Name);
			_progress.WriteVerbose("({0} is {1})", source.Name, ServerSettingsModel.RemovePasswordForLog(targetUri));
			CheckAndUpdateHgrc();

			var transport = CreateTransportBetween(source, targetUri);
			transport.Push();
		}

		private static bool GetIsLocalUri(string uri)
		{
			return !(uri.StartsWith("http") || uri.StartsWith("ssh"));
		}

		public Revision GetTip()
		{
			var rev = GetRevisionsFromQuery("tip " + DetailedRevisionTemplate).FirstOrDefault();
			if (rev == null || rev.Number.LocalRevisionNumber == "-1")
				return null;
			return rev;
		}

		public List<Revision> GetHeads()
		{
			_progress.WriteVerbose("Getting heads of {0}", _userName);
			return GetRevisionsFromQuery("heads " + DetailedRevisionTemplate);
		}

		public bool MakeBundle(string[] baseRevisions, string filePath)
		{
			string command;
			if (baseRevisions.Length == 0 || baseRevisions.Contains("0")) // empty list or "0" means "all revisions"
			{
				command = string.Format("bundle --all \"{0}\"", filePath);
			}
			else
			{
				var revisionFlags = @"";
				foreach (var baseRevision in baseRevisions)
				{
					revisionFlags += string.Format(@"--base {0} ", baseRevision);
				}
				command = string.Format("bundle {0} \"{1}\"", revisionFlags, filePath);
			}

			string result = GetTextFromQuery(command);
			//_progress.WriteVerbose("While creating bundle at {0} with base {1}: {2}", filePath, baseRevision, result.Trim());
			var theFile = new FileInfo(filePath);
			if (theFile.Exists && theFile.Length > 0 || result.Contains("no changes found"))
			{
				return true;
			}
			return false;
		}

		internal string GetTextFromQuery(string query)
		{
			return GetTextFromQuery(query, SecondsBeforeTimeoutOnLocalOperation);
		}

		private string GetTextFromQuery(string query, int secondsBeforeTimeoutOnLocalOperation)
		{
			var result = ExecuteErrorsOk(query, secondsBeforeTimeoutOnLocalOperation);

			var standardOutputText = result.StandardOutput.Trim();
			if (!string.IsNullOrEmpty(standardOutputText))
			{
				_progress.WriteVerbose(standardOutputText);
			}

			var standardErrorText = result.StandardError.Trim();
			if (!string.IsNullOrEmpty(standardErrorText))
			{
				_progress.WriteError(standardErrorText);
			}

			if (GetHasLocks())
			{
				_progress.WriteWarning("Hg Command {0} left lock", query);
			}

			return result.StandardOutput;
		}

		/// <summary>
		/// Method only for testing.
		/// </summary>
		/// <param name="filePath"></param>
		public void TestOnlyAddSansCommit(string filePath)
		{
			TrackFile(filePath);
		}

		public void AddAndCheckinFile(string filePath)
		{
			TrackFile(filePath);
			Commit(_mercurialTwoCompatible, " Add " + Path.GetFileName(filePath));
		}

		private void TrackFile(string filePath)
		{
			CheckAndUpdateHgrc();
			_progress.WriteVerbose("Adding {0} to the files that are tracked for {1}: ", Path.GetFileName(filePath),
								   _userName);
			Execute(SecondsBeforeTimeoutOnLocalOperation, "add", SurroundWithQuotes(filePath));
		}

		public virtual void Commit(bool forceCreationOfChangeSet, string message, params object[] args)
		{
			CheckAndUpdateHgrc();
			message = string.Format(message, args);
			_progress.WriteVerbose("{0} committing with comment: {1}", _userName, message);
			ExecutionResult result = Execute(false, _mercurialTwoCompatible,SecondsBeforeTimeoutOnLocalOperation, "ci", "-u " + SurroundWithQuotes(_userName), "-m " + SurroundWithQuotes(message));
			_progress.WriteVerbose(result.StandardOutput);
		}

		///<summary>
		/// Tell Hg to forget the specified file, so it won't track it anymore.
		///</summary>
		/// <remarks>
		/// 'forget' will mark it as deleted in the repo (keeping its history, of course),
		/// but it will leave it in the user's workspace.
		/// </remarks>
		public void ForgetFile(string filepath)
		{
			CheckAndUpdateHgrc();
			_progress.WriteWarning("{0} is removing {1} from system. The file will remain in the history and on disk.", _userName, Path.GetFileName(filepath));
			Execute(SecondsBeforeTimeoutOnLocalOperation, "forget ", SurroundWithQuotes(filepath));
		}

		public ExecutionResult Execute(int secondsBeforeTimeout, string cmd, params string[] rest)
		{
			return Execute(false, _mercurialTwoCompatible, secondsBeforeTimeout, cmd, rest);
		}

		protected ExecutionResult Execute(bool failureIsOk, int secondsBeforeTimeout, string cmd, params string[] rest)
		{
			return Execute(failureIsOk, _mercurialTwoCompatible, secondsBeforeTimeout, cmd, rest);
		}

		/// <summary>
		///
		/// </summary>
		/// <exception cref="System.TimeoutException"/>
		/// <returns></returns>
		private ExecutionResult Execute(bool failureIsOk, bool noChangeIsOk, int secondsBeforeTimeout, string cmd, params string[] rest)
		{
			if(_progress.CancelRequested)
			{
				throw new UserCancelledException();
			}
			var b = new StringBuilder();
			b.Append(cmd + " ");
			foreach (var s in rest)
			{
				b.Append(s + " ");
			}
			var hgCmdArgs = b.ToString();

			ExecutionResult result = ExecuteErrorsOk(hgCmdArgs, secondsBeforeTimeout);
			if (HgProcessOutputReader.kCancelled == result.ExitCode)
			{
				_progress.WriteWarning("User Cancelled");
				return result;
			}
			if (0 != result.ExitCode && !failureIsOk && !(1 == result.ExitCode && noChangeIsOk))
			{
				var detailsBuilder = new StringBuilder().AppendLine().AppendLine("hg command was")
					.AppendLine(ServerSettingsModel.RemovePasswordForLog(hgCmdArgs));
				try
				{
					var versionInfo = GetTextFromQuery("version", secondsBeforeTimeout);
					//trim the verbose copyright stuff
					versionInfo = versionInfo.Substring(0, versionInfo.IndexOf("Copyright", StringComparison.Ordinal));
					detailsBuilder.Append($"hg version is {versionInfo}");
				}
				catch (Exception)
				{
					detailsBuilder.Append("Could not get HG VERSION");
				}


				if (string.IsNullOrEmpty(result.StandardError))
				{
					throw new ApplicationException(detailsBuilder.Insert(0, result.ExitCode).Insert(0, "Got return value ").ToString());
				}
				if (result.StandardError.Contains(@"unresolved merge conflicts"))
				{
					return RecoverFromFailedMerge(failureIsOk, secondsBeforeTimeout, cmd, rest);
				}
				if(result.StandardError.Contains(@"interrupted"))
				{
					return RecoverFromInterruptedUpdate(failureIsOk, secondsBeforeTimeout, cmd, rest);
				}
				if (result.StandardError.Contains("No such file or directory"))// trying to track down http://jira.palaso.org/issues/browse/BL-284
				{
					detailsBuilder.Append(SafeGetStatus());
				}
				throw new ApplicationException(detailsBuilder.Insert(0, result.StandardError).ToString());

			}
			return result;
		}

		/// <summary>
		/// The procedure for recovering from an interrupted update is to update again.
		/// </summary>
		private ExecutionResult RecoverFromInterruptedUpdate(bool failureIsOk, int secondsBeforeTimeout, string cmd, string[] rest)
		{
			Update();
			return Execute(failureIsOk, secondsBeforeTimeout, cmd, rest);
		}

		/// <summary>
		/// Attempt to recover from a failed merge by asking mercurial to try again.
		/// </summary>
		/// <remarks>
		/// A failed merge means that not all of the files with conflicts were successfully resolved for some reason.
		/// Candidates include power failure during the merge, system crash during the merge, impatient user killing processes during the merge.
		/// Another likely cause is a crash in client code run by chorus merge and trying to recover will not succeed until a new version of
		/// the client is shipped.
		///</remarks>
		private ExecutionResult RecoverFromFailedMerge(bool failureIsOk, int secondsBeforeTimeout, string cmd, string[] rest)
		{
			var heads = GetHeads();
			if(heads.Count() > 2)
			{
				throw new ApplicationException("Unable to recover from failed merge: [Too many heads in the repository]");
			}
			// set the environment variables necessary for ChorusMerge to retry the merge
			MergeSituation.PushRevisionsToEnvironmentVariables(heads[0].UserId, heads[0].Number.Hash,
																heads[1].UserId, heads[1].Number.Hash);

			MergeOrder.PushToEnvironmentVariables(_pathToRepository);
			using(new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName,
				MergeOrder.ConflictHandlingModeChoices.WeWin.ToString()))
			{
				_progress.WriteMessageWithColor(@"Blue", "Attempting to recover from failed merge.");
				var result = Execute(true, SecondsBeforeTimeoutOnMergeOperation, "resolve", "--all");
				if(!string.IsNullOrEmpty(result.StandardError))
				{
					throw new ApplicationException($"Unable to recover from failed merge: [{result.StandardError}]");
				}
			}
			_progress.WriteMessageWithColor(@"Green", "Successfully recovered from failed merge.");
			return Execute(failureIsOk, secondsBeforeTimeout, cmd, rest);
		}

		private string SafeGetStatus()
		{
			try
			{
				return Environment.NewLine + "Status:" + Environment.NewLine + (GetTextFromQuery("status"));
			}
			catch (Exception e)
			{
#if DEBUG
				throw e;
#else
				//else swallow
				return "Error in SafeGetStatus(): " + e.Message;
#endif
			}
		}

		/// <exception cref="System.TimeoutException"/>
		private ExecutionResult ExecuteErrorsOk(string command, int secondsBeforeTimeout)
		{
			if (_progress.CancelRequested)
			{
				throw new UserCancelledException();
			}

			var commandWithoutPasswordForLog = ServerSettingsModel.RemovePasswordForLog(command);

#if DEBUG
			if (GetHasLocks(_pathToRepository, _progress))
			{
				_progress.WriteWarning("Found a lock before executing: {0}.", commandWithoutPasswordForLog);
			}
#endif

			ExecutionResult result;
			// The only commands safe to cache are `hg log -rREVNUM --template "{node}"`, as their output never changes for a given repo.
			// There are two exceptions to this: `hg log` commands requesting a negative revision number, which means "N revisions back from tip",
			// and cases where the result is an all-zero identifier, which usually means the repo is empty and that will change soon.
			if (command.StartsWith("log -r") && !command.StartsWith("log -r-") && command.Trim().EndsWith("--template \"{node}\"")) {
				if (_hgLogCache.TryGetValue(command, out result)) {
					_progress.WriteVerbose("Using cached result: " + commandWithoutPasswordForLog);
				} else {
					_progress.WriteVerbose("Executing and caching: " + commandWithoutPasswordForLog);
					result = HgRunner.Run("hg " + command, _pathToRepository, secondsBeforeTimeout, _progress);
					if (!string.IsNullOrEmpty(result.StandardOutput) && result.StandardOutput.StartsWith(EmptyRepoIdentifier)) {
						_progress.WriteVerbose("Not caching an all-zero result");
					} else {
						_hgLogCache[command] = result;
					}
				}
			} else {
				_progress.WriteVerbose("Executing: " + commandWithoutPasswordForLog);
				result = HgRunner.Run("hg " + command, _pathToRepository, secondsBeforeTimeout, _progress);
			}
			if (result.DidTimeOut)
			{
				throw new TimeoutException(result.StandardError);
			}
			if (result.UserCancelled)
			{
				throw new UserCancelledException();
			}
			if (!string.IsNullOrEmpty(result.StandardError))
			{
				_progress.WriteVerbose("standerr: " + result.StandardError);//not necessarily an *error* down this deep
			}
			if (!string.IsNullOrEmpty(result.StandardOutput))
			{
				_progress.WriteVerbose("standout: " + result.StandardOutput);//not necessarily an *error* down this deep
			}

#if DEBUG
			//nb: store/lock is so common with recover (in hg 1.3) that we don't even want to mention it
			// enhance: what are the odds the username or password contain "recover"?
			if (!commandWithoutPasswordForLog.Contains("recover") && GetHasLocks(_pathToRepository, _progress))
			{
				_progress.WriteWarning("{0} left a lock.", commandWithoutPasswordForLog);
			}
#endif
			return result;
		}

		public string PathWithQuotes
		{
			get
			{
				return "\"" + _pathToRepository + "\"";
			}
		}

		public string PathToRepo
		{
			get { return _pathToRepository; }
		}

		public string GetFilePath(string name)
		{
			return Path.Combine(_pathToRepository, name);
		}

		public List<string> GetChangedFiles()
		{
			CheckAndUpdateHgrc();
			ExecutionResult result = Execute(SecondsBeforeTimeoutOnLocalOperation, "status");
			string[] lines = result.StandardOutput.Split('\n');
			List<string> files = new List<string>();
			foreach (string line in lines)
			{
				if (line.Trim() != "")
					files.Add(line.Substring(2)); //! data.txt
			}

			return files;
		}

		public void Update()
		{
			_progress.WriteVerbose("{0} updating", _userName);
			CheckAndUpdateHgrc();
			Execute(SecondsBeforeTimeoutOnLocalOperation, "update", "-C");
		}

		public void Update(string revision)
		{
			_progress.WriteVerbose("{0} updating (making working directory contain) revision {1}", _userName, revision);
			CheckAndUpdateHgrc();
			Execute(SecondsBeforeTimeoutOnLocalOperation, "update", "-r", revision, "-C");
		}

		public UpdateResults UpdateToLongHash(string desiredLongHash)
		{
			if (string.IsNullOrWhiteSpace(Identifier))
			{
				return UpdateResults.NoCommitsInRepository;
			}
			var workingSetRevision = GetRevisionWorkingSetIsBasedOn();
			if (desiredLongHash == workingSetRevision.Number.LongHash)
			{
				// Already on it.
				return UpdateResults.AlreadyOnIt;
			}
			// Find it the hard way.
			foreach (var currentRevision in GetAllRevisions())
			{
				var currentLongHash = currentRevision.Number.LongHash;
				if (currentLongHash != desiredLongHash)
				{
					continue;
				}
				// Update to it.
				Update(currentRevision.Number.Hash);
				return UpdateResults.Success;
			}

			// No such commit!
			return UpdateResults.NoSuchRevision;
		}

		public UpdateResults UpdateToBranchHead(string desiredBranchName)
		{
			if (string.IsNullOrWhiteSpace(Identifier))
			{
				return UpdateResults.NoCommitsInRepository;
			}
			Revision highestHead = null;
			foreach (var head in GetHeads().Where(head => head.Branch == desiredBranchName))
			{
				if (highestHead == null)
				{
					highestHead = head;
				}
				else
				{
					// Ugh! more than one head in branch, so use the one with the highest local revision number.
					// The extra head(s) will be merged in the next S/R that does merges.
					if (int.Parse(highestHead.Number.LocalRevisionNumber) < int.Parse(head.Number.LocalRevisionNumber))
					{
						highestHead = head;
					}
				}
			}
			if (highestHead == null)
			{
				// No such branch.
				return UpdateResults.NoSuchBranch;
			}
			if (GetRevisionWorkingSetIsBasedOn().Number.LongHash == highestHead.Number.LongHash)
			{
				return UpdateResults.AlreadyOnIt;
			}

			Update(highestHead.Number.Hash);
			return UpdateResults.Success;
		}

        public string Verify()
        {
            _progress.WriteVerbose("{0} verifying", _userName);
            CheckAndUpdateHgrc();
            ExecutionResult result = Execute(SecondsBeforeTimeoutOnLocalOperation, "verify");

            if (result.StandardOutput.Contains("run hg recover"))
            {
                // Failed to verify. Try simple recover command
                _progress.WriteVerbose("{0} failed to verify - Attempting recovery", _userName);
                Execute(SecondsBeforeTimeoutOnLocalOperation, "recover");

                result = Execute(SecondsBeforeTimeoutOnLocalOperation, "verify");
            }
            if (result.ExitCode == 0)
                return null;
            return result.StandardOutput + result.StandardError;
        }

		//        public void GetRevisionOfFile(string fileRelativePath, string revision, string fullOutputPath)
		//        {
		//            //for "hg cat" (surprisingly), the relative path isn't relative to the start of the repo, but to the current
		//            // directory.
		//            string absolutePathToFile = SurroundWithQuotes(Path.Combine(_pathToRepository, fileRelativePath));
		//
		//            Execute("cat", _pathToRepository, "-o ",fullOutputPath," -r ",revision,absolutePathToFile);
		//        }

		/// <summary>
		/// Note, this uses the value of AllowDotEncodeRepositoryFormat
		/// </summary>
		public static HgRepository CreateRepositoryInExistingDir(string path, IProgress progress)
		{
			CheckMercurialIni();
			var repo = new HgRepository(path, progress);
			repo.Init();

			return repo;
		}

		/// <summary>
		/// Note, this uses the value of AllowDotEncodeRepositoryFormat
		/// </summary>
		public void Init()
		{
			CheckMercurialIni();
			Execute(20, "init", SurroundWithQuotes(_pathToRepository));
			CheckAndUpdateHgrc();
		}

		public void AddAndCheckinFiles(List<string> includePatterns, List<string> excludePatterns, string message)
		{
			CheckAndUpdateHgrc();
			StringBuilder args = new StringBuilder();
			foreach (string pattern in includePatterns)
			{
				string p = Path.Combine(_pathToRepository, pattern);
				args.Append(" -I " + SurroundWithQuotes(p));
			}

			foreach (string pattern in excludePatterns)
			{
				//this fails:   hg add -R "E:\Users\John\AppData\Local\Temp\ChorusTest"  -X "**/cache"
				//but this works  -X "E:\Users\John\AppData\Local\Temp\ChorusTest/**/cache"
				string p = Path.Combine(_pathToRepository, pattern);
				args.Append(" -X " + SurroundWithQuotes(p));
			}

			//enhance: what happens if something is covered by the exclusion pattern that was previously added?  Will the old
			// version just be stuck on the head? NB: to remove a file from the checkin but not delete it, do "hg remove -Af"

			if (GetIsAtLeastOneMissingFileInWorkingDir())
			{
				_progress.WriteVerbose("At least one file was removed from the working directory.  Telling Hg to record the deletion.");

				Execute(SecondsBeforeTimeoutOnLocalOperation, "rm -A");
			}

			_progress.WriteVerbose("Adding files to be tracked ({0}", args.ToString());
			Execute(SecondsBeforeTimeoutOnLocalOperation, "add", args.ToString());

			_progress.WriteVerbose("Committing \"{0}\"", message);
			Commit(_mercurialTwoCompatible, message);
		}

		public static string GetRepositoryRoot(string directoryPath, ExecutionResult secondsBeforeTimeout)
		{
			//            string old = Directory.GetCurrentDirectory();
			//            try
			//            {
			// Directory.SetCurrentDirectory(directoryPath);
			ExecutionResult result = secondsBeforeTimeout;
			if (result.ExitCode == 0)
			{
				var output = result.StandardOutput;
				int index = output.IndexOf('\n');
				if (index > 0)
				{
					output = output.Substring(0, index);
				}
				return output.Trim();
			}
			return null;
			//            }
			//            finally
			//            {
			//                Directory.SetCurrentDirectory(old);
			//            }
		}


		private void PrintHeads(List<Revision> heads, Revision myHead)
		{
			_progress.WriteMessage("Current Heads:");
			foreach (Revision head in heads)
			{
				if (head.Number.LocalRevisionNumber == myHead.Number.LocalRevisionNumber)
				{
					_progress.WriteVerbose("  ME {0} {1} {2}", head.UserId, head.Number.LocalRevisionNumber, head.Summary);
				}
				else
				{
					_progress.WriteVerbose("      {0} {1} {2}", head.UserId, head.Number.LocalRevisionNumber, head.Summary);
				}
			}
		}


		/// <summary>
		/// Will throw exception in case of error.
		/// Will never time out.
		/// Will honor state of the progress.CancelRequested property
		/// </summary>
		public void CloneFromSource(string sourceLabel, string sourceUri)
		{
			try
			{
				Execute(int.MaxValue, "clone -U", DoWorkOfDeterminingProxyConfigParameterString(_pathToRepository, _progress), SurroundWithQuotes(sourceUri) + " " + SurroundWithQuotes(_pathToRepository));
			}
			catch (Exception error)
			{
				if (UriProblemException.ErrorMatches(error))
				{
					throw new UriProblemException(sourceUri);
				}
				else if (ProjectLabelErrorException.ErrorMatches(error))
				{
					throw new ProjectLabelErrorException(sourceUri);
				}
				else if (FirewallProblemSuspectedException.ErrorMatches(error))
				{
					throw new FirewallProblemSuspectedException();
				}
				else if (ServerErrorException.ErrorMatches(error))
				{
					throw new ServerErrorException();
				}
				else if (PortProblemException.ErrorMatches(error))
				{
					throw new PortProblemException(sourceUri);
				}
				else if (PortProblemException.ErrorMatches(error))
				{
					throw new PortProblemException(sourceUri);
				}
				else if (RepositoryAuthorizationException.ErrorMatches(error))
				{
					throw new RepositoryAuthorizationException();
				}

				throw;
			}
		}

		/// <summary>
		/// Attempt to clone to the target, making a new target folder if that one already exists
		/// </summary>
		/// <param name="source"></param>
		/// <param name="targetPath"></param>
		/// <param name="progress"></param>
		/// <returns>because this automatically changes the target name if it already exists, it returns the *actual* target used</returns>
		public static string Clone(RepositoryAddress source, string targetPath, IProgress progress)
		{
			progress.WriteMessage("Getting project...");
			targetPath = GetUniqueFolderPath(progress,
											 "Folder at {0} already exists, so can't be used. Creating clone in {1}, instead.",
											 targetPath);
			var repo = new HgRepository(targetPath, progress);
			repo.LogBasicInfo(new List<RepositoryAddress> {source});

			// Cannot pass repo.Identifier because the local repo doesn't exist yet.
			var transport = repo.CreateTransportBetween(source, source.GetPotentialRepoUri(null, null, progress));
			transport.Clone();
			repo.Update();
			progress.WriteMessage("Finished copying to this computer at {0}", targetPath);
			progress.WriteVerbose($"Finished at {DateTime.UtcNow:u}");
			return targetPath;
		}

		/// <summary>
		/// Log basic diagnostic information, including the username and repository name
		/// </summary>
		public void LogBasicInfo(IList<RepositoryAddress> potentialAddresses)
		{
			_progress.WriteVerbose($"Started at {DateTime.UtcNow:u}");
			_progress.WriteVerbose($"Local User: {GetUserIdInUse()}");
			if (potentialAddresses.Any(a => a is HttpRepositoryPath))
			{
				_progress.WriteVerbose($"LanguageForge User: {Settings.Default.LanguageForgeUser}");
			}

			_progress.WriteVerbose($"Repository URI: {string.Join(Environment.NewLine, potentialAddresses.Select(RepositoryURIForLog))}");
			_progress.WriteVerbose($"Local Directory: {_pathToRepository}{Environment.NewLine}");
		}

		internal string RepositoryURIForLog(RepositoryAddress address)
		{
			try
			{
				// Attempt to resolve the project name variable (needed for sync with ChorusHub, but not clone).
				// USB Keys also have a low-information URI, but attempting to resolve it at this point is difficult if not impossible,
				// and it will appear later in the log.
				if(address.URI.Contains(RepositoryAddress.ProjectNameVariable))
				{
					return address.GetPotentialRepoUri(Identifier,
						Path.GetFileNameWithoutExtension(_pathToRepository) + Path.GetExtension(_pathToRepository),
						_progress);
				}
			}
			catch { /* Don't throw trying to get extra information to log */ }
			return address.URI;
		}

		/// <summary>
		/// Here we only create the .hg, no files. This is good because the people aren't tempted to modify
		/// files in that directory, where nothing will ever check the changes in.
		///
		/// NB: Caller may well want to call Update on this repository,
		/// say when the clone is from a USB or shared network folder TO a local working folder,
		/// and the caller plans to use the actual data files in the repository.
		/// </summary>
		public string CloneLocalWithoutUpdate(string proposedTargetPath, string additionalOptions = null)
		{
			CheckMercurialIni();
			var actualTarget = GetUniqueFolderPath(_progress, proposedTargetPath);

			Execute(SecondsBeforeTimeoutOnLocalOperation, "clone", "-U", "--uncompressed", additionalOptions, PathWithQuotes, SurroundWithQuotes(actualTarget));

			return actualTarget;
		}

		private List<Revision> GetRevisionsFromQuery(string query)
		{
			string result = GetTextFromQuery(query);
			return GetRevisionsFromQueryResultText(result);
		}


		public List<Revision> GetAllRevisions()
		{
			/*
				changeset:0:074a37a5bbaf
				branch:default
				user:chirt
				date:Thu, 08 Sep 2011 14:35:53 +0700
				tag:
				summary:base checkin
			*/

			string result = GetTextFromQuery("log " + DetailedRevisionTemplate);
			return GetRevisionsFromQueryResultText(result);
		}

		public List<Revision> GetRevisionsFromQueryResultText(string queryResultText)
		{
			TextReader reader = new StringReader(queryResultText);
			string line = reader.ReadLine();


			List<Revision> items = new List<Revision>();
			Revision item = null;
			int infiniteLoopChecker = 0; //trying to pin down WS-14981 send/receive hangs
			while (line != null && infiniteLoopChecker < 100)
			{
				int colonIndex = line.IndexOf(":");
				if (colonIndex > 0)
				{
					string label = line.Substring(0, colonIndex);

					//On the Palaso TeamCity server, we found that the summary was coming in with a leading Byte Order Mark.
					//With .net 3.5, this is removed by Trim(). With .net 4, it is not(!!!).
					//This lead to a failing test. We have no idea where it comes from, nor the cause.
					//The only thing that should be different on the server is that it is Windows XP.
					string value = line.Substring(colonIndex + 1).Trim()
						.Trim(new char[] { '\uFEFF', '\u200B' }).Trim();
					switch (label)
					{
						default:
							if (Platform.IsMono)
								infiniteLoopChecker++;
							break;
						case "changeset":
							item = new Revision(this);
							items.Add(item);
							item.SetRevisionAndHashFromCombinedDescriptor(value);
							break;
						case "parent":
							item.AddParentFromCombinedNumberAndHash(value);
							break;

						case "branch":
							item.Branch = value;
							break;

						case "user":
							item.UserId = value;
							break;

						case "date":
							item.DateString = value;
							break;

						case "summary":
							item.Summary = value;
							break;

						case "tag":
							item.Tag = value;
							break;
					}
				}

				line = reader.ReadLine();
			}

			if (infiniteLoopChecker > 99)
			{
				_progress.WriteWarning(
					"Had to break out of infinite loop in GetRevisionsFromQueryResultText(). See WS-14981: 'send/receive hangs'.");
			}

			return items;
		}

		/// <summary>
		/// If we checked in now, which revision would be the parent?
		/// </summary>
		public Revision GetRevisionWorkingSetIsBasedOn()
		{
			return GetRevisionsFromQuery("parents --template \"changeset:{rev}:{node|short}\nbranch:{branches}\nuser:{author}\ndate:{date|rfc822date}\ntag:{tags}\nsummary:{desc}\nparent:{p1rev}:{p1node}\"").FirstOrDefault();
		}

		public string GetUserIdInUse()
		{
			var defaultName = Environment.UserName.Replace(" ", "");
			if (GetIsLocalUri(_pathToRepository))
			{
				return GetUserNameFromIni(_progress, defaultName);
			}

			var username = UrlHelper.GetUserName(_pathToRepository);
			return string.IsNullOrEmpty(username) ? defaultName : username;
		}

		public bool GetFileExistsInRepo(string subPath)
		{
			string result = GetTextFromQuery("locate " + subPath);
			return !string.IsNullOrEmpty(result.Trim());
		}
		public bool GetIsAtLeastOneMissingFileInWorkingDir()
		{
			string result = GetTextFromQuery("status -d ");
			return !string.IsNullOrEmpty(result.Trim());
		}

		/// <summary>
		///  From IRetrieveFileVersionsFromRepository
		/// </summary>
		/// <returns>path to a temp file. caller is responsible for deleting the file.</returns>
		public string RetrieveHistoricalVersionOfFile(string relativePath, string revOrHash)
		{
			Guard.Against(string.IsNullOrEmpty(revOrHash), "The revision cannot be empty (note: the first revision has an empty string for its parent revision");
			var f = TempFile.WithExtension(Path.GetExtension(relativePath));

			var cmd = string.Format("cat -o \"{0}\" -r {1} \"{2}\"", f.Path, revOrHash, relativePath);
			ExecutionResult result = ExecuteErrorsOk(cmd, SecondsBeforeTimeoutOnLocalOperation);
			if (!string.IsNullOrEmpty(result.StandardError.Trim()))
			{
				// At least zap the temp file, since it isn't to be returned.
				File.Delete(f.Path);
				throw new ApplicationException($"Could not retrieve version {revOrHash} of {relativePath}. Mercurial said: {result.StandardError}");
			}
			return f.Path;
		}

		public string GetCommonAncestorOfRevisions(string rev1, string rev2)
		{
			var result = GetTextFromQuery("debugancestor " + rev1 + " " + rev2);
			if (result.StartsWith("-1"))
				return null;
			return new RevisionNumber(this, result).LocalRevisionNumber;
		}

		public IEnumerable<FileInRevision> GetFilesInRevision(Revision revision)
		{
			List<FileInRevision> files = new List<FileInRevision>();
			//nb: there can be 2 parents, and at the moment, I don't know how to figure
			//out what changed except by comparing this to each revision (seems dumb)
			var revisionRanges = GetRevisionRangesFoDiffingARevision(revision);
			foreach (var r in revisionRanges)
			{
				var query = "status --rev " + r;
				foreach (var file in GetFilesInRevisionFromQuery(revision, query))
				{
					//only add if we don't already have it, from comparing with another parent
					if (null == files.FirstOrDefault(f => f.FullPath == file.FullPath))
					{
						if (file.ActionThatHappened != FileInRevision.Action.Unknown)
							files.Add(file);
					}
				}
			}

			return files;
		}



		public List<FileInRevision> GetFilesInRevisionFromQuery(Revision revisionToAssignToResultingFIRs, string query)
		{
			var result = GetTextFromQuery(query);
			string[] lines = result.Split('\n');
			var revisions = new List<FileInRevision>();
			foreach (string line in lines)
			{
				if (line.Trim() == "")
					continue;
				var actionLetter = line[0];
				//               if(actionLetter == '?') //this means it wasn't actually committed, like maybe ignored?
				//                 continue;
				var action = ParseActionLetter(actionLetter);

				//if this is the first rev in the whole repo, then the only way to list the fils
				//is to include the "clean" ones.  Better to represent that as an Add
				if (action == FileInRevision.Action.NoChanges)
					action = FileInRevision.Action.Added;

				string revToAssign = revisionToAssignToResultingFIRs == null ? "-1" : revisionToAssignToResultingFIRs.Number.LocalRevisionNumber;
				revisions.Add(new FileInRevision(revToAssign, Path.Combine(PathToRepo, line.Substring(2)), action));
			}
			return revisions;
		}

		private IEnumerable<string> GetRevisionRangesFoDiffingARevision(Revision revision)
		{
			var parents = GetParentsOfRevision(revision.Number.LocalRevisionNumber);
			if (parents.Count() == 0)
				yield return string.Format("{0}:{0} -A", revision.Number.LocalRevisionNumber);

			foreach (var parent in parents)
			{
				yield return string.Format("{0}:{1}", parent, revision.Number.LocalRevisionNumber);
			}
		}

		public IEnumerable<string> GetParentsOfRevision(string localRevisionNumber)
		{
			return from x in GetRevisionsFromQuery("parent -r " + localRevisionNumber)
				   select x.Number.LocalRevisionNumber;
		}

		public IEnumerable<RevisionNumber> GetParentsRevisionNumbers(string localRevisionNumber)
		{
			return from x in GetRevisionsFromQuery("parent -r " + localRevisionNumber)
				   select x.Number;
		}

		private static FileInRevision.Action ParseActionLetter(char actionLetter)
		{
			switch (actionLetter)
			{
				case 'A':
					return FileInRevision.Action.Added;
				case 'M':
					return FileInRevision.Action.Modified;
				case 'R':
					return FileInRevision.Action.Deleted;
				case '!':
					return FileInRevision.Action.Deleted;
				case 'C':
					return FileInRevision.Action.NoChanges;
				default:
					return FileInRevision.Action.Unknown;
			}
		}

		///<summary>
		/// Returns the repository addresses configured in the paths section of this repos hgrc
		///</summary>
		///<returns></returns>
		public IEnumerable<RepositoryAddress> GetRepositoryPathsInHgrc()
		{
			var section = GetMercurialConfigForRepository().Sections.GetOrCreate("paths");
			foreach (var name in section.GetKeys())
			{
				var uri = section.GetValue(name);
				yield return RepositoryAddress.Create(name, uri);
			}
		}


		/// <summary>
		/// TODO: sort out this vs. the UserName property
		/// </summary>
		/// <returns></returns>
		public string GetUserNameFromIni(IProgress progress, string defaultName)
		{
			try
			{
				var p = GetPathToHgrc();
				if (File.Exists(p))
				{
					var doc = GetMercurialConfigForRepository();
					var section = doc.Sections["ui"];
					if ((section != null) && section.Contains("username"))
						return section.GetValue("username");
				}

				return defaultName;
			}
			catch (Exception)
			{
				progress.WriteMessage("Couldn't determine user name, will use {0}", defaultName);
				return defaultName;
			}
		}

		internal string GetPathToHgrc()
		{
			var d = Path.Combine(_pathToRepository, ".hg");
			return Path.Combine(d, "hgrc");
		}

		internal IniDocument GetMercurialConfigForRepository()
		{
			var p = GetPathToHgrc();
			if (!File.Exists(p))
			{
				string d = Path.GetDirectoryName(p);
				if (!Directory.Exists(d))
					throw new ApplicationException("There is no repository at " + d);

				File.WriteAllText(p, "");
			}
			return new IniDocument(p, IniFileType.MercurialStyle);
		}

		internal static IniDocument GetMercurialConfigInMercurialFolder()
		{
			var mercurialIniFilePath = Path.Combine(MercurialLocation.PathToMercurialFolder, "mercurial.ini");
			if (!File.Exists(mercurialIniFilePath))
			{
				File.WriteAllText(mercurialIniFilePath, "");
			}
			return new IniDocument(mercurialIniFilePath, IniFileType.MercurialStyle);
		}

		public void SetUserNameInIni(string name, IProgress progress)
		{
			try
			{
				var doc = GetMercurialConfigForRepository();
				doc.Sections.GetOrCreate("ui").Set("username", name);
				doc.SaveAndGiveMessageIfCannot();
				_userName = GetUserIdInUse();//update the _userName we're using (would expect it to change to this name)
			}
			catch (IOException e)
			{
				progress.WriteError("Could not set the user name from the hgrc ini file: " + e.Message);
				throw new TimeoutException(e.Message, e);//review... this keeps things simpler, but for a lack of precision
			}

			catch (Exception error)
			{
				progress.WriteWarning("Could not set the user name from the hgrc ini file: " + error.Message);
			}
		}

		public void SetKnownRepositoryAddresses(IEnumerable<RepositoryAddress> addresses)
		{
			var doc = GetMercurialConfigForRepository();
			doc.Sections.RemoveSection("paths");//clear it out
			var section = doc.Sections.GetOrCreate("paths");
			foreach (var address in addresses)
			{
				section.Set(address.Name, address.URI);
			}
			doc.SaveAndGiveMessageIfCannot();
		}

		public void RemoveCredentialsFromIniIfNecessary()
		{
			Uri uri;
			if (Uri.TryCreate(GetDefaultNetworkAddress<HttpRepositoryPath>()?.URI, UriKind.Absolute, out uri)
				&& !string.IsNullOrEmpty(uri.UserInfo))
			{
				// The username and password are saved in the URL in the hgrc file. Simply loading the file into a ServerSettingsModel
				// and saving should strip this information and save it in user settings with the password encrypted.
				var serverSettingsModel = new ServerSettingsModel();
				serverSettingsModel.InitFromProjectPath(_pathToRepository);
				serverSettingsModel.SaveSettings();
			}
		}

		// REVIEW: does this have to be public? Looks like it should be private or internal.
		public void EnsureCacertsIsSet()
		{
			var doc = GetMercurialConfigForRepository();
			var section = doc.Sections.GetOrCreate("web");

			section.Set("cacerts", PathToCertificateFile);

			doc.SaveAndGiveMessageIfCannot();
		}

		private static string PathToCertificateFile
		{
			get
			{
				if (Platform.IsLinux)
				{
					// Linux comes with a set of root certificates installed on the system.
					// Unfortunately there is no fixed location or a defined way how to get that
					// location, so we have to try the locations we know.
					// See https://www.mercurial-scm.org/wiki/CACertificates and https://serverfault.com/a/722646
					var certFiles = new[] {
						"/etc/ssl/certs/ca-certificates.crt",               // Debian/Ubuntu/Gentoo etc.
						"/etc/pki/tls/certs/ca-bundle.crt",                 // Fedora/RHEL 6
						"/etc/ssl/ca-bundle.pem",                           // OpenSUSE
						"/etc/pki/tls/cacert.pem",                          // OpenELEC
						"/etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem" // CentOS/RHEL 7
					};

					foreach (var file in certFiles)
					{
						if (File.Exists(file))
							return file;
					}
				}

				// On Windows Mercurial comes with it's own certificate file.
				// On Linux, if we didn't a file at any of the predefined locations we return
				// the same file as on Windows and hope that it exists...
				return Path.Combine(MercurialLocation.PathToMercurialFolder, "cacert.pem");
			}
		}

		public void SetTheOnlyAddressOfThisType(RepositoryAddress address)
		{
			List<RepositoryAddress> addresses = new List<RepositoryAddress>(GetRepositoryPathsInHgrc());
			RepositoryAddress match = addresses.FirstOrDefault(p=>p.GetType()  ==  address.GetType());
			if(match!=null)
			{
				addresses.Remove(match);
			}
			addresses.Add(address);
			SetKnownRepositoryAddresses(addresses);
		}

		public Revision GetRevision(string numberOrHash)
		{
			return GetRevisionsFromQuery(string.Format("log --rev {0} {1}", numberOrHash, DetailedRevisionTemplate)).FirstOrDefault();
		}

		/// <summary>
		/// this is a chorus-specific concept, that there are 0 or more repositories
		/// which we always try to sync with
		/// </summary>
		public void SetDefaultSyncRepositoryAliases(IEnumerable<string> aliases)
		{
			var doc = GetMercurialConfigForRepository();
			doc.Sections.RemoveSection("ChorusDefaultRepositories");//clear it out
			IniSection section = GetDefaultRepositoriesSection(doc);
			foreach (var alias in aliases)
			{
				section.Set(alias, string.Empty); //so we'll have "LanguageForge =", which is weird, but it's the hgrc style
			}
			doc.SaveAndGiveMessageIfCannot();

		}

		private IniSection GetDefaultRepositoriesSection(IniDocument doc)
		{
			var section = doc.Sections.GetOrCreate("ChorusDefaultRepositories");
			section.Comment = "Used by chorus to track which repositories should always be checked.  To enable a path, enter it in the [paths] section, e.g. fiz='http://fis.com/fooproject', then in this section, just add 'fiz='";
			return section;
		}

		public List<string> GetDefaultSyncAliases()
		{
			var list = new List<RepositoryAddress>();
			var doc = GetMercurialConfigForRepository();
			var section = GetDefaultRepositoriesSection(doc);
			return new List<string>(section.GetKeys());
		}

		internal void EnsureTheseExtensionsAndFormatSet(Dictionary<string, string> extensions)
		{
			var doc = GetMercurialConfigForRepository();

			IniSection section = doc.Sections.GetOrCreate("format");

			SetExtensions(doc, extensions);

			doc.SaveAndThrowIfCannot();
		}

		private static void SetExtensions(IniDocument doc, IEnumerable<KeyValuePair<string, string>> extensionDeclarations)
		{
			doc.Sections.RemoveSection("extensions");
			var section = doc.Sections.GetOrCreate("extensions");
			foreach (var pair in extensionDeclarations)
			{
				section.Set(pair.Key, pair.Value);
			}
		}

		/// <summary/>
		/// <remarks>
		/// Older versions of Chorus wrote out extensions that are incompatible with new hg versions
		/// so we now remove extensions that aren't specified.
		/// </remarks>
		/// <returns>
		/// Returns true if the extensions in the repo contain only the extensions declared in the given map.
		///</returns>
		internal static bool CheckExtensions(IniDocument doc, Dictionary<string, string> extensionDeclarations)
		{
			var extensionSection = doc.Sections.GetOrCreate("extensions");
			if(extensionDeclarations.Any(pair => extensionSection.GetValue(pair.Key) != pair.Value))
			{
				return false;
			}
			return extensionSection.GetKeys().All(key => extensionDeclarations.ContainsKey(key));
		}

		// TODO Move this to Chorus.TestUtilities when we have one CP 2012-04
		public IEnumerable<string> GetEnabledExtension()
		{
			var doc = GetMercurialConfigForRepository();
			var section = doc.Sections.GetOrCreate("extensions");
			return section.GetKeys();
		}

		public void SetIsOneDefaultSyncAddresses(RepositoryAddress address, bool doInclude)
		{
			var doc = GetMercurialConfigForRepository();
			var section = GetDefaultRepositoriesSection(doc);
			if (doInclude)
			{
				section.Set(address.Name, string.Empty);
			}
			else
			{
				section.Remove(address.Name);
			}
			doc.SaveAndGiveMessageIfCannot();
		}

		/// <summary>
		/// Tests Network and Internet connection to a URI. Gives the best diagnostics with a log box.
		/// Uses Ping and (failing that) DNS resolution to determine connection state.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="progress"></param>
		/// <returns></returns>
		public bool GetCanConnectToRemote(string uri, IProgress progress)
		{

			// No longer uses "hg incoming", since that takes just as long as a pull, according to the hg mailing list
			//    ExecutionResult result = ExecuteErrorsOk(string.Format("incoming -l 1 {0}", SurroundWithQuotes(uri)), SecondsBeforeTimeoutOnLocalOperation);
			//so we're going to just ping

			try
			{
				//strip everything but the host name
				if (!Uri.TryCreate(uri, UriKind.Absolute, out var uriObject))
				{
					var uri4Log = ServerSettingsModel.RemovePasswordForLog(uri);
					progress.WriteError($"Please check that the address is formatted correctly and that your password has no special characters: {uri4Log}");
					return false;
				}
				var host = uriObject.Host;

				if (!NetworkInterface.GetIsNetworkAvailable())
				{
					progress.WriteWarning("This machine does not have a live network connection.");
					return false;
				}

				progress.WriteVerbose("Pinging {0}...", host);
				using (var ping = new Ping())
				{
					var result = ping.Send(host, 3000);//arbitrary... what's a reasonable wait?
					if (result?.Status == IPStatus.Success)
					{
						progress.WriteVerbose("Ping took {0} milliseconds", result.RoundtripTime);
						return true;
					}
					progress.WriteVerbose("Ping failed. Trying google.com...");

					//ok, at least in Ukarumpa, sometimes pings just are impossible.  Determine if that's what's going on by pinging google
					result = ping.Send("google.com", 3000);
					if (result?.Status != IPStatus.Success)
					{
						progress.WriteVerbose("Ping to google failed, too.");
						if (Dns.GetHostAddresses(host).Any())
						{
							progress.WriteVerbose(
								"Did resolve the host name, so it's worth trying to use hg to connect... some places block ping.");
							return true;
						}
						progress.WriteVerbose("Could not resolve the host name '{0}'.", host);
						return false;
					}

					if (Dns.GetHostAddresses(host).Any())
					{
						// cjh 2012-03 : excluded languageforge.org from this check since it doesn't respond to ping requests
						// TODO: what we should really do is build in a server check to see if we can retrieve a small file from the server instead of trying to ping it
						if (!host.Contains("languageforge.org"))
						{
							progress.WriteMessage(
							"Chorus could ping google, and did get IP address for {0}, but could not ping it, so it could be that the server is temporarily unavailable.", host);
						}
						return true;
					}

					progress.WriteError("Please check the spelling of address {0}.  Chorus could not resolve it to an IP address.", host);
					return false; // assume the network is ok, but the hg server is inaccessible
				}
			}
			catch (Exception e)
			{
				progress.WriteException(e);
				return false;
			}
		}



		public void RecoverFromInterruptedTransactionIfNeeded()
		{
			CheckAndUpdateHgrc();
			var result = Execute(true, SecondsBeforeTimeoutOnLocalOperation, "recover");

			if (GetHasLocks())
			{   //recover very often leaves store/lock, at least in hg 1.3
				RemoveOldLocks("hg.exe", false);
				_progress.WriteVerbose("Recover may have left a lock, which was removed unless otherwise reported.");
			}

			if (result.StandardError.Contains("no interrupted"))//constains rather than starts with because there may be a preceding message about locks (bl-292)
			{
				return;
			}

			if (!string.IsNullOrEmpty(result.StandardError))
			{
				_progress.WriteError(result.StandardError);
				throw new ApplicationException("Trying to recover, got: "+result.StandardError);
			}
			if (!string.IsNullOrEmpty(result.StandardOutput))
			{
				_progress.WriteWarning("Recovered: " + result.StandardOutput);
			}

		}

		/// <summary>
		///
		/// </summary>
		/// <exception cref="ApplicationException"/>
		/// <param name="localRepositoryPath"></param>
		/// <param name="revisionNumber"></param>
		/// <returns>false if nothing needed to be merged, true if the merge was done. Throws exception if there is an error.</returns>
		public bool Merge(string localRepositoryPath, string revisionNumber)
		{
			CheckAndUpdateHgrc();
			var result = Execute(true, SecondsBeforeTimeoutOnMergeOperation, "merge", "-r", revisionNumber);

			if (result.ExitCode != 0)
			{
				if (result.StandardError.Contains("nothing to merge"))
				{
					if (!string.IsNullOrEmpty(result.StandardOutput))
					{
						_progress.WriteVerbose(result.StandardOutput);
					}
					if (!string.IsNullOrEmpty(result.StandardError))
					{
						_progress.WriteVerbose(result.StandardError);
					}
					return false;
				}
				else
				{
					_progress.WriteError(result.StandardError);
					if (!string.IsNullOrEmpty(result.StandardOutput))
					{
						_progress.WriteError("Also had this in the standard output:");
						_progress.WriteError(result.StandardOutput);
					}
					throw new ApplicationException(result.StandardError);
				}
			}
			return true;
		}

		/// <summary>
		/// Attempts to remove lock and wlocks if it looks safe to do so
		/// </summary>
		/// <returns></returns>
		public bool RemoveOldLocks()
		{
			return RemoveOldLocks("hg.exe", false);
		}


		public bool GetHasLocks()
		{
			return GetHasLocks(_pathToRepository, _progress);
		}

		public static bool GetHasLocks(string path, IProgress progress)
		{
			var wlockPath = Path.Combine(Path.Combine(path, ".hg"), "wlock");
			if (File.Exists(wlockPath))
				return true;

			var lockPath = Path.Combine(path, ".hg");
			lockPath = Path.Combine(lockPath, "store");
			lockPath = Path.Combine(lockPath, "lock");

			if (File.Exists(lockPath))
				return true;

			return false;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="processNameToMatch">the process to look for, instead of "hg.exe"</param>
		/// <param name="registerWarningIfFound"></param>
		/// <returns></returns>
		public bool RemoveOldLocks(string processNameToMatch, bool registerWarningIfFound)
		{
			var wlockPath = Path.Combine(Path.Combine(_pathToRepository, ".hg"), "wlock");
			if (!RemoveOldLockFile(processNameToMatch, wlockPath, true))
				return false;

			var lockPath = Path.Combine(_pathToRepository, ".hg");
			lockPath = Path.Combine(lockPath, "store");
			lockPath = Path.Combine(lockPath, "lock");
			return RemoveOldLockFile(processNameToMatch, lockPath, registerWarningIfFound);
		}



		private bool RemoveOldLockFile(string processNameToMatch, string pathToLock, bool registerWarningIfFound)
		{
			if (File.Exists(pathToLock))
			{
				if (registerWarningIfFound)
				{
					_progress.WriteWarning("Trying to remove a lock at {0}...", pathToLock);
				}
				var hgIsRunning = Process.GetProcessesByName(processNameToMatch).Length > 0;
				if (hgIsRunning)
				{
					_progress.WriteError("There is at last one {0} running, so {1} cannot be removed.  You may need to restart the computer.", processNameToMatch, Path.GetFileName(pathToLock));
					return false;
				}
				try
				{
					File.Delete(pathToLock);
					if (registerWarningIfFound)
					{
						_progress.WriteWarning("Lock safely removed.");
					}
				}
				catch (Exception error)
				{
					try
					{
						var dest = Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
						File.Move(pathToLock, dest);
						_progress.WriteWarning("Lock could not be deleted, but was moved to temp directory.");
					}
					catch (Exception)
					{
						_progress.WriteError(
							"The file {0} could not be removed.  You may need to restart the computer.",
							Path.GetFileName(pathToLock));
						_progress.WriteError(error.Message);
						return false;
					}
				}
			}

			return true;
		}

		public void RollbackWorkingDirectoryToLastCheckin()
		{
			CheckAndUpdateHgrc();
			Execute(false, 30, "update --clean");
		}

		public void RollbackWorkingDirectoryToRevision(string revision)
		{
			CheckAndUpdateHgrc();
			Execute(false, 30, "update --clean --rev " + revision);
		}

		/// <summary>
		/// use this, for example, if a clone fails
		/// </summary>
		public void GetDiagnosticInformationForRemoteProject(IProgress progress, string url)
		{
			progress.WriteMessage("Gathering diagnostics data (can't actually tell you anything about the remote server)...");
			progress.WriteMessage(GetTextFromQuery("version", 30));

			progress.WriteMessage("Using Mercurial at: "+MercurialLocation.PathToHgExecutable);
			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("remote url = " + url);

			try
			{
				progress.WriteMessage("Client = " + Assembly.GetEntryAssembly()?.FullName);
				progress.WriteMessage("Chorus = " + Assembly.GetExecutingAssembly().FullName);
			}
			catch (Exception)
			{
				progress.WriteWarning("Could not get all assembly info.");
			}

			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("config:");
			progress.WriteMessage(GetTextFromQuery("showconfig", 30));
			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("Done.");
		}

		public void GetDiagnosticInformation(IProgress progress)
		{
			progress.WriteMessage("Gathering diagnostics data...");
			progress.WriteMessage(GetTextFromQuery("version", 30));
			progress.WriteMessage("Using Mercurial at: "+MercurialLocation.PathToHgExecutable);
			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("path = " + _pathToRepository);

			try
			{
				progress.WriteMessage("Client = " + Assembly.GetEntryAssembly()?.FullName);
				progress.WriteMessage("Chorus = " + Assembly.GetExecutingAssembly().FullName);
			}
			catch (Exception)
			{
				progress.WriteWarning("Could not get all assembly info.");
			}

			progress.WriteMessage("---------------------------------------------------");
			progress.WriteMessage("heads:");
			progress.WriteMessage(GetTextFromQuery("heads", 30));

			if (GetHeads().Count() > 1)
			{
				progress.WriteError("This project has some 'changesets' which have not been merged together. If this is still true after Send/Receive, then you will need expert help to get things merging again.");
			}

			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("status:");
			progress.WriteMessage(GetTextFromQuery("status", 30));

			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("Log of last 100 changesets:");
			try
			{   //use glog if it is installd and enabled
				progress.WriteMessage(GetTextFromQuery("glog -l 100", 30));
			}
			catch
			{
				progress.WriteMessage(GetTextFromQuery("log -l 100", 30));
			}
			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("config:");
			progress.WriteMessage(GetTextFromQuery("showconfig", 30));
			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("manifest:");
			progress.WriteMessage(GetTextFromQuery("manifest", 30));
			progress.WriteMessage("---------------------------------------------------");


			progress.WriteMessage(".hgignore");
			try
			{
				progress.WriteMessage(File.ReadAllText(Path.Combine(_pathToRepository, ".hgignore")));
			}
			catch
			{
				progress.WriteMessage("No .hgignore found");
			}
			progress.WriteMessage("---------------------------------------------------");

			progress.WriteMessage("hgrc");
			try
			{
				progress.WriteMessage(File.ReadAllText(Path.Combine(Path.Combine(_pathToRepository, ".hg"), "hgrc")));
			}
			catch
			{
				progress.WriteError("No .hg/hgrc found");
			}

			CheckIntegrity(progress);

			progress.WriteMessage("Done.");
		}

		public enum IntegrityResults { Good, Bad }

		public enum UpdateResults { Success, AlreadyOnIt, NoSuchBranch, NoSuchRevision, NoCommitsInRepository }

		public IntegrityResults CheckIntegrity(IProgress progress)
		{
			progress.WriteMessage("Validating Repository... (this can take a long time)");
			var result = GetTextFromQuery("verify", 60 * 60);
			if (result.ToLower().Contains("error"))
			{
				progress.WriteError(result);
				return IntegrityResults.Bad;
			}
			else
			{
				progress.WriteMessage(result);
				return IntegrityResults.Good;
			}
		}

		public string GetLog(int maxChangeSetsToShow)
		{
			return GetTextFromQuery(maxChangeSetsToShow > 0 ? $"log -G -l {maxChangeSetsToShow}" : "log -G");
		}

		public void SetupEndOfLineConversion(IEnumerable<string> extensionsOfKnownTextFileTypes)
		{
			var doc = GetMercurialConfigForRepository();
			doc.Sections.RemoveSection("encode");//clear it out
			var section = doc.Sections.GetOrCreate("encode");
			foreach (string extension in extensionsOfKnownTextFileTypes)
			{
				string ext = extension.TrimStart(new char[] { '.' });
				section.Set("**." + ext, "dumbencode:");
			}
			doc.SaveAndThrowIfCannot();
		}

		/// <summary>
		/// NB: this adds a new changeset
		/// </summary>
		public void TagRevision(string revisionNumber, string tag)
		{
			CheckAndUpdateHgrc();
			Execute(false, SecondsBeforeTimeoutOnLocalOperation, "tag -f -r " + revisionNumber + " \"" + tag + "\"");
		}

		internal static string EscapeDoubleQuotes(string message)
		{
			return message.Replace("\"", "\\\"");
		}

		internal static string SurroundWithQuotes(string path)
		{
			return "\"" + EscapeDoubleQuotes(path) + "\"";
		}

		/// <summary>
		/// Does a backout of the specified revision, which must be the head of its branch
		/// (this simplifies things, because we don't have to worry about non-trivial merging of the
		/// backout changeset).
		/// Afterwards, the current head will be the backout revision.
		/// </summary>
		/// <returns>The local revision # of the backout changeset (which will always be tip)</returns>
		public string BackoutHead(string revisionNumber, string changeSetSummary)
		{
			if (GetHasOneOrMoreChangeSets())
			{
				Guard.Against(!GetIsHead(revisionNumber), "BackoutHead() requires that the specified revision be a head, because this is the only scenario which is handled and unit-tested.");

				var previousRevisionOfWorkingDir = GetRevisionWorkingSetIsBasedOn();

				Update(revisionNumber);//move over to this branch, if necessary

				Execute(false, SecondsBeforeTimeoutOnLocalOperation,
							string.Format("backout -r {0} -m \"{1}\"", revisionNumber, changeSetSummary));
				//if we were not backing out the "current" revision, move back over to it.
				if (!previousRevisionOfWorkingDir.GetMatchesLocalOrHash(revisionNumber))
				{
					Update(previousRevisionOfWorkingDir.Number.Hash);
				}
				return GetTip().Number.LocalRevisionNumber;
			}
			else //hg cannot "backout" the very first revision
			{
				//it's not clear what I should do
				throw new ApplicationException("Cannot backout the very first changeset.");
			}
		}

		private bool GetIsHead(string localOrHashNumber)
		{
			return GetHeads().Any(h => h.Number.LocalRevisionNumber == localOrHashNumber || h.Number.Hash == localOrHashNumber);
		}

		private bool GetHasOneOrMoreChangeSets()
		{
			return GetTip() != null;
		}

		/// <summary>
		/// Mercurial gives us a way to set proxy info in the hgrc or ini files, but that
		/// a) has to be noticed and set up prior to Send/Receive and
		/// b) may go in and out of correctness, as the user travels between network connections.
		/// c) leaves the credentials stored in clear text on the hard drive
		///
		/// So for now, we're going to see how it works out there in the world if we just always
		/// handle this ourselves, never paying attention to the hgrc/mercurial.ini
		/// </summary>
		public string GetProxyConfigParameterString(string httpUrl)
		{
			if (!_haveLookedIntoProxySituation && !GetIsLocalUri(httpUrl))
			{
				_proxyCongfigParameterString = DoWorkOfDeterminingProxyConfigParameterString(httpUrl, _progress);
				_haveLookedIntoProxySituation = true;
			}
			return _proxyCongfigParameterString;
		}


		public static string DoWorkOfDeterminingProxyConfigParameterString(string httpUrl, IProgress progress)
		{
			/* The hg url itself would be more robust for the theoretical possibility of different
				* proxies for different destinations, but some hg servers (notably language depot) require a login.
				* So we're ignoring what we were given, and just using a known address, for now.
				*/
			httpUrl = "http://proxycheck.palaso.org";

			progress.WriteVerbose("Checking for proxy by trying to http-get {0}...", httpUrl);

			try
			{
				//The following, which comes from the palaso library, will take care of remembering, between runs,
				//what credentials the user entered.  If they are needed but are missing or don't work,
				//it will put a dialog asking for them.
				string hostAndPort;
				string userName;
				string password;
				if(!RobustNetworkOperation.DoHttpGetAndGetProxyInfo(httpUrl, out hostAndPort, out userName, out password,
					msg=>progress.WriteVerbose(msg)))
				{
					return string.Empty;
				}
				else
				{
					return MakeProxyConfigParameterString(hostAndPort.Replace("http://",""), userName, password);
				}
			}
			catch(Exception e)
			{
				progress.WriteWarning("Failed to determine if we need to use authentication for a proxy...");
				progress.WriteException(e);
				return " ";// space is safer when appending params than string.Empty;
			}
		}

		private static string MakeProxyConfigParameterString(string proxyHostAndPort, string proxyUserName, string proxyPassword)
		{
			var builder = new StringBuilder();
			builder.AppendFormat(" --config \"http_proxy.host={0}\" ", proxyHostAndPort);
			if(!string.IsNullOrEmpty(proxyUserName))
			{
				builder.AppendFormat(" --config \"http_proxy.user={0}\" ", proxyUserName);

				if (!string.IsNullOrEmpty(proxyPassword))
				{
					builder.AppendFormat(" --config \"http_proxy.passwd={0}\" ", proxyPassword);
				}
			}

			return builder.ToString();
		}


		/// <summary>
		/// Tells whether is looks like we have enough information to attempt an internet send/receive
		/// </summary>
		/// <param name="message">An english string which will convey the readiness status</param>
		/// <returns></returns>
		public bool GetIsReadyForInternetSendReceive(out string message)
		{
			var address = GetDefaultNetworkAddress<HttpRepositoryPath>();

			if (address==null || string.IsNullOrEmpty(address.URI))
			{
				message = LocalizationManager.GetString("GetInternetStatus.AddressIsEmpty", "The address of the server is empty.");
				return false;
			}

			Uri uri;
			if (!Uri.TryCreate(address.URI, UriKind.Absolute, out uri))
			{
				message = LocalizationManager.GetString("GetInternetStatus.AddressHasProblems", "The address of the server has problems.");
				return false;
			}

			if (string.IsNullOrEmpty(uri.PathAndQuery))
			{
				message = string.Format(
					LocalizationManager.GetString("GetInternetStatus.ProjectNameIsMissing", "The project name at {0} is missing."), uri.Host);
				return false;
			}

			if (string.IsNullOrEmpty(Settings.Default.LanguageForgeUser))
			{
				message = LocalizationManager.GetString("GetInternetStatus.AccountNameIsMissing", "The account name is missing.");
				return false;
			}

			if (string.IsNullOrEmpty(ServerSettingsModel.PasswordForSession))
			{
				message = string.Format(
					LocalizationManager.GetString("GetInternetStatus.PasswordIsMissing", "The password for {0} is missing."), uri.Host);
				return false;
			}

			message = string.Format(
				LocalizationManager.GetString("GetInternetStatus.ReadyToSR", "Ready to send/receive to {0} with project '{1}' and user '{2}'"),
				uri.Host, uri.PathAndQuery.Trim('/'), Settings.Default.LanguageForgeUser);

			return true;
		}

		public static string GetAliasFromPath(string path)
		{
			return path.Replace(@":\", "_") //   ":\" on the left side of an assignment messes up the hgrc reading, becuase colon is an alternative to "=" here
			.Replace(":", "_") // catch one without a slash
			.Replace("=", "_"); //an = in the path would also mess things up
		}

		public bool Unbundle(string bundlePath)
		{
			CheckAndUpdateHgrc();
			string command = string.Format("unbundle \"{0}\"", bundlePath);
			string result = GetTextFromQuery(command);
			if (result.Contains("adding changesets"))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// In late 2011, we added the fixutf8 extension on windows, which preserves file names requiring unicode. However, if files were previously put
		/// into the repo with messed-up names (because hg by default does some western encoding), this is supposed to detect the name change and fix them.
		/// http://mercurial.selenic.com/wiki/FixUtf8Extension
		/// </summary>
		public void FixUnicodeAudio()
		{
			CheckAndUpdateHgrc();
			ExecuteErrorsOk("addremove -s 100 -I **.wav", SecondsBeforeTimeoutOnLocalOperation);
		}

		private static string GetUniqueFolderPath(IProgress progress, string proposedTargetDirectory)
		{
			// proposedTargetDirectory and actualTarget may be the same, or actualTarget may have 1 (or higher) appended to it.
			var uniqueTarget = GetUniqueFolderPath(progress,
														 "Could not use folder {0}, since it already exists. Using new folder {1}, instead.",
														 proposedTargetDirectory);
			progress.WriteMessage("Creating new repository at " + uniqueTarget);
			return uniqueTarget;
		}

		/// <summary>
		/// Ensure a local clone is going into a uniquely named and nonexistent folder.
		/// </summary>
		/// <returns>The original folder name, or one similar to it, but with a counter digit appended to to it to make it unique.</returns>
		public static string GetUniqueFolderPath(IProgress progress, string formattableMessage, string targetDirectory)
		{
			if (Directory.Exists(targetDirectory) && DirectoryHelper.GetSafeDirectories(targetDirectory).Length == 0 && Directory.GetFiles(targetDirectory).Length == 0)
			{
				// Empty folder, so delete it, so the clone can be made in the original folder, rather than in another with a 1 after it.
				Directory.Delete(targetDirectory);
			}

			var uniqueTarget = PathHelper.GetUniqueFolderPath(targetDirectory);
			if (targetDirectory != uniqueTarget)
				progress.WriteWarning(string.Format(formattableMessage, targetDirectory, uniqueTarget));

			return uniqueTarget; // It may be the original, if it was unique.
		}

		public bool IsInitialized
		{
			get
			{
				return Directory.Exists(Path.Combine(_pathToRepository, ".hg"));
			}
		}
	}
}
