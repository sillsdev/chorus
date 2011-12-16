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
			RequireThat.Directory(targetDirectory).DoesNotExist();
			RequireThat.Directory(targetDirectory).Parent().Exists();

			HgRepository local = new HgRepository(sourcePath, progress);

			if (!local.RemoveOldLocks())
			{
				progress.WriteError("Chorus could not create the clone at this time.  Try again after restarting the computer.");
				return string.Empty;
			}

			targetDirectory = GetUniqueFolderPath(progress,
												  "There is a folder with the name {0}, which is the same as this project. However, that folder cannot be used, so a new folder named {1} has been created and used, instead.",
												  targetDirectory);

			using (new ConsoleProgress("Creating repository clone at {0}", targetDirectory))
			{
				local.CloneLocal(targetDirectory);
				if (alsoDoCheckout)
				{
					// string userIdForCLone = string.Empty; /* don't assume it's this user... a repo on a usb key probably shouldn't have a user default */
					HgRepository clone = new HgRepository(targetDirectory, progress);
					clone.Update();
				}
				return targetDirectory;
			}
		}

		public static string GetUniqueFolderPath(IProgress progress, string formattableMessage, string targetDirectory)
		{
			var uniqueTarget = DirectoryUtilities.GetUniqueFolderPath(targetDirectory);
			if (targetDirectory != uniqueTarget)
				progress.WriteWarning(string.Format(formattableMessage, targetDirectory, uniqueTarget));

			return uniqueTarget; // It may be the original, if it was unique.
		}
	}
}