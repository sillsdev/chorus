using SIL.Code;
using SIL.Progress;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgHighLevel
	{
		public static string MakeCloneFromUsbToLocal(string sourcePath, string targetDirectory, IProgress progress)
		{
			return MakeCloneFromLocalToLocal(sourcePath, targetDirectory, true, progress);
		}

		public static string MakeCloneFromLocalToUsb(string sourcePath, string targetDirectory, IProgress progress)
		{
			return MakeCloneFromLocalToLocal(sourcePath, targetDirectory, false, progress);
		}

		//TODO: get rid of this, or somehow combine it with the other Clone() options out there
		/// <returns>path to clone, or empty if it failed</returns>
		private static string MakeCloneFromLocalToLocal(string sourcePath, string targetDirectory, bool cloningFromUsb, IProgress progress)
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
				// Make a backward compatibile clone if cloning to USB (http://mercurial.selenic.com/wiki/UpgradingMercurial) 
				targetDirectory = local.CloneLocalWithoutUpdate(targetDirectory, cloningFromUsb ? null : "--config format.dotencode=false --pull");

				var clone = new HgRepository(targetDirectory, cloningFromUsb, progress);
				clone.Update();

				return targetDirectory;
			}
		}
	}
}