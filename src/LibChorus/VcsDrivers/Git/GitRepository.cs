using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Palaso.Providers;

#if notyet
namespace Chorus
{
	public partial class GitRepository
	{
		protected readonly string _pathToRepository;
		protected readonly string _userName;


		protected RevisionDescriptor GetMyHead()
		{
//            using (new ConsoleProgress("Getting real head of {0}", _userName))
//            {
//                string result = GetTextFromQuery(_pathToRepository, "identify -nib");
//                string[] parts = result.Split(new char[] {' ','(',')'}, StringSplitOptions.RemoveEmptyEntries);
//                Revision descriptor = new Revision(parts[2],parts[1], parts[0], "unknown");
//
//                return descriptor;
//            }
		}


		public static GitRepository CreateNewDirectoryAndRepository(string parentDirOfNewRepository, string userName)
		{
		 //   string repositoryPath = MakeDirectoryForUser(parentDirOfNewRepository, userName);
			string repositoryPath = Path.Combine(parentDirOfNewRepository, userName);

			using (new ConsoleProgress("Creating {0} from scratch", userName))
			{
				Execute("init", null, SurroundWithQuotes(repositoryPath));
				SetupPerson(repositoryPath, userName);

				GitRepository repo = new GitRepository(repositoryPath, userName);
				repo.AddFake();
				return repo;
			}
		}

		private void AddFake()
		{
				//hack to force a changeset
				string fake = Path.Combine(_pathToRepository, _userName + "_fake");
				//hack
				File.WriteAllText(fake, DateTimeProvider.Current.Now.Ticks.ToString().Substring(14));
				AddAndCheckinFile(fake);
		}

		protected void UpdateFake()
		{
			//hack to force a changeset
			string fake = Path.Combine(_pathToRepository, _userName + "_fake");
			//hack
			File.WriteAllText(fake, DateTimeProvider.Current.Now.Ticks.ToString().Substring(14));
		}

		public static GitRepository CreateNewByCloning(GitRepository sourceRepo, string parentDirOfNewRepository, string newPersonName)
		{
			string repositoryPath = Path.Combine(parentDirOfNewRepository, newPersonName );
			using (new ConsoleProgress("Creating {0} from {1}", newPersonName, sourceRepo.UserName))
			{
				Execute("clone", null, sourceRepo.PathWithQuotes + " " + SurroundWithQuotes(repositoryPath));
				SetupPerson(repositoryPath, newPersonName);
				GitRepository repository = new GitRepository(repositoryPath, newPersonName);
				repository.AddFake();
				return repository;
			}
		}

		private static string MakeDirectoryForUser(string parentDirOfNewRepository, string userName)
		{
			string repositoryPath = Path.Combine(parentDirOfNewRepository, userName);
			System.IO.Directory.CreateDirectory(repositoryPath);
			return repositoryPath;
		}



		public GitRepository(string pathToRepository, string userName /*todo: figure this out from the repo*/)
		{
			_pathToRepository = pathToRepository;
			_userName = userName; //todo: figure out the user name from the repo

//            _progress.WriteMessage(
//                "ATTENTION: if the real kdiff3.exe comes up while running this, you're doomed.  Find it and rename it.");
		}



		static private void SetupPerson(string pathToRepository, string userName)
		{
			using (new ConsoleProgress("setting name and branch"))
			{
				Execute("config", pathToRepository, "user.name " + userName);
				Execute("branch", pathToRepository, userName, " -f");

			}
		}


		protected void PullFromRepository(GitRepository otherRepository)
		{
			using (new ConsoleProgress("{0} pulling from {1}", _userName,otherRepository.UserName))
			{
				Execute("pull", _pathToRepository, otherRepository.PathWithQuotes);
			}
		}


		private List<RevisionDescriptor> GetBranches()
		{
			string what= "branch -v";
			using (new ConsoleProgress("Getting {0} of {1}", what, _userName))
			{
				string result = GetTextFromQuery(_pathToRepository, what);

				string[] lines = result.Split('\n');
				List<RevisionDescriptor> branches = new List<RevisionDescriptor>();
				foreach (string line in lines)
				{
					if (line.Trim() == "")
						continue;
//TODO          * master c3feafe first
//                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//                    if (parts.Length < 2)
//                        continue;
//                    string[] revisionParts = parts[1].Split(':');
//                    branches.Add(new Revision(parts[0], revisionParts[0], revisionParts[1], "unknown"));
				}
				return branches;
			}
		}

//        private Revision GetTip()
//        {
//            return GetRevisionsFromQuery("tip")[0];
//        }

		protected List<RevisionDescriptor> GetHeads()
		{
			using (new ConsoleProgress("Getting heads of {0}", _userName))
			{
				return GetRevisionsFromQuery("show head --pretty=format:\"***** %h, %cn, %s\"");
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

		public void Commit(bool forceCreationOfChangeSet, string message, params object[] args)
		{
			if (forceCreationOfChangeSet)
			{
				UpdateFake();
			}
			message = string.Format(message, args);
			using (new ConsoleProgress("{0} committing with comment: {1}", _userName, message))
			{
				ExecutionResult result = Execute("ci", _pathToRepository, "-m " + SurroundWithQuotes(_userName + ": " + message));
				_progress.WriteMessage(result.StandardOutput);
				if (forceCreationOfChangeSet && result.StandardOutput.Contains("nothing changed"))
				{
					throw new ApplicationException("Did not get the commit we needed.");
				}

				//nothing changed
				if (!string.IsNullOrEmpty(result.StandardError))
					_progress.WriteMessage(result.StandardError);
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


		protected static ExecutionResult ExecuteErrorsOk(string command)
		{
			_progress.WriteMessage("git "+command);

			return WrapShellCall.WrapShellCallRunner.Run("git " + command);
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
			//for "git cat" (surprisingly), the relative path isn't relative to the start of the repo, but to the current
			// directory.
			string absolutePathToFile = SurroundWithQuotes(Path.Combine(_pathToRepository, fileRelativePath));

			Execute("is it show?", _pathToRepository, "-o ",fullOutputPath," -r ",revision,absolutePathToFile);
		}
	}
}
#endif