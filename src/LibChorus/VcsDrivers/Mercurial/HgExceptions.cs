using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgCommonException:Exception
	{

	}

	public class RepositoryAuthorizationException : HgCommonException
	{
		public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("authorization") || error.Message.Contains("403");
		}

		public override string Message
		{
			get { return "The server rejected the project name, user name, or password. Also make sure this user is a member of the project, with permission to read data."; }
		}

	}

	/// <summary>
	/// http 400 error: bad request
	/// </summary>
	public class FirewallProblemSuspectedException : HgCommonException
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
	public class ServerErrorException : HgCommonException
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

		public class UriProblemException : HgCommonException
	{
			private readonly string _sourceUri;

			public UriProblemException(string sourceUri)
			{
				_sourceUri = sourceUri;
			}

			public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("502");
		}

		public override string Message
		{
			get
			{
				var x = new Uri(_sourceUri);
				return string.Format("Check that the name {0} is correct", x.Host);
			}
		}
	}

	public class ProjectLabelErrorException : HgCommonException
	{
		private readonly string _sourceUri;

		public ProjectLabelErrorException(string sourceUri)
		{
			_sourceUri = sourceUri;
		}

		public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("404");
		}

		public override string Message
		{
			get
			{
				var x = new Uri(_sourceUri);
				return string.Format("Check that {0} really hosts a project labeled '{1}'", x.Host, x.PathAndQuery.Trim('/'));
			}
		}
	}

}
