using System;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	/// <summary>
	/// Interface that handles getting a teammate's shared project.
	/// </summary>
	public interface IGetSharedProject
	{
		/// <summary>
		/// Get a teammate's shared project from the specified source.
		/// </summary>
		/// <returns>
		/// A CloneResult that provides the clone results (e.g., success or failure) and the desired and actual clone locations.
		/// </returns>
		CloneResult GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, Func<string, bool> projectFilter, string baseLocalProjectDir, string preferredClonedFolderName);
	}

	/// <summary>
	/// The results of a clone attempt.
	/// </summary>
	public class CloneResult
	{
		internal CloneResult(string actualLocation, CloneStatus cloneStatus)
		{
			ActualLocation = actualLocation;
			CloneStatus = cloneStatus;
		}

		/// <summary>Get the actual location of a clone. (May, or may not, be the same as the desired location.)</summary>
		public string ActualLocation { get; private set; }
		/// <summary>Get the status of the clone attempt.</summary>
		public CloneStatus CloneStatus { get; private set; }
	}

	/// <summary>
	/// An enumeration of the possible repository sources.
	/// </summary>
	public enum ExtantRepoSource
	{
		/// <summary>Get a clone from the internet</summary>
		Internet,
		/// <summary>Get a clone from a USB drive</summary>
		Usb,
		/// <summary>Get a clone from a sharded network folder</summary>
		LocalNetwork
	}

	/// <summary>
	/// An indication of the success/failure of the clone attempt.
	/// </summary>
	public enum CloneStatus
	{
		/// <summary>The clone was made</summary>
		Created,
		/// <summary>The clone operation was cancelled</summary>
		Cancelled,
		/// <summary>The clone could not be created</summary>
		NotCreated
	}
}
