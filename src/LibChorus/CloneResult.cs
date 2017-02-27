// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace Chorus
{
	/// <summary>
	/// The results of a clone attempt.
	/// </summary>
	public class CloneResult
	{
		/// <summary>Constructor</summary>
		public CloneResult(string actualLocation, CloneStatus cloneStatus)
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

