using System;
using System.Collections.Generic;
using System.ServiceModel;
using Chorus.VcsDrivers;

namespace Chorus.ChorusHub
{
	public class ChorusHubClient
	{
		private IEnumerable<RepositoryInformation> _repositoryNames;
		private readonly ChorusHubServerInfo _chorusHubServerInfo;

		public ChorusHubClient(ChorusHubServerInfo chorusHubServerInfo)
		{
			_chorusHubServerInfo = chorusHubServerInfo; // May be null, which is fine.
		}

		public string HostName
		{
			get { return _chorusHubServerInfo == null ? "" : _chorusHubServerInfo.HostName; }
		}

		public bool ServerIsCompatibleWithThisClient
		{
			get { return _chorusHubServerInfo != null && _chorusHubServerInfo.ServerIsCompatibleWithThisClient; }
		}

		public IEnumerable<RepositoryInformation> GetRepositoryInformation(string queryString)
		{
			if(_repositoryNames!=null)
				return _repositoryNames; //for now, there's no way to get an updated list except by making a new client

			if (_chorusHubServerInfo == null)
				throw new ApplicationException("Programmer, call Find() and get a non-null response before GetRepositoryInformation");

			const string genericUrl = "scheme://path?";
			var finalUrl = string.IsNullOrEmpty(queryString)
							   ? queryString
							   : genericUrl + queryString;
			var binding = new NetTcpBinding
			{
				Security = {Mode = SecurityMode.None}
			};

			var factory = new ChannelFactory<IChorusHubService>(binding, _chorusHubServerInfo.ServiceUri);

			var channel = factory.CreateChannel();
			try
			{
				var jsonStrings = channel.GetRepositoryInformation(finalUrl);
				_repositoryNames = ImitationHubJSONService.ParseJsonStringsToChorusHubRepoInfos(jsonStrings);
			}
			finally
			{
				(channel as ICommunicationObject).Close();
			}
			return _repositoryNames;
		}

		public string GetUrl(string repositoryName)
		{
			return _chorusHubServerInfo.GetHgHttpUri(repositoryName);
		}

		/// <summary>
		/// Since Hg Serve doesn't provide a way to make new repositories, this asks our ChorusHub wrapper
		/// to create the repository. The complexity comes in the timing; hg serve will eventually
		/// notice the new server, but we don't really know when.
		/// </summary>
		/// <param name="directoryName"></param>
		/// <param name="repositoryId"></param>
		/// <returns>true if we create a new repository and recommend the client wait until hg notices</returns>
		public bool PrepareHubToSync(string directoryName, string repositoryId)
		{
			//Enchance: after creating and init'ing the folder, it would be possible to keep asking
			//hg serve if it knows about the repository until finally it says "yes", instead of just
			//guessing at a single amount of time to wait
			var binding = new NetTcpBinding
			{
				Security = {Mode = SecurityMode.None}
			};
			var factory = new ChannelFactory<IChorusHubService>(binding, _chorusHubServerInfo.ServiceUri);

			var channel = factory.CreateChannel();
			try
			{
				var doWait = channel.PrepareToReceiveRepository(directoryName, repositoryId);
				return doWait;
			}
			catch (Exception error)
			{
				throw new ApplicationException("There was an error on the Chorus Hub Server, which was transmitted to the client.", error);
			}
			finally
			{
				var comChannel = (ICommunicationObject)channel;
				if (comChannel.State == CommunicationState.Opened)
				{
					comChannel.Close();
				}
			}
		}
	}
}