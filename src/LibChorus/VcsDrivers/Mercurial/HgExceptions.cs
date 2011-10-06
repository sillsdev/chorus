using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chorus.VcsDrivers.Mercurial
{

	public class RepositoryAuthorizationException : Exception
	{

	}

	/// <summary>
	/// http 400 error: bad request
	/// </summary>
	public class FirewallProblemSuspectedException : Exception
	{
		public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("400");
		}

		public override string Message
		{
			get { return "The program could not communicate with the server on the internet.  If you are connecting from an office or campus environment which has a firewall intended to provide security, it is likely that this firewall is preventing this program from communicating with the server.  Please go to http://palaso.org/opening-firewalls-to-work-with-palaso-applications for instructions."; }
		}
	}

	/// <summary>
	/// http 500 error: Internal server error
	/// </summary>
	public class ServerErrorException : Exception
	{
		public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("500");
		}

		public override string Message
		{
			get { return "The internet server reported that it is having problems. There isn't anything you can do about that except try again later."; }
		}
	}
}
