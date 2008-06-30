using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.merge;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgRepository
	{
		protected readonly string _pathToRepository;
		protected readonly string _userName;
		protected IProgress _progress;


		protected RevisionDescriptor GetMyHead()
		{
			using (new ConsoleProgress("Getting real head of {0}", _userName))
			{
				string result = GetTextFromQuery(_pathToRepository, "identify -nib");
				string[] parts = result.Split(new char[] {' ','(',')'}, StringSplitOptions.RemoveEmptyEntries);
				RevisionDescriptor descriptor = new RevisionDescriptor(parts[2],parts[1], parts[0], "unknown");

				return descriptor;
			}
		}


		public HgRepository(string pathToRepository, IProgress progress, string userName /*todo: figure this out from the repo*/)
		{
			_pathToRepository = pathToRepository;
			_progress = progress;
			_userName = userName; //todo: figure out the user name from the repo
		}

		static protected void SetupPerson(string pathToRepository, string userName)
		{
			using (new ConsoleProgress("setting name and branch"))
			{
				Execute("config", pathToRepository, "--local ui.username " + userName);
				Execute("branch", pathToRepository, userName);

			}
		}

		public void TryToPull(string resolvedUri, string sourceName, IProgress progress, SyncResults results)
		{
			HgRepository repo = new HgRepository(resolvedUri, progress, sourceName);
			PullFromRepository(repo, false);
		}

		public void Push(RepositorySource targetRepoInfo, IProgress progress, SyncResults results)
		{
			using (new ConsoleProgress("{0} pushing to {1}", _userName, targetRepoInfo.URI))
			{
				try
				{
					Execute("push", _pathToRepository, SurroundWithQuotes(targetRepoInfo.URI));
				}
				catch (Exception err)
				{
					_progress.WriteWarning("Could not push to " + targetRepoInfo.URI + Environment.NewLine+err.Message);
				}
			}
		}

		protected void PullFromRepository(HgRepository otherRepo,bool throwIfCannot)
		{
			using (new ConsoleProgress("{0} pulling from {1}", _userName,otherRepo.Name))
			{
				try
				{
					Execute("pull", _pathToRepository, otherRepo.PathWithQuotes);
				}
				catch (Exception err)
				{
					if (throwIfCannot)
					{
						throw err;
					}
					_progress.WriteWarning("Could not pull from " + otherRepo.Name);
				}
			}
		}


		private List<RevisionDescriptor> GetBranches()
		{
			string what= "branches";
			using (new ConsoleProgress("Getting {0} of {1}", what, _userName))
			{
				string result = GetTextFromQuery(_pathToRepository, what);

				string[] lines = result.Split('\n');
				List<RevisionDescriptor> branches = new List<RevisionDescriptor>();
				foreach (string line in lines)
				{
					if (line.Trim() == "")
						continue;

					string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length < 2)
						continue;
					string[] revisionParts = parts[1].Split(':');
					branches.Add(new RevisionDescriptor(parts[0], revisionParts[0], revisionParts[1], "unknown"));
				}
				return branches;
			}
		}

		private RevisionDescriptor GetTip()
		{
			return GetRevisionsFromQuery("tip")[0];
		}

		protected List<RevisionDescriptor> GetHeads()
		{
			using (new ConsoleProgress("Getting heads of {0}", _userName))
			{
				return GetRevisionsFromQuery("heads");
			}
		}

		private List<RevisionDescriptor> GetRevisionsFromQuery(string query)
		{
			string result = GetTextFromQuery(_pathToRepository, query);
			return RevisionDescriptor.GetRevisionsFromQueryOutput(result);
		}


		protected static string GetTextFromQuery(string repositoryPath, string s)
		{
			ExecutionResult result= ExecuteErrorsOk(s + " -R " + SurroundWithQuotes(repositoryPath));
			Debug.Assert(string.IsNullOrEmpty(result.StandardError), result.StandardError);
			return result.StandardOutput;
		}

		public void AddAndCheckinFile(string filePath)
		{
			TrackFile(filePath);
			Commit(false, " Add " + Path.GetFileName(filePath));
		}

		private void TrackFile(string filePath)
		{
			using (new ConsoleProgress("Adding {0} to the files that are tracked for {1}: ", Path.GetFileName(filePath), _userName))
			{
				Execute("add", _pathToRepository, SurroundWithQuotes(filePath));
			}
		}

		public virtual void Commit(bool forceCreationOfChangeSet, string message, params object[] args)
		{
			message = string.Format(message, args);
			using (new ConsoleProgress("{0} committing with comment: {1}", _userName, message))
			{
				ExecutionResult result = Execute("ci", _pathToRepository, "-m " + SurroundWithQuotes(_userName + ": " + message));
				_progress.WriteMessage(result.StandardOutput);
			}
		}




		public void Branch(string branchName)
		{
			using (new ConsoleProgress("{0} changing working dir to branch: {1}", _userName, branchName))
			{
				Execute("branch -f ", _pathToRepository, SurroundWithQuotes(branchName));
			}
		}

		protected static ExecutionResult Execute(string cmd, string repositoryPath, params string[] rest)
		{
			return Execute(false, cmd, repositoryPath, rest);
		}
		protected static ExecutionResult Execute(bool failureIsOk, string cmd, string repositoryPath, params string[] rest)
		{
			StringBuilder b = new StringBuilder();
			b.Append(cmd + " ");
			if (!string.IsNullOrEmpty(repositoryPath))
			{
				b.Append("-R " + SurroundWithQuotes(repositoryPath) + " ");
			}
			foreach (string s in rest)
			{
				b.Append(s + " ");
			}

			ExecutionResult result = ExecuteErrorsOk(b.ToString());
			if (0 != result.ExitCode && !failureIsOk)
			{
				if (!string.IsNullOrEmpty(result.StandardError))
				{
					throw new ApplicationException(result.StandardError);
				}
				else
				{
					throw new ApplicationException("Got return value " + result.ExitCode);
				}
			}
			return result;
		}

		protected static ExecutionResult ExecuteErrorsOk(string command, string fromDirectory)
		{
			//    _progress.WriteMessage("hg "+command);

			return WrapShellCallRunner.Run("hg " + command, fromDirectory);
		}

		protected static ExecutionResult ExecuteErrorsOk(string command)
		{
			return ExecuteErrorsOk(command, null);
		}


		protected static string SurroundWithQuotes(string path)
		{
			return "\"" + path + "\"";
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

		public string UserName
		{
			get { return _userName; }
		}

		private string Name
		{
			get { return _userName; } //enhance... location is important, too
		}

		public string GetFilePath(string name)
		{
			return Path.Combine(_pathToRepository, name);
		}

		public List<string> GetChangedFiles()
		{
			ExecutionResult result= Execute("status", _pathToRepository);
			string[] lines = result.StandardOutput.Split('\n');
			List<string> files = new List<string>();
			foreach (string line in lines)
			{
				if(line.Trim()!="")
					files.Add(line.Substring(2)); //! data.txt
			}

			return files;
		}

		public void Update()
		{
			using (new ConsoleProgress("{0} updating",_userName))
			{
				Execute("update", _pathToRepository);
			}
		}

		public void Update(string revision)
		{
			using (new ConsoleProgress("{0} updating (making working directory contain) revision {1}", _userName, revision))
			{
				Execute("update", _pathToRepository, "-r", revision, "-C");
			}
		}

		public void GetRevisionOfFile(string fileRelativePath, string revision, string fullOutputPath)
		{
			//for "hg cat" (surprisingly), the relative path isn't relative to the start of the repo, but to the current
			// directory.
			string absolutePathToFile = SurroundWithQuotes(Path.Combine(_pathToRepository, fileRelativePath));

			Execute("cat", _pathToRepository, "-o ",fullOutputPath," -r ",revision,absolutePathToFile);
		}

		public static void CreateRepositoryInExistingDir(string path)
		{
			Execute("init", null, SurroundWithQuotes(path));
		}



		public void MergeHeads(IProgress progress, SyncResults results)
		{
			RevisionDescriptor rev= GetMyHead();

			bool didMerge = false;
			List<RevisionDescriptor> heads = GetHeads();
			RevisionDescriptor myHead = GetMyHead();
			foreach (RevisionDescriptor theirHead in heads)
			{
				if (theirHead._revision != myHead._revision)
				{
					didMerge |= MergeTwoChangeSets(myHead, theirHead);
				}
			}

			Commit(false, "Merged.");
		}

		private bool MergeTwoChangeSets(RevisionDescriptor head, RevisionDescriptor theirHead)
		{
			ExecutionResult result = null;
			using (new ShortTermEnvironmentalVariable("HGMERGE", Path.Combine(Other.DirectoryOfExecutingAssembly, "ChorusMerge.exe")))
			{
				using (new ShortTermEnvironmentalVariable(MergeDispatcher.MergeOrder.kConflictHandlingModeEnvVarName, MergeDispatcher.MergeOrder.ConflictHandlingMode.TheyWin.ToString()))

				{
					result = Execute(true, "merge", _pathToRepository, "-r", theirHead._revision);
				}
			}
			if (result.ExitCode != 0)
			{
				if (result.StandardError.Contains("nothing to merge"))
				{
//                    _progress.WriteMessage("Nothing to merge, updating instead to revision {0}.", theirChangeSet._revision);
//                    Update(theirChangeSet._revision);//REVIEW
					return false;
				}
				else
				{
					throw new ApplicationException(result.StandardError);
				}
			}
			return true;
		}

		public void AddAndCheckinFiles(List<string> includePatterns, List<string> excludePatterns, string message)
		{
			StringBuilder args = new StringBuilder();
			foreach (string pattern in includePatterns)
			{
				string p = Path.Combine(this._pathToRepository, pattern);
				args.Append(" -I " + SurroundWithQuotes(p));
			}
			foreach (string pattern in excludePatterns)
			{
				//this fails:   hg add -R "E:\Users\John\AppData\Local\Temp\ChorusTest"  -X "**/cache"
				//but this works  -X "E:\Users\John\AppData\Local\Temp\ChorusTest/**/cache"
				string p = Path.Combine(this._pathToRepository, pattern);
				args.Append(" -X " + SurroundWithQuotes(p));
			}

			using (new ConsoleProgress("Adding files to be tracked."))
			{
				Execute("add", _pathToRepository, args.ToString());
			}
			using (new ConsoleProgress("Committing \"{0}\"", message))
			{
				Commit(false, message);
			}
		}

		public static string GetRepositoryRoot(string directoryPath)
		{
//            string old = Directory.GetCurrentDirectory();
//            try
//            {
			// Directory.SetCurrentDirectory(directoryPath);
			ExecutionResult result = ExecuteErrorsOk("root", directoryPath);
			if (result.ExitCode == 0)
			{
				return result.StandardOutput.Trim();
			}
			return null;
//            }
//            finally
//            {
//                Directory.SetCurrentDirectory(old);
//            }
		}


		private void PrintHeads(List<RevisionDescriptor> heads, RevisionDescriptor myHead)
		{
			_progress.WriteMessage("Current Heads:");
			foreach (RevisionDescriptor head in heads)
			{
				if (head._revision == myHead._revision)
				{
					_progress.WriteMessage("  ME {0} {1} {2}", head._name, head._revision, head._comment);
				}
				else
				{
					_progress.WriteMessage("      {0} {1} {2}", head._name, head._revision, head._comment);
				}
			}
		}

		public void Clone(string path)
		{
			Execute("clone", null, PathWithQuotes + " " + SurroundWithQuotes(path));
		}


	}
}