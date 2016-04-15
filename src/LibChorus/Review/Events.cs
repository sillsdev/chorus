// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.Review
{
	/// <summary>
	/// RevisionSelected event.
	/// </summary>
	public class RevisionSelectedEvent : Event<Revision>
	{
	}

	/// <summary>
	/// ChangedRecordSelected event.
	/// </summary>
	public class ChangedRecordSelectedEvent : Event<IChangeReport>
	{
	}

	/// <summary>
	/// NavigateToRecord event.
	/// </summary>
	public class NavigateToRecordEvent : Event<string>
	{
	}

	/// <summary>
	/// Revision event arguments.
	/// </summary>
	public class RevisionEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Chorus.UI.Review.RevisionEventArgs"/> class.
		/// </summary>
		public RevisionEventArgs(Revision revision)
		{
			Revision = revision;
		}

		/// <summary>
		/// Gets the revision.
		/// </summary>
		public Revision Revision { get; private set; }
	}
}