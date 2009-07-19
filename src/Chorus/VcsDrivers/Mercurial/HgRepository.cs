using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chorus.retrieval;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.merge;
using Nini.Ini;

namespace Chorus.VcsDrivers.Mercurial
{

	public class HgRepository : IRetrieveFile
	{
		protected readonly string _pathToRepository;
		protected readonly string _userName;
		protected IProgress _progress;

		public static string GetEnvironmentReadinessMessage(string messageLanguageId)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = "hg";
			startInfo.Arguments = "version";
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			try
			{
				System.Diagnostics.Process.Start(startInfo);
			}
			catch(Exception)
			{
				 return "Sorry, this feature requires the Mercurial version control system.  It must be installed and part of the PATH environment variable.  Windows users can download and install TortoiseHg";
			}
			return null;
		}

//        protected Revision GetMyHead()
//        {
//            using (new ConsoleProgress("Getting real head of {0}", _userName))
//            {
////                string result = GetTextFromQuery(_pathToRepository, "identify -nib");
////                string[] parts = result.Split(new char[] {' ','(',')'}, StringSplitOptions.RemoveEmptyEntries);
////                Revision descriptor = new Revision(this, parts[2],parts[1], parts[0], "unknown");
//
//
//                return descriptor;
//            }
//        }


		public HgRepository(string pathToRepository, IProgress progress)
		{
			_pathToRepository = pathToRepository;
			_progress = progress;
			_userName = GetUserIdInUse();
		}

		static protected void SetupPerson(string pathToRepository, string userName)
		{
			using (new ConsoleProgress("setting name and branch"))
			{
				using (new ShortTermEnvironmentalVariable("HGUSER", userName))
				{
					Execute("branch", pathToRepository, userName);
				}
			}
		}

		public void TryToPull(string resolvedUri)
		{
			HgRepository repo = new HgRepository(resolvedUri, _progress);
			PullFromRepository(repo, false);
		}

		public void Push(string targetUri, IProgress progress, SyncResults results)
		{
			using (new ConsoleProgress("{0} pushing to {1}", _userName, targetUri))
			{
				try
				{
					Execute("push", _pathToRepository, SurroundWithQuotes(targetUri));
				}
				catch (Exception err)
				{
					_progress.WriteWarning("Could not push to " + targetUri + Environment.NewLine + err.Message);
				}
				try
				{
					Execute("update", targetUri, "-C"); // for usb keys and other local repositories
				}
				catch (Exception err)
				{
					_progress.WriteWarning("Could not update the actual files after a pull at " + targetUri + Environment.NewLine + err.Message);
				}
			}
		}

		protected void PullFromRepository(HgRepository otherRepo,bool throwIfCannot)
		{
			_progress.WriteStatus("{0} pulling from {1}", _userName,otherRepo.Name);
			//using (new ConsoleProgress("{0} pulling from {1}", _userName,otherRepo.Name))
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


		private List<Revision> GetBranches()
		{
			string what= "branches";
			using (new ConsoleProgress("Getting {0} of {1}", what, _userName))
			{
				string result = GetTextFromQuery(_pathToRepository, what);

				string[] lines = result.Split('\n');
				List<Revision> branches = new List<Revision>();
				foreach (string line in lines)
				{
					if (line.Trim() == "")
						continue;

					string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length < 2)
						continue;
					string[] revisionParts = parts[1].Split(':');
					branches.Add(new Revision(this, parts[0], revisionParts[0], revisionParts[1], "unknown"));
				}
				return branches;
			}
		}

		public Revision GetTip()
		{
			var rev= GetRevisionsFromQuery("tip").FirstOrDefault();
			if(rev==null || rev.Number.LocalRevisionNumber == "-1")
				return null;
			return rev;
		}

		public List<Revision> GetHeads()
		{
			using (new ConsoleProgress("Getting heads of {0}", _userName))
			{
				return GetRevisionsFromQuery("heads");
			}
		}


