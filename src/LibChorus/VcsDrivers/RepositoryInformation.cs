namespace Chorus.VcsDrivers
{
	/// <summary>
	/// ChorusHubClient.GetRepositoryInformation returns an IEnumerable of these.
	/// </summary>
	public class RepositoryInformation
	{
		public string RepoName { get; private set; }
		public string RepoID { get; private set; }

		public RepositoryInformation(string name, string id)
		{
			RepoID = id;
			RepoName = name;
		}
	}
}
