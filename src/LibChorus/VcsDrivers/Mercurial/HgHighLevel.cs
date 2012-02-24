using System.IO;
using Chorus.Utilities;
using Chorus.Utilities.code;
using Palaso.IO;
using Palaso.Progress.LogBox;

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
				if (alsoDoCheckout)
				{
					// string userIdForCLone = string.Empty; /* don't assume it's this user... a repo on a usb key probably shouldn't have a user default */
					var clone = new HgRepository(targetDirectory, progress);
					clone.Update();
				}
				return targetDirectory;
			}
		}
	}
}