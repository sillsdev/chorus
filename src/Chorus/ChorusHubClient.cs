using System;
using System.Collections.Generic;
using System.ServiceModel;
using Chorus.ChorusHub;
using Chorus.VcsDrivers;
using Palaso.Code;

namespace Chorus
{
	public class ChorusHubClient
	{
		private IEnumerable<RepositoryInformation> _repositoryNames;
		private readonly ChorusHubServerInfo _chorusHubServerInfo;

		public ChorusHubClient(ChorusHubServerInfo chorusHubServerInfo)
		{
			Guard.AgainstNull(chorusHubServerInfo, "chorusHubServerInfo");

			_chorusHubServerInfo = chorusHubServerInfo;
		}

		public string HostName
		{
			get { return _chorusHubServerInfo.HostName; }
		}

		public bool ServerIsCompatibleWithThisClient
		{
			get { return _chorusHubServerInfo.ServerIsCompatibleWithThisClient; }
		}

		public IEnumerable<RepositoryInformation> GetRepositoryInformation(string queryString)
		{
			if(_repositoryNames != null)
				return _repositoryNames; //for now, there's no way to get an updated list except by making a new client

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
				((IContextChannel)channel).OperationTimeout = TimeSpan.FromMinutes(15);
				var jsonStrings = channel.GetRepositoryInformation(finalUrl);
				_repositoryNames = ImitationHubJSONService.ParseJsonStringsToChorusHubRepoInfos(jsonStrings);
			}
			finally
			{
				var comChannel = (ICommunicationObject)channel;	
				if (comChannel.State != CommunicationState.Faulted)
				{
					comChannel.Close();
				}
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
				((IContextChannel)channel).OperationTimeout = TimeSpan.FromMinutes(15);
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
				if (comChannel.State != CommunicationState.Faulted)
				{
					comChannel.Close();
				}
			}
		}
	}
}