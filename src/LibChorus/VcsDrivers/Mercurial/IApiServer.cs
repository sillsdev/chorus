using System.Collections.Generic;
using System.Net;

namespace Chorus.VcsDrivers.Mercurial
{
	public interface IApiServer
	{
		HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout);
		HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] contentToSend, int secondsBeforeTimeout);
		string Identifier { get; }
		string ProjectId { get; }
		string Url { get; }
	}

	public class HgResumeApiResponse
	{
		public Dictionary<string, string> Headers = new Dictionary<string, string>();
		public HttpStatusCode StatusCode;
		public byte[] Content;
		public long ResponseTimeInMilliseconds;
	}
}