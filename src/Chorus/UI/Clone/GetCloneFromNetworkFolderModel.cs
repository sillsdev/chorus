using System;
using System.Collections.Generic;
using System.IO;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Clone
{
	public class GetCloneFromNetworkFolderModel
	{
		///<summary>
		/// This serves as both the initial folder to present/search, and also the selected repository path.
		///</summary>
		public string FolderPath { get; set; }

		public IEnumerable<string> GetDirectoriesWithMecurialRepos()
		{
			throw new NotImplementedException();
		}

		///<summary>
		/// Makes a Mercurial clone of a repository from sourcePath to parentDirectoryToPutCloneIn
		///</summary>
		///<param name="sourcePath">Existing Hg repo</param>
		///<param name="parentDirectoryToPutCloneIn">Target folder for new clone</param>
		///<param name="progress">Progress indicator object</param>
		///<returns>Directory that clone was actually placed in (allows for renaming to avoid duplicates)</returns>
		public string MakeClone(string sourcePath, string parentDirectoryToPutCloneIn, IProgress progress)
		{
			return HgHighLevel.MakeCloneFromLocalToLocal(sourcePath,
														 Path.Combine(parentDirectoryToPutCloneIn, Path.GetFileName(sourcePath)),
														 true,
														 progress);
		}

		/// <summary>
		/// Decides if the given folder path is worth the hassle of examining for Hg repositories.
		/// </summary>
		/// <param name="path"></param>
		/// <returns>true if a search is a good idea</returns>
		public bool IsFolderWorthSearching(string path)
		{
			return true;
		}

		public bool IsValidRepository(string folderPath)
		{
			return Directory.Exists(folderPath + Path.DirectorySeparatorChar + @".hg");
		}
	}
}
