using System.Collections.Generic;

namespace Chorus.sync
{
	public class SyncOptions
	{
		private bool _doPullFromOthers;
		private bool _doPushToLocalSources;
		private bool _doMergeWithOthers;
		private string _checkinDescription;
		public List<RepositorySource> RepositorySourcesToTry=new List<RepositorySource>();

		public SyncOptions()
		{
			_doPullFromOthers = true;
			_doMergeWithOthers = true;
			_doPushToLocalSources = true;
			_checkinDescription = "missing checkin description";
		}

		/// <summary>
		/// Attempt to get stuff from sources listed in "RepositorySourcesToTry"
		/// </summary>
		public bool DoPullFromOthers
		{
			get { return _doPullFromOthers; }
			set { _doPullFromOthers = value; }
		}

		 /// <summary>
		 /// After pulling new stuff, merge them in
		 /// </summary>
		public bool DoMergeWithOthers
		{
			get { return _doMergeWithOthers; }
			set { _doMergeWithOthers = value; }
		}

		public string CheckinDescription
		{
			get { return _checkinDescription; }
			set { _checkinDescription = value; }
		}

		/// <summary>
		/// Push differences to, for example, usb keys, second hard drives, or sd cards that are
		/// specified in the "RepositorySourcesToTry"
		/// </summary>
		public bool DoPushToLocalSources
		{
			get { return _doPushToLocalSources; }
			set { _doPushToLocalSources = value; }
		}
	}
}