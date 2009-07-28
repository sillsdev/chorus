using System.Collections.Generic;

namespace Chorus.sync
{
	public class SyncOptions
	{
		public List<RepositoryAddress> RepositorySourcesToTry=new List<RepositoryAddress>();

		public SyncOptions()
		{
			DoPullFromOthers = true;
			DoMergeWithOthers = true;
			DoPushToLocalSources = true;
			CheckinDescription = "missing checkin description";
		}

		/// <summary>
		/// Attempt to get stuff from sources listed in "RepositorySourcesToTry"
		/// </summary>
		public bool DoPullFromOthers { get; set; }

		/// <summary>
		/// After pulling new stuff, merge them in
		/// </summary>
		public bool DoMergeWithOthers { get; set; }

		public string CheckinDescription { get; set; }

		/// <summary>
		/// Push differences to, for example, usb keys, second hard drives, or sd cards that are
		/// specified in the "RepositorySourcesToTry"
		/// </summary>
		public bool DoPushToLocalSources { get; set; }
	}
}