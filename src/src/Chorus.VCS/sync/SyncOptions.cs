using System.Collections.Generic;

namespace Chorus.sync
{
	public class SyncOptions
	{
		private bool _doPullFromOthers;
		private bool _doMergeWithOthers;
		private string _checkinDescription;
		public List<RepositorySource> RepositorySourcesToTry=new List<RepositorySource>();

		public SyncOptions()
		{
			_doPullFromOthers = true;
			_doMergeWithOthers = true;
			_checkinDescription = "missing checkin description";
		}

		public bool DoPullFromOthers
		{
			get { return _doPullFromOthers; }
			set { _doPullFromOthers = value; }
		}

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
	}
}