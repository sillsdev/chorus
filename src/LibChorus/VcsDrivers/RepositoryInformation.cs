namespace Chorus.VcsDrivers
{
	/// <summary>
	///     ChorusHubClient.GetRepositoryInformation returns an IEnumerable of these.
	/// </summary>
	public class RepositoryInformation
	{
		/// <summary>
		///     The default ID of a new, uninitialized repository
		/// </summary>
		public const string NEW_REPO = "newRepo";

		public RepositoryInformation(string name, string id)
		{
			RepoID = id;
			RepoName = name;
		}

		public string RepoName { get; private set; }
		public string RepoID { get; private set; }

		/// <summary>
		///     Determines whether this is a new (uninitialized) repository (RepoID == 'newRepo')
		/// </summary>
		/// <returns>true if this is a new, uninitialized repository; false otherwise</returns>
		public bool IsNew()
		{
			return RepoID == NEW_REPO;
		}
	}
}
