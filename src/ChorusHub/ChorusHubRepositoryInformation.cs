namespace ChorusHub
{
	/// <summary>
	/// ChorusHubClient.GetRepositoryInformation returns an IEnumerable of these.
	/// </summary>
	public class ChorusHubRepositoryInformation
	{
		public string RepoName { get; private set; }
		public string RepoID { get; private set; }

		public ChorusHubRepositoryInformation(string name, string id)
		{
			RepoID = id;
			RepoName = name;
		}
	}
}
