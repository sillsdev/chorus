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
		/// <param name="searchUrl"></param>
		/// <returns></returns>
		[OperationContract]
		string GetRepositoryInformation(string searchUrl);

		/// <summary>
		/// Prepares to receive a repository by checking that it exists on the server, and if not,
		/// creating a new directory with that name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="id"></param>
		/// <returns>true if a new directory is created</returns>
		[OperationContract]
		bool PrepareToReceiveRepository(string name, string id);
	}
}