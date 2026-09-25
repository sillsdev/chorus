using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgCommonException:Exception
	{
		// hg reports HTTP failures as e.g. "abort: HTTP Error 404: Not Found".
		// [0-9] rather than \d, which also matches non-ASCII digits that int.Parse rejects.
		private static readonly Regex HttpErrorStatusRegex = new Regex(@"HTTP Error ([0-9]{3})(?![0-9])", RegexOptions.Compiled);

		/// <summary>
		/// True if hg reported one of the given HTTP status codes. Only the status hg itself prints is considered,
		/// because the exception message also carries the full hg command line (local paths, project names) and the
		/// hg version, any of which can happen to contain digits like "404".
		/// </summary>
		protected static bool ErrorHasHttpStatus(Exception error, params int[] statusCodes)
		{
			return HttpErrorStatusRegex.Matches(error.Message).Cast<Match>()
				.Any(m => statusCodes.Contains(int.Parse(m.Groups[1].Value)));
		}
	}

	public class RepositoryAuthorizationException : HgCommonException
	{
		public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("authorization") || ErrorHasHttpStatus(error, 403);
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
			return ErrorHasHttpStatus(error, 400);
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
			return ErrorHasHttpStatus(error, 500, 503);
		}

		public override string Message
		{
			get { return "The internet server reported that it is having problems. There isn't anything you can do about that except try again later."; }
		}
	}

	public class PortProblemException : HgCommonException
	{
		private readonly string _targetUri;

		public PortProblemException(string targetUri)
		{
			_targetUri = targetUri;
		}

		public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("refused");//"No connection could be made because the target machine actively refused it
		}

		public override string Message
		{
			get
			{
				if(_targetUri.ToLower().Contains("chorushub"))
				{
					return "Your computer could reach the Chorus Hub computer, but couldn't communicate with ChorusHub itself. Possible causes:\r\n1) Something is wrong with Chorus Hub.  Go to the machine running ChorusHub, and try quitting ChorusHub and running it again. 2) A firewall on your machine or on your network is blocking the communication on this port (the number after the colon here: "+_targetUri+").";
				}
				else if(_targetUri.ToLower().Contains("languageforge"))
				{
					return "Your computer could reach Lexbox, but couldn't communicate with the Chorus server there. Possible causes:\r\n1) The Chorus server on Lexbox might be temporarily out of order. If it is, try again later/tomorrow. \r\n2) A firewall on your machine or on your network is blocking the communication with Lexbox.";
				}
				else
				{
					return "Your computer could reach the target computer, but they couldn't communicate. Possible causes:\r\n1) on the target machine, the service is not running.\r\n2) A firewall on your machine or on your network is blocking the communication on this port (the number after the colon here: " + _targetUri + ").";
				}
			}
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
			return ErrorHasHttpStatus(error, 502);
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


	public class UnrelatedRepositoryErrorException : HgCommonException
	{
		private readonly string _sourceUri;

		public UnrelatedRepositoryErrorException(string sourceUri)
		{
			_sourceUri = sourceUri;
		}

		public static bool ErrorMatches(Exception error)
		{
			return error.Message.Contains("unrelated");
		}

		public override string Message
		{
			get
			{
				return
					string.Format(
						"The repository(a.k.a 'project' or 'collection') that you tried to synchronize with has the same name as yours, but it does not have the same heritage, so it cannot be synchronized. In order to Send/Receive projects, you have to start with a single project/collection, then copy that around. Don't feel bad if this is confusing, just ask for some technical help.");
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
			return ErrorHasHttpStatus(error, 404);
		}

		public override string Message
		{
			get
			{
				var x = new Uri(_sourceUri);
				if (_sourceUri.ToLower().Contains("chorushub"))
				{
					return string.Format("That Chorus Hub does not yet host a project labeled '{0}'. It may be busy making a place for it now, so try again in 5 minutes.", x.PathAndQuery.Trim('/').Replace("%20"," "));
				}
				else
				{
					return string.Format("Check that {0} really hosts a project labeled '{1}'", x.Host,
										 x.PathAndQuery.Trim('/'));
				}
			}
		}
	}

}
