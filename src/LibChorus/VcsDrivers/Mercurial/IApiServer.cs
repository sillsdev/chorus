using System;
using System.Collections.Generic;
using System.Net;

namespace Chorus.VcsDrivers.Mercurial
{
	public interface IApiServer
	{
		HgResumeApiResponse Execute(string method, HgResumeApiParameters request, int secondsBeforeTimeout);
		HgResumeApiResponse Execute(string method, HgResumeApiParameters request, byte[] contentToSend, int secondsBeforeTimeout);
		string Host { get; }
		string ProjectId { get; }
		string Url { get; }
	}

	public class HgResumeApiResponse
	{
		public HgResumeApiResponseHeaders ResumableResponse;
		public HttpStatusCode HttpStatus;
		public byte[] Content;
		public long ResponseTimeInMilliseconds;
	}

	public class HgResumeApiParameters
	{
		public HgResumeApiParameters()
		{
			StartOfWindow = -1;
			ChunkSize = -1;
			BundleSize = -1;
			Quantity = -1;
		}

		// parameters used by push/pull API
		public int StartOfWindow { get; set; }
		public int ChunkSize { get; set; }
		public int BundleSize { get; set; }

		public string TransId { get; set; }
		public string[] BaseHashes { get; set; }
		public string RepoId { get; set; }

		// used only by getRevisions API
		public int Quantity { get; set; }

		/// <summary>
		/// returns a query string, prefixed with a '?' unless there are no parameters
		/// </summary>
		/// <returns></returns>
		public string BuildQueryString()
		{
			string query = "?";
			if (StartOfWindow >= 0)
			{
				query += String.Format("offset={0}&", StartOfWindow);
			}
			if (ChunkSize >= 0)
			{
				query += String.Format("chunkSize={0}&", ChunkSize);
			}
			if (BundleSize >= 0)
			{
				query += String.Format("bundleSize={0}&", BundleSize);
			}
			if (Quantity >= 0)
			{
				query += String.Format("quantity={0}&", Quantity);
			}
			if (!String.IsNullOrEmpty(TransId))
			{
				query += String.Format("transId={0}&", TransId);
			}
			if (BaseHashes != null && BaseHashes.Length != 0)
			{
				foreach (var baseHash in BaseHashes)
				{
					query += String.Format("baseHashes[]={0}&", baseHash);
				}
			}
			if (!String.IsNullOrEmpty(RepoId))
			{
				query += String.Format("repoId={0}&", RepoId);
			}
			if (query == "?")
			{
				return "";
			}
			return query.TrimEnd('&');
		}
	}

	public class HgResumeApiResponseHeaders
	{
		private WebHeaderCollection _headers;
		private Dictionary<string, string> _keyMap;
		public const string headerPrefix = "x-hgr-";

		public HgResumeApiResponseHeaders(WebHeaderCollection headers)
		{
			_headers = headers;
			_keyMap = new Dictionary<string, string>();
			foreach (var header in headers)
			{
				if (header.ToString().ToLower().StartsWith(headerPrefix))
				{
					_keyMap.Add(header.ToString().ToLower().Substring(headerPrefix.Length), header.ToString());
				}
			}
		}

		public string TransId
		{
			get
			{
				const string needle = "transid";
				if (_keyMap.ContainsKey(needle))
				{
					return _headers[_keyMap[needle]];
				}
				return "";
			}
		}

		public int StartOfWindow
		{
			get
			{
				const string needle = "sow";
				if (_keyMap.ContainsKey(needle))
				{
					return Convert.ToInt32(_headers[_keyMap[needle]]);
				}
				return 0;
			}
		}

		public int BundleSize
		{
			get
			{
				const string needle = "bundlesize";
				if (_keyMap.ContainsKey(needle))
				{
					return Convert.ToInt32(_headers[_keyMap[needle]]);
				}
				return 0;
			}
		}

		public int ChunkSize
		{
			get
			{
				const string needle = "chunksize";
				if (_keyMap.ContainsKey(needle))
				{
					return Convert.ToInt32(_headers[_keyMap[needle]]);
				}
				return 0;
			}
		}

		public string Version
		{
			get
			{
				const string needle = "version";
				if (_keyMap.ContainsKey(needle))
				{
					return _headers[_keyMap[needle]];
				}
				throw new HgResumeException("Something went wrong, since no version header could be retrieved!");
			}
		}

		public string Status
		{
			get
			{
				const string needle = "status";
				if (_keyMap.ContainsKey(needle))
				{
					return _headers[_keyMap[needle]];
				}
				throw new HgResumeException("Something went wrong, since no hgr status header could be retrieved!");
			}
		}

		public string Error
		{
			get
			{
				const string needle = "error";
				if (_keyMap.ContainsKey(needle))
				{
					return _headers[_keyMap[needle]];
				}
				return "";
			}
		}

		public bool HasError
		{
			get { return !String.IsNullOrEmpty(Error); }
		}

		public string Note
		{
			get
			{
				const string needle = "note";
				if (_keyMap.ContainsKey(needle))
				{
					return _headers[_keyMap[needle]];
				}
				return "";
			}
		}

		public bool HasNote
		{
			get { return !String.IsNullOrEmpty(Note); }
		}
	}
}