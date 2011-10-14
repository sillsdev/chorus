using System.Collections.Generic;
using System.Net;

namespace Chorus.VcsDrivers.Mercurial
{
	public interface IApiServer
	{
		HttpWebResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout = 10);
		HttpWebResponse Execute(string method, IDictionary<string, string> parameters, string contentToSend, int secondsBeforeTimeout = 10);
	}
}