using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	internal enum ApiServerType
	{
		Dummy,
		Pull,
		Push
	}

	internal class HgResumeTransportProvider : IDisposable
	{
		public HgResumeTransportProvider(HgResumeTransport transport)
		{
			Transport = transport;
		}

		public void Dispose()
		{
			Transport.RemoveCache();
		}

		public HgResumeTransport Transport { get; private set; }
	}

	internal class TestEnvironment : IDisposable
	{

		public TestEnvironment(string testName, ApiServerType type)
		{
			Local = new RepositorySetup(testName + "-local");
			Remote = new RepositorySetup(testName + "-remote");
			Progress = new ProgressForTest();
			switch (type)
			{
				case ApiServerType.Dummy:
					ApiServer = new DummyApiServerForTest(testName);
					break;
				case ApiServerType.Pull:
					ApiServer = new PullHandlerApiServerForTest(Remote.Repository, Progress);
					break;
				case ApiServerType.Push:
					ApiServer = new PushHandlerApiServerForTest(Remote.Repository, Progress);
					break;
			}
			Label = testName;
			MultiProgress = new MultiProgress(new IProgress[] { new ConsoleProgress { ShowVerbose = true }, Progress });
		}

		public RepositorySetup Local { get; private set; }
		public RepositorySetup Remote { get; private set; }
		public IApiServerForTest ApiServer { get; private set; }
		public ProgressForTest Progress { get; private set; }
		public string Label { get; private set; }
		public IProgress MultiProgress { get; private set; }

		public void Dispose()
		{
			Local.Dispose();
			Remote.Dispose();
			if (ApiServer is IDisposable)
				(ApiServer as IDisposable).Dispose();
		}

		public virtual string LocalAddAndCommit()
		{
			var filename = Path.GetRandomFileName();
			Local.AddAndCheckinFile(filename, "localcheckin");
			return filename;
		}

		public virtual void LocalChangeAndCommit(string fileToChange)
		{
			Local.ChangeFile(fileToChange, DateTime.Now.GetHashCode().ToString());
		}

		public virtual void RemoteAddAndCommit()
		{
			string filename = Path.GetRandomFileName();
			Remote.AddAndCheckinFile(filename, "remotecheckin");
		}

		public virtual void LocalAddAndCommitLargeFile()
		{
			LocalAddAndCommitLargeFile(1);
		}

		public void LocalAddAndCommitLargeFile(int sizeInMb)
		{
			var filePath = Local.ProjectFolder.GetPathForNewTempFile(false);
			byte[] data = new byte[sizeInMb * 1024 * 1024];  // MB file
			Random rng = new Random();
			rng.NextBytes(data);
			File.WriteAllBytes(filePath, data);
			Local.Repository.AddAndCheckinFile(filePath);
		}

		public virtual void RemoteAddAndCommitLargeFile()
		{
			RemoteAddAndCommitLargeFile(1);
		}

		public virtual void RemoteAddAndCommitLargeFile(int sizeInMb)
		{
			var filePath = Remote.ProjectFolder.GetPathForNewTempFile(false);
			byte[] data = new byte[sizeInMb * 1024 * 1024];  // 5MB file
			Random rng = new Random();
			rng.NextBytes(data);
			File.WriteAllBytes(filePath, data);
			Remote.Repository.AddAndCheckinFile(filePath);
		}

		public void CloneRemoteFromLocal()
		{
			var address = new DirectoryRepositorySource("localrepo", Local.Repository.PathToRepo, false);
			Remote.Repository.Pull(address, Local.Repository.PathToRepo);
			Remote.Repository.Update();
		}
	}

	internal class BranchingTestEnvironment : TestEnvironment
	{


		public BranchingTestEnvironment(string testName, ApiServerType type) : base(testName, type)
		{
			Local.Synchronizer = new Synchronizer(Local.Repository.PathToRepo, Local.ProjectFolderConfig, new NullProgress());
			Remote.Synchronizer = new Synchronizer(Remote.Repository.PathToRepo, Remote.ProjectFolderConfig, new NullProgress());
		}

		public void SetLocalAdjunct(ISychronizerAdjunct adjunct)
		{
			if (Local.Synchronizer == null)
				Local.Synchronizer = Local.CreateSynchronizer();
			Local.Synchronizer.SynchronizerAdjunct = adjunct;
		}

		public void SetRemoteAdjunct(ISychronizerAdjunct adjunct)
		{
			if (Remote.Synchronizer == null)
				Remote.Synchronizer = Remote.CreateSynchronizer();
			Remote.Synchronizer.SynchronizerAdjunct = adjunct;
		}

		public override string LocalAddAndCommit()
		{
			var opts = new SyncOptions {CheckinDescription = "localaddandcommit"};
			Local.Synchronizer.SyncNow(opts);
			return base.LocalAddAndCommit();
		}

		public override void RemoteAddAndCommit()
		{
			var opts = new SyncOptions { CheckinDescription = "remoteaddandcommit" };
			Remote.Synchronizer.SyncNow(opts);
			base.RemoteAddAndCommit();
		}
	}


	internal interface IApiServerForTest : IApiServer
	{
		void AddTimeoutResponse(int executeCount);
		void AddServerUnavailableResponse(int executeCount, string serverMessage);
		void AddFailResponse(int executeCount);
		void AddCancelResponse(int executeCount);
		void PrepareBundle(string[] revHash);
		void AddResponse(HgResumeApiResponse response);
		void AddTimeOut();
		string StoragePath { get; }
		string CurrentTip { get; }
	}

	internal class ApiServerForTest
	{
		public void ValidateParameters(string method, HgResumeApiParameters request, byte[] contentToSend, int secondsBeforeTimeout)
		{
			Assert.That(method, Is.Not.Empty);
			Assert.That(secondsBeforeTimeout, Is.GreaterThan(0));
			if (method == "getRevisions")
			{
				Assert.That(request.RepoId, Is.Not.Empty);
				Assert.That(request.Quantity, Is.GreaterThan(0));
				Assert.That(request.StartOfWindow, Is.GreaterThan(-1));
			}
			else if (method == "pullBundleChunk")
			{
				Assert.That(request.RepoId, Is.Not.Empty);
				Assert.That(request.BaseHashes, Is.Not.Empty);
				Assert.That(request.ChunkSize, Is.GreaterThan(0));
				Assert.That(request.StartOfWindow, Is.GreaterThanOrEqualTo(0));
				Assert.That(request.TransId, Is.Not.Empty);
			}
			else if (method == "pushBundleChunk")
			{
				Assert.That(request.RepoId, Is.Not.Empty);
				Assert.That(request.BundleSize, Is.GreaterThan(0));
				Assert.That(request.StartOfWindow, Is.GreaterThanOrEqualTo(0));
				Assert.That(request.TransId, Is.Not.Empty);
				Assert.That(contentToSend.Length, Is.GreaterThan(0));
			}
			else if (method == "finishPushBundle")
			{
				Assert.That(request.TransId, Is.Not.Empty);
			}
			else if (method == "finishPullBundle")
			{
				Assert.That(request.TransId, Is.Not.Empty);
			}
			else
			{
				throw new HgResumeException(String.Format("unknown method '{0}'", method));
			}
		}
	}

	internal class DummyApiServerForTest : ApiServerForTest, IApiServerForTest, IDisposable
	{
		private readonly Queue<HgResumeApiResponse> _responseQueue = new Queue<HgResumeApiResponse>();

		public DummyApiServerForTest() :
			this("DummyApiServerForTest")
		{
		}

		public DummyApiServerForTest(string identifier)
		{
			Host = identifier;
			ProjectId = "SampleProject";
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters request, int secondsBeforeTimeout)
		{
			return Execute(method, request, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters request, byte[] contentToSend, int secondsBeforeTimeout)
		{
			ValidateParameters(method, request, contentToSend, secondsBeforeTimeout);
			HgResumeApiResponse response;
			if (_responseQueue.Count > 0)
			{
				response = _responseQueue.Dequeue();
				if (response.HttpStatus == HttpStatusCode.RequestTimeout)
				{
					return null;
				}
			} else
			{
				response = new HgResumeApiResponse {HttpStatus = HttpStatusCode.InternalServerError};
			}
			response.ResponseTimeInMilliseconds = 200;
			return response;
		}

		public string Host { get; private set; }

		public string ProjectId { get; set; }

		public string Url
		{
			get { return "fake api server"; }
		}

		public void AddTimeoutResponse(int executeCount)
		{
			throw new NotImplementedException();
		}

		public void AddServerUnavailableResponse(int executeCount, string serverMessage)
		{
			throw new NotImplementedException();
		}

		public void AddFailResponse(int executeCount)
		{
			throw new NotImplementedException();
		}

		public void AddCancelResponse(int executeCount)
		{
			throw new NotImplementedException();
		}

		public void PrepareBundle(string[] revHash)
		{
			throw new NotImplementedException();
		}

		public void AddResponse(HgResumeApiResponse response)
		{
			_responseQueue.Enqueue(response);
		}

		public void AddTimeOut()
		{
			// we are hijacking the HTTP 408 request timeout to mean a client-side networking timeout...
			// it works for our testing purposes even though that's not what the status code means
			_responseQueue.Enqueue(new HgResumeApiResponse {HttpStatus = HttpStatusCode.RequestTimeout});
		}

		public string StoragePath
		{
			get { throw new NotImplementedException(); }
		}

		public string CurrentTip
		{
			get { throw new NotImplementedException(); }
		}

		public void Dispose()
		{
		}
	}

	internal class ServerUnavailableResponse
	{
		public string Message;
		public int ExecuteCount;
	}

	internal class PushHandlerApiServerForTest : ApiServerForTest, IApiServerForTest, IDisposable
	{

		private PullStorageManager _helper;  // yes, we DO want to use the PullStorageManager for the PushHandler (this is the other side of the API, so it's opposite)
		private readonly HgRepository _repo;
		private readonly TemporaryFolder _localStorage;
		private int _executeCount;
		private readonly List<int> _timeoutList;
		private readonly List<ServerUnavailableResponse> _serverUnavailableList;
		private int _failCount;
		private int _cancelCount;

		private ProgressForTest _progress;

		public PushHandlerApiServerForTest(HgRepository repo, ProgressForTest progress)
		{
			const string identifier = "PushHandlerApiServerForTest";
			_localStorage = new TemporaryFolder(identifier);
			_repo = repo;
			_progress = progress;
			Host = identifier;
			ProjectId = "SampleProject";
			_executeCount = 0;
			_failCount = -1;
			_cancelCount = -1;
			_timeoutList = new List<int>();
			_serverUnavailableList = new List<ServerUnavailableResponse>();
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters request, int secondsBeforeTimeout)
		{
			return Execute(method, request, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters request, byte[] bytesToWrite, int secondsBeforeTimeout)
		{
			ValidateParameters(method, request, bytesToWrite, secondsBeforeTimeout);
			if (method == "getRevisions")
			{
				IEnumerable<Revision> revisions = _repo.GetAllRevisions();

				if (revisions.Count() == 0)
				{
					return ApiResponses.Revisions("0:default");
				}
				var revisionStrings = _repo.GetAllRevisions().Select(rev => rev.Number.Hash + ':' + rev.Branch);
				return ApiResponses.Revisions(string.Join("|", revisionStrings.ToArray()));
			}
			if (method == "finishPushBundle")
			{
				return ApiResponses.PushComplete();
			}
			if (method == "pushBundleChunk")
			{
				_executeCount++;
				if (_cancelCount == _executeCount)
				{
					_progress.CancelRequested = true;
					return ApiResponses.Failed("");
				}
				if (_failCount == _executeCount)
				{
					return ApiResponses.Failed("");
				}
				if (_timeoutList.Contains(_executeCount))
				{
					return null;
				}
				if (_serverUnavailableList.Any(i => i.ExecuteCount == _executeCount))
				{
					return ApiResponses.NotAvailable(
							_serverUnavailableList.Where(i => i.ExecuteCount == _executeCount).First().Message
							);
				}
				_helper = new PullStorageManager(_localStorage.Path, request.TransId);

				//int bundleSize = Convert.ToInt32(parameters["bundleSize"]);
				//int offset = Convert.ToInt32(parameters["offset"]);
				//int chunkSize = bytesToWrite.Length;

				_helper.WriteChunk(request.StartOfWindow, bytesToWrite);

				if (request.StartOfWindow + request.ChunkSize == request.BundleSize)
				{
					if (_repo.Unbundle(_helper.BundlePath))
					{
						return ApiResponses.PushComplete();
					}
					return ApiResponses.Reset();
				}
				if (request.StartOfWindow + request.ChunkSize < request.BundleSize)
				{
					return ApiResponses.PushAccepted(_helper.StartOfWindow);
				}
				return ApiResponses.Failed("offset + chunkSize > bundleSize !");
			}
			return ApiResponses.Custom(HttpStatusCode.InternalServerError);
		}

		public string Host { get; private set; }

		public string ProjectId { get; set; }

		public void AddTimeOut()
		{
			throw new NotImplementedException();
		}

		public string StoragePath
		{
			get { return _localStorage.Path; }
		}

		public string CurrentTip
		{
			get { throw new NotImplementedException(); }
		}

		public string Url
		{
			get { return "fake api server"; }
		}

		public void Dispose()
		{
			_localStorage.Dispose();
		}

		public void AddTimeoutResponse(int executeCount)
		{
			if (!_timeoutList.Contains(executeCount))
			{
				_timeoutList.Add(executeCount);
			}
		}

		public void AddServerUnavailableResponse(int executeCount, string serverMessage)
		{
			if (!_serverUnavailableList.Any(i => i.ExecuteCount == executeCount))
			{
				_serverUnavailableList.Add(new ServerUnavailableResponse {ExecuteCount = executeCount, Message = serverMessage});
			}
		}

		public void AddFailResponse(int executeCount)
		{
			_failCount = executeCount;
		}

		public void AddCancelResponse(int executeCount)
		{
			_cancelCount = executeCount;
		}

		public void PrepareBundle(string[] revHash)
		{
			throw new NotImplementedException();
		}

		public void AddResponse(HgResumeApiResponse response)
		{
			throw new NotImplementedException();
		}
	}

	internal class PullHandlerApiServerForTest : ApiServerForTest, IApiServerForTest, IDisposable
	{
		private readonly PushStorageManager _helper;
		private readonly HgRepository _repo;
		private int _executeCount;
		private int _failCount;
		private int _cancelCount;
		private readonly List<int> _timeoutList;
		private readonly List<ServerUnavailableResponse> _serverUnavailableList;
		private readonly TemporaryFolder _storageFolder;
		public string OriginalTip;

		private ProgressForTest _progress;

		public PullHandlerApiServerForTest(HgRepository repo, ProgressForTest progress)
		{
			const string identifier = "PullHandlerApiServerForTest";
			_storageFolder = new TemporaryFolder(identifier);
			_repo = repo;
			_progress = progress;
			_helper = new PushStorageManager(_storageFolder.Path, "randomHash");
			Host = identifier;
			ProjectId = "SampleProject";
			_executeCount = 0;
			_failCount = -1;
			_timeoutList = new List<int>();
			_serverUnavailableList = new List<ServerUnavailableResponse>();
			OriginalTip = "";
		}

		private string CurrentTip
		{
			get { return _repo.GetTip().Number.Hash; }
		}

		public void AddTimeOut()
		{
			throw new NotImplementedException();
		}

		public string StoragePath
		{
			get { return _storageFolder.Path; }
		}

		string IApiServerForTest.CurrentTip
		{
			get { return CurrentTip; }
		}

		public void AddCancelResponse(int executeCount)
		{
			_cancelCount = executeCount;
		}

		public void PrepareBundle(string[] revHash)
		{
			if(File.Exists(_helper.BundlePath))
			{
				File.Delete(_helper.BundlePath);
			}
			_repo.MakeBundle(revHash, _helper.BundlePath);
			OriginalTip = CurrentTip;
		}

		public void AddResponse(HgResumeApiResponse response)
		{
			throw new NotImplementedException();
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters request, int secondsBeforeTimeout)
		{
			return Execute(method, request, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters request, byte[] bytesToWrite, int secondsBeforeTimeout)
		{
			ValidateParameters(method, request, bytesToWrite, secondsBeforeTimeout);
			_executeCount++;
			if (method == "finishPullBundle")
			{
				if (CurrentTip != OriginalTip)
				{
					PrepareBundle(HgResumeTransport.GetHashStringsFromRevisions(_repo.BranchingHelper.GetBranches()));
					return ApiResponses.Reset(); // repo changed in between pulls
				}
				return ApiResponses.Custom(HttpStatusCode.OK);
			}
			if (method == "getRevisions")
			{
				IEnumerable<string> revisions = _repo.GetAllRevisions().Select(rev => rev.Number.Hash + ':' + rev.Branch);
				return ApiResponses.Revisions(string.Join("|", revisions.ToArray()));
			}
			if (method == "pullBundleChunk")
			{
				if (_cancelCount == _executeCount)
				{
					_progress.CancelRequested = true;
					return ApiResponses.Failed("");
				}
				if (_failCount == _executeCount)
				{
					return ApiResponses.Failed("");
				}
				if (_timeoutList.Contains(_executeCount))
				{
					return null;
				}
				if (_serverUnavailableList.Any(i => i.ExecuteCount == _executeCount))
				{
					return ApiResponses.NotAvailable(
							_serverUnavailableList.Where(i => i.ExecuteCount == _executeCount).First().Message
							);
				}
				if (Array.BinarySearch(request.BaseHashes, 0, request.BaseHashes.Length, CurrentTip) >= 0)
				{
					return ApiResponses.PullNoChange();
				}

				var bundleFileInfo = new FileInfo(_helper.BundlePath);
				if (bundleFileInfo.Exists && bundleFileInfo.Length == 0  || !bundleFileInfo.Exists)
				{
					PrepareBundle(request.BaseHashes);
				}
				//int offset = Convert.ToInt32(request["offset"]);
				//int chunkSize = Convert.ToInt32(request["chunkSize"]);
				var bundleFile = new FileInfo(_helper.BundlePath);
				if (request.StartOfWindow >= bundleFile.Length)
				{
					return ApiResponses.Failed("offset greater than bundleSize");
				}
				var chunk = _helper.GetChunk(request.StartOfWindow, request.ChunkSize);
				return ApiResponses.PullOk(Convert.ToInt32(bundleFile.Length), chunk);
			}
			return ApiResponses.Custom(HttpStatusCode.InternalServerError);
		}

		public string Host { get; private set; }

		public string ProjectId { get; set; }

		public string Url
		{
			get { return "fake api server"; }
		}

		public void Dispose()
		{
			_storageFolder.Dispose();
		}

		public void AddTimeoutResponse(int executeCount)
		{
			if (!_timeoutList.Contains(executeCount))
			{
				_timeoutList.Add(executeCount);
			}
		}

		public void AddServerUnavailableResponse(int executeCount, string serverMessage)
		{
			if (!_serverUnavailableList.Any(i => i.ExecuteCount == executeCount))
			{
				_serverUnavailableList.Add(new ServerUnavailableResponse { ExecuteCount = executeCount, Message = serverMessage });
			}
		}

		public void AddFailResponse(int executeCount)
		{
			_failCount = executeCount;
		}
	}


	internal class ProgressForTest : IProgress, IDisposable
	{
		public List<string> Statuses = new List<string>();
		public List<string> Messages = new List<string>();
		public List<string> Warnings = new List<string>();
		public List<Exception> Exceptions = new List<Exception>();
		public List<string> Errors = new List<string>();
		public List<string> Verbose = new List<string>();
		private List<string> _all = new List<string>();

		public void WriteStatus(string message, params object[] args)
		{
			Statuses.Add(message);
			_all.Add(message);
		}

		public void WriteMessage(string message, params object[] args)
		{
			Messages.Add(message);
			_all.Add(message);
		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			Messages.Add(message);
			_all.Add(message);
		}

		public void WriteWarning(string message, params object[] args)
		{
			Warnings.Add(message);
			_all.Add(message);
		}

		public void WriteException(Exception error)
		{
			Exceptions.Add(error);
			_all.Add(error.Message);
		}

		public void WriteError(string message, params object[] args)
		{
			Errors.Add(message);
			_all.Add(message);
		}

		public void WriteVerbose(string message, params object[] args)
		{
			Verbose.Add(message);
			_all.Add(message);
		}

		public bool ShowVerbose
		{
			set { }
		}

		public List<string> AllMessages
		{
			get
			{
				return _all;
			}
		}

		public string AllMessagesString()
		{
			return String.Join(" ", _all.ToArray());
		}

		public bool CancelRequested { get; set; }

		public bool ErrorEncountered
		{
			get { return false; }
			set { }
		}

		public IProgressIndicator ProgressIndicator
		{
			get;
			set;
		}
		public SynchronizationContext SyncContext { get; set; }

		public void Dispose()
		{
		}
	}

	public class ApiResponses
	{
		public static WebHeaderCollection GetWebHeaderCollection(Dictionary<string, string> parameters)
		{
			var headers = new WebHeaderCollection();
			foreach (var pair in parameters)
			{
				headers.Set(pair.Key, pair.Value);
			}
			return headers;
		}

		public static HgResumeApiResponse PushComplete()
		{
			return new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.OK,
				ResponseTimeInMilliseconds = 200,
				ResumableResponse = new HgResumeApiResponseHeaders(
					GetWebHeaderCollection(
						new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"}
										 }))
			};
		}

		public static HgResumeApiResponse PushAccepted(int startOfWindow)
		{
			return new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.Accepted,
				ResumableResponse = new HgResumeApiResponseHeaders(
					GetWebHeaderCollection(
						new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "RECEIVED"},
											 {"X-HgR-Version", "1"},
											 {"X-HgR-Sow", startOfWindow.ToString()}
										 })),
				ResponseTimeInMilliseconds = 200
			};
		}

		public static HgResumeApiResponse Reset()
		{
			return new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.BadRequest,
				ResumableResponse = new HgResumeApiResponseHeaders(
					GetWebHeaderCollection(
						new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "RESET"},
											 {"X-HgR-Version", "1"}
										 })),
				ResponseTimeInMilliseconds = 200
			};
		}


		public static HgResumeApiResponse Failed(string message)
		{
			var parameters = new Dictionary<string, string>
								 {
									 {"X-HgR-Status", "FAIL"},
									 {"X-HgR-Version", "1"}
								 };
			if (!String.IsNullOrEmpty(message))
			{
				parameters.Add("X-HgR-Error", message);
			}
			var response = new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.BadRequest,
				ResumableResponse = new HgResumeApiResponseHeaders(GetWebHeaderCollection(parameters)),
				ResponseTimeInMilliseconds = 200
			};
			return response;
		}

		public static HgResumeApiResponse Custom(HttpStatusCode status)
		{
			return new HgResumeApiResponse
					   {
						   HttpStatus = status,
						   ResumableResponse = new HgResumeApiResponseHeaders(new WebHeaderCollection()),
						   Content = new byte[0],
						   ResponseTimeInMilliseconds = 200
			};
		}

		public static HgResumeApiResponse Revisions(string revisions)
		{
			return new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.OK,
				ResumableResponse = new HgResumeApiResponseHeaders(GetWebHeaderCollection(
				new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"}
										 })),
				Content = Encoding.UTF8.GetBytes(revisions),
				ResponseTimeInMilliseconds = 200
			};
		}

		public static HgResumeApiResponse PullOk(int bundleSize, byte[] contentToSend)
		{
			if (contentToSend.Length > bundleSize)
			{
				throw new ArgumentException("bundleSize must be larger than the size of the content you are sending");
			}

			return new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.OK,
				ResumableResponse = new HgResumeApiResponseHeaders(GetWebHeaderCollection(
						new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "SUCCESS"},
											 {"X-HgR-Version", "1"},
											 {"X-HgR-BundleSize", bundleSize.ToString()},
											 {"X-HgR-ChunkSize", contentToSend.Length.ToString()}
										 })),
				Content = contentToSend,
				ResponseTimeInMilliseconds = 200
			};
		}

		public static HgResumeApiResponse PullNoChange()
		{
			return new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.NotModified,
				ResumableResponse = new HgResumeApiResponseHeaders(GetWebHeaderCollection(
						new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "NOCHANGE"},
											 {"X-HgR-Version", "1"}
										 })),
				ResponseTimeInMilliseconds = 200
			};
		}

		public static HgResumeApiResponse NotAvailable(string message)
		{
			return new HgResumeApiResponse
			{
				HttpStatus = HttpStatusCode.ServiceUnavailable,
				ResumableResponse = new HgResumeApiResponseHeaders(GetWebHeaderCollection(
						new Dictionary<string, string>
										 {
											 {"X-HgR-Status", "NOTAVAILABLE"},
											 {"X-HgR-Version", "1"}
										 })),
				Content = Encoding.UTF8.GetBytes(message),
				ResponseTimeInMilliseconds = 200
			};
		}
	}
}