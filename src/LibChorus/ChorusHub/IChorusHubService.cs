using System.ServiceModel;

namespace Chorus.ChorusHub
{
	[ServiceContract]
	public interface IChorusHubService
	{
		/// <summary>
		/// Returns information about the Hg repositories that the ChorusHub knows about.
		///
		/// The search can be trimmed by use of the 'searchUrl' parameter.
		/// Everything about the searchUrl except the query string is fabricated
		/// by the ChorusHubClient. The query string is fed in by the application.
		///
		/// The possible search values are:
		/// filePattern -- This key can have multiple values separated by the '|' character
		///
		/// Each repository generates a JSON string consisting of two name/value pairs.
		/// The two names are "name" and "id". The JSON strings are concatenated, separated by /.
		/// (An earlier version returned an enumeration of json strings. But Mono could not
		/// marshal this.)
		/// </summary>
		/// <example>searchUrl: "scheme://path?filePattern=*.lift|*.CustomProperties"</example>
		/// <example>returned repo info string: {"name": "someProject", "id": "123abc"}</example>
		[OperationContract]
		string GetRepositoryInformation(string searchUrl);

		/// <summary>
		/// Returns information about the Hg repositories that the ChorusHub knows about.
		/// The id for each repository will be an empty string.
		///
		/// The search can be trimmed by use of the 'searchUrl' parameter.
		/// Everything about the searchUrl except the query string is fabricated
		/// by the ChorusHubClient. The query string is fed in by the application.
		///
		/// The possible search values are:
		/// filePattern -- This key can have multiple values separated by the '|' character
		///
		/// Each repository generates a JSON string consisting of two name/value pairs.
		/// The two names are "name" and "id". The JSON strings are concatenated, separated by /.
		/// (An earlier version returned an enumeration of json strings. But Mono could not
		/// marshal this.)
		/// </summary>
		/// <example>searchUrl: "scheme://path?filePattern=*.lift|*.CustomProperties"</example>
		/// <example>returned repo info string: {"name": "someProject", "id": ""}</example>
		[OperationContract]
		string GetRepositoryInformationWithoutIds(string searchUrl);
		
		/// <summary>
		/// Prepares to receive a repository by checking that it exists on the server, and if not,
		/// creating a new directory with that name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="id"></param>
		/// <returns>true if a new directory is created</returns>
		[OperationContract]
		bool PrepareToReceiveRepository(string name, string id);

		/// <summary>
		/// Gets a file's contents from a revision directly from a repository
		/// </summary>
		[OperationContract]
		byte[] GetFileRevision(string repositoryName, string fileRelativePath, string revision);

		/// <summary>
		/// Verifies the integrity of the mecurial repository. If errors are found, attempts a mecurial recovery.
		/// </summary>
		/// <returns>The console output of the verify or null if no errors found</returns>
		[OperationContract]
		string Verify(string repositoryName);

		/// <summary>
		/// Renames a repository
		/// </summary>
		/// <returns>True if successful, false otherwise</returns>
		[OperationContract]
		bool Rename(string repositoryName, string newName);
	}
}