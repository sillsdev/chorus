using System.Collections.Generic;
using System.Net;

namespace Chorus.VcsDrivers.Mercurial
{
	public interface IApiServer
	{
		HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout = 10);
		HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] contentToSend, int secondsBeforeTimeout = 10);
		string Identifier { get; }
	}

	public class HgResumeApiResponse
	{
		public Dictionary<string, string> Headers = new Dictionary<string, string>();
		public HttpStatusCode StatusCode;
		public byte[] Content;
	}
}