		protected static string GetTextFromQuery(string repositoryPath, string s)
		{
			ExecutionResult result= ExecuteErrorsOk(s + " -R " + SurroundWithQuotes(repositoryPath));
			Debug.Assert(string.IsNullOrEmpty(result.StandardError), result.StandardError);
			return result.StandardOutput;
		}
		protected static string GetTextFromQuery(string s)
		{
			ExecutionResult result = ExecuteErrorsOk(s);
			//TODO: we need a way to get this kind of error back the devs for debugging
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
			//enhance: this is normally going to be redundant, as we always use the same branch.
			//but it does it set the first time, and handles the case where the user's account changes (either
			//because they've logged in as a different user, or changed the name of a their account.

			//NB: I (JH) and not yet even clear we need branches, and it makes reading the tree somewhat confusing
			//If Bob merges with Sally, his new "tip" can very well be labelled "Sally".

			//disabled because then Update failed to get the latest, if it was the other user's branch
			//      Branch(_userName);

			message = string.Format(message, args);
			using (new ConsoleProgress("{0} committing with comment: {1}", _userName, message))
			{
				ExecutionResult result = Execute("ci", _pathToRepository, "-m " + SurroundWithQuotes(message));
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
				var details = "\r\n" + "hg Command was " + "\r\n" +  b.ToString();
				try
				{
					details += "\r\nhg version was \r\n" + GetTextFromQuery("version");
				}
				catch (Exception)
				{
					details += "\r\nCould not get HG VERSION";

				}

				if (!string.IsNullOrEmpty(result.StandardError))
				{
					throw new ApplicationException(result.StandardError + details);
				}
				else
				{
					throw new ApplicationException("Got return value " + result.ExitCode + details);
				}
			}
			return result;
		}

		protected static ExecutionResult ExecuteErrorsOk(string command, string fromDirectory)
		{
			//    _progress.WriteMessage("hg "+command);

			return HgRunner.Run("hg " + command, fromDirectory);
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
				Execute("update", _pathToRepository, "-C");
			}
		}

		public void Update(string revision)
		{
			using (new ConsoleProgress("{0} updating (making working directory contain) revision {1}", _userName, revision))
			{
				Execute("update", _pathToRepository, "-r", revision, "-C");
			}
		}

