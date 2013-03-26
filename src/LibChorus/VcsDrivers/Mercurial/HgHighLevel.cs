using System.IO;
using Palaso.Code;
using Palaso.Progress;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgHighLevel
	{
		//TODO: get rid of this, or somehow combine it with the other Clone() options out there
		/// <returns>path to clone, or empty if it failed</returns>
		public static string MakeCloneFromLocalToLocal(string sourcePath, string targetDirectory, bool alsoDoCheckout, IProgress progress)
		{
			RequireThat.Directory(sourcePath).Exists();
			//Handled by GetUniqueFolderPath call now down in CloneLocal call. RequireThat.Directory(targetDirectory).DoesNotExist();
			RequireThat.Directory(targetDirectory).Parent().Exists();

			HgRepository local = new HgRepository(sourcePath, progress);

			if (!local.RemoveOldLocks())
			{
				progress.WriteError("Chorus could not create the clone at this time.  Try again after restarting the computer.");
				return string.Empty;
			}

			using (new ConsoleProgress("Trying to Create repository clone at {0}", targetDirectory))
			{
				targetDirectory = local.CloneLocalWithoutUpdate(targetDirectory);
				File.WriteAllText(Path.Combine(targetDirectory, "~~Folder has an invisible repository.txt"), "In this folder, there is a (possibly hidden) folder named '.hg' that contains the actual data of this Chorus repository. Depending on your Operating System settings, that leading '.' might make the folder invisible to you. But Chorus clients (WeSay, FLEx, OneStory, etc.) can see it and can use this folder to perform Send/Receive operations.");

				if (alsoDoCheckout)
				{
					// string userIdForCLone = string.Empty; /* don't assume it's this user... a repo on a usb key probably shouldn't have a user default */
					var clone = new HgRepository(targetDirectory, progress);
					var tip = clone.GetTip();
					if (tip != null)
					{
						clone.Update(tip.Number.LocalRevisionNumber);
					}
					//else Skip update for repo with no revisions.
				}
				return targetDirectory;
			}
		}
	}
}