//        public void GetRevisionOfFile(string fileRelativePath, string revision, string fullOutputPath)
//        {
//            //for "hg cat" (surprisingly), the relative path isn't relative to the start of the repo, but to the current
//            // directory.
//            string absolutePathToFile = SurroundWithQuotes(Path.Combine(_pathToRepository, fileRelativePath));
//
//            Execute("cat", _pathToRepository, "-o ",fullOutputPath," -r ",revision,absolutePathToFile);
//        }

		public static void CreateRepositoryInExistingDir(string path)
		{
			Execute("init", null, SurroundWithQuotes(path));
		}


		/// <summary>
		/// note: intentionally does not commit afterwards
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="results"></param>
		public IList<string> MergeHeads(IProgress progress, SyncResults results)
		{
			List<string> peopleWeMergedWith= new List<string>();

			List<Revision> heads = GetHeads();
			Revision myHead = GetRevisionWorkingSetIsBasedOn();
			foreach (Revision head in heads)
			{
				MergeSituation.PushRevisionsToEnvironmentVariables(myHead.UserId, myHead.Number.LocalRevisionNumber, head.UserId, head.Number.LocalRevisionNumber);

				MergeOrder.PushToEnvironmentVariables(_pathToRepository);
				if (head.Number.LocalRevisionNumber != myHead.Number.LocalRevisionNumber)
				{
					progress.WriteStatus("Merging with {0}...", head.UserId);
					RemoveMergeObstacles(myHead, head);
					bool didMerge = MergeTwoChangeSets(myHead, head);
					if (didMerge)
					{
						peopleWeMergedWith.Add(head.UserId);
					}
				}
			}

			return peopleWeMergedWith;
		}

		 /// <summary>
		/// There may be more, but for now: take care of the case where one guy has a file not
		/// modified (and not checked in), and the other guy is going to hammer it (with a remove
		/// or change).
		/// </summary>
		private void RemoveMergeObstacles(Revision rev1, Revision rev2)
		{
			 var files = GetFilesInRevisionFromQuery(rev1 /*this param is bogus*/, "status -ru --rev " + rev2.Number.LocalRevisionNumber);
			 foreach (var file in files)
			 {
				 if (file.ActionThatHappened == FileInRevision.Action.Unknown)
				 {
					 if (files.Any(f => f.FullPath == file.FullPath))
					 {
						 //string newPath=string.Empty;
//                         foreach (var suffix in new string[] {"", ".1", ".2", ".3", ".4", ".5" })
//                         {
//                             newPath = file.FullPath + suffix + ".chorusRescue";//intentionally changing the extension, so it won't get checked in
//                             try
//                             {
//                                 File.Move(file.FullPath, newPath);
//                                 break;
//                             }
//                             catch (Exception error)
//                             {
//                                 _progress.WriteWarning("Could not copy {0} to {1}", file.FullPath, newPath);
//                             }
//                             ///arggghhh... they're all in use *and locked*!  try a random name
//                             File.Move(file.FullPath, Path.GetRandomFileName()+".chorusRescue");
//                         }
						 var newPath = file.FullPath+"-"+Path.GetRandomFileName() + ".chorusRescue";
						 File.Move(file.FullPath, newPath);
						 _progress.WriteWarning("Renamed {0} to {1} because it is not part of {2}'s repository but it is part of {3}'s, and this would otherwise prevent a merge.", file.FullPath, Path.GetFileName(newPath), rev1.UserId, rev2.UserId);
					 }
				 }
			 }
		}



		private bool MergeTwoChangeSets(Revision head, Revision theirHead)
		{
			ExecutionResult result = null;
			using (new ShortTermEnvironmentalVariable("HGMERGE", Path.Combine(Other.DirectoryOfExecutingAssembly, "ChorusMerge.exe")))
			{
				using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.TheyWin.ToString()))

				{
					result = Execute(true, "merge", _pathToRepository, "-r", theirHead.Number.LocalRevisionNumber);
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
					_progress.WriteError(result.StandardError);
					return false;
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

			args.Append(" -I " + SurroundWithQuotes(Path.Combine(this._pathToRepository, "**.conflicts")));
			args.Append(" -I " + SurroundWithQuotes(Path.Combine(this._pathToRepository, "**.conflicts.txt")));
			args.Append(" -X " + SurroundWithQuotes(Path.Combine(this._pathToRepository, "**.chorusRescue")));

			foreach (string pattern in excludePatterns)
			{
				//this fails:   hg add -R "E:\Users\John\AppData\Local\Temp\ChorusTest"  -X "**/cache"
				//but this works  -X "E:\Users\John\AppData\Local\Temp\ChorusTest/**/cache"
				string p = Path.Combine(this._pathToRepository, pattern);
				args.Append(" -X " + SurroundWithQuotes(p));
			}

			//enhance: what happens if something is covered by the exclusion pattern that was previously added?  Will the old
			// version just be stuck on the head? NB: to remove a file from the checkin but not delete it, do "hg remove -Af"

			if (GetIsAtLeastOneMissingFileInWorkingDir())
			{
				using (new ConsoleProgress("At least one file was removed from the working directory.  Telling Hg to record the deletion."))
				{
					Execute("rm -A", _pathToRepository);
				}
			}
			using (new ConsoleProgress("Adding files to be tracked ({0}", args.ToString()))
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


		private void PrintHeads(List<Revision> heads, Revision myHead)
		{
			_progress.WriteMessage("Current Heads:");
			foreach (Revision head in heads)
			{
				if (head.Number.LocalRevisionNumber == myHead.Number.LocalRevisionNumber)
				{
					_progress.WriteMessage("  ME {0} {1} {2}", head.UserId, head.Number.LocalRevisionNumber, head.Summary);
				}
				else
				{
					_progress.WriteMessage("      {0} {1} {2}", head.UserId, head.Number.LocalRevisionNumber, head.Summary);
				}
			}
		}

		public void Clone(string path)
		{
			Execute("clone", null, PathWithQuotes + " " + SurroundWithQuotes(path));
		}

		private List<Revision> GetRevisionsFromQuery(string query)
		{
			string result = GetTextFromQuery(_pathToRepository, query);
			return GetRevisionsFromQueryResultText(result);
		}

 /*       private static List<Revision> GetRevisionsFromQueryOutput(string result)
		{
			//Debug.WriteLine(result);
			string[] lines = result.Split('\n');
			List<Dictionary<string, string>> rawChangeSets = new List<Dictionary<string, string>>();
			Dictionary<string, string> rawChangeSet = null;
			foreach (string line in lines)
			{
				if (line.StartsWith("changeset:"))
				{
					rawChangeSet = new Dictionary<string, string>();
					rawChangeSets.Add(rawChangeSet);
				}
				string[] parts = line.Split(new char[] { ':' });
				if (parts.Length < 2)
					continue;
				//join all but the first back together
				string contents = string.Join(":", parts, 1, parts.Length - 1);
				rawChangeSet[parts[0].Trim()] = contents.Trim();
			}

			List<Revision> revisions = new List<Revision>();
			foreach (Dictionary<string, string> d in rawChangeSets)
			{
				string[] revisionParts = d["changeset"].Split(':');
				string summary = string.Empty;
				if (d.ContainsKey("summary"))
				{
					summary = d["summary"];
				}
				Revision revision = new Revision(d["user"], revisionParts[0], /*revisionParts[1]/"unknown", summary);
				if (d.ContainsKey("tag"))
				{
					revision.Tag = d["tag"];
				}
				revisions.Add(revision);

			}
			return revisions;
		}
*/
		public List<Revision> GetAllRevisions()
		{
			/*
				changeset:   0:7ee3570760cd
				tag:         tip
				user:        hattonjohn@gmail.com
				date:        Wed Jul 02 16:40:26 2008 -0600
				summary:     bob: first one
			 */

			string result = GetTextFromQuery(_pathToRepository, "log");
			return GetRevisionsFromQueryResultText(result);
		}

		public List<Revision> GetRevisionsFromQueryResultText(string queryResultText)
		{
			TextReader reader = new StringReader(queryResultText);
			string line = reader.ReadLine();


			List<Revision> items = new List<Revision>();
			Revision item = null;
			while(line !=null)
			{
				int colonIndex = line.IndexOf(":");
				if(colonIndex >0 )
				{
					string label = line.Substring(0, colonIndex);
					string value = line.Substring(colonIndex + 1).Trim();
					switch (label)
					{
						default:
							break;
						case "changeset":
							item = new Revision(this);
							items.Add(item);
							item.SetRevisionAndHashFromCombinedDescriptor(value);
							break;
						case "parent":
							item.AddParentFromCombinedNumberAndHash(value);
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
			return items;
		}

		/// <summary>
		/// If we checked in now, which revision would be the parent?
		/// </summary>
		public Revision GetRevisionWorkingSetIsBasedOn()
		{
			return GetRevisionsFromQuery("parents").FirstOrDefault();
		}

		[Obsolete("Use the non-static member instead... this is just here for the old partial merger")]
		public static void SetUserId(string path, string userId)
		{
			var hg = new HgRepository(path, new NullProgress());
			hg.SetUserNameInIni(userId, new NullProgress());
		  //Environment.SetEnvironmentVariable("hguser", userId);
		  //defunct Execute("config", path, "--local ui.username " + userId);

		}

		public string GetUserIdInUse()
		{
			return GetUserNameFromIni(_progress);
//this gave the global name, we want the name associated with this repository
			//return GetTextFromQuery(_pathToRepository, "showconfig ui.username").Trim();
		}

		public bool GetFileExistsInRepo(string subPath)
		{
			string result = GetTextFromQuery(_pathToRepository, "locate " + subPath);
			return !String.IsNullOrEmpty(result.Trim());
		}
		public bool GetIsAtLeastOneMissingFileInWorkingDir()
		{
			string result = GetTextFromQuery(_pathToRepository, "status -d ");
			return !String.IsNullOrEmpty(result.Trim());
		}

		/// <summary>
		///  From IRetrieveFile
		/// </summary>
		/// <returns>path to a temp file. caller is responsible for deleting the file.</returns>
		public string RetrieveHistoricalVersionOfFile(string relativePath, string revOrHash)
		{
			Guard.Against(string.IsNullOrEmpty(revOrHash), "The revision cannot be empty (note: the first revision has an empty string for its parent revision");
			var f =  TempFile.CreateWithExtension(Path.GetExtension(relativePath));

			var cmd = string.Format("cat -o \"{0}\" -r {1} \"{2}\"", f.Path, revOrHash, relativePath);
			ExecutionResult result = ExecuteErrorsOk(cmd, _pathToRepository);
			if(!string.IsNullOrEmpty(result.StandardError.Trim()))
			{
				throw new ApplicationException(String.Format("Could not retrieve version {0} of {1}. Mercurial said: {2}", revOrHash, relativePath, result.StandardError));
			}
			return f.Path;
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
						if(file.ActionThatHappened != FileInRevision.Action.Unknown)
							files.Add(file);
					}
				}
			}

			return files;
		}



		private List<FileInRevision> GetFilesInRevisionFromQuery(Revision revisionToAssignToResultingFIRs, string query)
		{
			var result = GetTextFromQuery(_pathToRepository,query);
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

				revisions.Add(new FileInRevision(revisionToAssignToResultingFIRs.Number.LocalRevisionNumber, Path.Combine(PathToRepo, line.Substring(2)), action));
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
			return from x in  GetRevisionsFromQuery("parent -r " + localRevisionNumber)
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

		public IEnumerable<RepositoryPath> GetKnownPeerRepositories()
		{
//            //TODO: we actually only one the ones in the repo, but this
//            //will give us global ones as well
//            var r =GetTextFromQuery(_pathToRepository, "paths");
//            var lines = r.Split('\n');
//            foreach (var line in lines)
//            {
//                var parts = line.Split('=');
//                if(parts.Length != 2)
//                    continue;
//                yield return RepositoryPath.Create(parts[1].Trim(), parts[0].Trim(), false);
//            }

			var section = GetHgrcDoc().Sections.GetOrCreate("paths");
			foreach (var name in section.GetKeys())
			{
				var uri = section.GetValue(name);
				yield return RepositoryPath.Create(uri, name, false);
			}
		}


		/// <summary>
		/// TODO: sort out this vs. the UserName property
		/// </summary>
		/// <returns></returns>
		public string GetUserNameFromIni(IProgress progress)
		{
			try
			{
				var doc = GetHgrcDoc();
				var x = doc.Sections["ui"].GetValue("username");
				return doc.Sections["ui"].GetValue("username");
			}
			catch (Exception error)
			{
				progress.WriteWarning("Could not retrieve the user name from the hgrc ini file: "+ error.Message);
				return string.Empty;
			}
		}

		private IniDocument GetHgrcDoc()
		{
			var p = Path.Combine(Path.Combine(_pathToRepository, ".hg"), "hgrc");
			if (!File.Exists(p))
			{
				File.WriteAllText(p,"");
			}
			return new Nini.Ini.IniDocument(p, IniFileType.MercurialStyle);
		}

		public void SetUserNameInIni(string name, IProgress progress)
		{
			try
			{
				var doc = GetHgrcDoc();
				doc.Sections.GetOrCreate("ui").Set("username",name);
				doc.Save();
			}
			catch (Exception error)
			{
				progress.WriteWarning("Could not set the user name from the hgrc ini file: " + error.Message);
			}
		}

		public void SetKnownPeerAddresses(List<RepositoryAddress> addresses)
		{
			var doc = GetHgrcDoc();
			doc.Sections.Remove("paths");//clear it out
			var section = doc.Sections.GetOrCreate("paths");
			foreach (var address in addresses)
			{
				section.Set(address.Name, address.URI);
			}
			doc.Save();
		}


		public Revision GetRevision(string numberOrHash)
		{
			return GetRevisionsFromQuery("log --rev " + numberOrHash).FirstOrDefault();
		}
	}

	public class RepositoryAddress
	{
		public string Name { get; set; }
		public string URI { get; set; }

		public RepositoryAddress(string name, string uri)
		{
			Name = name;
			URI = uri;
		}
	}
}